using System;

namespace KinectEx.DVR
{
    public abstract class ReplayFrame : IComparable
    {
        public readonly static String EndOfFrameMarker = "[EOF]";

        public FrameTypes FrameType { get; set; }

        public TimeSpan RelativeTime { get; set; }

        internal long FrameSize;

#if !NOSDK
        // Used to create linked list for queuing encoding and saving
        internal ReplayFrame NextFrame;
#endif

        public int CompareTo(object obj)
        {
            if (obj is ReplayFrame)
                return this.RelativeTime.CompareTo(((ReplayFrame)obj).RelativeTime);
            return 0;
        }
    }
}
