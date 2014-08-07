using System;

namespace KinectEx.DVR
{
    public class ReplayFrameArrivedEventArgs<T> : EventArgs where T : ReplayFrame
    {
        public T Frame { get; internal set; }
    }
}
