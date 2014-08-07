using System;
using System.IO;

#if NETFX_CORE
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
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
    public class ReplayDepthFrame : ReplayFrame
    {
        internal Stream Stream;
        internal long StreamPosition;

        public uint DepthMinReliableDistance { get; set; }

        public uint DepthMaxReliableDistance { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public uint BytesPerPixel { get; private set; }

        private ushort[] _frameData = null;
        public ushort[] FrameData
        {
            get
            {
                if (_frameData == null)
                {
                    // Assume we must read it from disk
                    var reader = new BinaryReader(Stream);
                    var data = new ushort[this.Width * this.Height];

                    long savedPosition = Stream.Position;
                    Stream.Position = StreamPosition;

                    for (int index = 0; index < data.Length; index++)
                    {
                        data[index] = reader.ReadUInt16();
                    }

                    Stream.Position = savedPosition;

                    return data;
                }
                return _frameData;
            }
        }

        internal ReplayDepthFrame() { }

#if !NOSDK
        internal ReplayDepthFrame(DepthFrame frame)
        {
            this.FrameType = FrameTypes.Depth;
            this.RelativeTime = frame.RelativeTime;

            this.DepthMinReliableDistance = frame.DepthMinReliableDistance;
            this.DepthMaxReliableDistance = frame.DepthMaxReliableDistance;

            this.Width = frame.FrameDescription.Width;
            this.Height = frame.FrameDescription.Height;
            this.BytesPerPixel = frame.FrameDescription.BytesPerPixel;

            _frameData = new ushort[this.Width * this.Height];

            frame.CopyFrameDataToArray(_frameData);
        }

        internal ReplayDepthFrame(DepthFrame frame, ushort[] frameData)
        {
            this.FrameType = FrameTypes.Depth;
            this.RelativeTime = frame.RelativeTime;

            this.DepthMinReliableDistance = frame.DepthMinReliableDistance;
            this.DepthMaxReliableDistance = frame.DepthMaxReliableDistance;

            this.Width = frame.FrameDescription.Width;
            this.Height = frame.FrameDescription.Height;
            this.BytesPerPixel = frame.FrameDescription.BytesPerPixel;

            _frameData = frameData;
        }
#endif

        internal static ReplayDepthFrame FromReader(BinaryReader reader)
        {
            var frame = new ReplayDepthFrame();

            frame.FrameType = FrameTypes.Depth;
            frame.RelativeTime = TimeSpan.FromMilliseconds(reader.ReadDouble());
            frame.FrameSize = reader.ReadInt64();

            long frameStartPos = reader.BaseStream.Position;

            frame.DepthMinReliableDistance = reader.ReadUInt32();
            frame.DepthMaxReliableDistance = reader.ReadUInt32();

            frame.Width = reader.ReadInt32();
            frame.Height = reader.ReadInt32();
            frame.BytesPerPixel = reader.ReadUInt32();

            frame.Stream = reader.BaseStream;
            frame.StreamPosition = frame.Stream.Position;

            frame.Stream.Position += frame.Width * frame.Height * frame.BytesPerPixel;

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

        private byte[] ConvertDepthFrame()
        {
            var frameData = this.FrameData;
            var min = this.DepthMinReliableDistance;
            var max = this.DepthMaxReliableDistance;
            var bytes = new byte[this.Width * this.Height * 4];
            int colorPixelIndex = 0;
            for (int i = 0; i < frameData.Length; ++i)
            {
                ushort depth = frameData[i];
                byte intensity = (byte)(depth >= min && depth <= max ? depth : 0);
                bytes[colorPixelIndex++] = intensity; // B
                bytes[colorPixelIndex++] = intensity; // G
                bytes[colorPixelIndex++] = intensity; // R
                bytes[colorPixelIndex++] = 255;       // A
            }
            return bytes;
        }

        public BitmapSource GetBitmap()
        {
            return BitmapFactory.New(this.Width, this.Height).FromByteArray(ConvertDepthFrame());
        }

    }
}
