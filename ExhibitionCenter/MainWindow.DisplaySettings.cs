using System.Windows;
using ExhibitionCenter.Admin;
using ExhibitionCenter.Services;

namespace ExhibitionCenter
{
    public partial class MainWindow
    {
        private readonly DisplaySettingsService
            _displaySettingsService = new();


        private void MainWindow_LoadedSettings(
            object sender,
            RoutedEventArgs e)
        {
            ApplyDisplayTextSettings();
        }


        private void OpenTextSettings_Click(
            object sender,
            RoutedEventArgs e)
        {
            var editor =
                new TextSettingsWindow(
                    AdminHotspot.Text,
                    _categories,
                    _catalog)
                {
                    Owner = this
                };


            bool? result =
                editor.ShowDialog();


            if (result == true)
            {
                ApplyDisplayTextSettings();

                ApplyLanguage(false);

                RefreshAdminDashboard();
            }
        }


        private void ApplyDisplayTextSettings()
        {
            var settings =
                _displaySettingsService.Load();


            if (!string.IsNullOrWhiteSpace(
                    settings.MainTitle))
            {
                AdminHotspot.Text =
                    settings.MainTitle;
            }


            foreach (var category in _categories)
            {
                if (!settings.Categories.TryGetValue(
                        category.Key,
                        out var text))
                {
                    continue;
                }


                if (!string.IsNullOrWhiteSpace(
                        text.Name))
                {
                    category.Name =
                        text.Name;
                }


                if (!string.IsNullOrWhiteSpace(
                        text.Subtitle))
                {
                    category.Subtitle =
                        text.Subtitle;
                }


                category.ImagePath =
                    text.ImagePath ?? string.Empty;
            }


            CategoryStrip.ItemsSource = null;
            CategoryStrip.ItemsSource =
                _categories;


            if (AdminCategoryList is not null)
            {
                AdminCategoryList.ItemsSource = null;
                AdminCategoryList.ItemsSource =
                    _categories;
            }
        }
    }
}


