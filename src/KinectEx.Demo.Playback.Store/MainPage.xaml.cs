using KinectEx.DVR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace KinectEx.Demo.Playback.Store
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        KinectReplay _replay;
        bool _locationSetByHand = false;

        FrameTypes _displayType = FrameTypes.Body;

        ColorFrameBitmap _colorBitmap = null;
        DepthFrameBitmap _depthBitmap = null;
        InfraredFrameBitmap _infraredBitmap = null;

        public MainPage()
        {
            this.InitializeComponent();

            OpenButton.Click += OpenButton_Click;
            PlayButton.Click += PlayButton_Click;
            OutputCombo.SelectionChanged += OutputCombo_SelectionChanged;
            LocationSlider.ValueChanged += LocationSlider_ValueChanged;
        }

        async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (_replay != null)
            {
                if (_replay.IsStarted)
                    _replay.Stop();

                _replay.PropertyChanged -= _replay_PropertyChanged;

                if (_replay.HasBodyFrames)
                    _replay.BodyFrameArrived -= _replay_BodyFrameArrived;
                if (_replay.HasColorFrames)
                    _replay.ColorFrameArrived -= _replay_ColorFrameArrived;
                if (_replay.HasDepthFrames)
                    _replay.DepthFrameArrived -= _replay_DepthFrameArrived;
                if (_replay.HasInfraredFrames)
                    _replay.InfraredFrameArrived -= _replay_InfraredFrameArrived;
                _replay = null;
            }

            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".kdvr");

            var file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                _colorBitmap = null; // reset to force recreation for new file

                OutputCombo.Items.Clear();
                _replay = new KinectReplay(await file.OpenStreamForReadAsync());
                _replay.PropertyChanged += _replay_PropertyChanged;
                if (_replay.HasBodyFrames)
                {
                    _replay.BodyFrameArrived += _replay_BodyFrameArrived;
                    OutputCombo.Items.Add("Body");
                }
                if (_replay.HasColorFrames)
                {
                    _replay.ColorFrameArrived += _replay_ColorFrameArrived;
                    OutputCombo.Items.Add("Color");
                }
                if (_replay.HasDepthFrames)
                {
                    _replay.DepthFrameArrived += _replay_DepthFrameArrived;
                    OutputCombo.Items.Add("Depth");
                }
                if (_replay.HasInfraredFrames)
                {
                    _replay.InfraredFrameArrived += _replay_InfraredFrameArrived;
                    OutputCombo.Items.Add("Infrared");
                }

                if (OutputCombo.Items.Count > 0)
                {
                    OutputCombo.SelectedIndex = 0;
                    LocationSlider.Maximum = _replay.Duration.TotalMilliseconds;
                    LocationSlider.TickFrequency = 1000;
                    LocationSlider.SmallChange = 1000;
                }
                else
                {
                    PlayButton.IsEnabled = false;
                }
            }
        }

        void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_replay == null)
                return;

            if (!_replay.IsStarted)
            {
                _replay.Start();
                PlayButton.Content = "Pause";
            }
            else
            {
                _replay.Stop();
                PlayButton.Content = "Play";
            }
        }

        void OutputCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OutputCombo.SelectedValue == null)
                return;

            if (OutputCombo.SelectedValue.ToString() == "Body")
            {
                _displayType = FrameTypes.Body;
                OutputImage.Source = null;
            }
            else if (OutputCombo.SelectedValue.ToString() == "Color")
            {
                _displayType = FrameTypes.Color;
                if (_colorBitmap != null)
                    OutputImage.Source = _colorBitmap.Bitmap;
            }
            else if (OutputCombo.SelectedValue.ToString() == "Depth")
            {
                _displayType = FrameTypes.Depth;
                if (_depthBitmap != null)
                    OutputImage.Source = _depthBitmap.Bitmap;
            }
            else
            {
                _displayType = FrameTypes.Infrared;
                if (_infraredBitmap != null)
                    OutputImage.Source = _infraredBitmap.Bitmap;
            }
        }

        void LocationSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_locationSetByHand)
            {
                if (_replay != null)
                    _replay.ScrubTo(TimeSpan.FromMilliseconds(LocationSlider.Value));
            }
            else
            {
                _locationSetByHand = true;
            }
        }

        void _replay_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == KinectReplay.LocationPropertyName)
            {
                _locationSetByHand = false;
                LocationSlider.Value = _replay.Location.TotalMilliseconds;
            }
            else if (e.PropertyName == KinectReplay.IsFinishedPropertyName)
            {
                PlayButton.Content = "Play";
            }
        }

        void _replay_BodyFrameArrived(object sender, ReplayFrameArrivedEventArgs<ReplayBodyFrame> e)
        {
            if (_displayType == FrameTypes.Body)
            {
                OutputImage.Source = e.Frame.Bodies.GetBitmap(Colors.LightGreen, Colors.Yellow);
            }
        }

        void _replay_ColorFrameArrived(object sender, ReplayFrameArrivedEventArgs<ReplayColorFrame> e)
        {
            if (_displayType == FrameTypes.Color)
            {
                if (_colorBitmap == null)
                {
                    _colorBitmap = new ColorFrameBitmap(e.Frame);
                    OutputImage.Source = _colorBitmap.Bitmap;
                }
                _colorBitmap.Update(e.Frame);
            }
        }

        void _replay_DepthFrameArrived(object sender, ReplayFrameArrivedEventArgs<ReplayDepthFrame> e)
        {
            if (_displayType == FrameTypes.Depth)
            {
                if (_depthBitmap == null)
                {
                    _depthBitmap = new DepthFrameBitmap(e.Frame.Width, e.Frame.Height);
                    OutputImage.Source = _depthBitmap.Bitmap;
                }
                _depthBitmap.Update(e.Frame);
            }
        }

        void _replay_InfraredFrameArrived(object sender, ReplayFrameArrivedEventArgs<ReplayInfraredFrame> e)
        {
            if (_displayType == FrameTypes.Infrared)
            {
                if (_infraredBitmap == null)
                {
                    _infraredBitmap = new InfraredFrameBitmap(e.Frame.Width, e.Frame.Height);
                    OutputImage.Source = _infraredBitmap.Bitmap;
                }
                _infraredBitmap.Update(e.Frame);
            }
        }
    }
}
