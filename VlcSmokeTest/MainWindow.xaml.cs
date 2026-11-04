using System;
using System.IO;
using System.Windows;
using LibVLCSharp.Shared;

namespace VlcSmokeTest
{
    public partial class MainWindow : Window
    {
        private LibVLC? _libVLC;
        private MediaPlayer? _mediaPlayer;
        private Media? _media;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                Core.Initialize();

                string video =
                    Path.Combine(
                        AppContext.BaseDirectory,
                        "TestVideo.mp4");

                if (!File.Exists(video))
                {
                    StatusText.Text =
                        "TestVideo.mp4 NOT FOUND";

                    return;
                }

                _libVLC =
                    new LibVLC(
                        "--no-video-title-show");

                _mediaPlayer =
                    new MediaPlayer(_libVLC);

                VideoView.MediaPlayer =
                    _mediaPlayer;

                _media =
                    new Media(
                        _libVLC,
                        video,
                        FromType.FromPath);

                _mediaPlayer.Playing +=
                    (_, __) =>
                    {
                        Dispatcher.Invoke(
                            () =>
                            {
                                StatusText.Text =
                                    "LIBVLC PLAYING";
                            });
                    };

                _mediaPlayer.EncounteredError +=
                    (_, __) =>
                    {
                        Dispatcher.Invoke(
                            () =>
                            {
                                StatusText.Text =
                                    "LIBVLC PLAYBACK ERROR";
                            });
                    };

                bool started =
                    _mediaPlayer.Play(_media);

                StatusText.Text =
                    started
                        ? "Opening video..."
                        : "Could not start LibVLC";
            }
            catch (Exception ex)
            {
                StatusText.Text =
                    "ERROR: " + ex.Message;
            }
        }

        private void MainWindow_Closed(
            object? sender,
            EventArgs e)
        {
            try
            {
                _mediaPlayer?.Stop();
            }
            catch
            {
            }

            _media?.Dispose();
            _mediaPlayer?.Dispose();
            _libVLC?.Dispose();
        }
    }
}
