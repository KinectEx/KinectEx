using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

#if NETFX_CORE
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
        BinaryReader _reader;
        Stream _stream;

        readonly DispatcherTimer _timer = new DispatcherTimer()
        {
            Interval = FrameTime
        };
        readonly Stopwatch _stopwatch = new Stopwatch();

#if NETFX_CORE
        readonly CoreDispatcher _dispatcher;
#else
        readonly SynchronizationContext _synchronizationContext;
#endif

        // Replay
        ReplayColorSystem _colorReplay;
        ReplayDepthSystem _depthReplay;
        ReplayBodySystem _bodyReplay;

        List<ReplaySystem> _activeReplaySystems = new List<ReplaySystem>();

        TimeSpan _minTimespan = TimeSpan.MaxValue;
        TimeSpan _maxTimespan = TimeSpan.MinValue;

        // Property Backers
        private bool _isStarted = false;
        private TimeSpan _location = TimeSpan.Zero;

        ////////////////////////////////////////////////////////////////////////////
        #region PUBLIC STATIC MEMBERS
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The length of each frame (effectively 30fps ==> 33ms)
        /// </summary>
        public static TimeSpan FrameTime = TimeSpan.FromTicks(333334);

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

#if NETFX_CORE
			if(CoreWindow.GetForCurrentThread() != null)
				_dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
#else
            _synchronizationContext = SynchronizationContext.Current;
#endif

            _timer.Tick += _timer_Tick;

            var metadata = JsonConvert.DeserializeObject<FileMetadata>(_reader.ReadString());
            Version version = this.GetType().GetTypeInfo().Assembly.GetName().Version;
            Version.TryParse(metadata.Version, out version);

            while (_reader.BaseStream.Position != _reader.BaseStream.Length)
            {
                try
                {
                    FrameTypes type = (FrameTypes)_reader.ReadInt32();
                    switch (type)
                    {
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

            foreach (var replaySystem in _activeReplaySystems)
            {
                if (replaySystem.Frames.Count > 0)
                    replaySystem.StartingOffset = _minTimespan;
            }

            this.Duration = _maxTimespan - _minTimespan;
        }
        
        ~KinectReplay()
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

        #endregion

        ////////////////////////////////////////////////////////////////////////////
        #region SUPPORT CODE
        ////////////////////////////////////////////////////////////////////////////

        System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
#if NETFX_CORE
        void _timer_Tick(object sender, object e)
#else
        void _timer_Tick(object sender, EventArgs e)
#endif
        {
            _stopwatch.Start();
            this.Location += FrameTime;

            foreach (var replaySystem in _activeReplaySystems)
                replaySystem.CurrentRelativeTime = replaySystem.StartingOffset + this.Location;

            var interval = FrameTime - _stopwatch.Elapsed;
            _timer.Interval = interval < TimeSpan.Zero ? TimeSpan.Zero : interval;
            System.Diagnostics.Debug.WriteLine("{0}", interval);
            _stopwatch.Reset();
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
        private async void colorReplay_FrameArrived(ReplayColorFrame frame)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (ColorFrameArrived != null)
                    ColorFrameArrived(this, new ReplayFrameArrivedEventArgs<ReplayColorFrame> { Frame = frame });
            });
        }
        private async void depthReplay_FrameArrived(ReplayDepthFrame frame)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (DepthFrameArrived != null)
                    DepthFrameArrived(this, new ReplayFrameArrivedEventArgs<ReplayDepthFrame> { Frame = frame });
            });
        }
        private async void bodyReplay_FrameArrived(ReplayBodyFrame frame)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (BodyFrameArrived != null)
                    BodyFrameArrived(this, new ReplayFrameArrivedEventArgs<ReplayBodyFrame> { Frame = frame });
            });
        }
#else
        private void colorReplay_FrameArrived(ReplayColorFrame frame)
        {
            _synchronizationContext.Send(state =>
            {
                if (ColorFrameArrived != null)
                    ColorFrameArrived(this, new ReplayFrameArrivedEventArgs<ReplayColorFrame> { Frame = frame });
            }, null);
        }
        private void depthReplay_FrameArrived(ReplayDepthFrame frame)
        {
            _synchronizationContext.Send(state =>
            {
                if (DepthFrameArrived != null)
                    DepthFrameArrived(this, new ReplayFrameArrivedEventArgs<ReplayDepthFrame> { Frame = frame });
            }, null);
        }

        private void bodyReplay_FrameArrived(ReplayBodyFrame frame)
        {
            _synchronizationContext.Send(state =>
            {
                if (BodyFrameArrived != null)
                    BodyFrameArrived(this, new ReplayFrameArrivedEventArgs<ReplayBodyFrame> { Frame = frame });
            }, null);
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
