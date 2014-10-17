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
        private int _mostRecentCurrentFrame = -1;

        private TimeSpan _currentRelativeTime;
        public static readonly string IsFinishedPropertyName = "IsFinished";
        private bool _isFinished = false;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the list of frames.
        /// </summary>
        public List<ReplayFrame> Frames { get; set; }

        /// <summary>
        /// Gets or sets the dictionary relating FrameTime to the frame's index
        /// in the Frames list.
        /// </summary>
        public Dictionary<TimeSpan, int> FrameTimeToIndex { get; set; }

        /// <summary>
        /// Gets or sets the starting offset.
        /// </summary>
        public TimeSpan StartingOffset { get; set; }

#if DEBUG
        System.Diagnostics.Stopwatch DEBUG_sw = new System.Diagnostics.Stopwatch();
        string DEBUG_name = "";
        TimeSpan DEBUG_lastRelativeTime = TimeSpan.Zero;
#endif

        /// <summary>
        /// Gets or sets the current relative time. Setting the time causes the
        /// current frame at that time to be pushed to consumer using the
        /// FrameArrived event (defined the derived classes).
        /// </summary>
        public TimeSpan CurrentRelativeTime
        {
            get { return _currentRelativeTime; }
            set
            {
                if (value == _currentRelativeTime)
                    return;

                _currentRelativeTime = value;

                var frame = this.CurrentFrame;
                if (frame != _mostRecentCurrentFrame)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(">>> PushCurrentFrame({0}) {1}ms {2}ms",
                        frame,
                        (Frames[frame].RelativeTime - DEBUG_lastRelativeTime).TotalMilliseconds,
                        DEBUG_sw.ElapsedMilliseconds);
                    DEBUG_lastRelativeTime = Frames[frame].RelativeTime;
                    DEBUG_sw.Restart();
#endif

                    _mostRecentCurrentFrame = frame;
                    this.PushCurrentFrame();

#if DEBUG
                    System.Diagnostics.Debug.WriteLine("<<< PushCurrentFrame() {0}", DEBUG_sw.ElapsedTicks);
#endif

                    if (frame == this.FrameCount - 1)
                        IsFinished = true;
                    else
                        IsFinished = false;
                }
            }
        }

        /// <summary>
        /// Gets the current frame number.
        /// </summary>
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

        /// <summary>
        /// Gets the frame count.
        /// </summary>
        public int FrameCount
        {
            get
            {
                return this.Frames.Count;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is finished.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaySystem"/> class.
        /// </summary>
        public ReplaySystem()
        {
#if DEBUG
            DEBUG_name = this.GetType().Name;
#endif

            this.Frames = new List<ReplayFrame>();
            this.FrameTimeToIndex = new Dictionary<TimeSpan, int>();
        }

        /// <summary>
        /// Notifies the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

        }

        /// <summary>
        /// Pushes the current frame.
        /// </summary>
        public abstract void PushCurrentFrame();
    }
}
