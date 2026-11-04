using System;
using System.Windows;
using ExhibitionCenter.Services;

namespace ExhibitionCenter.Admin
{
    public partial class StartupSettingsWindow : Window
    {
        private readonly WindowsStartupService _startupService = new();

        public StartupSettingsWindow()
        {
            InitializeComponent();
            Loaded += (_, _) => LoadRegistryState();
        }

        private void LoadRegistryState()
        {
            try
            {
                bool enabled = _startupService.IsEnabled();
                StartupToggle.IsChecked = enabled;
                StartupStatusText.Text = enabled ? "مفعّل" : "غير مفعّل";
            }
            catch (Exception ex)
            {
                StartupStatusText.Text = "تعذر قراءة الحالة";
                ShowRegistryError("تعذر قراءة إعداد التشغيل مع Windows.", ex);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool requestedEnabled = StartupToggle.IsChecked == true;
                if (requestedEnabled)
                    _startupService.Enable();
                else
                    _startupService.Disable();

                bool enabled = _startupService.IsEnabled();
                if (enabled != requestedEnabled)
                {
                    throw new InvalidOperationException(
                        "حالة بدء التشغيل في Windows لا تطابق الاختيار المحفوظ.");
                }

                StartupStatusText.Text = enabled ? "مفعّل" : "غير مفعّل";
                DialogResult = true;
            }
            catch (Exception ex)
            {
                ShowRegistryError("تعذر حفظ إعداد التشغيل مع Windows.", ex);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ShowRegistryError(string message, Exception error)
        {
            MessageBox.Show(this,
                message + Environment.NewLine + error.Message,
                "خطأ في التشغيل مع Windows",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
