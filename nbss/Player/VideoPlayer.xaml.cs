using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace nbss.Player
{
    public class TrackItem
    {
        public string DisplayName { get; set; }
        public MediaTrack Track { get; set; }

        public override string ToString() => DisplayName;
    }

    /// <summary>
    /// Interaction logic for VideoPlayer.xaml
    /// </summary>

    public partial class VideoPlayer : Window
    {
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private Media _media;
        private bool _isSeeking = false;
        private DispatcherTimer _positionTimer;
        private bool _isFullscreen = false;
        private WindowState _prevWindowState;
        private WindowStyle _prevWindowStyle;
        private ResizeMode _prevResizeMode;
        private double _prevLeft;
        private double _prevTop;
        private double _prevWidth;
        private double _prevHeight;
        private bool _isPaused = true;
        private bool _firstPlay = true;
        private DispatcherTimer _mouseIdleTimer;
        private Point _lastMousePosition;
        private bool _controlsVisible = true;

        public VideoPlayer()
        {
            InitializeComponent();
            Core.Initialize();

            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);
            videoView.MediaPlayer = _mediaPlayer;

            loadingText.Visibility = Visibility.Visible;

            _mouseIdleTimer = new DispatcherTimer();
        }

        public async Task SetVideo(string path)
        {
            loadingText.Visibility = Visibility.Visible;

            _media?.Dispose();
            _media = new Media(_libVLC, path, FromType.FromLocation);

            await Task.Run(() => _media.Parse(MediaParseOptions.ParseNetwork));

            Dispatcher.Invoke(() =>
            {
                _mediaPlayer.Playing += PlayHandler;
                // Actually load the video
                if (!_mediaPlayer.Play(_media))
                {
                    loadingText.Text = "Failed to play media.";
                    return;
                }

                _positionTimer = new DispatcherTimer();
                _positionTimer.Interval = TimeSpan.FromMilliseconds(500);
                _positionTimer.Tick += (s, e) =>
                {
                    if (!_isSeeking && _mediaPlayer?.IsPlaying == true)
                    {
                        double durationSec = _mediaPlayer.Length / 1000.0;
                        double currentSec = _mediaPlayer.Time / 1000.0;

                        SeekBar.Maximum = durationSec;
                        SeekBar.Value = currentSec;

                        TimeDisplay.Text = $"{FormatTime(currentSec)} / {FormatTime(durationSec)}";
                    }
                };

                _positionTimer.Start();

                _mouseIdleTimer.Interval = TimeSpan.FromSeconds(0.5);
                _mouseIdleTimer.Tick += CheckMouseIdle;
                _mouseIdleTimer.Start();

                videoView.MouseMove += VideoPlayer_MouseMove;
            });
        }

        private void SeekBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isSeeking && _mediaPlayer != null)
            {
                _mediaPlayer.Time = (long)(SeekBar.Value * 1000); // Convert seconds to ms
            }
        }

        private void SeekBar_PreviewMouseDown(object sender, MouseButtonEventArgs e) => _isSeeking = true;
        private void SeekBar_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isSeeking = false;
            _mediaPlayer.Time = (long)(SeekBar.Value * 1000); // Apply seek
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Pause();
                _isPaused = true;
                playButton.Content = "Play";
            }
            else
            {
                _mediaPlayer.Play();
                _isPaused = false;
                playButton.Content = "Pause";
            }
        }


        private void AudioTracksCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (audioTracksCombo.SelectedItem is TrackItem item)
                _mediaPlayer.SetAudioTrack(item.Track.Id);
        }

        private void SubtitleTracksCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (subtitleTracksCombo.SelectedItem is TrackItem item)
                _mediaPlayer.SetSpu(item.Track.Id);
        }

        private void FullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isFullscreen)
            {
                _prevWindowState = WindowState;
                _prevWindowStyle = WindowStyle;
                _prevResizeMode = ResizeMode;
                Topmost = true;

                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                WindowState = WindowState.Normal;

                var screen = System.Windows.Forms.Screen.PrimaryScreen;

                _prevLeft = Left;
                _prevTop = Top;
                _prevWidth = Width;
                _prevHeight = Height;

                Left = screen.Bounds.Left;
                Top = screen.Bounds.Top;
                Width = screen.Bounds.Width;
                Height = screen.Bounds.Height;

                _isFullscreen = true;
            }
            else
            {
                Topmost = false;
                WindowStyle = _prevWindowStyle;
                ResizeMode = _prevResizeMode;
                WindowState = _prevWindowState;

                Left = _prevLeft;
                Top = _prevTop;
                Width = _prevWidth;
                Height = _prevHeight;

                _isFullscreen = false;
            }
        }

        private string FormatTime(double seconds)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            return ts.Hours > 0 ? ts.ToString(@"h\:mm\:ss") : ts.ToString(@"m\:ss");
        }

        private string TrackDisplay(MediaTrack t)
        {
            var lang = string.IsNullOrWhiteSpace(t.Language) ? "Unknown" : t.Language;
            return $"{lang} (ID: {t.Id})";
        }

        public void PlayHandler<TEventArgs>(object sender, TEventArgs e)
        {
            if (loadingText.Visibility == Visibility.Visible)
            {
                Dispatcher.Invoke(() => { loadingText.Visibility = Visibility.Collapsed; });
            }

            if (_firstPlay)
            {
                Dispatcher.Invoke(() =>
                {
                    var audioTracks = _media.Tracks?.Where(t => t.TrackType == TrackType.Audio).ToList() ?? new List<MediaTrack>();
                    var subtitleTracks = _media.Tracks?.Where(t => t.TrackType == TrackType.Text).ToList() ?? new List<MediaTrack>();

                    audioTracksCombo.ItemsSource = audioTracks
                        .Select(t => new TrackItem { DisplayName = TrackDisplay(t), Track = t })
                        .ToList();

                    audioTracksCombo.SelectedIndex = 0;

                    subtitleTracksCombo.ItemsSource = subtitleTracks
                        .Select(t => new TrackItem { DisplayName = TrackDisplay(t), Track = t })
                        .ToList();

                    subtitleTracksCombo.SelectedIndex = 0;

                    if (audioTracks.Count > 0)
                        _mediaPlayer.SetAudioTrack(audioTracks[0].Id);

                    if (subtitleTracks.Count > 0)
                        _mediaPlayer.SetSpu(subtitleTracks[0].Id);

                    double durationSec = _mediaPlayer.Length / 1000.0;
                    double currentSec = _mediaPlayer.Time / 1000.0;

                    SeekBar.Maximum = durationSec;
                    SeekBar.Value = currentSec;

                    TimeDisplay.Text = $"{FormatTime(currentSec)} / {FormatTime(durationSec)}";

                    _mediaPlayer.Pause();
                    _isPaused = true;
                    _firstPlay = false;
                    loadingText.Text = "Media is ready for playback";
                    loadingText.Visibility = Visibility.Visible;
                });
            }
        }

        private void VideoPlayer_MouseMove(object sender, MouseEventArgs e)
        {
            var currentPosition = e.GetPosition(this);

            if (currentPosition != _lastMousePosition)
            {
                _lastMousePosition = currentPosition;

                if (!_controlsVisible)
                {
                    controlsPanel.Visibility = Visibility.Visible; // your control panel
                    _controlsVisible = true;
                }

                _mouseIdleTimer.Stop();
                _mouseIdleTimer.Start(); // restart idle timer
            }
        }

        private void CheckMouseIdle(object sender, EventArgs e)
        {
            controlsPanel.Visibility = Visibility.Collapsed; // hide controls
            _controlsVisible = false;
        }

        private void SubsCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (subtitleTracksCombo.SelectedItem is TrackItem item)
            {
                _mediaPlayer.SetSpu(item.Track.Id);
            }
        }

        private void SubsCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.SetSpu(-1);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _positionTimer?.Stop();
            _mediaPlayer?.Stop();
            _mediaPlayer?.Dispose();
            _libVLC?.Dispose();
        }

        private void ControlsPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            _mouseIdleTimer.Stop();
        }

        private void ControlsPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            _mouseIdleTimer.Start();
        }
    }
}
