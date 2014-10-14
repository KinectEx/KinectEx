using System;
using System.IO;
using System.Threading;
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
    /// A recordable / replayable version of a <c>ColorFrame</c>.
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
        /// The pixel data for the bitmap contained in this frame.
        /// </summary>
        public byte[] FrameData
        {
            get
            {
                if (_frameData == null)
                {
                    // Assume we must read it from disk
                    return GetFrameDataAsync().Result;
                }
                return _frameData;
            }
        }

        /// <summary>
        /// Retrieve and decode the bitmap contained in this frame.
        /// </summary>
        [Obsolete("Use a ColorFrameBitmap instead")]
        public async Task<BitmapSource> GetBitmapAsync()
        {
            ColorFrameBitmap bitmap = new ColorFrameBitmap(this);
            bitmap.Update(this);
            return await Task.FromResult(bitmap.Bitmap);
        }

        // Multiple Constructor options

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplayColorFrame"/> class.
        /// </summary>
        internal ReplayColorFrame() { }

#if !NOSDK
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplayColorFrame"/> class
        /// based on the specified <c>ColorFrame</c>.
        /// </summary>
        /// <param name="frame">The frame.</param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplayColorFrame"/> class
        /// based on the specified <c>ColorFrame</c> and <c>byte</c> array.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="bytes">The bytes.</param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplayColorFrame"/> class
        /// by reading from the specified <c>BinaryReader</c> using the specified
        /// <c>IColorCodec</c>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="codec">The codec.</param>
        /// <returns>The <c>ReplayColorFrame</c></returns>
        /// <exception cref="System.IO.IOException">The recording appears to be corrupt.</exception>
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
            var isGoodFrame = false;
            try
            {
                if (reader.ReadString() == ReplayFrame.EndOfFrameMarker)
                {
                    isGoodFrame = true;
                }
            }
            catch { }

            if (!isGoodFrame)
            {
                System.Diagnostics.Debug.WriteLine("BAD FRAME...RESETTING");
                reader.BaseStream.Position = frameStartPos + frame.FrameSize;
                
                try
                {
                    if (reader.ReadString() != ReplayFrame.EndOfFrameMarker)
                    {
                        throw new IOException("The recording appears to be corrupt.");
                    }
                    return null;
                }
                catch
                {
                    throw new IOException("The recording appears to be corrupt.");
                }

            }

            return frame;
        }

        /// <summary>
        /// Used during replay to retrieve the uncompressed pixel data stored on 
        /// disk for this frame. Pixels will be in BGRA32 format.
        /// </summary>
        public Task<byte[]> GetFrameDataAsync()
        {
            return Task<byte[]>.Run(async () =>
            {
                Monitor.Enter(Stream);
                var bytes = new byte[FrameDataSize];

                long savedPosition = Stream.Position;
                Stream.Position = StreamPosition;

                Stream.Read(bytes, 0, FrameDataSize);

                Stream.Position = savedPosition;
                Monitor.Exit(Stream);

                return await Codec.DecodeAsync(bytes);
            });
        }
    }
}
