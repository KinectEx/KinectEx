using System;
using System.IO;
using System.Threading.Tasks;

#if NETFX_CORE
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
#else
using System.Windows;
using System.Windows.Media.Imaging;
#endif

#if NOSDK
using KinectEx.KinectSDK;
#elif NETFX_CORE
using WindowsPreview.Kinect;
#else
using Microsoft.Kinect;
#endif

namespace KinectEx.DVR
{
    public class ReplayColorFrame : ReplayFrame
    {
        internal Stream Stream;
        internal long StreamPosition;
        internal IColorCodec Codec;

        public int Width { get; internal set; }

        public int Height { get; internal set; }

        internal int FrameDataSize { get; set; }

        private byte[] _frameData = null;
        public byte[] FrameData
        {
            get
            {
                if (_frameData == null)
                {
                    // Assume we must read it from disk
                    var bytes = new byte[FrameDataSize];

                    long savedPosition = Stream.Position;
                    Stream.Position = StreamPosition;

                    Stream.Read(bytes, 0, FrameDataSize);

                    Stream.Position = savedPosition;

                    return bytes;
                }
                return _frameData;
            }
        }

        internal ReplayColorFrame() { }

#if !NOSDK
        internal ReplayColorFrame(ColorFrame frame)
        {
            this.Codec = ColorCodecs.Raw;

            this.FrameType = FrameTypes.Color;
            this.RelativeTime = frame.RelativeTime;

            this.Width = frame.FrameDescription.Width;
            this.Height = frame.FrameDescription.Height;

            this.FrameDataSize = this.Width * this.Height * 4; // BGRA is 4 bytes per pixel
            this._frameData = new Byte[this.FrameDataSize];

            if (frame.RawColorImageFormat == ColorImageFormat.Bgra)
            {
                frame.CopyRawFrameDataToArray(_frameData);
            }
            else
            {
                frame.CopyConvertedFrameDataToArray(_frameData, ColorImageFormat.Bgra);
            }
        }

        internal ReplayColorFrame(ColorFrame frame, byte[] bytes)
        {
            this.Codec = ColorCodecs.Raw;

            this.FrameType = FrameTypes.Color;
            this.RelativeTime = frame.RelativeTime;

            this.Width = frame.FrameDescription.Width;
            this.Height = frame.FrameDescription.Height;

            this.FrameDataSize = this.Width * this.Height * 4; // BGRA is 4 bytes per pixel
            this._frameData = bytes;
        }
#endif

        internal static ReplayColorFrame FromReader(BinaryReader reader, IColorCodec codec)
        {
            var frame = new ReplayColorFrame();

            frame.FrameType = FrameTypes.Color;
            frame.RelativeTime = TimeSpan.FromMilliseconds(reader.ReadDouble());
            frame.FrameSize = reader.ReadInt64();

            long frameStartPos = reader.BaseStream.Position;

            frame.Codec = codec;
            frame.Codec.ReadHeader(reader, frame);

            frame.Stream = reader.BaseStream;
            frame.StreamPosition = frame.Stream.Position;
            frame.Stream.Position += frame.FrameDataSize;

            // Do Frame Integrity Check
            if (reader.ReadString() != ReplayFrame.EndOfFrameMarker)
            {
                System.Diagnostics.Debug.WriteLine("BAD FRAME...RESETTING");
                reader.BaseStream.Position = frameStartPos + frame.FrameSize;
                if (reader.ReadString() != ReplayFrame.EndOfFrameMarker)
                {
                    throw new IOException("The recording appears to be corrupt.");
                }
                return null;
            }

            return frame;
        }

        public async Task<BitmapSource> GetBitmapAsync()
        {
            return await Codec.DecodeAsync(this.FrameData);
        }
    }
}
