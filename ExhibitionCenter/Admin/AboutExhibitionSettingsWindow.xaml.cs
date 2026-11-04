using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ExhibitionCenter.Models;
using ExhibitionCenter.Services;
using Microsoft.Win32;

namespace ExhibitionCenter.Admin
{
    public partial class AboutExhibitionSettingsWindow : Window
    {
        private readonly AboutExhibitionSettingsService _service =
            new();

        private AboutExhibitionSettings _settings;

        private string _pendingImagePath =
            string.Empty;

        private bool _removeImage;


        public AboutExhibitionSettingsWindow()
        {
            InitializeComponent();

            _settings =
                _service.Load();

            LoadSettings();
        }


        private void LoadSettings()
        {
            AccentHexTextBox.Text =
                _settings.AccentColor;

            AboutTitleArTextBox.Text =
                _settings.TitleAr;

            AboutTitleEnTextBox.Text =
                _settings.TitleEn;


            Note1TitleArTextBox.Text =
                _settings.Notes[0].TitleAr;

            Note1BodyArTextBox.Text =
                _settings.Notes[0].BodyAr;

            Note1TitleEnTextBox.Text =
                _settings.Notes[0].TitleEn;

            Note1BodyEnTextBox.Text =
                _settings.Notes[0].BodyEn;


            Note2TitleArTextBox.Text =
                _settings.Notes[1].TitleAr;

            Note2BodyArTextBox.Text =
                _settings.Notes[1].BodyAr;

            Note2TitleEnTextBox.Text =
                _settings.Notes[1].TitleEn;

            Note2BodyEnTextBox.Text =
                _settings.Notes[1].BodyEn;


            Note3TitleArTextBox.Text =
                _settings.Notes[2].TitleAr;

            Note3BodyArTextBox.Text =
                _settings.Notes[2].BodyAr;

            Note3TitleEnTextBox.Text =
                _settings.Notes[2].TitleEn;

            Note3BodyEnTextBox.Text =
                _settings.Notes[2].BodyEn;


            Note4TitleArTextBox.Text =
                _settings.Notes[3].TitleAr;

            Note4BodyArTextBox.Text =
                _settings.Notes[3].BodyAr;

            Note4TitleEnTextBox.Text =
                _settings.Notes[3].TitleEn;

            Note4BodyEnTextBox.Text =
                _settings.Notes[3].BodyEn;


            LoadImagePreview(
                _settings.ImagePath);

            UpdateAccentPreview();
        }


        private void ChooseImage_Click(
            object sender,
            RoutedEventArgs e)
        {
            var dialog =
                new OpenFileDialog
                {
                    Title =
                        "اختيار صورة معلومات المعرض",

                    Filter =
                        "Image Files|*.jpg;*.jpeg;*.png;*.bmp",

                    Multiselect =
                        false
                };


            if (dialog.ShowDialog(this) != true)
            {
                return;
            }


            _pendingImagePath =
                dialog.FileName;

            _removeImage =
                false;


            LoadImagePreview(
                _pendingImagePath);
        }


        private void RemoveImage_Click(
            object sender,
            RoutedEventArgs e)
        {
            _pendingImagePath =
                string.Empty;

            _removeImage =
                true;


            ImagePreviewSurface.Fill =
                new SolidColorBrush(
                    Color.FromRgb(
                        24,
                        20,
                        15));


            ImagePreviewPlaceholder.Visibility =
                Visibility.Visible;
        }


        private void AccentHexTextBox_TextChanged(
            object sender,
            System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateAccentPreview();
        }


        private void UpdateAccentPreview()
        {
            if (AccentPreviewBorder is null ||
                AccentHexTextBox is null)
            {
                return;
            }


            AccentPreviewBorder.Background =
                GetBrush(
                    AccentHexTextBox.Text);
        }


        private static Brush GetBrush(
            string value)
        {
            try
            {
                return
                    (Brush)new BrushConverter()
                        .ConvertFromString(
                            value)!;
            }
            catch
            {
                return
                    new SolidColorBrush(
                        Color.FromRgb(
                            226,
                            178,
                            88));
            }
        }


        private static string NormalizeColor(
            string value)
        {
            string result =
                value.Trim();


            if (!result.StartsWith("#"))
            {
                result =
                    "#" + result;
            }


            try
            {
                _ =
                    (Brush)new BrushConverter()
                        .ConvertFromString(
                            result);

                return result;
            }
            catch
            {
                return "#E2B258";
            }
        }


        private void LoadImagePreview(
            string path)
        {
            ImagePreviewSurface.Fill =
                new SolidColorBrush(
                    Color.FromRgb(
                        24,
                        20,
                        15));


            ImagePreviewPlaceholder.Visibility =
                Visibility.Visible;


            if (string.IsNullOrWhiteSpace(path) ||
                !File.Exists(path))
            {
                return;
            }


            try
            {
                var bitmap =
                    new BitmapImage();


                bitmap.BeginInit();

                bitmap.CacheOption =
                    BitmapCacheOption.OnLoad;

                bitmap.UriSource =
                    new Uri(
                        path,
                        UriKind.Absolute);

                bitmap.EndInit();

                bitmap.Freeze();


                ImagePreviewSurface.Fill =
                    new ImageBrush(
                        bitmap)
                    {
                        Stretch =
                            Stretch.UniformToFill
                    };


                ImagePreviewPlaceholder.Visibility =
                    Visibility.Collapsed;
            }
            catch
            {
                ImagePreviewPlaceholder.Visibility =
                    Visibility.Visible;
            }
        }


        private void Save_Click(
            object sender,
            RoutedEventArgs e)
        {
            _settings.AccentColor =
                NormalizeColor(
                    AccentHexTextBox.Text);


            _settings.TitleAr =
                AboutTitleArTextBox.Text.Trim();

            _settings.TitleEn =
                AboutTitleEnTextBox.Text.Trim();


            SetNote(
                _settings.Notes[0],
                Note1TitleArTextBox.Text,
                Note1BodyArTextBox.Text,
                Note1TitleEnTextBox.Text,
                Note1BodyEnTextBox.Text);


            SetNote(
                _settings.Notes[1],
                Note2TitleArTextBox.Text,
                Note2BodyArTextBox.Text,
                Note2TitleEnTextBox.Text,
                Note2BodyEnTextBox.Text);


            SetNote(
                _settings.Notes[2],
                Note3TitleArTextBox.Text,
                Note3BodyArTextBox.Text,
                Note3TitleEnTextBox.Text,
                Note3BodyEnTextBox.Text);


            SetNote(
                _settings.Notes[3],
                Note4TitleArTextBox.Text,
                Note4BodyArTextBox.Text,
                Note4TitleEnTextBox.Text,
                Note4BodyEnTextBox.Text);


            if (_removeImage)
            {
                AboutExhibitionImageStorage.Remove();

                _settings.ImagePath =
                    string.Empty;
            }


            if (!_removeImage &&
                !string.IsNullOrWhiteSpace(
                    _pendingImagePath))
            {
                _settings.ImagePath =
                    AboutExhibitionImageStorage.Store(
                        _pendingImagePath);
            }


            _service.Save(
                _settings);


            DialogResult =
                true;

            Close();
        }


        private static void SetNote(
            AboutExhibitionNote note,
            string titleAr,
            string bodyAr,
            string titleEn,
            string bodyEn)
        {
            note.TitleAr =
                titleAr.Trim();

            note.BodyAr =
                bodyAr.Trim();

            note.TitleEn =
                titleEn.Trim();

            note.BodyEn =
                bodyEn.Trim();
        }


        private void Cancel_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult =
                false;

            Close();
        }
    }
}
