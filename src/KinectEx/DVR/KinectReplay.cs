using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

#if NETFX_CORE
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
#else
using System.Windows.Threading;
#endif

namespace KinectEx.DVR
{
    /// <summary>
    /// This class is one of two primary programmatic interfaces into the 
    /// KinectEx.DVR subsystem. Created to enable playback of frames from
    /// a <c>Stream</c>.
    /// </summary>
    public class KinectReplay : IDisposable, INotifyPropertyChanged
    {
        private BinaryReader _reader;
        private Stream _stream;

        private readonly DispatcherTimer _timer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromTicks(FrameTime.Ticks / 2)
        };
        private readonly Stopwatch _actualElapsedTimeStopwatch = new Stopwatch();

        // Replay
        private ReplayBodySystem _bodyReplay;
        private ReplayColorSystem _colorReplay;
        private ReplayDepthSystem _depthReplay;
        private ReplayInfraredSystem _infraredReplay;

        private List<ReplaySystem> _activeReplaySystems = new List<ReplaySystem>();

        private TimeSpan _minTimespan = TimeSpan.MaxValue;
        private TimeSpan _maxTimespan = TimeSpan.MinValue;

        // Property Backers
        private bool _isStarted = false;
        private TimeSpan _location = TimeSpan.Zero;

        ////////////////////////////////////////////////////////////////////////////
        #region PUBLIC STATIC MEMBERS
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The length of each frame (effectively 30fps ==> 33.3ms)
        /// </summary>
        public static TimeSpan FrameTime = TimeSpan.FromTicks(333333);

        /// <summary>
        /// The string name of the IsStarted property (for use in PropertyChanged
        /// event handlers).
        /// </summary>
        public static readonly string IsStartedPropertyName = "IsStarted";

        /// <summary>
        /// The string name of the IsFinished property (for use in PropertyChanged
        /// event handlers).
        /// </summary>
        public static readonly string IsFinishedPropertyName = "IsFinished";

        /// <summary>
        /// The string name of the Location property (for use in PropertyChanged
        /// event handlers).
        /// </summary>
        public static readonly string LocationPropertyName = "Location";

        #endregion

        ////////////////////////////////////////////////////////////////////////////
        #region EVENTS
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Signals the "arrival" of a new <c>ReplayBodyFrame</c>.
        /// </summary>
        public event EventHandler<ReplayFrameArrivedEventArgs<ReplayBodyFrame>> BodyFrameArrived;

        /// <summary>
        /// Signals the "arrival" of a new <c>ReplayColorFrame</c>.
        /// </summary>
        public event EventHandler<ReplayFrameArrivedEventArgs<ReplayColorFrame>> ColorFrameArrived;

        /// <summary>
        /// Signals the "arrival" of a new <c>ReplayDepthFrame</c>.
        /// </summary>
        public event EventHandler<ReplayFrameArrivedEventArgs<ReplayDepthFrame>> DepthFrameArrived;

        /// <summary>
        /// Signals the "arrival" of a new <c>InfraredDepthFrame</c>.
        /// </summary>
        public event EventHandler<ReplayFrameArrivedEventArgs<ReplayInfraredFrame>> InfraredFrameArrived;

        /// <summary>
        /// Signals a change in value of one of the properties.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        ////////////////////////////////////////////////////////////////////////////
        #region PROPERTIES
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Whether this instance of <c>KinectReplay</c> is currently started.
        /// </summary>
        public bool IsStarted
        {
            get { return _isStarted; }
            internal set
            {
                _isStarted = value;
                NotifyPropertyChanged(IsStartedPropertyName);
            }
        }

        /// <summary>
        /// Whether this instance of <c>KinectReplay</c> has finished playback.
        /// While IsStarted can change to false any time playback is stopped,
        /// this property is only changed when playback stops due to reaching the
        /// end of the playback stream.
        /// </summary>
        public bool IsFinished
        {
            get
            {
                foreach (var replaySystem in _activeReplaySystems)
                    if (!replaySystem.IsFinished)
                        return false;

                return true;
            }
        }

        /// <summary>
        /// The current location within the playback stream.
        /// </summary>
        public TimeSpan Location
        {
            get
            {
                return _location;
            }
            private set
            {
                _location = value;
                NotifyPropertyChanged(LocationPropertyName);
            }
        }

        /// <summary>
        /// The total duration of the playback stream.
        /// </summary>
        public TimeSpan Duration { get; private set; }

        /// <summary>
        /// Whether this playback stream contains <c>ReplayBodyFrame</c> frames.
        /// </summary>
        public bool HasBodyFrames
        {
            get { return _bodyReplay != null; }
        }

        /// <summary>
        /// Whether this playback stream contains <c>ReplayColorFrame</c> frames.
        /// </summary>
        public bool HasColorFrames
        {
            get { return _colorReplay != null; }
        }

        /// <summary>
        /// Whether this playback stream contains <c>ReplayDepthFrame</c> frames.
        /// </summary>
        public bool HasDepthFrames
        {
            get { return _depthReplay != null; }
        }

        /// <summary>
        /// Whether this playback stream contains <c>InfraredDepthFrame</c> frames.
        /// </summary>
        public bool HasInfraredFrames
        {
            get { return _infraredReplay != null; }
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////
        #region CONSTRUCTOR / DESTRUCTOR
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of a <c>KinectReplay</c> using the referenced stream.
        /// This will read in the headers for all frames in the stream and make them
        /// available for playback. The stream will remain open until the instance
        /// is disposed.
        /// </summary>
        public KinectReplay(Stream stream)
        {
            this._stream = stream;
            _reader = new BinaryReader(stream);

            _timer.Tick += _timer_Tick;

            var metadata = JsonConvert.DeserializeObject<FileMetadata>(_reader.ReadString());
            Version version = this.GetType().GetTypeInfo().Assembly.GetName().Version; // default to this
            Version.TryParse(metadata.Version, out version);

            while (_reader.BaseStream.Position != _reader.BaseStream.Length)
            {
                try
                {
                    FrameTypes type = (FrameTypes)_reader.ReadInt32();
                    switch (type)
                    {
                        case FrameTypes.Body:
                            if (_bodyReplay == null)
                            {
                                _bodyReplay = new ReplayBodySystem();
                                _activeReplaySystems.Add(_bodyReplay);
                                _bodyReplay.PropertyChanged += replay_PropertyChanged;
                                _bodyReplay.FrameArrived += bodyReplay_FrameArrived;
                            }
                            _bodyReplay.AddFrame(_reader, version);
                            break;
                        case FrameTypes.Color:
                            if (_colorReplay == null)
                            {
                                IColorCodec codec = new RawColorCodec();
                                if (metadata.ColorCodecId == ColorCodecs.Jpeg.CodecId)
                                    codec = new JpegColorCodec();

                                _colorReplay = new ReplayColorSystem(codec);
                                _activeReplaySystems.Add(_colorReplay);
                                _colorReplay.PropertyChanged += replay_PropertyChanged;
                                _colorReplay.FrameArrived += colorReplay_FrameArrived;
                            }
                            _colorReplay.AddFrame(_reader);
                            break;
                        case FrameTypes.Depth:
                            if (_depthReplay == null)
                            {
                                _depthReplay = new ReplayDepthSystem();
                                _activeReplaySystems.Add(_depthReplay);
                                _depthReplay.PropertyChanged += replay_PropertyChanged;
                                _depthReplay.FrameArrived += depthReplay_FrameArrived;
                            }
                            _depthReplay.AddFrame(_reader);
                            break;
                        case FrameTypes.Infrared:
                            if (_infraredReplay == null)
                            {
                                _infraredReplay = new ReplayInfraredSystem();
                                _activeReplaySystems.Add(_infraredReplay);
                                _infraredReplay.PropertyChanged += replay_PropertyChanged;
                                _infraredReplay.FrameArrived += infraredReplay_FrameArrived;
                            }
                            _infraredReplay.AddFrame(_reader);
                            break;
                    }
                }
                catch
                {
                    throw;
                }
            }

            foreach (var replaySystem in _activeReplaySystems)
            {
                if (replaySystem.Frames.Count > 0)
                {
                    replaySystem.Frames.Sort();

                    for (var i = 0; i < replaySystem.Frames.Count; i++)
                    {
                        replaySystem.FrameTimeToIndex[replaySystem.Frames[i].RelativeTime] =  i;
                    }

                    var first = replaySystem.Frames.First().RelativeTime;
                    var last = replaySystem.Frames.Last().RelativeTime;
                    if (first < _minTimespan)
                        _minTimespan = first;
                    if (last > _maxTimespan)
                        _maxTimespan = last;
                }
            }

            bool hasFrames = false;

            foreach (var replaySystem in _activeReplaySystems)
            {
                if (replaySystem.Frames.Count > 0)
                {
                    replaySystem.StartingOffset = _minTimespan;
                    hasFrames = true;
                }
            }

            if (hasFrames)
            {
                this.Duration = _maxTimespan - _minTimespan;
            }
            else
            {
                this.Duration = TimeSpan.Zero;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="KinectReplay"/> class.
        /// </summary>
        ~KinectReplay()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();

                _colorReplay = null;
                _depthReplay = null;
                _bodyReplay = null;

                if (_reader != null)
                {
                    _reader.Dispose();
                    _reader = null;
                }

                if (_stream != null)
                {
                    _stream.Dispose();
                    _stream = null;
                }
            }
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////
        #region PUBLIC METHODS
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Start playback.
        /// </summary>
        public void Start()
        {
            _timer.Start();
            IsStarted = true;
        }

        /// <summary>
        /// Stop playback.
        /// </summary>
        public void Stop()
        {
            _timer.Stop();
            _actualElapsedTimeStopwatch.Reset();
            IsStarted = false;
        }

        /// <summary>
        /// Move the playback location to the specified time.
        /// </summary>
        public void ScrubTo(TimeSpan newLocation)
        {
            if (newLocation > this.Duration)
                newLocation = this.Duration;

            this.Location = newLocation;

            foreach (var replaySystem in _activeReplaySystems)
            {
                replaySystem.CurrentRelativeTime = _minTimespan + newLocation;
                replaySystem.PushCurrentFrame();
            }
        }

        /// <summary>
        /// Export all of the frames in this 
        /// </summary>
        /// <param name="exportDir"></param>
        public async Task ExportColorFramesAsync(string exportDir)
        {
            if (!this.HasColorFrames || _colorReplay == null)
            {
                throw new InvalidOperationException("KDVR file has no color frames.");
            }

            int frameCounter = 0;
            var jpegCodec = ColorCodecs.Jpeg;
            var jpegCodecId = jpegCodec.CodecId;
            var lastRelativeTime = TimeSpan.MaxValue;
            foreach (var frame in _colorReplay.Frames)
            {
                ReplayColorFrame rcf = frame as ReplayColorFrame;

                var elapsed = rcf.RelativeTime - lastRelativeTime;
                lastRelativeTime = rcf.RelativeTime;
                var numFrames = 1;
                var mills = (int)Math.Ceiling(elapsed.TotalMilliseconds);
                if (mills > 60)
                {
                    numFrames = mills / 33;
                }

                for (int i = 0; i < numFrames; i++)
                {
                    var fileName = string.Format("\\{0:000000}.jpeg", frameCounter++);

#if NETFX_CORE
                    var file = await StorageFile.GetFileFromPathAsync(exportDir + fileName);
                    using (var jpegStream = await file.OpenStreamForWriteAsync())

#else
                    using (var jpegStream = new FileStream(exportDir + fileName, FileMode.Create, FileAccess.Write))
#endif
                    {
                        using (var jpegWriter = new BinaryWriter(jpegStream))
                        {
                            var bytes = rcf.GetRawFrameData();

                            if (rcf.Codec.CodecId == jpegCodecId)
                            {
                                jpegWriter.Write(bytes);
                            }
                            else
                            {
                                await jpegCodec.EncodeAsync(bytes, jpegWriter);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////
        #region SUPPORT CODE
        ////////////////////////////////////////////////////////////////////////////

#if NETFX_CORE
        void _timer_Tick(object sender, object e)
#else
        void _timer_Tick(object sender, EventArgs e)
#endif
        {
            this.Location += _actualElapsedTimeStopwatch.Elapsed;
            _actualElapsedTimeStopwatch.Restart();

            foreach (var replaySystem in _activeReplaySystems)
                replaySystem.CurrentRelativeTime = replaySystem.StartingOffset + this.Location;
        }

        private void replay_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ReplaySystem.IsFinishedPropertyName)
            {
                if (this.IsFinished) // checks all replay systems
                {
                    foreach (var replaySystem in _activeReplaySystems)
                        replaySystem.CurrentRelativeTime = replaySystem.StartingOffset;

                    Stop();
                    this.Location = TimeSpan.Zero;
                    NotifyPropertyChanged(IsFinishedPropertyName);
                }
            }
        }

#if NETFX_CORE
        private void bodyReplay_FrameArrived(ReplayBodyFrame frame)
        {
            if (BodyFrameArrived != null)
                BodyFrameArrived(this, new ReplayFrameArrivedEventArgs<ReplayBodyFrame> { Frame = frame });
        }
        private void colorReplay_FrameArrived(ReplayColorFrame frame)
        {
            if (ColorFrameArrived != null)
                ColorFrameArrived(this, new ReplayFrameArrivedEventArgs<ReplayColorFrame> { Frame = frame });
        }
        private void depthReplay_FrameArrived(ReplayDepthFrame frame)
        {
            if (DepthFrameArrived != null)
                DepthFrameArrived(this, new ReplayFrameArrivedEventArgs<ReplayDepthFrame> { Frame = frame });
        }
        private void infraredReplay_FrameArrived(ReplayInfraredFrame frame)
        {
            if (InfraredFrameArrived != null)
                InfraredFrameArrived(this, new ReplayFrameArrivedEventArgs<ReplayInfraredFrame> { Frame = frame });
        }
#else
        private void bodyReplay_FrameArrived(ReplayBodyFrame frame)
        {
            if (BodyFrameArrived != null)
                BodyFrameArrived(this, new ReplayFrameArrivedEventArgs<ReplayBodyFrame> { Frame = frame });
        }
        private void colorReplay_FrameArrived(ReplayColorFrame frame)
        {
            if (ColorFrameArrived != null)
                ColorFrameArrived(this, new ReplayFrameArrivedEventArgs<ReplayColorFrame> { Frame = frame });
        }
        private void depthReplay_FrameArrived(ReplayDepthFrame frame)
        {
            if (DepthFrameArrived != null)
                DepthFrameArrived(this, new ReplayFrameArrivedEventArgs<ReplayDepthFrame> { Frame = frame });
        }
        private void infraredReplay_FrameArrived(ReplayInfraredFrame frame)
        {
            if (InfraredFrameArrived != null)
                InfraredFrameArrived(this, new ReplayFrameArrivedEventArgs<ReplayInfraredFrame> { Frame = frame });
        }
#endif

        void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
