using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace KinectEx.DVR
{
    /// <summary>
    /// Internal base class for replay systems used to playback
    /// <c>ReplayFrame</c> instances.
    /// </summary>
    internal abstract class ReplaySystem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public List<ReplayFrame> Frames { get; set; }

        public Dictionary<TimeSpan, int> FrameTimeToIndex { get; set; }

        public TimeSpan StartingOffset { get; set; }

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        private TimeSpan _currentRelativeTime;
        public TimeSpan CurrentRelativeTime
        {
            get { return _currentRelativeTime; }
            set
            {
                if (value == _currentRelativeTime)
                    return;

                System.Diagnostics.Debug.WriteLine("??? CurrentRelativeTime Changed");

                var frame = this.CurrentFrame;
                if (frame != _currentFrame)
                {
                    sw.Start();
                    System.Diagnostics.Debug.WriteLine(">>> PushCurrentFrame()");
                    _currentFrame = frame;
                    //this.PushCurrentFrame();
                    this.PushCurrentFrame();
                    System.Diagnostics.Debug.WriteLine("<<< PushCurrentFrame() {0}", sw.ElapsedTicks);
                    sw.Reset();

                    if (frame == this.FrameCount - 1)
                        IsFinished = true;
                    else
                        IsFinished = false;
                }

                _currentRelativeTime = value;
            }
        }

        private int _currentFrame = -1;
        public int CurrentFrame
        {
            get
            {
                var key = this.FrameTimeToIndex.Keys.LastOrDefault(ts => ts <= _currentRelativeTime);
                if (key == TimeSpan.Zero)
                    return 0;
                else
                    return FrameTimeToIndex[key];
            }
        }

        public int FrameCount
        {
            get
            {
                return this.Frames.Count;
            }
        }

        public static readonly string IsFinishedPropertyName = "IsFinished";
        private bool _isFinished = false;
        public bool IsFinished
        {
            get { return _isFinished; }
            protected set
            {
                if (value == _isFinished)
                    return;

                _isFinished = value;
                NotifyPropertyChanged(IsFinishedPropertyName);
            }
        }

        public ReplaySystem()
        {
            this.Frames = new List<ReplayFrame>();
            this.FrameTimeToIndex = new Dictionary<TimeSpan, int>();
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

        }

        public abstract void PushCurrentFrame();
    }
}
