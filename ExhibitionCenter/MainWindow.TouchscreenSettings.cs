using System.Windows;
using System.Windows.Input;
using ExhibitionCenter.Admin;
using ExhibitionCenter.Models;
using ExhibitionCenter.Services;

namespace ExhibitionCenter
{
    public partial class MainWindow
    {
        private readonly TouchscreenSettingsService
            _touchscreenSettingsService =
                new();

        private TouchscreenSettings
            _touchscreenSettings =
                new();


        private void InitializeTouchscreenSettings()
        {
            _touchscreenSettings =
                _touchscreenSettingsService.Load();

            AdminContent.IsVisibleChanged +=
                (_, _) =>
                    ApplyTouchscreenCursor();

            Closed +=
                (_, _) =>
                    Mouse.OverrideCursor = null;

            ApplyTouchscreenCursor();
        }


        private void ApplyTouchscreenCursor()
        {
            bool adminVisible =
                AdminContent.Visibility ==
                Visibility.Visible;

            if (_touchscreenSettings.Enabled &&
                !adminVisible)
            {
                Mouse.OverrideCursor =
                    Cursors.None;

                return;
            }

            Mouse.OverrideCursor = null;
        }


        private void OpenTouchscreenSettings_Click(
            object sender,
            RoutedEventArgs e)
        {
            Mouse.OverrideCursor = null;

            var window =
                new TouchscreenSettingsWindow(
                    _touchscreenSettingsService)
                {
                    Owner = this
                };

            bool? result =
                window.ShowDialog();

            if (result == true)
            {
                _touchscreenSettings =
                    _touchscreenSettingsService.Load();
            }

            ApplyTouchscreenCursor();
        }
    }
}
