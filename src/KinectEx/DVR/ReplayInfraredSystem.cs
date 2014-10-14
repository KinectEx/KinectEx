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
        /// <summary>
        /// Occurs when a new frame is ready to be displayed.
        /// </summary>
        public event Action<ReplayInfraredFrame> FrameArrived;

        /// <summary>
        /// Adds a frame to the Frames list.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public void AddFrame(BinaryReader reader)
        {
            var frame = ReplayInfraredFrame.FromReader(reader);
            if (frame != null)
                this.Frames.Add(frame);
        }

        /// <summary>
        /// Pushes the current frame.
        /// </summary>
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
