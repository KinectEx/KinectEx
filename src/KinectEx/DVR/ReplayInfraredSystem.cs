using System;
using System.IO;

namespace KinectEx.DVR
{
    /// <summary>
    /// Internal class that provides the services necessary to decode and playback
    /// a <c>ReplayInfraredFrame</c>.
    /// </summary>
    internal class ReplayInfraredSystem : ReplaySystem
    {
        public event Action<ReplayInfraredFrame> FrameArrived;

        public void AddFrame(BinaryReader reader)
        {
            var frame = ReplayInfraredFrame.FromReader(reader);
            if (frame != null)
                this.Frames.Add(frame);
        }

        public override void PushCurrentFrame()
        {
            if (this.FrameCount == 0)
                return;

            var frame = (ReplayInfraredFrame)this.Frames[CurrentFrame];
            if (FrameArrived != null)
                FrameArrived(frame);
        }
    }
}
