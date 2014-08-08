using KinectEx.DVR;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace KinectEx.Demo.Playback.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectReplay _replay;
        bool _locationSetByHand = false;

        public MainWindow()
        {
            InitializeComponent();

            OpenButton.Click += OpenButton_Click;
            PlayButton.Click += PlayButton_Click;
            LocationSlider.ValueChanged += LocationSlider_ValueChanged;
        }

        void OpenButton_Click(object sender, RoutedEventArgs e)
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
                _replay = null;
            }

            var dlg = new OpenFileDialog()
            {
                DefaultExt = ".kdvr",
                Filter = "KinectEx.DVR Files (*.kdvr)|*.kdvr"
            };

            if (dlg.ShowDialog().GetValueOrDefault())
            {
                OutputCombo.Items.Clear();
                _replay = new KinectReplay(File.Open(dlg.FileName, FileMode.Open, FileAccess.Read));
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

                if (OutputCombo.Items.Count > 0)
                {
                    OutputCombo.SelectedIndex = 0;
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

        void LocationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_locationSetByHand)
            {
                if (_replay != null)
                    _replay.ScrubTo(TimeSpan.FromMilliseconds((LocationSlider.Value / 100.0) * _replay.Duration.TotalMilliseconds));
            }
            else
            {
                _locationSetByHand = true;
            }
        }

        void _replay_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == KinectReplay.IsFinishedPropertyName)
            {
                PlayButton.Content = "Play";
            }
            else if (e.PropertyName == KinectReplay.LocationPropertyName)
            {
                _locationSetByHand = false;
                LocationSlider.Value = 100 - (100 * ((_replay.Duration.TotalMilliseconds - _replay.Location.TotalMilliseconds) / _replay.Duration.TotalMilliseconds));
            }
        }

        void _replay_BodyFrameArrived(object sender, ReplayFrameArrivedEventArgs<ReplayBodyFrame> e)
        {
            if (OutputCombo.SelectedValue.ToString() == "Body")
            {
                OutputImage.Source = e.Frame.Bodies.GetBitmap(Colors.LightGreen, Colors.Yellow);
            }
        }

        async void _replay_ColorFrameArrived(object sender, ReplayFrameArrivedEventArgs<ReplayColorFrame> e)
        {
            if (OutputCombo.SelectedValue.ToString() == "Color")
            {
                OutputImage.Source = await e.Frame.GetBitmapAsync();
            }
        }

        void _replay_DepthFrameArrived(object sender, ReplayFrameArrivedEventArgs<ReplayDepthFrame> e)
        {
            if (OutputCombo.SelectedValue.ToString() == "Depth")
            {
                OutputImage.Source = e.Frame.GetBitmap();
            }
        }
    }
}
