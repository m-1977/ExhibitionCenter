using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace ExhibitionCenter.Services
{
    public sealed class WindowsStartupService
    {
        private const string RunKeyPath =
            @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "Museum";
        private const string ExecutableName = "ExhibitionCenter.exe";

        public bool IsEnabled()
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
            return key?.GetValue(ValueName) is string value &&
                   !string.IsNullOrWhiteSpace(value);
        }

        public void Enable()
        {
            string executablePath = GetExecutablePath();
            string command = $"\"{executablePath}\"";
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKeyPath)
                ?? throw new InvalidOperationException(
                    "تعذر فتح إعدادات بدء التشغيل في Windows."))
            {
                key.SetValue(ValueName, command, RegistryValueKind.String);
            }

            using RegistryKey? verificationKey =
                Registry.CurrentUser.OpenSubKey(RunKeyPath);
            if (verificationKey?.GetValueNames().Contains(ValueName,
                    StringComparer.OrdinalIgnoreCase) != true ||
                verificationKey.GetValueKind(ValueName) != RegistryValueKind.String ||
                !string.Equals(verificationKey.GetValue(ValueName) as string,
                    command, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "لم يتم تأكيد تسجيل Museum في بدء تشغيل Windows.");
            }
        }

        public void Disable()
        {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(
                RunKeyPath, writable: true))
            {
                key?.DeleteValue(ValueName, throwOnMissingValue: false);
            }

            using RegistryKey? verificationKey =
                Registry.CurrentUser.OpenSubKey(RunKeyPath);
            if (verificationKey?.GetValueNames().Contains(ValueName,
                    StringComparer.OrdinalIgnoreCase) == true)
            {
                throw new InvalidOperationException(
                    "لم يتم تأكيد إزالة Museum من بدء تشغيل Windows.");
            }
        }

        public string GetExecutablePath()
        {
            string? processPath = Environment.ProcessPath;
            if (!string.IsNullOrWhiteSpace(processPath) &&
                string.Equals(Path.GetFileName(processPath), ExecutableName,
                    StringComparison.OrdinalIgnoreCase) &&
                File.Exists(processPath))
            {
                return Path.GetFullPath(processPath);
            }

            string applicationExecutable = Path.Combine(
                AppContext.BaseDirectory, ExecutableName);
            if (File.Exists(applicationExecutable))
                return Path.GetFullPath(applicationExecutable);

            throw new FileNotFoundException(
                "لم يتم العثور على ExhibitionCenter.exe بجانب التطبيق. " +
                "شغّل نسخة تحتوي على ملف EXE لتسجيل التشغيل مع Windows.",
                applicationExecutable);
        }
    }
}
