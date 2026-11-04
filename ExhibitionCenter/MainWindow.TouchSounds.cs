using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ExhibitionCenter
{
    public partial class MainWindow
    {
        private readonly MediaPlayer
            _touchClickMediaPlayer =
                new();

        private DateTime
            _lastTouchClickSound =
                DateTime.MinValue;


        private void InitializeTouchClickSound()
        {
            try
            {
                string path =
                    Path.Combine(
                        AppContext.BaseDirectory,
                        "Assets",
                        "Sounds",
                        "TouchClick.wav");

                if (!File.Exists(path))
                    return;

                _touchClickMediaPlayer.Volume =
                    0.70;

                _touchClickMediaPlayer.Open(
                    new Uri(
                        path,
                        UriKind.Absolute));

                Closed += (_, _) =>
                {
                    try
                    {
                        _touchClickMediaPlayer.Close();
                    }
                    catch
                    {
                    }
                };
            }
            catch
            {
                // Sound must never interrupt the museum UI.
            }
        }


        private void PlayTouchClick()
        {
            try
            {
                // Prevent duplicate touch + promoted mouse sound.
                DateTime now =
                    DateTime.UtcNow;

                if ((now - _lastTouchClickSound)
                    .TotalMilliseconds < 120)
                {
                    return;
                }

                _lastTouchClickSound =
                    now;

                _touchClickMediaPlayer.Position =
                    TimeSpan.Zero;

                _touchClickMediaPlayer.Play();
            }
            catch
            {
                // Ignore sound errors.
            }
        }


        private bool IsDeviceCardHit(
            DependencyObject? source)
        {
            if (source is null)
                return false;

            return
                ItemsControl.ContainerFromElement(
                    CameraStrip,
                    source)
                is ListBoxItem;
        }
    }
}
