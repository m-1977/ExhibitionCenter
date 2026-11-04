using System;
using System.Linq;
using System.Windows;
using ExhibitionCenter.Admin;
using ExhibitionCenter.Services;

namespace ExhibitionCenter
{
    public partial class MainWindow
    {
        private readonly DeviceSettingsService
            _deviceSettingsService = new();


        private void MainWindow_LoadedAllSettings(
            object sender,
            RoutedEventArgs e)
        {
            bool catalogLoaded =
                CatalogService.LoadInto(
                    _categories,
                    _catalog);


            ApplyDisplayTextSettings();

            ApplyDeviceSettings();


            if (!catalogLoaded)
            {
                CatalogService.Save(
                    _categories,
                    _catalog);
            }


            CategoryStrip.ItemsSource =
                null;


            CategoryStrip.ItemsSource =
                _categories;


            RefreshAdminDashboard();


            InitializeLanguage();
        }


        private void OpenDeviceSettings_Click(
            object sender,
            RoutedEventArgs e)
        {
            var editor =
                new DeviceSettingsWindow(
                    _categories,
                    _catalog)
                {
                    Owner = this
                };


            bool? result =
                editor.ShowDialog();


            if (result == true)
            {
                ApplyDeviceSettings();

                RefreshAdminDashboard();
            }
        }


        private void ApplyDeviceSettings()
        {
            var settings =
                _deviceSettingsService.Load();


            foreach (var category
                     in _catalog)
            {
                foreach (var item
                         in category.Value)
                {
                    if (string.IsNullOrWhiteSpace(
                            item.Id))
                    {
                        item.Id =
                            DeviceSettingsService
                                .MakeStableId(
                                    item.Category,
                                    item.Name);
                    }


                    if (!settings.Devices.TryGetValue(
                            item.Id,
                            out var saved))
                    {
                        continue;
                    }


                    item.Name =
                        saved.Name;

                    item.Subtitle =
                        saved.Subtitle;

                    item.Description =
                        saved.Description;

                    item.Year =
                        saved.Year;

                    item.Company =
                        saved.Company;

                    item.Type =
                        saved.Type;

                    item.Material =
                        saved.Material;

                    item.ImagePath =
                        saved.ImagePath;

                    item.VideoPath =
                        saved.VideoPath;
                }
            }


            if (_currentItems.Count == 0)
            {
                return;
            }


            string? selectedId =
                _selectedItem?.Id;


            CameraStrip.ItemsSource =
                null;


            CameraStrip.ItemsSource =
                _currentItems;


            if (!string.IsNullOrWhiteSpace(
                    selectedId))
            {
                var selected =
                    _currentItems.FirstOrDefault(
                        x => string.Equals(
                            x.Id,
                            selectedId,
                            StringComparison.OrdinalIgnoreCase));


                if (selected is not null)
                {
                    CameraStrip.SelectedItem =
                        selected;
                }
            }
        }
    }
}


