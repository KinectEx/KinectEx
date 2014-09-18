using KinectEx.DVR;
using KinectEx.Smoothing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WindowsPreview.Kinect;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace KinectEx.Demo.Recorder.Store
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        KinectSensor _sensor = null;
        BodyFrameReader _bodyReader = null;
        ColorFrameReader _colorReader = null;
        DepthFrameReader _depthReader = null;

        byte[] _colorData;
        ushort[] _depthData;

        ColorFrameBitmap _colorBitmap = new ColorFrameBitmap();
        DepthFrameBitmap _depthBitmap = new DepthFrameBitmap();

        KinectRecorder _recorder = null;

        List<CustomBody> _bodies = null;
        SmoothedBodyList<KalmanSmoother> _kalmanBodies = null;
        SmoothedBodyList<ExponentialSmoother> _exponentialBodies = null;

        public MainPage()
        {
            this.InitializeComponent();

            RecordButton.Click += RecordButton_Click;

            CompressionCombo.Items.Add("None (1920x1080)");
            CompressionCombo.Items.Add("None (1280x720)");
            CompressionCombo.Items.Add("None (640x360)");
            CompressionCombo.Items.Add("Jpeg (1920x1080)");
            CompressionCombo.Items.Add("Jpeg (1280x720)");
            CompressionCombo.Items.Add("Jpeg (640x360)");
            CompressionCombo.SelectionChanged += CompressionCombo_SelectionChanged;
            CompressionCombo.SelectedIndex = 0;

            SmoothingCombo.Items.Add("None");
            SmoothingCombo.Items.Add("Kalman Filter");
            SmoothingCombo.Items.Add("Double Exponential");
            SmoothingCombo.SelectionChanged += SmoothingCombo_SelectionChanged;
            SmoothingCombo.SelectedIndex = 0;

            DisplayCombo.Items.Add("Body");
            DisplayCombo.Items.Add("Color");
            DisplayCombo.Items.Add("Depth");
            DisplayCombo.SelectionChanged += DisplayCombo_SelectionChanged;
            DisplayCombo.SelectedIndex = 0;

            _sensor = KinectSensor.GetDefault();

            _bodyReader = _sensor.BodyFrameSource.OpenReader();
            _bodyReader.FrameArrived += _bodyReader_FrameArrived;

            _colorReader = _sensor.ColorFrameSource.OpenReader();
            _colorReader.FrameArrived += _colorReader_FrameArrived;
            var colorFrameDesc = _sensor.ColorFrameSource.FrameDescription;
            _colorData = new byte[colorFrameDesc.LengthInPixels * 4];

            _depthReader = _sensor.DepthFrameSource.OpenReader();
            _depthReader.FrameArrived += _depthReader_FrameArrived;
            var depthFrameDesc = _sensor.DepthFrameSource.FrameDescription;
            _depthData = new ushort[depthFrameDesc.LengthInPixels];

            _sensor.Open();
        }

        void _bodyReader_FrameArrived(BodyFrameReader sender, BodyFrameArrivedEventArgs e)
        {
            bool shouldRecord = _recorder != null && _recorder.IsStarted && BodyCheckBox.IsChecked.GetValueOrDefault();
            bool shouldDisplay = DisplayCombo.SelectedIndex == 0;

            if (shouldRecord || shouldDisplay)
            {
                IEnumerable<IBody> bodies = null; // to make the RecordFrame & GetBitmap calls a little cleaner
                using (var frame = e.FrameReference.AcquireFrame())
                {
                    if (frame != null)
                    {
                        if (SmoothingCombo.SelectedIndex == 0)
                        {
                            frame.GetAndRefreshBodyData(_bodies);
                            bodies = _bodies;
                        }
                        else if (SmoothingCombo.SelectedIndex == 1)
                        {
                            frame.GetAndRefreshBodyData(_kalmanBodies);
                            bodies = _kalmanBodies;
                        }
                        else
                        {
                            frame.GetAndRefreshBodyData(_exponentialBodies);
                            bodies = _exponentialBodies;
                        }

                        if (shouldRecord)
                        {
                            _recorder.RecordFrame(frame, bodies.Cast<CustomBody>().ToList());
                        }
                    }
                }

                if (shouldDisplay)
                {
                    if (bodies != null)
                    {
                        OutputImage.Source = bodies.GetBitmap(Colors.LightGreen, Colors.Yellow);
                    }
                    else
                    {
                        OutputImage.Source = null;
                    }
                }
            }
        }

        void _colorReader_FrameArrived(ColorFrameReader sender, ColorFrameArrivedEventArgs e)
        {
            bool shouldRecord = _recorder != null && _recorder.IsStarted && ColorCheckBox.IsChecked.GetValueOrDefault();
            bool shouldDisplay = DisplayCombo.SelectedIndex == 1;
            if (shouldRecord || shouldDisplay)
            {
                using (var frame = e.FrameReference.AcquireFrame())
                {
                    if (frame != null)
                    {
                        frame.CopyConvertedFrameDataToArray(_colorData, ColorImageFormat.Bgra);
                        if (shouldRecord)
                        {
                            _recorder.RecordFrame(frame, _colorData);
                        }
                    }
                    else
                    {
                        shouldDisplay = false;
                    }
                }
                if (shouldDisplay)
                {
                    _colorBitmap.Update(_colorData);
                }
            }
        }

        private void _depthReader_FrameArrived(DepthFrameReader sender, DepthFrameArrivedEventArgs e)
        {
            bool shouldRecord = _recorder != null && _recorder.IsStarted && DepthCheckBox.IsChecked.GetValueOrDefault();
            bool shouldDisplay = DisplayCombo.SelectedIndex == 2;
            ushort minDepth = 0, maxDepth = 0;
            if (shouldRecord || shouldDisplay)
            {
                using (var frame = e.FrameReference.AcquireFrame())
                {
                    if (frame != null)
                    {
                        frame.CopyFrameDataToArray(_depthData);
                        minDepth = frame.DepthMinReliableDistance;
                        maxDepth = frame.DepthMaxReliableDistance;
                        if (shouldRecord)
                        {
                            _recorder.RecordFrame(frame, _depthData);
                        }
                    }
                    else
                    {
                        shouldDisplay = false;
                    }
                }
                if (shouldDisplay)
                {
                    _depthBitmap.Update(_depthData, minDepth, maxDepth);
                }
            }
        }

        async void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (_recorder == null)
            {
                var picker = new FileSavePicker();
                picker.FileTypeChoices.Add("KinectEx.DVR Files", new List<string>() {".kdvr"});
                picker.DefaultFileExtension = ".kdvr";
                picker.SuggestedFileName = DateTime.Now.ToString("MM-dd-yyyy-hh-mm-ss");

                var file = await picker.PickSaveFileAsync();

                if (file != null)
                {
                    // NOTE : Unlike the WPF sample which uses the "Automatic" mode, this example
                    //        shows the use of the manual recording mode and does some work to
                    //        optimize the recording process. The result is a better experience
                    //        for this application and a better recording because fewer frames
                    //        end up getting lost.
                    _recorder = new KinectRecorder(await file.OpenStreamForWriteAsync());

                    // NOTE : Default ColorRecorderCodec is Raw @ 1920 x 1080. Only need to change the
                    //        bits that differ from the default.

                    if (CompressionCombo.SelectedIndex > 2)
                    {
                        _recorder.ColorRecorderCodec = new JpegColorCodec();
                    }
                    if (CompressionCombo.SelectedIndex % 3 == 1) // 1280 x 720
                    {
                        _recorder.ColorRecorderCodec.OutputWidth = 1280;
                        _recorder.ColorRecorderCodec.OutputHeight = 720;
                    }
                    else if (CompressionCombo.SelectedIndex % 3 == 2) // 640 x 360
                    {
                        _recorder.ColorRecorderCodec.OutputWidth = 640;
                        _recorder.ColorRecorderCodec.OutputHeight = 360;
                    }

                    _recorder.Start();

                    RecordButton.Content = "Stop Recording";
                    BodyCheckBox.IsEnabled = false;
                    ColorCheckBox.IsEnabled = false;
                    DepthCheckBox.IsEnabled = false;
                    CompressionCombo.IsEnabled = false;
                }
            }
            else
            {
                RecordButton.IsEnabled = false;

                await _recorder.StopAsync();
                _recorder = null;

                RecordButton.Content = "Record";
                RecordButton.IsEnabled = true;
                BodyCheckBox.IsEnabled = true;
                ColorCheckBox.IsEnabled = true;
                DepthCheckBox.IsEnabled = true;
                CompressionCombo.IsEnabled = true;
            }
        }

        void CompressionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        void SmoothingCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _bodies = new List<CustomBody>();
            _kalmanBodies = new SmoothedBodyList<KalmanSmoother>();
            _exponentialBodies = new SmoothedBodyList<ExponentialSmoother>();
        }

        void DisplayCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DisplayCombo.SelectedIndex == 0)
            {
                OutputImage.Source = null;
            }
            else if (DisplayCombo.SelectedIndex == 1)
            {
                OutputImage.Source = _colorBitmap.Bitmap;
            }
            else
            {
                OutputImage.Source = _depthBitmap.Bitmap;
            }
        }
    }
}
