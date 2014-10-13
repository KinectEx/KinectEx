using KinectEx.DVR;
using System;

#if NETFX_CORE
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
#endif

#if NOSDK
#elif NETFX_CORE
using WindowsPreview.Kinect;
#else
using Microsoft.Kinect;
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
	
#if !NOSDK
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorFrameBitmap"/> class.
        /// </summary>
        public ColorFrameBitmap()
        {
            var sensor = KinectSensor.GetDefault();
            int width = sensor.ColorFrameSource.FrameDescription.Width;
            int height = sensor.ColorFrameSource.FrameDescription.Height;
            _bitmap = BitmapFactory.New(width, height);
            _bytes = new byte[width * height * 4];
#if NETFX_CORE
            _stream = _bitmap.PixelBuffer.AsStream();
#else
            _dirtyRect = new Int32Rect(0, 0, width, height);
#endif
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorFrameBitmap" /> class
        /// suitable for displaying the supplied <see cref="ReplayColorFrame" />.
        /// </summary>
        /// <param name="frame">The frame.</param>
        public ColorFrameBitmap(ReplayColorFrame frame)
        {
#if NETFX_CORE
            _bitmap = BitmapFactory.New(frame.Width, frame.Height);
            _bytes = new byte[_bitmap.PixelWidth * _bitmap.PixelHeight * 4];
            _stream = _bitmap.PixelBuffer.AsStream();
#else
            // force population of PixelFormat
            var data = frame.GetFrameDataAsync().Result;
            _bitmap = new WriteableBitmap(frame.Width, frame.Height, 96, 96, frame.Codec.PixelFormat, null);
            _bytes = new byte[_bitmap.PixelWidth * _bitmap.PixelHeight * (_bitmap.Format.BitsPerPixel / 8)];
            _dirtyRect = new Int32Rect(0, 0, frame.Width, frame.Height);
#endif
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ColorFrameBitmap"/> class.
        /// </summary>
        ~ColorFrameBitmap()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
#if NETFX_CORE
            _stream.Dispose();
            _stream = null;
#endif
            _bitmap = null;
        }

#if !NOSDK
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
                _stream.Seek(0, SeekOrigin.Begin);
                _stream.Write(_bytes, 0, _bytes.Length);
                _bitmap.Invalidate();
#else
                _bitmap.Lock();
                frame.CopyConvertedFrameDataToIntPtr(_bitmap.BackBuffer, (uint)_bytes.Length, ColorImageFormat.Bgra);
                _bitmap.AddDirtyRect(_dirtyRect);
                _bitmap.Unlock();
#endif
            }
        }
#endif

        /// <summary>
        /// Update the Bitmap from the supplied <c>ReplayColorFrame</c>.
        /// </summary>
        public void Update(ReplayColorFrame frame)
        {
            if (frame != null)
            {
                frame.GetFrameDataAsync().ContinueWith(async (pixels) =>
                {
#if NETFX_CORE
                    await _bitmap.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                        _stream.Seek(0, SeekOrigin.Begin);
                        _stream.Write(pixels.Result, 0, pixels.Result.Length);
                        _bitmap.Invalidate();
                    });
#else
                    await _bitmap.Dispatcher.InvokeAsync(() => {
                        _bitmap.Lock();
                        _bitmap.WritePixels(_dirtyRect, pixels.Result, frame.Width * (frame.Codec.PixelFormat.BitsPerPixel / 8), 0);
                        _bitmap.AddDirtyRect(_dirtyRect);
                        _bitmap.Unlock();
                    });
#endif
                });
            }
        }

        /// <summary>
        /// Update the Bitmap from the supplied color frame data values.
        /// </summary>
        public async void UpdateAsync(byte[] bytes)
        {
#if NETFX_CORE
            await _bitmap.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                _stream.Seek(0, SeekOrigin.Begin);
                _stream.Write(bytes, 0, bytes.Length);
                _bitmap.Invalidate();
            });
#else
            await _bitmap.Dispatcher.InvokeAsync(() => {
                _bitmap.FromByteArray(bytes);
            });
#endif
        }
    }
}
