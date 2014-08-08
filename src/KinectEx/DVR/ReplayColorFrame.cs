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
    /// <summary>
    /// A recordable / replayable version of a <b>ColorFrame</b>.
    /// </summary>
    public class ReplayColorFrame : ReplayFrame
    {
        private byte[] _frameData = null;

        // internal fields & properties
        internal Stream Stream;
        internal long StreamPosition;
        internal IColorCodec Codec;

        // needs to be an internal property as it is used by the codec
        internal int FrameDataSize { get; set; }

        /// <summary>
        /// The width in pixels of the bitmap contained in this frame.
        /// </summary>
        public int Width { get; internal set; }

        /// <summary>
        /// The height in pixels of the bitmap contained in this frame.
        /// </summary>
        public int Height { get; internal set; }

        /// <summary>
        /// The raw (potentially compressed) bits comprising the bitmap
        /// contained in this frame.
        /// </summary>
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

        /// <summary>
        /// Retrieve and decode the bitmap contained in this frame.
        /// </summary>
        public async Task<BitmapSource> GetBitmapAsync()
        {
            Codec.Width = this.Width;
            Codec.Height = this.Height;
            return await Codec.DecodeAsync(this.FrameData);
        }

        // Multiple Constructor options

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

        // and a factory method

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
    }
}
