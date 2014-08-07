using System;
using System.IO;

namespace KinectEx.DVR
{
    internal class ReplayBodySystem : ReplaySystem
    {
        public event Action<ReplayBodyFrame> FrameArrived;

        public void AddFrame(BinaryReader reader)
        {
            var frame = ReplayBodyFrame.FromReader(reader);
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
