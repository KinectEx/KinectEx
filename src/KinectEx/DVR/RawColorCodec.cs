using System;
using System.IO;
using System.Threading.Tasks;

#if NETFX_CORE
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows;
using System.Windows.Media.Imaging;
#endif

namespace KinectEx.DVR
{
    /// <summary>
    /// An <c>IColorCodec</c> that performs no image compression. By default,
    /// this will just encode and decode the raw bitmap data. However, if the
    /// OutputWidth and/or OutputHeight are changed, it will resize an encoded
    /// bitmap accordingly.
    /// </summary>
    public class RawColorCodec : IColorCodec
    {
        private int _outputHeight = int.MinValue;
        private int _outputWidth = int.MinValue;

        /// <summary>
        /// Uniue ID for this <c>IColorCodec</c> instance.
        /// </summary>
        public int CodecId { get { return 0; } }

        /// <summary>
        /// Width of the frame in pixels.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Height of the frame in pixels.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// If changed, resizes the width of an encoded bitmap to the specified
        /// number of pixels. By default, the frame will be encoded using the
        /// same width as the input bitmap.
        /// </summary>
        public int OutputWidth
        {
            get { return _outputWidth == int.MinValue ? Width : _outputWidth; }
            set { _outputWidth = value; }
        }

        /// <summary>
        /// If changed, resizes the height of an encoded bitmap to the specified
        /// number of pixels. By default, the frame will be encoded using the
        /// same height as the input bitmap.
        /// </summary>
        public int OutputHeight
        {
            get { return _outputHeight == int.MinValue ? Height : _outputHeight; }
            set { _outputHeight = value; }
        }

        /// <summary>
        /// Encodes the specified bitmap data and outputs it to the specified
        /// <c>BinaryWriter</c>. Bitmap data should be in BGRA format.
        /// For internal use only.
        /// </summary>
        public async Task EncodeAsync(byte[] bytes, BinaryWriter writer)
        {
            await Task.Delay(0); // can't run writeable bitmap stuff in background thread
            if (this.Width == this.OutputWidth && this.Height == this.OutputHeight)
            {
                // Header
                writer.Write(this.Width);
                writer.Write(this.Height);
                writer.Write(bytes.Length);

                // Data
                writer.Write(bytes);
            }
            else
            {
                WriteableBitmap bmp = BitmapFactory.New(this.Width, this.Height);
#if NETFX_CORE
                var ras = new InMemoryRandomAccessStream();
                ras.AsStream().Write(bytes, 0, bytes.Length);
                ras.AsStream().Flush();
                bmp.SetSource(ras);
#else
                int stride = this.Width * 4; // 4 bytes per pixel in BGRA
                var dirtyRect = new Int32Rect(0, 0, this.Width, this.Height);
                bmp.WritePixels(dirtyRect, bytes, stride, 0);
#endif
                var newBytes = bmp.Resize(this.OutputHeight, this.OutputHeight, WriteableBitmapExtensions.Interpolation.Bilinear).ToByteArray();

                // Header
                writer.Write(this.OutputWidth);
                writer.Write(this.OutputHeight);
                writer.Write(newBytes.Length);

                // Data
                writer.Write(newBytes);
            }
        }

        /// <summary>
        /// Reads the codec-specific header information from the supplied
        /// <c>BinaryReader</c> and writes it to the supplied <c>ReplayFrame</c>.
        /// For internal use only.
        /// </summary>
        public void ReadHeader(BinaryReader reader, ReplayFrame frame)
        {
            var colorFrame = frame as ReplayColorFrame;

            if (colorFrame == null)
                throw new InvalidOperationException("Must be a ReplayColorFrame");

            colorFrame.Width = reader.ReadInt32();
            colorFrame.Height = reader.ReadInt32();
            colorFrame.FrameDataSize = reader.ReadInt32();
        }

        /// <summary>
        /// Decodes the supplied encoded bitmap data and outputs a <c>BitmapSource</c>.
        /// For internal use only.
        /// </summary>
        public async Task<BitmapSource> DecodeAsync(byte[] bytes)
        {
            await Task.Delay(0);
            return BitmapFactory.New(this.Width, this.Height).FromByteArray(bytes);
        }
    }
}
