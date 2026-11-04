using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ExhibitionCenter.Models;
using ExhibitionCenter.Services;

namespace ExhibitionCenter
{
    public partial class MainWindow
    {
        private readonly ClockScreenSettingsService
            _clockRuntimeSettingsService =
                new();

        private ClockScreenSettings
            _clockRuntimeSettings =
                new();

        private readonly DispatcherTimer
            _clockRuntimeTimer =
                new();

        private long _clockRuntimeBaseline =
            Environment.TickCount64;

        [StructLayout(LayoutKind.Sequential)]
        private struct LastInputInfo
        {
            public uint Size;
            public uint Time;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetLastInputInfo(ref LastInputInfo info);

        [DllImport("kernel32.dll")]
        private static extern uint GetTickCount();

        private ClockScreenWindow? _clockScreenWindow;


        private void InitializeClockScreenRuntime()
        {
            ReloadClockScreenSettings();

            _clockRuntimeTimer.Interval =
                TimeSpan.FromSeconds(1);

            _clockRuntimeTimer.Tick +=
                ClockRuntimeTimer_Tick;

            _clockRuntimeTimer.Start();

        }


        private void ReloadClockScreenSettings()
        {
            _clockRuntimeSettings =
                _clockRuntimeSettingsService.Load();

            ResetClockIdleCountdown();

            if (!_clockRuntimeSettings.Enabled)
            {
                HideClockScreen();
            }
        }


        private void ClockRuntimeTimer_Tick(
            object? sender,
            EventArgs e)
        {
            if (_clockScreenWindow is not null)
                return;

            if (!_clockRuntimeSettings.Enabled)
                return;

            if (HasVisibleOwnedWindow())
            {
                ResetClockIdleCountdown();
                return;
            }

            if (!TryGetClockIdleTime(out TimeSpan idleTime))
                return;

            if (idleTime < TimeSpan.FromMinutes(
                    _clockRuntimeSettings.IdleMinutes))
            {
                return;
            }

            ShowClockScreen();
        }


        private bool TryGetClockIdleTime(out TimeSpan idleTime)
        {
            var info = new LastInputInfo
            {
                Size = (uint)Marshal.SizeOf<LastInputInfo>()
            };

            if (!GetLastInputInfo(ref info))
            {
                idleTime = TimeSpan.Zero;
                return false;
            }

            // Both Win32 values use the same 32-bit tick clock. Subtraction
            // also handles its wraparound after approximately 49 days.
            uint windowsIdleMilliseconds =
                unchecked(GetTickCount() - info.Time);
            long countdownMilliseconds =
                Math.Max(0, Environment.TickCount64 - _clockRuntimeBaseline);

            idleTime = TimeSpan.FromMilliseconds(
                Math.Min(windowsIdleMilliseconds, countdownMilliseconds));
            return true;
        }


        private void ResetClockIdleCountdown()
        {
            _clockRuntimeBaseline = Environment.TickCount64;
        }


        private bool HasVisibleOwnedWindow()
        {
            foreach (Window window
                     in OwnedWindows)
            {
                if (window.IsVisible)
                    return true;
            }

            return false;
        }


        private void ShowClockScreen()
        {
            if (_clockScreenWindow is not null || HasVisibleOwnedWindow())
                return;

            var clockWindow = new ClockScreenWindow(_clockRuntimeSettings)
            {
                Owner = this
            };
            clockWindow.Closed += ClockScreenWindow_Closed;
            _clockScreenWindow = clockWindow;
            clockWindow.Show();
            clockWindow.BringToForeground();
        }


        private void HideClockScreen()
        {
            if (_clockScreenWindow is null)
            {
                ResetClockIdleCountdown();
                return;
            }

            _clockScreenWindow.Close();
        }


        private void ClockScreenWindow_Closed(object? sender, EventArgs e)
        {
            _clockScreenWindow = null;
            ResetClockIdleCountdown();
            if (IsVisible)
                Activate();
        }


        private void RegisterClockInteraction()
        {
            if (_clockScreenWindow is not null)
            {
                HideClockScreen();

                return;
            }

            ResetClockIdleCountdown();
        }


        protected override void OnPreviewMouseMove(
            MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            // MouseMove can fire repeatedly inside WPF.
            // Do not reset the idle timer from movement while
            // the museum interface is active.
            //
            // When the clock is visible, however, any real
            // mouse movement wakes the interface immediately.
            if (_clockScreenWindow is not null)
            {
                HideClockScreen();
            }
        }


        protected override void OnPreviewMouseDown(
            MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            RegisterClockInteraction();
        }


        protected override void OnPreviewTouchDown(
            TouchEventArgs e)
        {
            base.OnPreviewTouchDown(e);

            RegisterClockInteraction();
        }


        protected override void OnPreviewKeyDown(
            KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            RegisterClockInteraction();
        }
    }
}


