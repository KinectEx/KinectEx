using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

#if NETFX_CORE
using WindowsPreview.Kinect;
#else
using Microsoft.Kinect;
#endif

namespace KinectEx.DVR
{
    /// <summary>
    /// This class is one of two primary programmatic interfaces into the 
    /// KinectEx.DVR subsystem. Created to enable recording of frames to
    /// a <c>Stream</c>.
    /// </summary>
    public class KinectRecorder : IDisposable
    {
        BinaryWriter _writer;
        SemaphoreSlim _writerSemaphore = new SemaphoreSlim(1);
        KinectSensor _sensor;

        BodyRecorder _bodyRecorder;
        ColorRecorder _colorRecorder;
        DepthRecorder _depthRecorder;
        InfraredRecorder _infraredRecorder;

        BodyFrameReader _bodyReader;
        ColorFrameReader _colorReader;
        DepthFrameReader _depthReader;
        InfraredFrameReader _infraredReader;

        ConcurrentQueue<ReplayFrame> _recordQueue = new ConcurrentQueue<ReplayFrame>();

        bool _isStarted = false;
        bool _isStopped = false;
        Task _processFramesTask = null;
        CancellationTokenSource _processFramesCancellationTokenSource;

        DateTime _previousFlushTime;

        private bool _enableBodyRecorder;
        private bool _enableColorRecorder;
        private bool _enableDepthRecorder;
        private bool _enableInfraredRecorder;

        ////////////////////////////////////////////////////////////////////////////
        #region PROPERTIES
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Indicates whether the recorder is currently started. Will be true
        /// any time between the calls to Start() and StopAsync().
        /// </summary>
        public bool IsStarted
        {
            get { return _isStarted; }
        }

        /// <summary>
        /// Determines whether the KinectRecorder will record Body frames. Applies only when the
        /// KinectRecorder is in "Automatic" mode. Cannot be changed after recording has started.
        /// </summary>
        public bool EnableBodyRecorder
        {
            get { return _enableBodyRecorder; }
            set
            {
                if (_isStarted)
                    throw new InvalidOperationException("Cannot modify recorder properties after recording has started");

                _enableBodyRecorder = value;
            }
        }

        /// <summary>
        /// Determines whether the KinectRecorder will record Color frames. Applies only when the
        /// KinectRecorder is in "Automatic" mode. Cannot be changed after recording has started.
        /// </summary>
        public bool EnableColorRecorder
        {
            get { return _enableColorRecorder; }
            set
            {
                if (_isStarted)
                    throw new InvalidOperationException("Cannot modify recorder properties after recording has started");

                _enableColorRecorder = value;
            }
        }

        /// <summary>
        /// Determines whether the KinectRecorder will record Depth frames. Applies only when the
        /// KinectRecorder is in "Automatic" mode. Cannot be changed after recording has started.
        /// </summary>
        public bool EnableDepthRecorder
        {
            get { return _enableDepthRecorder; }
            set
            {
                if (_isStarted)
                    throw new InvalidOperationException("Cannot modify recorder properties after recording has started");

                _enableDepthRecorder = value;
            }
        }

        /// <summary>
        /// Determines whether the KinectRecorder will record Infrared frames. Applies only when the
        /// KinectRecorder is in "Automatic" mode. Cannot be changed after recording has started.
        /// </summary>
        public bool EnableInfraredRecorder
        {
            get { return _enableInfraredRecorder; }
            set
            {
                if (_isStarted)
                    throw new InvalidOperationException("Cannot modify recorder properties after recording has started");

                _enableInfraredRecorder = value;
            }
        }

        /// <summary>
        /// The codec used to encode Color frame images. By default, this is a <c>RawColorCodec</c>
        /// that records at full resolution (i.e., 1920 x 1080 x 4 bits/pixel). Cannot be changed
        /// after recording has started.
        /// </summary>
        public IColorCodec ColorRecorderCodec
        {
            get { return _colorRecorder.Codec; }
            set
            {
                if (value == null)
                    throw new NullReferenceException("Cannot set ColorRecorderCodec to null");

                if (_isStarted)
                    throw new InvalidOperationException("Cannot modify recorder properties after recording has started");

                _colorRecorder.Codec = value;
            }
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////
        #region CONSTRUCTOR / DESTRUCTOR
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// <para>
        ///     Creates a new instance of a <c>KinectRecorder</c> which can save frames to
        ///     the referenced stream.
        /// </para>
        /// <para>
        ///     The KinectRecorder can operate in two distinct modes. The "Automatic" mode
        ///     requires only that you pass a valid <c>KinectSensor</c> object to this
        ///     constructor. Recording of frames for each enabled frame type happens automatically
        ///     between the time Start() and StopAsync() are called.
        /// </para>
        /// <para>
        ///     In certain situations, the developer may wish to have more precise control
        ///     over when and how frames are recorded. If no <c>KinectSensor</c> is passed in
        ///     to this constructor, Start() and StopAsync() must still be called to begin and
        ///     end the recording session. However, the KinectRecorder will be in "Manual" mode,
        ///     and frames are recorded only when passed in to the RecordFrame() method.
        /// </para>
        /// </summary>
        /// <param name="stream">
        ///     The stream to which frames will be stored.
        /// </param>
        /// <param name="sensor">
        ///     If supplied, the <c>KinectSensor</c> from which frames will be automatically
        ///     retrieved for recording.
        /// </param>
        public KinectRecorder(Stream stream, KinectSensor sensor = null)
        {
            _writer = new BinaryWriter(stream);
            _sensor = sensor;
            _bodyRecorder = new BodyRecorder(_writer);
            _colorRecorder = new ColorRecorder(_writer);
            _depthRecorder = new DepthRecorder(_writer);
            _infraredRecorder = new InfraredRecorder(_writer);
        }

        ~KinectRecorder()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_bodyReader != null)
                {
                    _bodyReader.FrameArrived -= _bodyReader_FrameArrived;
                    _bodyReader.Dispose();
                    _bodyReader = null;
                }

                if (_colorReader != null)
                {
                    _colorReader.FrameArrived -= _colorReader_FrameArrived;
                    _colorReader.Dispose();
                    _colorReader = null;
                }

                if (_depthReader != null)
                {
                    _depthReader.FrameArrived -= _depthReader_FrameArrived;
                    _depthReader.Dispose();
                    _depthReader = null;
                }

                if (_infraredReader != null)
                {
                    _infraredReader.FrameArrived -= _infraredReader_FrameArrived;
                    _infraredReader.Dispose();
                    _infraredReader = null;
                }

                try
                {
                    _writerSemaphore.Wait();
                    if (_writer != null)
                    {
                        _writer.Flush();

                        if (_writer.BaseStream != null)
                        {
                            _writer.BaseStream.Flush();
                        }

                        _writer.Dispose();
                        _writer = null;
                    }
                }
                catch (Exception ex)
                {
                    // TODO: Change to log the error
                    System.Diagnostics.Debug.WriteLine(ex);
                }
                finally
                {
                    _writerSemaphore.Dispose();
                }
            }
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////
        #region PUBLIC METHODS
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Start the <c>KinectRecorder</c> session. This will write the file header and
        /// enable the recorder to begin processing frames.
        /// </summary>
        public void Start()
        {
            if (_isStarted)
                return;

            if (_isStopped)
                throw new InvalidOperationException("Cannot restart a recording after it has been stopped");

            if (_sensor != null)
            {
                if (EnableBodyRecorder)
                {
                    _bodyReader = _sensor.BodyFrameSource.OpenReader();
                    _bodyReader.FrameArrived += _bodyReader_FrameArrived;
                }

                if (EnableColorRecorder)
                {
                    _colorReader = _sensor.ColorFrameSource.OpenReader();
                    _colorReader.FrameArrived += _colorReader_FrameArrived;
                }

                if (EnableDepthRecorder)
                {
                    _depthReader = _sensor.DepthFrameSource.OpenReader();
                    _depthReader.FrameArrived += _depthReader_FrameArrived;
                }

                if (EnableInfraredRecorder)
                {
                    _infraredReader = _sensor.InfraredFrameSource.OpenReader();
                    _infraredReader.FrameArrived += _infraredReader_FrameArrived;
                }

                if (!_sensor.IsOpen)
                    _sensor.Open();

            }

            _isStarted = true;

            try
            {
                _writerSemaphore.Wait();

                // initialize and write file metadata
                var metadata = new FileMetadata()
                {
                    Version = this.GetType().GetTypeInfo().Assembly.GetName().Version.ToString(),
                    ColorCodecId = this.ColorRecorderCodec.CodecId
                };
                if (_sensor != null)
                {
                    //metadata.DepthCameraIntrinsics = _sensor.CoordinateMapper.GetDepthCameraIntrinsics();
                    //metadata.DepthFrameToCameraSpaceTable = _sensor.CoordinateMapper.GetDepthFrameToCameraSpaceTable();
                }
                else
                {
                    var sensor = KinectSensor.GetDefault();
                    if (sensor != null)
                    {
                        //metadata.DepthCameraIntrinsics = sensor.CoordinateMapper.GetDepthCameraIntrinsics();
                        //metadata.DepthFrameToCameraSpaceTable = sensor.CoordinateMapper.GetDepthFrameToCameraSpaceTable();
                    }
                }
                _writer.Write(JsonConvert.SerializeObject(metadata));
            }
            catch (Exception ex)
            {
                // TODO: Change to log the error
                System.Diagnostics.Debug.WriteLine(ex);
            }
            finally
            {
                _writerSemaphore.Release();
            }

            _processFramesTask = ProcessFramesAsync();
        }

        /// <summary>
        /// Stops the <c>KinectRecorder</c>, writes all frames remaining in the
        /// record queue, and closes the associated stream.
        /// </summary>
        public async Task StopAsync()
        {
            if (_isStopped)
                return;

            System.Diagnostics.Debug.WriteLine(">>> StopAsync (queue size {0})", _recordQueue.Count);

            _isStarted = false;
            _isStopped = true;

            if (_bodyReader != null)
            {
                _bodyReader.FrameArrived -= _bodyReader_FrameArrived;
                _bodyReader.Dispose();
                _bodyReader = null;
            }

            if (_colorReader != null)
            {
                _colorReader.FrameArrived -= _colorReader_FrameArrived;
                _colorReader.Dispose();
                _colorReader = null;
            }

            if (_depthReader != null)
            {
                _depthReader.FrameArrived -= _depthReader_FrameArrived;
                _depthReader.Dispose();
                _depthReader = null;
            }

            if (_infraredReader != null)
            {
                _infraredReader.FrameArrived -= _infraredReader_FrameArrived;
                _infraredReader.Dispose();
                _infraredReader = null;
            }

            try
            {
                _processFramesCancellationTokenSource.Cancel();
                await _processFramesTask;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("!!! Process Canceled");
            }
            _processFramesTask = null;

            await CloseWriterAsync();

            System.Diagnostics.Debug.WriteLine("<<< StopAsync (DONE!)");
        }

        /// <summary>
        /// Stops the <c>KinectRecorder</c>, discards all remaining frames in the
        /// record queue, and closes the associated stream.
        /// </summary>
        public async void CancelAsync()
        {
            if (_processFramesTask == null)
                return;

            System.Diagnostics.Debug.WriteLine(">>> CancelAsync (queue size {0})", _recordQueue.Count);

            _isStarted = false;
            _isStopped = true;

            try
            {
                _processFramesCancellationTokenSource.Cancel();
                await _processFramesTask;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("--- Cancel Acknowledged");
            }
            _processFramesTask = null;

            await CloseWriterAsync();

            System.Diagnostics.Debug.WriteLine("<<< CancelAsync (DONE!)");
        }

        /// <summary>
        /// Used in "Manual" mode to record a single <c>BodyFrame</c>.
        /// </summary>
        public void RecordFrame(BodyFrame frame)
        {
            if (!_isStarted)
                throw new InvalidOperationException("Cannot record frames unless the KinectRecorder is started.");

            if (frame != null)
            {
                _recordQueue.Enqueue(new ReplayBodyFrame(frame));
                System.Diagnostics.Debug.WriteLine("+++ Enqueued Body Frame ({0})", _recordQueue.Count);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("!!! FRAME SKIPPED (Body in KinectRecorder)");
            }
        }

        /// <summary>
        /// Used in "Manual" mode to record a single <c>BodyFrame</c> if
        /// the body frame data has already been retrieved from the frame.
        /// </summary>
        public void RecordFrame(BodyFrame frame, List<CustomBody> bodies)
        {
            if (!_isStarted)
                throw new InvalidOperationException("Cannot record frames unless the KinectRecorder is started.");

            if (frame != null)
            {
                _recordQueue.Enqueue(new ReplayBodyFrame(frame, bodies));
                System.Diagnostics.Debug.WriteLine("+++ Enqueued Body Frame ({0})", _recordQueue.Count);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("!!! FRAME SKIPPED (Body in KinectRecorder)");
            }
        }

        /// <summary>
        /// Used in "Manual" mode to record a single <c>BodyFrame</c> if
        /// the body frame data has already been retrieved from the frame.
        /// </summary>
        public void RecordFrame(BodyFrame frame, Body[] bodies)
        {
            if (!_isStarted)
                throw new InvalidOperationException("Cannot record frames unless the KinectRecorder is started.");

            if (frame != null)
            {
                _recordQueue.Enqueue(new ReplayBodyFrame(frame, bodies));
                System.Diagnostics.Debug.WriteLine("+++ Enqueued Body Frame ({0})", _recordQueue.Count);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("!!! FRAME SKIPPED (Body in KinectRecorder)");
            }
        }

        /// <summary>
        /// Used in "Manual" mode to record a single <c>ColorFrame</c>.
        /// </summary>
        public void RecordFrame(ColorFrame frame)
        {
            if (!_isStarted)
                throw new InvalidOperationException("Cannot record frames unless the KinectRecorder is started.");

            if (frame != null)
            {
                _recordQueue.Enqueue(new ReplayColorFrame(frame));
                System.Diagnostics.Debug.WriteLine("+++ Enqueued Color Frame ({0})", _recordQueue.Count);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("!!! FRAME SKIPPED (Color in KinectRecorder)");
            }
        }

        /// <summary>
        /// Used in "Manual" mode to record a single <c>ColorFrame</c> if
        /// the color frame data has already been retrieved from the frame.
        /// Note that the frame data must have been converted to BGRA format.
        /// </summary>
        public void RecordFrame(ColorFrame frame, byte[] bytes)
        {
            if (!_isStarted)
                throw new InvalidOperationException("Cannot record frames unless the KinectRecorder is started.");

            if (frame != null)
            {
                _recordQueue.Enqueue(new ReplayColorFrame(frame, bytes));
                System.Diagnostics.Debug.WriteLine("+++ Enqueued Color Frame ({0})", _recordQueue.Count);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("!!! FRAME SKIPPED (Color in KinectRecorder)");
            }
        }

        /// <summary>
        /// Used in "Manual" mode to record a single <c>DepthFrame</c>.
        /// </summary>
        public void RecordFrame(DepthFrame frame)
        {
            if (!_isStarted)
                throw new InvalidOperationException("Cannot record frames unless the KinectRecorder is started.");

            if (frame != null)
            {
                _recordQueue.Enqueue(new ReplayDepthFrame(frame));
                System.Diagnostics.Debug.WriteLine("+++ Enqueued Depth Frame ({0})", _recordQueue.Count);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("!!! FRAME SKIPPED (Depth in KinectRecorder)");
            }
        }

        /// <summary>
        /// Used in "Manual" mode to record a single <c>DepthFrame</c> if
        /// the depth frame data has already been retrieved from the frame.
        /// </summary>
        public void RecordFrame(DepthFrame frame, ushort[] frameData)
        {
            if (!_isStarted)
                throw new InvalidOperationException("Cannot record frames unless the KinectRecorder is started.");

            if (frame != null)
            {
                _recordQueue.Enqueue(new ReplayDepthFrame(frame, frameData));
                System.Diagnostics.Debug.WriteLine("+++ Enqueued Depth Frame ({0})", _recordQueue.Count);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("!!! FRAME SKIPPED (Depth in KinectRecorder)");
            }
        }

        /// <summary>
        /// Used in "Manual" mode to record a single <c>InfraredFrame</c>.
        /// </summary>
        public void RecordFrame(InfraredFrame frame)
        {
            if (!_isStarted)
                throw new InvalidOperationException("Cannot record frames unless the KinectRecorder is started.");

            if (frame != null)
            {
                _recordQueue.Enqueue(new ReplayInfraredFrame(frame));
                System.Diagnostics.Debug.WriteLine("+++ Enqueued Infrared Frame ({0})", _recordQueue.Count);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("!!! FRAME SKIPPED (Infrared in KinectRecorder)");
            }
        }

        /// <summary>
        /// Used in "Manual" mode to record a single <c>InfraredFrame</c> if
        /// the infrared frame data has already been retrieved from the frame.
        /// </summary>
        public void RecordFrame(InfraredFrame frame, ushort[] frameData)
        {
            if (!_isStarted)
                throw new InvalidOperationException("Cannot record frames unless the KinectRecorder is started.");

            if (frame != null)
            {
                _recordQueue.Enqueue(new ReplayInfraredFrame(frame, frameData));
                System.Diagnostics.Debug.WriteLine("+++ Enqueued Infrared Frame ({0})", _recordQueue.Count);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("!!! FRAME SKIPPED (Infrared in KinectRecorder)");
            }
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////
        #region SUPPORT CODE
        ////////////////////////////////////////////////////////////////////////////

#if NETFX_CORE
        void _bodyReader_FrameArrived(BodyFrameReader sender, BodyFrameArrivedEventArgs args)
#else
        void _bodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs args)
#endif
        {
            using (var frame = args.FrameReference.AcquireFrame())
            {
                if (_isStarted)
                    RecordFrame(frame);
            }
        }

#if NETFX_CORE
        void _colorReader_FrameArrived(ColorFrameReader sender, ColorFrameArrivedEventArgs args)
#else
        void _colorReader_FrameArrived(object sender, ColorFrameArrivedEventArgs args)
#endif
        {
            using (var frame = args.FrameReference.AcquireFrame())
            {
                if (_isStarted)
                    RecordFrame(frame);
            }
        }

#if NETFX_CORE
        void _depthReader_FrameArrived(DepthFrameReader sender, DepthFrameArrivedEventArgs args)
#else
        void _depthReader_FrameArrived(object sender, DepthFrameArrivedEventArgs args)
#endif
        {
            using (var frame = args.FrameReference.AcquireFrame())
            {
                if (_isStarted)
                    RecordFrame(frame);
            }
        }

#if NETFX_CORE
        void _infraredReader_FrameArrived(InfraredFrameReader sender, InfraredFrameArrivedEventArgs args)
#else
        void _infraredReader_FrameArrived(object sender, InfraredFrameArrivedEventArgs args)
#endif
        {
            using (var frame = args.FrameReference.AcquireFrame())
            {
                if (_isStarted)
                    RecordFrame(frame);
            }
        }

        private async Task ProcessFramesAsync()
        {
            _previousFlushTime = DateTime.Now;
            var cancellationToken = _processFramesCancellationTokenSource.Token;
            await Task.Run(async () =>
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    ReplayFrame frame;
                    if (_recordQueue.TryDequeue(out frame))
                    {
                        try
                        {
                            await _writerSemaphore.WaitAsync();

                            if (frame is ReplayBodyFrame)
                            {
                                await _bodyRecorder.RecordAsync((ReplayBodyFrame)frame);
                                System.Diagnostics.Debug.WriteLine("--- Processed Body Frame ({0})", _recordQueue.Count);
                            }
                            else if (frame is ReplayColorFrame)
                            {
                                await _colorRecorder.RecordAsync((ReplayColorFrame)frame);
                                System.Diagnostics.Debug.WriteLine("--- Processed Color Frame ({0})", _recordQueue.Count);
                            }
                            else if (frame is ReplayDepthFrame)
                            {
                                await _depthRecorder.RecordAsync((ReplayDepthFrame)frame);
                                System.Diagnostics.Debug.WriteLine("--- Processed Depth Frame ({0})", _recordQueue.Count);
                            }
                            else if (frame is ReplayInfraredFrame)
                            {
                                await _infraredRecorder.RecordAsync((ReplayInfraredFrame)frame);
                                System.Diagnostics.Debug.WriteLine("--- Processed Infrared Frame ({0})", _recordQueue.Count);
                            }
                            Flush();
                        }
                        catch (Exception ex)
                        {
                            // TODO: Change to log the error
                            System.Diagnostics.Debug.WriteLine(ex);
                        }
                        finally
                        {
                            _writerSemaphore.Release();
                        }
                    }
                    else
                    {
                        await Task.Delay(100);
                        if (_recordQueue.IsEmpty && _isStarted == false)
                        {
                            break;
                        }
                    }
                }
            }, cancellationToken).ConfigureAwait(false);
        }

        private void Flush()
        {
            var now = DateTime.Now;

            if (now.Subtract(_previousFlushTime).TotalSeconds > 10)
            {
                _previousFlushTime = now;
                _writer.Flush();
            }
        }

        private async Task CloseWriterAsync()
        {
            try
            {
                await _writerSemaphore.WaitAsync();
                if (_writer != null)
                {
                    _writer.Flush();

                    if (_writer.BaseStream != null)
                    {
                        _writer.BaseStream.Flush();
                    }

                    _writer.Dispose();
                    _writer = null;
                }
            }
            catch (Exception ex)
            {
                // TODO: Change to log the error
                System.Diagnostics.Debug.WriteLine(ex);
            }
            finally
            {
                _writerSemaphore.Release();
            }
        }

        #endregion
    }
}
