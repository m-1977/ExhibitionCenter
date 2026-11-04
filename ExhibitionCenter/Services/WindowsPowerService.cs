using System;
using System.Runtime.InteropServices;

namespace ExhibitionCenter.Services
{
    public sealed class WindowsPowerService
    {
        private const uint PowerRequestContextVersion = 0;
        private const uint PowerRequestContextSimpleString = 0x00000001;
        private const int PowerRequestDisplayRequired = 0;
        private const int PowerRequestSystemRequired = 1;

        // Shared because MainWindow applies the setting and App releases it on exit.
        private static readonly object Sync = new();
        private static IntPtr _requestHandle = IntPtr.Zero;
        private static bool _displaySet;
        private static bool _systemSet;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct ReasonContext
        {
            public uint Version;
            public uint Flags;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string SimpleReasonString;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr PowerCreateRequest(ref ReasonContext context);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PowerSetRequest(IntPtr powerRequest, int requestType);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PowerClearRequest(IntPtr powerRequest, int requestType);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        public bool Apply(bool keepAwake)
        {
            lock (Sync)
            {
                try
                {
                    if (!keepAwake)
                        return ReleaseCore();

                    if (_requestHandle != IntPtr.Zero)
                        return _displaySet && _systemSet;

                    var reason = new ReasonContext
                    {
                        Version = PowerRequestContextVersion,
                        Flags = PowerRequestContextSimpleString,
                        SimpleReasonString = "ExhibitionCenter museum kiosk is active"
                    };

                    IntPtr handle = PowerCreateRequest(ref reason);
                    if (handle == IntPtr.Zero || handle == new IntPtr(-1))
                        return false;

                    _requestHandle = handle;
                    _displaySet = PowerSetRequest(handle, PowerRequestDisplayRequired);
                    if (_displaySet)
                        _systemSet = PowerSetRequest(handle, PowerRequestSystemRequired);

                    if (_displaySet && _systemSet)
                        return true;

                    ReleaseCore();
                    return false;
                }
                catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or MarshalDirectiveException)
                {
                    ReleaseCore();
                    return false;
                }
            }
        }

        public bool Release()
        {
            lock (Sync)
            {
                try
                {
                    return ReleaseCore();
                }
                catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
                {
                    return false;
                }
            }
        }

        private static bool ReleaseCore()
        {
            if (_requestHandle == IntPtr.Zero)
                return true;

            bool clearedDisplay = !_displaySet || PowerClearRequest(_requestHandle, PowerRequestDisplayRequired);
            bool clearedSystem = !_systemSet || PowerClearRequest(_requestHandle, PowerRequestSystemRequired);
            bool closed = CloseHandle(_requestHandle);

            _requestHandle = IntPtr.Zero;
            _displaySet = false;
            _systemSet = false;
            return clearedDisplay && clearedSystem && closed;
        }
    }
}
