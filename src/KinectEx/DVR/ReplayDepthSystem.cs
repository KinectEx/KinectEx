using System;
using System.IO;

namespace KinectEx.DVR
{
    /// <summary>
    /// Internal class that provides the services necessary to decode and playback
    /// a <c>ReplayDepthFrame</c>.
    /// </summary>
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

            var frame = (ReplayDepthFrame)this.Frames[CurrentFrame];
            if (FrameArrived != null)
                FrameArrived(frame);
        }
    }
}
