using System;
using System.IO;

namespace KinectEx.DVR
{
    /// <summary>
    /// Internal class that provides the services necessary to decode and playback
    /// a <c>ReplayBodyFrame</c>.
    /// </summary>
    internal class ReplayBodySystem : ReplaySystem
    {
        public event Action<ReplayBodyFrame> FrameArrived;

        public void AddFrame(BinaryReader reader, Version version)
        {
            var frame = ReplayBodyFrame.FromReader(reader, version);
            if (frame != null)
                this.Frames.Add(frame);
        }

        public override void PushCurrentFrame()
        {
            if (this.FrameCount == 0)
                return;

            var frame = (ReplayBodyFrame)this.Frames[CurrentFrame];
            if (FrameArrived != null)
                FrameArrived(frame);
        }
    }
}
