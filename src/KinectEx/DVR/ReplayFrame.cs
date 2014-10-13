using System;

namespace KinectEx.DVR
{
    /// <summary>
    /// Base class for all forms of recordable / replayable frames.
    /// </summary>
    public abstract class ReplayFrame : IComparable
    {
        internal readonly static String EndOfFrameMarker = "[EOF]";

        internal long FrameSize;

        /// <summary>
        /// The type of frame represented by this <c>ReplayFrame</c>.
        /// </summary>
        public FrameTypes FrameType { get; set; }

        /// <summary>
        /// The unique relative time at which this frame was captured.
        /// </summary>
        public TimeSpan RelativeTime { get; set; }

        /// <summary>
        /// Compare this frame to another for the purposes of sorting.
        /// </summary>
        public int CompareTo(object obj)
        {
            if (obj is ReplayFrame)
                return this.RelativeTime.CompareTo(((ReplayFrame)obj).RelativeTime);
            return 0;
        }
    }
}
