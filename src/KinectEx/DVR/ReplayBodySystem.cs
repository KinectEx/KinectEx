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
        /// <summary>
        /// Occurs when a new frame is ready to be displayed.
        /// </summary>
        public event Action<ReplayBodyFrame> FrameArrived;

        /// <summary>
        /// Adds a frame to the Frames list.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="version">The version.</param>
        public void AddFrame(BinaryReader reader, Version version)
        {
            var frame = ReplayBodyFrame.FromReader(reader, version);
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

            var frame = (ReplayBodyFrame)this.Frames[CurrentFrame];
            if (FrameArrived != null)
                FrameArrived(frame);
        }
    }
}
