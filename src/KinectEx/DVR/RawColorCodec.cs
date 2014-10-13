using System;
using System.IO;
using System.Threading.Tasks;

#if NETFX_CORE
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows;
using System.Windows.Media;
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
        /// Unique ID for this <c>IColorCodec</c> instance.
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

#if !NETFX_CORE
        /// <summary>
        /// Gets the pixel format of the last image decoded.
        /// </summary>
        public PixelFormat PixelFormat { get; private set; }
#endif

        /// <summary>
        /// Encodes the specified bitmap data and outputs it to the specified
        /// <c>BinaryWriter</c>. Bitmap data should be in BGRA format.
        /// For internal use only.
        /// </summary>
        public async Task EncodeAsync(byte[] bytes, BinaryWriter writer)
        {
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
#if NETFX_CORE
                using (var bmpStream = new InMemoryRandomAccessStream())
                {
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, bmpStream);
                    encoder.BitmapTransform.ScaledWidth = (uint)this.OutputWidth;
                    encoder.BitmapTransform.ScaledHeight = (uint)this.OutputHeight;

                    encoder.SetPixelData(
                        BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Straight,
                        (uint)this.Width,
                        (uint)this.Height,
                        96,
                        96,
                        bytes);
                    await encoder.FlushAsync();

                    bmpStream.Seek(0);
                    var dec = await BitmapDecoder.CreateAsync(BitmapDecoder.BmpDecoderId, bmpStream);
                    var pixelDataProvider = await dec.GetPixelDataAsync();
                    var pixelData = pixelDataProvider.DetachPixelData();

                    if (writer.BaseStream == null || writer.BaseStream.CanWrite == false)
                        return;

                    // Header
                    writer.Write(this.OutputWidth);
                    writer.Write(this.OutputHeight);
                    writer.Write((int)pixelData.Length);

                    // Data
                    writer.Write(pixelData);
                }
#else
                WriteableBitmap bmp = BitmapFactory.New(this.Width, this.Height);
                int stride = this.Width * 4; // 4 bytes per pixel in BGRA
                var dirtyRect = new Int32Rect(0, 0, this.Width, this.Height);
                bmp.WritePixels(dirtyRect, bytes, stride, 0);
                var newBytes = await Task.FromResult(bmp.Resize(this.OutputWidth, this.OutputHeight, WriteableBitmapExtensions.Interpolation.NearestNeighbor).ToByteArray());

                // Header
                writer.Write(this.OutputWidth);
                writer.Write(this.OutputHeight);
                writer.Write(newBytes.Length);

                // Data
                writer.Write(newBytes);
#endif
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
        /// Decodes the supplied encoded bitmap data into an array of pixels.
        /// For internal use only.
        /// </summary>
        public async Task<byte[]> DecodeAsync(byte[] encodedBytes)
        {
#if !NETFX_CORE
            this.PixelFormat = PixelFormats.Pbgra32;
#endif
            return await Task.FromResult(encodedBytes);
        }
    }
}
