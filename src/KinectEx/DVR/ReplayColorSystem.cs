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

        public event Action<ReplayColorFrame> FrameArrived;

        public ReplayColorSystem(IColorCodec codec)
        {
            this._codec = codec;
        }

        public void AddFrame(BinaryReader reader)
        {
            var frame = ReplayColorFrame.FromReader(reader, _codec);
            if (frame != null)
                this.Frames.Add(frame);
        }

        public override void PushCurrentFrame()
        {
            if (this.FrameCount == 0)
                return;

            ReplayColorFrame frame = (ReplayColorFrame)this.Frames[CurrentFrame];
            if (FrameArrived != null)
                FrameArrived(frame);
        }
    }
}
