using System;
using System.IO;

namespace KinectEx.DVR
{
    internal class ReplayDepthSystem : ReplaySystem
    {
        public event Action<ReplayDepthFrame> FrameArrived;

        public void AddFrame(BinaryReader reader)
        {
            var frame = ReplayDepthFrame.FromReader(reader);
            if (frame != null)
                this.Frames.Add(frame);
        }

        public override void PushCurrentFrame()
        {
            if (this.FrameCount == 0)
                return;

            ReplayDepthFrame frame = (ReplayDepthFrame)this.Frames[CurrentFrame];
            if (FrameArrived != null)
                FrameArrived(frame);
        }
    }
}
