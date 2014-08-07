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
    public class ColorFrameBitmap
    {
        private WriteableBitmap _bitmap = null;
        private byte[] _bytes = null;
#if NETFX_CORE
        private Stream _stream = null;
#else
        private Int32Rect _dirtyRect;
#endif

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

        public void Update(ColorFrameReference frameReference)
        {
            using (var frame = frameReference.AcquireFrame())
            {
                Update(frame);
            }
        }

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
