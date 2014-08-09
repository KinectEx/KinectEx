using System;

#if NETFX_CORE
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using WindowsPreview.Kinect;
#else
using Microsoft.Kinect;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endif

namespace KinectEx
{
    /// <summary>
    /// A class that makes it easy to create and update a WriteableBitmap
    /// from a Kinect SDK <c>ColorFrame</c>.
    /// </summary>
    public class ColorFrameBitmap : IDisposable
    {
        private WriteableBitmap _bitmap = null;
        private byte[] _bytes = null;
#if NETFX_CORE
        private Stream _stream = null;
#else
        private Int32Rect _dirtyRect;
#endif

        /// <summary>
        /// The <c>WriteableBitmap</c> representation of the <c>ColorFrame</c>
        /// (or frame data) passed in to one of the Update() methods.
        /// </summary>
        public WriteableBitmap Bitmap
	    {
		    get { return _bitmap; }
	    }
	
        public ColorFrameBitmap()
        {
            var sensor = KinectSensor.GetDefault();
            _bitmap = BitmapFactory.New(sensor.ColorFrameSource.FrameDescription.Width, sensor.ColorFrameSource.FrameDescription.Height);
            _bytes = new byte[_bitmap.PixelWidth * _bitmap.PixelHeight * 4];
#if NETFX_CORE
            _stream = _bitmap.PixelBuffer.AsStream();
#else
            _dirtyRect = new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight);
#endif
        }

        ~ColorFrameBitmap()
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
            _bytes = null;
            _bitmap = null;
        }

        /// <summary>
        /// Update the Bitmap from the supplied <c>ColorFrameReference</c>.
        /// </summary>
        public void Update(ColorFrameReference frameReference)
        {
            using (var frame = frameReference.AcquireFrame())
            {
                Update(frame);
            }
        }

        /// <summary>
        /// Update the Bitmap from the supplied <c>ColorFrame</c>.
        /// </summary>
        public void Update(ColorFrame frame)
        {
            if (frame != null)
            {
#if NETFX_CORE
                frame.CopyConvertedFrameDataToArray(_bytes, ColorImageFormat.Bgra);
#else
                _bitmap.Lock();
                frame.CopyConvertedFrameDataToIntPtr(_bitmap.BackBuffer, (uint)_bytes.Length, ColorImageFormat.Bgra);
                _bitmap.AddDirtyRect(_dirtyRect);
                _bitmap.Unlock();
#endif
            }
        }

        /// <summary>
        /// Update the Bitmap from the supplied color frame data values.
        /// </summary>
        public void Update(byte[] bytes)
        {
#if NETFX_CORE
            _stream.Seek(0, SeekOrigin.Begin);
            _stream.Write(bytes, 0, bytes.Length);
            _bitmap.Invalidate();
#else
            _bitmap.FromByteArray(bytes);
#endif
        }
    }
}
