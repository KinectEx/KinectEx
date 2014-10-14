using KinectEx.DVR;
using System;
using System.Linq;
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
    /// from a Kinect SDK <c>InfraredFrame</c>.
    /// </summary>
    public class InfraredFrameBitmap : IDisposable
    {
        private WriteableBitmap _bitmap = null;
        private ushort[] _data = null;
        private byte[] _bytes = null;
        private int _stride = 0;
#if NETFX_CORE
        private static Stream _stream = null;
#else
        private Int32Rect _dirtyRect;
#endif

        // used to (dynamically) adjust the brightness of the IR frame's output
        private float _infraredOutputValueMinimum = 0.01f;
        private float _infraredOutputValueMaximum = 1.0f;
        private int _resetAvgSdCounter = 15;
        private float _avg = 0.08f;
        private float _sd = 3.0f;
        private float _max = (float)ushort.MaxValue;

        /// <summary>
        /// The <c>WriteableBitmap</c> representation of the <c>InfraredFrame</c>
        /// (or frame data) passed in to one of the Update() methods.
        /// </summary>
        public WriteableBitmap Bitmap
	    {
		    get { return _bitmap;}
	    }

#if !NOSDK
        /// <summary>
        /// Initializes a new instance of the <see cref="InfraredFrameBitmap"/> class.
        /// </summary>
        public InfraredFrameBitmap()
        {
            var sensor = KinectSensor.GetDefault();
            Init(sensor.DepthFrameSource.FrameDescription.Width, sensor.DepthFrameSource.FrameDescription.Height);
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="InfraredFrameBitmap"/> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public InfraredFrameBitmap(int width, int height)
        {
            Init(width, height);
        }

        private void Init(int width, int height)
        {
            _bitmap = BitmapFactory.New(width, height);
            _data = new ushort[width * height];
            _bytes = new byte[width * height * 4];
            _stride = width * 4;
#if NETFX_CORE
            _stream = _bitmap.PixelBuffer.AsStream();
#else
            _dirtyRect = new Int32Rect(0, 0, width, height);
#endif
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="InfraredFrameBitmap"/> class.
        /// </summary>
        ~InfraredFrameBitmap()
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
        /// Update the Bitmap from the supplied <c>InfraredFrameReference</c>.
        /// </summary>
        public async void Update(InfraredFrameReference frameReference)
        {
            bool processed = false;
            using (var frame = frameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    frame.CopyFrameDataToArray(_data);
                    processed = true;
                }
            }

            if (processed)
            {
                await UpdateAsync(_data);
            }
        }

        /// <summary>
        /// Update the Bitmap from the supplied <c>InfraredFrame</c>.
        /// </summary>
        public async void Update(InfraredFrame frame)
        {
            if (frame != null)
            {
                frame.CopyFrameDataToArray(_data);
                await UpdateAsync(_data);
            }
        }
#endif

        /// <summary>
        /// Update the Bitmap from the supplied <c>ReplayInfraredFrame</c>.
        /// </summary>
        public async void Update(ReplayInfraredFrame frame)
        {
            if (frame != null)
            {
                await frame.GetFrameDataAsync().ContinueWith(async (frameData) =>
                {
                    await UpdateAsync(frameData.Result);
                });
            }
        }

        /// <summary>
        /// Update the Bitmap from the supplied infrared frame data values.
        /// </summary>
        public async void Update(ushort[] data)
        {
            await UpdateAsync(data);
        }

        /// <summary>
        /// Update the Bitmap from the supplied infrared frame data values.
        /// </summary>
        public async Task UpdateAsync(ushort[] data)
        {
            await Task.Run(async () =>
            {
                int colorPixelIndex = 0;
                int dataLen = data.Length;
                float avgSd = _avg * _sd;

                if (_resetAvgSdCounter++ == 15)
                {
                    _avg = data.Average(d => (d / _max));
                    _resetAvgSdCounter = 0;
                }
                for (int i = 0; i < dataLen; ++i)
                {
                    float intensityRatio = (float)data[i] / _max;
                    intensityRatio /= avgSd;
                    intensityRatio = Math.Min(_infraredOutputValueMaximum, intensityRatio);
                    intensityRatio = Math.Max(_infraredOutputValueMinimum, intensityRatio);
                    byte intensity = (byte)(intensityRatio * 255.0f);
                    _bytes[colorPixelIndex++] = intensity;  // B
                    _bytes[colorPixelIndex++] = intensity;  // G
                    _bytes[colorPixelIndex++] = intensity;  // R
                    _bytes[colorPixelIndex++] = 255;        // A
                }

#if NETFX_CORE
                await _bitmap.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
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
