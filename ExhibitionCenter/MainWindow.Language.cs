using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ExhibitionCenter.Models;
using ExhibitionCenter.Services;

namespace ExhibitionCenter
{
    public partial class MainWindow
    {
        private AppLanguage
            _currentLanguage =
                AppLanguage.Arabic;


        private static readonly
            Dictionary<string, string>
            ArabicToEnglishUi =
                new()
                {
                    ["اختر القسم الذي ترغب في استكشافه"] =
                        "Choose a section to explore",

                    ["الصفحة الرئيسية"] =
                        "Home",

                    ["السنة"] =
                        "Year",

                    ["الشركة"] =
                        "Company",

                    ["النوع"] =
                        "Type",

                    ["المادة"] =
                        "Material",

                    ["الوصف"] =
                        "Description",

                    ["معلومات الجهاز"] =
                        "Device information",

                    ["تشغيل الفيديو"] =
                        "Play video",

                    ["لا يوجد فيديو"] =
                        "No video",

                    ["تعذر تشغيل الفيديو"] =
                        "Unable to play video"
                };


        // =====================================================
        // STARTUP
        // =====================================================

        private void InitializeLanguage()
        {
            _currentLanguage =
                LanguageService
                    .LoadPreference();


            ApplyLanguage(
                false);
        }


        // =====================================================
        // LANGUAGE BUTTON
        // =====================================================

        private void LanguageSwitch_MouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            _currentLanguage =
                _currentLanguage ==
                AppLanguage.Arabic
                    ? AppLanguage.English
                    : AppLanguage.Arabic;


            ApplyLanguage(
                true);


            e.Handled =
                true;
        }


        // =====================================================
        // APPLY
        // =====================================================

        private void ApplyLanguage(
            bool savePreference)
        {
            if (savePreference)
            {
                LanguageService.SetLanguage(
                    _currentLanguage);
            }
            else
            {
                // Keep runtime language synchronized
                // without rewriting preference file.
                LanguageService.SetLanguage(
                    _currentLanguage);
            }


            var displaySettings =
                _displaySettingsService.Load();


            if (_currentLanguage ==
                AppLanguage.Arabic)
            {
                AdminHotspot.Text =
                    string.IsNullOrWhiteSpace(
                        displaySettings.MainTitle)
                    ? "رحلة عبر تاريخ التقنية"
                    : displaySettings.MainTitle;


                HomeLanguageText.Text =
                    "العربية  |  English";


                ItemLanguageText.Text =
                    "🌐   العربية  |  English";


                HomeAboutText.Text =
                    "ⓘ   معلومات عن المعرض";


                HomeTouchHintText.Text =
                    "☝   المس أي بطاقة للاستكشاف";


                BackExhibitsText.Text =
                    "الصفحة الرئيسية";


                DeviceSwipeHintText.Text =
                    "اسحب يميناً أو يساراً لاستعراض الأجهزة";


                AdminLoginTitleText.Text =
                    "دخول الإدارة";


                PinClearButton.Content =
                    "مسح";


                PinCancelButton.Content =
                    "إلغاء";


                PinEnterButton.Content =
                    "دخول";
            }
            else
            {
                AdminHotspot.Text =
                    LanguageService
                        .GetContent()
                        .MainTitleEnglish;


                HomeLanguageText.Text =
                    "English  |  العربية";


                ItemLanguageText.Text =
                    "🌐   English  |  العربية";


                HomeAboutText.Text =
                    "ⓘ   About the Exhibition";


                HomeTouchHintText.Text =
                    "☝   Touch any card to explore";


                BackExhibitsText.Text =
                    "Home";


                DeviceSwipeHintText.Text =
                    "Swipe left or right to browse devices";


                AdminLoginTitleText.Text =
                    "Admin Access";


                PinClearButton.Content =
                    "Clear";


                PinCancelButton.Content =
                    "Cancel";


                PinEnterButton.Content =
                    "Enter";
            }


            TranslateKnownStaticText(
                HomeContent);


            TranslateKnownStaticText(
                CamerasContent);


            RefreshLanguageBindings();


            Dispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                new Action(
                    () =>
                    {
                        ApplyTextDirection(
                            HomeContent);

                        ApplyTextDirection(
                            CamerasContent);
                    }));
        }


        // =====================================================
        // REFRESH DYNAMIC CARDS
        // =====================================================

        private void RefreshLanguageBindings()
        {
            CategoryStrip.ItemsSource =
                null;


            CategoryStrip.ItemsSource =
                _categories;


            if (AdminCategoryList is not null)
            {
                AdminCategoryList.ItemsSource =
                    null;


                AdminCategoryList.ItemsSource =
                    _categories;
            }


            if (_currentItems is not null &&
                _currentItems.Count > 0)
            {
                string? selectedId =
                    _selectedItem?.Id;


                CameraStrip.ItemsSource =
                    null;


                CameraStrip.ItemsSource =
                    _currentItems;


                ExhibitItem? selected =
                    null;


                if (!string.IsNullOrWhiteSpace(
                        selectedId))
                {
                    selected =
                        _currentItems
                            .FirstOrDefault(
                                x => string.Equals(
                                    x.Id,
                                    selectedId,
                                    StringComparison.OrdinalIgnoreCase));
                }


                selected ??=
                    _currentItems
                        .FirstOrDefault();


                if (selected is not null)
                {
                    CameraStrip.SelectedItem =
                        selected;


                    _selectedItem =
                        selected;


                    RefreshSelectedItemLanguage(
                        selected);
                }
            }
        }


        private void RefreshSelectedItemLanguage(
            ExhibitItem item)
        {
            CameraTitle.Text =
                item.DisplayName;


            CameraSubtitle.Text =
                item.DisplaySubtitle;


            CameraDescription.Text =
                item.DisplayDescription;


            CameraYear.Text =
                item.Year;


            CameraCompany.Text =
                item.Company;


            CameraType.Text =
                item.DisplayType;


            CameraMaterial.Text =
                item.DisplayMaterial;


            PosterCameraName.Text =
                item.DisplayName;
        }


        // =====================================================
        // STATIC UI TRANSLATION
        // =====================================================

        private void TranslateKnownStaticText(
            DependencyObject parent)
        {
            int count =
                VisualTreeHelper
                    .GetChildrenCount(
                        parent);


            for (int i = 0;
                 i < count;
                 i++)
            {
                DependencyObject child =
                    VisualTreeHelper
                        .GetChild(
                            parent,
                            i);


                if (child is TextBlock text)
                {
                    text.Text =
                        TranslateUiText(
                            text.Text);
                }


                TranslateKnownStaticText(
                    child);
            }
        }


        private string TranslateUiText(
            string value)
        {
            if (string.IsNullOrWhiteSpace(
                    value))
            {
                return value;
            }


            if (_currentLanguage ==
                AppLanguage.English)
            {
                if (ArabicToEnglishUi.TryGetValue(
                        value,
                        out string? english))
                {
                    return english;
                }


                return value;
            }


            foreach (var pair
                     in ArabicToEnglishUi)
            {
                if (string.Equals(
                        pair.Value,
                        value,
                        StringComparison.Ordinal))
                {
                    return pair.Key;
                }
            }


            return value;
        }


        // =====================================================
        // RTL / LTR TEXT
        // We intentionally do NOT mirror the whole screen.
        // Media stays left and information stays right.
        // =====================================================

        private void ApplyTextDirection(
            DependencyObject parent)
        {
            FlowDirection direction =
                _currentLanguage ==
                AppLanguage.Arabic
                    ? FlowDirection.RightToLeft
                    : FlowDirection.LeftToRight;


            int count =
                VisualTreeHelper
                    .GetChildrenCount(
                        parent);


            for (int i = 0;
                 i < count;
                 i++)
            {
                DependencyObject child =
                    VisualTreeHelper
                        .GetChild(
                            parent,
                            i);


                if (child is TextBlock text)
                {
                    text.FlowDirection =
                        direction;
                }


                ApplyTextDirection(
                    child);
            }
        }
    }
}


