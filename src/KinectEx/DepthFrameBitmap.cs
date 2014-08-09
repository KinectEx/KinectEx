using System;

#if NETFX_CORE
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Media.Imaging;
using WindowsPreview.Kinect;
#else
using Microsoft.Kinect;
using System.Windows;
using System.Windows.Media.Imaging;
#endif

namespace KinectEx
{
    /// <summary>
    /// A class that makes it easy to create and update a WriteableBitmap
    /// from a Kinect SDK <c>DepthFrame</c>.
    /// </summary>
    public class DepthFrameBitmap : IDisposable
    {
        private WriteableBitmap _bitmap = null;
        private ushort[] _data = null;
        private byte[] _bytes = null;
#if NETFX_CORE
        private static Stream _stream = null;
#endif
        
        /// <summary>
        /// The <c>WriteableBitmap</c> representation of the <c>DepthFrame</c>
        /// (or frame data) passed in to one of the Update() methods.
        /// </summary>
        public WriteableBitmap Bitmap
	    {
		    get { return _bitmap;}
	    }

        public DepthFrameBitmap()
        {
            var sensor = KinectSensor.GetDefault();
            _bitmap = BitmapFactory.New(sensor.DepthFrameSource.FrameDescription.Width, sensor.DepthFrameSource.FrameDescription.Height);
            _data = new ushort[_bitmap.PixelWidth * _bitmap.PixelHeight];
            _bytes = new byte[_bitmap.PixelWidth * _bitmap.PixelHeight * 4];
#if NETFX_CORE
            _stream = _bitmap.PixelBuffer.AsStream();
#endif
        }

        ~DepthFrameBitmap()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
#if NETFX_CORE
            _stream.Dispose();
            _stream = null;
#endif
            _data = null;
            _bytes = null;
            _bitmap = null;
        }

        /// <summary>
        /// Update the Bitmap from the supplied <c>DepthFrameReference</c>.
        /// </summary>
        public void Update(DepthFrameReference frameReference)
        {
            bool processed = false;
            ushort minDepth = 0;
            ushort maxDepth = 0;
            using (var frame = frameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    frame.CopyFrameDataToArray(_data);
                    minDepth = frame.DepthMinReliableDistance;
                    maxDepth = frame.DepthMaxReliableDistance;
                    processed = true;
                }
            }

            if (processed)
            {
                Update(_data, minDepth, maxDepth);
            }
        }

        /// <summary>
        /// Update the Bitmap from the supplied <c>DepthFrame</c>.
        /// </summary>
        public void Update(DepthFrame frame)
        {
            if (frame != null)
            {
                frame.CopyFrameDataToArray(_data);
                ushort minDepth = frame.DepthMinReliableDistance;
                ushort maxDepth = frame.DepthMaxReliableDistance;
                Update(_data, minDepth, maxDepth);
            }
        }

        /// <summary>
        /// Update the Bitmap from the supplied depth frame data values.
        /// </summary>
        public void Update(ushort[] data, ushort minDepth, ushort maxDepth)
        {
            int colorPixelIndex = 0;
            for (int i = 0; i < data.Length; ++i)
            {
                ushort depth = data[i];
                byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);
                _bytes[colorPixelIndex++] = intensity; // B
                _bytes[colorPixelIndex++] = intensity; // G
                _bytes[colorPixelIndex++] = intensity; // R
                _bytes[colorPixelIndex++] = 255;       // A
            }

#if NETFX_CORE
            _stream.Seek(0, SeekOrigin.Begin);
            _stream.Write(_bytes, 0, _bytes.Length);
            _bitmap.Invalidate(); ;
#else
            _bitmap.WritePixels(
                new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight),
                _bytes,
                _bitmap.PixelWidth * 4,
                0);
#endif
        }
    }
}
