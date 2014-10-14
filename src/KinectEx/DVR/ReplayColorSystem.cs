using System;
using System.IO;

namespace KinectEx.DVR
{
    /// <summary>
    /// Internal class that provides the services necessary to decode and playback
    /// a <c>ReplayColorFrame</c>.
    /// </summary>
    internal class ReplayColorSystem : ReplaySystem
    {
        private IColorCodec _codec;

        /// <summary>
        /// Occurs when a new frame is ready to be displayed.
        /// </summary>
        public event Action<ReplayColorFrame> FrameArrived;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplayColorSystem"/> class.
        /// </summary>
        /// <param name="codec">The codec.</param>
        public ReplayColorSystem(IColorCodec codec)
        {
            this._codec = codec;
        }

        /// <summary>
        /// Adds a frame to the Frames list.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public void AddFrame(BinaryReader reader)
        {
            var frame = ReplayColorFrame.FromReader(reader, _codec);
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

            var frame = (ReplayColorFrame)this.Frames[CurrentFrame];
            if (FrameArrived != null)
                FrameArrived(frame);
        }
    }
}
