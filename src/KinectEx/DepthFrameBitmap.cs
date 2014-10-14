using KinectEx.DVR;
using System;
using System.Threading.Tasks;

#if NETFX_CORE
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows;
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
    /// from a Kinect SDK <c>DepthFrame</c>.
    /// </summary>
    public class DepthFrameBitmap : IDisposable
    {
        private WriteableBitmap _bitmap = null;
        private ushort[] _data = null;
        private byte[] _bytes = null;
#if NETFX_CORE
        private static Stream _stream = null;
#else
        private Int32Rect _dirtyRect;
        private int _stride = 0;
#endif

        /// <summary>
        /// The <c>WriteableBitmap</c> representation of the <c>DepthFrame</c>
        /// (or frame data) passed in to one of the Update() methods.
        /// </summary>
        public WriteableBitmap Bitmap
	    {
		    get { return _bitmap;}
	    }

#if !NOSDK
        /// <summary>
        /// Initializes a new instance of the <see cref="DepthFrameBitmap"/> class.
        /// </summary>
        public DepthFrameBitmap()
        {
            var sensor = KinectSensor.GetDefault();
            Init(sensor.DepthFrameSource.FrameDescription.Width, sensor.DepthFrameSource.FrameDescription.Height);
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthFrameBitmap"/> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public DepthFrameBitmap(int width, int height)
        {
            Init(width, height);
        }

        private void Init(int width, int height)
        {
            _bitmap = BitmapFactory.New(width, height);
            _data = new ushort[width * height];
            _bytes = new byte[width * height * 4];
#if NETFX_CORE
            _stream = _bitmap.PixelBuffer.AsStream();
#else
            _dirtyRect = new Int32Rect(0, 0, width, height);
            _stride = width * 4;
#endif
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="DepthFrameBitmap"/> class.
        /// </summary>
        ~DepthFrameBitmap()
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
            _data = null;
            _bytes = null;
            _bitmap = null;
        }

#if !NOSDK
        /// <summary>
        /// Update the Bitmap from the supplied <c>DepthFrameReference</c>.
        /// </summary>
        public async void Update(DepthFrameReference frameReference)
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
                await UpdateAsync(_data, minDepth, maxDepth);
            }
        }

        /// <summary>
        /// Update the Bitmap from the supplied <c>DepthFrame</c>.
        /// </summary>
        public async void Update(DepthFrame frame)
        {
            if (frame != null)
            {
                frame.CopyFrameDataToArray(_data);
                await UpdateAsync(_data, frame.DepthMinReliableDistance, frame.DepthMaxReliableDistance);
            }
        }
#endif

        /// <summary>
        /// Update the Bitmap from the supplied <c>ReplayDepthFrame</c>.
        /// </summary>
        public async void Update(ReplayDepthFrame frame)
        {
            if (frame != null)
            {
                await frame.GetFrameDataAsync().ContinueWith(async (frameData) =>
                {
                    await UpdateAsync(frameData.Result, (ushort)frame.DepthMinReliableDistance, (ushort)frame.DepthMaxReliableDistance);
                });
            }
        }

        /// <summary>
        /// Update the Bitmap from the supplied infrared frame data values.
        /// </summary>
        public async void Update(ushort[] data, ushort minDepth, ushort maxDepth)
        {
            await UpdateAsync(data, minDepth, maxDepth);
        }

        /// <summary>
        /// Update the Bitmap from the supplied depth frame data values.
        /// </summary>
        public async Task UpdateAsync(ushort[] data, ushort minDepth, ushort maxDepth)
        {
            await Task.Run(async () =>
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
            await _bitmap.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                _stream.Seek(0, SeekOrigin.Begin);
                _stream.Write(_bytes, 0, _bytes.Length);
                _bitmap.Invalidate();
            });
#else
                await _bitmap.Dispatcher.InvokeAsync(() =>
                {
                    _bitmap.FromByteArray(_bytes);
                });
#endif
            });
        }
    }
}
