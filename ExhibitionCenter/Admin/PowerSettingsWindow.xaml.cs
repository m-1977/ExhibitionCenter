using System;
using System.Windows;
using ExhibitionCenter.Models;
using ExhibitionCenter.Services;

namespace ExhibitionCenter.Admin
{
    public partial class PowerSettingsWindow : Window
    {
        private readonly PowerScreenSettingsService _service;
        private readonly Func<bool, bool> _apply;

        public PowerSettingsWindow(PowerScreenSettingsService service, Func<bool, bool> apply)
        {
            InitializeComponent();
            _service = service;
            _apply = apply;
            KeepAwakeToggle.IsChecked = _service.Load().KeepAwake;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var settings = new PowerScreenSettings { KeepAwake = KeepAwakeToggle.IsChecked == true };
            try
            {
                _service.Save(settings);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "تعذر حفظ إعداد الطاقة والشاشة.\n" + ex.Message,
                    "خطأ في الطاقة والشاشة", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_apply(settings.KeepAwake))
                DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
