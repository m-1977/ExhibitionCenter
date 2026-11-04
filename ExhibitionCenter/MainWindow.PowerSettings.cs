using System;
using System.Windows;
using ExhibitionCenter.Admin;
using ExhibitionCenter.Models;
using ExhibitionCenter.Services;

namespace ExhibitionCenter
{
    public partial class MainWindow
    {
        private readonly PowerScreenSettingsService _powerSettingsService = new();
        private readonly WindowsPowerService _windowsPowerService = new();

        private void InitializePowerSettings()
        {
            PowerScreenSettings settings = _powerSettingsService.Load();
            ApplyPowerSettings(settings.KeepAwake);
        }

        private bool ApplyPowerSettings(bool keepAwake)
        {
            if (_windowsPowerService.Apply(keepAwake))
                return true;

            MessageBox.Show(this,
                "تعذر تطبيق إعداد الطاقة والشاشة في Windows. قد يدخل الجهاز في وضع السكون أو تنطفئ الشاشة.",
                "خطأ في الطاقة والشاشة",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        private void OpenPowerSettings_Click(object sender, RoutedEventArgs e)
        {
            var window = new PowerSettingsWindow(_powerSettingsService, ApplyPowerSettings)
            {
                Owner = this
            };
            window.ShowDialog();
        }
    }
}
