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
    public class RawColorCodec : IColorCodec
    {
        public int CodecId { get { return 0; } }

        public int Width { get; set; }

        public int Height { get; set; }


        private int _outputWidth = int.MinValue;

        public int OutputWidth
        {
            get { return _outputWidth == int.MinValue ? Width : _outputWidth; }
            set { _outputWidth = value; }
        }

        private int _outputHeight = int.MinValue;

        public int OutputHeight
        {
            get { return _outputHeight == int.MinValue ? Height : _outputHeight; }
            set { _outputHeight = value; }
        }
        

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

        public void ReadHeader(BinaryReader reader, ReplayFrame frame)
        {
            var colorFrame = frame as ReplayColorFrame;

            if (colorFrame == null)
                throw new InvalidOperationException("Must be a ReplayColorFrame");

            colorFrame.Width = reader.ReadInt32();
            colorFrame.Height = reader.ReadInt32();
            colorFrame.FrameDataSize = reader.ReadInt32();
        }

        public async Task<BitmapSource> DecodeAsync(byte[] bytes)
        {
            return await Task<BitmapSource>.Run(() =>
            {
                return BitmapFactory.New(this.Width, this.Height).FromByteArray(bytes);
            });
        }
    }
}
