using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

#if NETFX_CORE
using Windows.UI.Core;
using Windows.UI.Xaml;
#else
using System.Windows.Threading;
#endif

namespace KinectEx.DVR
{
    public class KinectReplay : IDisposable, INotifyPropertyChanged
    {
        public static TimeSpan FrameTime = TimeSpan.FromTicks(333334);

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

        // Events
        public event EventHandler<ReplayFrameArrivedEventArgs<ReplayBodyFrame>> BodyFrameArrived;
        public event EventHandler<ReplayFrameArrivedEventArgs<ReplayColorFrame>> ColorFrameArrived;
        public event EventHandler<ReplayFrameArrivedEventArgs<ReplayDepthFrame>> DepthFrameArrived;

        public event PropertyChangedEventHandler PropertyChanged;

        // Replay
        ReplayColorSystem _colorReplay;
        ReplayDepthSystem _depthReplay;
        ReplayBodySystem _bodyReplay;

        List<ReplaySystem> _activeReplaySystems = new List<ReplaySystem>();

        public static readonly string IsStartedPropertyName = "IsStarted";
        private bool _isStarted = false;
        public bool IsStarted
        {
            get { return _isStarted; }
            internal set
            {
                _isStarted = value;
                NotifyPropertyChanged(IsStartedPropertyName);
            }
        }

        public static readonly string IsFinishedPropertyName = "IsFinished";
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

        public static readonly string LocationPropertyName = "Location";
        private TimeSpan _location = TimeSpan.Zero;
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

        public TimeSpan Duration { get; private set; }

        public bool HasBodyFrames
        {
            get { return _bodyReplay != null; }
        }

        public bool HasColorFrames
        {
            get { return _colorReplay != null; }
        }

        public bool HasDepthFrames
        {
            get { return _depthReplay != null; }
        }

        TimeSpan _minTimespan = TimeSpan.MaxValue;
        TimeSpan _maxTimespan = TimeSpan.MinValue;

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
                            _bodyReplay.AddFrame(_reader);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
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

        public void Start()
        {
            _timer.Start();
            IsStarted = true;
        }

        public void Stop()
        {
            _timer.Stop();
            IsStarted = false;
        }

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

        public void Dispose()
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

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
