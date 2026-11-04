using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ExhibitionCenter.Dialogs;

namespace ExhibitionCenter
{
    public partial class MainWindow
    {
        private bool _exitDialogOpen;

        private DispatcherTimer? _hiddenExitTimer;

        private bool _hiddenExitPressed;

        private bool _hiddenExitTriggered;


        // =====================================================
        // ESC
        // =====================================================

        private void MainWindow_PreviewKeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
                return;


            e.Handled = true;


            ShowExitConfirmation();
        }


        // =====================================================
        // HIDDEN EXIT HOTSPOT
        // Mouse
        // =====================================================

        private void HiddenExitHotspot_MouseDown(
            object sender,
            MouseButtonEventArgs e)
        {
            StartHiddenExitHold();

            e.Handled = true;
        }


        private void HiddenExitHotspot_MouseUp(
            object sender,
            MouseButtonEventArgs e)
        {
            CancelHiddenExitHold();

            e.Handled = true;
        }


        private void HiddenExitHotspot_MouseLeave(
            object sender,
            MouseEventArgs e)
        {
            CancelHiddenExitHold();
        }


        // =====================================================
        // HIDDEN EXIT HOTSPOT
        // Touch
        // =====================================================

        private void HiddenExitHotspot_TouchDown(
            object sender,
            TouchEventArgs e)
        {
            StartHiddenExitHold();

            e.Handled = true;
        }


        private void HiddenExitHotspot_TouchUp(
            object sender,
            TouchEventArgs e)
        {
            CancelHiddenExitHold();

            e.Handled = true;
        }


        // =====================================================
        // HOLD TIMER
        // =====================================================

        private void StartHiddenExitHold()
        {
            if (_hiddenExitPressed ||
                _exitDialogOpen)
            {
                return;
            }


            _hiddenExitPressed = true;

            _hiddenExitTriggered = false;


            EnsureHiddenExitTimer();


            _hiddenExitTimer!.Stop();

            _hiddenExitTimer.Interval =
                TimeSpan.FromSeconds(3);


            _hiddenExitTimer.Start();
        }


        private void CancelHiddenExitHold()
        {
            _hiddenExitPressed = false;


            _hiddenExitTimer?.Stop();
        }


        private void EnsureHiddenExitTimer()
        {
            if (_hiddenExitTimer is not null)
                return;


            _hiddenExitTimer =
                new DispatcherTimer();


            _hiddenExitTimer.Tick +=
                HiddenExitTimer_Tick;
        }


        private void HiddenExitTimer_Tick(
            object? sender,
            EventArgs e)
        {
            _hiddenExitTimer?.Stop();


            if (!_hiddenExitPressed ||
                _hiddenExitTriggered)
            {
                return;
            }


            _hiddenExitTriggered = true;

            _hiddenExitPressed = false;


            ShowExitConfirmation();
        }


        // =====================================================
        // EXIT CONFIRMATION
        // Shared by ESC and hidden hotspot
        // =====================================================

        private void ShowExitConfirmation()
        {
            if (_exitDialogOpen)
                return;


            _exitDialogOpen = true;


            try
            {
                StopVideo();


                var dialog =
                    new ExitConfirmationWindow
                    {
                        Owner = this
                    };


                bool? result =
                    dialog.ShowDialog();


                if (result == true &&
                    dialog.ExitRequested)
                {
                    Application.Current.Shutdown();

                    return;
                }


                if (_selectedItem is not null)
                {
                    LoadPoster(
                        _selectedItem);
                }
            }
            finally
            {
                _exitDialogOpen = false;

                _hiddenExitPressed = false;

                _hiddenExitTriggered = false;

                _hiddenExitTimer?.Stop();
            }
        }
    }
}
