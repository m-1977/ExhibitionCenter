using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ExhibitionCenter.Dialogs;
using ExhibitionCenter.Models;
using ExhibitionCenter.Services;
using Microsoft.Win32;

namespace ExhibitionCenter.Admin
{
    public partial class TextSettingsWindow : Window
    {
        private readonly DisplaySettingsService _service =
            new();


        private readonly List<CategoryItem>
            _sourceCategories;


        private readonly IDictionary<string, List<ExhibitItem>>
            _sourceCatalog;


        private Point _categoryDragStart;

        private double _categoryStartOffset;

        private bool _categoryDragging;

        private ScrollViewer? _categoryScrollViewer;


        public ObservableCollection<CategoryEditorItem>
            Categories { get; }


        public TextSettingsWindow(
            string currentTitle,
            List<CategoryItem> categories,
            IDictionary<string, List<ExhibitItem>> catalog)
        {
            InitializeComponent();


            _sourceCategories =
                categories;


            _sourceCatalog =
                catalog;


            MainTitleTextBox.Text =
                currentTitle;


            Categories =
                new ObservableCollection<CategoryEditorItem>(
                    categories.Select(
                        c => new CategoryEditorItem
                        {
                            Key =
                                c.Key,

                            Icon =
                                c.Icon,

                            Name =
                                c.Name,

                            Subtitle =
                                c.Subtitle,

                            ImagePath =
                                c.ImagePath
                        }));


            CategoryList.ItemsSource =
                Categories;


            if (Categories.Count > 0)
            {
                CategoryList.SelectedIndex =
                    0;
            }
        }


        // =====================================================
        // ADD CATEGORY
        // =====================================================

        private void AddCategory_Click(
            object sender,
            RoutedEventArgs e)
        {
            var category =
                new CategoryEditorItem
                {
                    Key =
                        "Category_" +
                        Guid.NewGuid()
                            .ToString("N"),

                    Icon =
                        "◈",

                    Name =
                        "قسم جديد",

                    Subtitle =
                        "اكتب وصف القسم",

                    ImagePath =
                        string.Empty
                };


            Categories.Add(
                category);


            CategoryList.SelectedItem =
                category;
        }


        // =====================================================
        // DELETE CATEGORY
        // =====================================================

        private void DeleteCategory_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (CategoryList.SelectedItem
                is not CategoryEditorItem category)
            {
                return;
            }


            int deviceCount =
                _sourceCatalog.TryGetValue(
                    category.Key,
                    out var devices)
                ? devices.Count
                : 0;


            string message =
                deviceCount > 0
                ? $"القسم يحتوي على {deviceCount} جهاز. حذف القسم سيحذف هذه الأجهزة أيضاً."
                : "سيتم حذف القسم من المعرض.";


            var confirm =
                new ConfirmActionWindow(
                    "حذف القسم؟",
                    message)
                {
                    Owner =
                        this
                };


            if (confirm.ShowDialog() != true)
            {
                return;
            }


            int index =
                CategoryList.SelectedIndex;


            Categories.Remove(
                category);


            if (Categories.Count > 0)
            {
                CategoryList.SelectedIndex =
                    Math.Min(
                        index,
                        Categories.Count - 1);
            }
        }


        // =====================================================
        // IMAGE
        // =====================================================

        private void ChooseImage_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (CategoryList.SelectedItem
                is not CategoryEditorItem category)
            {
                return;
            }


            var dialog =
                new OpenFileDialog
                {
                    Title =
                        "اختيار صورة القسم",

                    Filter =
                        "Image Files|*.jpg;*.jpeg;*.png;*.bmp|JPEG|*.jpg;*.jpeg|PNG|*.png|Bitmap|*.bmp",

                    Multiselect =
                        false
                };


            if (dialog.ShowDialog(this) == true)
            {
                category.ImagePath =
                    dialog.FileName;
            }
        }


        private void RemoveImage_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (CategoryList.SelectedItem
                is CategoryEditorItem category)
            {
                category.ImagePath =
                    string.Empty;
            }
        }


        // =====================================================
        // VERTICAL SLIDER
        // =====================================================

        private void CategoryList_MouseDown(
            object sender,
            MouseButtonEventArgs e)
        {
            _categoryScrollViewer ??=
                FindVisualChild<ScrollViewer>(
                    CategoryList);


            if (_categoryScrollViewer is null)
                return;


            _categoryDragStart =
                e.GetPosition(
                    CategoryList);


            _categoryStartOffset =
                _categoryScrollViewer
                    .VerticalOffset;


            _categoryDragging =
                false;
        }


        private void CategoryList_MouseMove(
            object sender,
            MouseEventArgs e)
        {
            if (e.LeftButton !=
                MouseButtonState.Pressed)
            {
                return;
            }


            if (_categoryScrollViewer is null)
                return;


            Point current =
                e.GetPosition(
                    CategoryList);


            double delta =
                current.Y -
                _categoryDragStart.Y;


            if (Math.Abs(delta) > 5)
            {
                _categoryDragging =
                    true;


                Mouse.Capture(
                    CategoryList);


                _categoryScrollViewer
                    .ScrollToVerticalOffset(
                        _categoryStartOffset -
                        delta);


                e.Handled =
                    true;
            }
        }


        private void CategoryList_MouseUp(
            object sender,
            MouseButtonEventArgs e)
        {
            if (_categoryDragging)
            {
                Mouse.Capture(null);

                _categoryDragging =
                    false;

                e.Handled =
                    true;

                return;
            }


            DependencyObject? source =
                e.OriginalSource
                as DependencyObject;


            if (source is not null)
            {
                ListBoxItem? clickedItem =
                    ItemsControl.ContainerFromElement(
                        CategoryList,
                        source)
                    as ListBoxItem;


                if (clickedItem is not null)
                {
                    CategoryList.SelectedItem =
                        clickedItem.DataContext;

                    clickedItem.IsSelected =
                        true;
                }
            }


            _categoryDragging =
                false;
        }


        private void CategoryList_MouseWheel(
            object sender,
            MouseWheelEventArgs e)
        {
            _categoryScrollViewer ??=
                FindVisualChild<ScrollViewer>(
                    CategoryList);


            if (_categoryScrollViewer is null)
                return;


            _categoryScrollViewer
                .ScrollToVerticalOffset(
                    _categoryScrollViewer.VerticalOffset -
                    e.Delta);


            e.Handled =
                true;
        }


        private static T?
            FindVisualChild<T>(
                DependencyObject parent)
            where T : DependencyObject
        {
            for (int i = 0;
                 i < VisualTreeHelper
                     .GetChildrenCount(parent);
                 i++)
            {
                DependencyObject child =
                    VisualTreeHelper
                        .GetChild(parent, i);


                if (child is T typedChild)
                {
                    return typedChild;
                }


                T? nested =
                    FindVisualChild<T>(
                        child);


                if (nested is not null)
                {
                    return nested;
                }
            }


            return null;
        }



        // =====================================================
        // CATEGORY ORDER
        // =====================================================

        private void MoveCategoryUp_Click(
            object sender,
            RoutedEventArgs e)
        {
            MoveSelectedCategory(-1);
        }


        private void MoveCategoryDown_Click(
            object sender,
            RoutedEventArgs e)
        {
            MoveSelectedCategory(1);
        }


        private void MoveSelectedCategory(
            int direction)
        {
            if (CategoryList.SelectedItem
                is not CategoryEditorItem selected)
            {
                return;
            }


            int oldIndex =
                Categories.IndexOf(selected);


            int newIndex =
                oldIndex + direction;


            if (oldIndex < 0 ||
                newIndex < 0 ||
                newIndex >= Categories.Count)
            {
                return;
            }


            Categories.Move(
                oldIndex,
                newIndex);


            CategoryList.SelectedItem =
                selected;


            CategoryList.ScrollIntoView(
                selected);
        }

        // =====================================================
        // SAVE
        // =====================================================

        private void Save_Click(
            object sender,
            RoutedEventArgs e)
        {
            var settings =
                new DisplaySettings
                {
                    MainTitle =
                        string.IsNullOrWhiteSpace(
                            MainTitleTextBox.Text)
                        ? "رحلة عبر تاريخ التقنية"
                        : MainTitleTextBox.Text.Trim()
                };


            var finalCategories =
                new List<CategoryItem>();


            foreach (var category
                     in Categories)
            {
                string imagePath =
                    category.ImagePath;


                if (string.IsNullOrWhiteSpace(
                        imagePath))
                {
                    CategoryImageStorage.Remove(
                        category.Key);


                    imagePath =
                        string.Empty;
                }
                else if (File.Exists(
                             imagePath))
                {
                    imagePath =
                        CategoryImageStorage.Store(
                            category.Key,
                            imagePath);
                }


                settings.Categories[
                    category.Key] =
                    new CategoryTextSetting
                    {
                        Name =
                            category.Name.Trim(),

                        Subtitle =
                            category.Subtitle.Trim(),

                        ImagePath =
                            imagePath
                    };


                finalCategories.Add(
                    new CategoryItem
                    {
                        Key =
                            category.Key,

                        Icon =
                            string.IsNullOrWhiteSpace(
                                category.Icon)
                            ? "◈"
                            : category.Icon,

                        Name =
                            category.Name.Trim(),

                        Subtitle =
                            category.Subtitle.Trim(),

                        ImagePath =
                            imagePath
                    });
            }


            var validKeys =
                finalCategories
                    .Select(x => x.Key)
                    .ToHashSet(
                        StringComparer.OrdinalIgnoreCase);


            var deletedKeys =
                _sourceCategories
                    .Where(
                        x => !validKeys.Contains(
                            x.Key))
                    .Select(
                        x => x.Key)
                    .ToList();


            foreach (string key
                     in deletedKeys)
            {
                CategoryImageStorage.Remove(
                    key);


                if (_sourceCatalog.TryGetValue(
                        key,
                        out var devices))
                {
                    foreach (var device
                             in devices)
                    {
                        if (!string.IsNullOrWhiteSpace(
                                device.Id))
                        {
                            DeviceMediaStorage.RemoveImage(
                                device.Id);

                            DeviceMediaStorage.RemoveVideo(
                                device.Id);
                        }
                    }
                }


                _sourceCatalog.Remove(
                    key);
            }


            _sourceCategories.Clear();


            foreach (var category
                     in finalCategories)
            {
                _sourceCategories.Add(
                    category);


                if (!_sourceCatalog.ContainsKey(
                        category.Key))
                {
                    _sourceCatalog[
                        category.Key] =
                        new List<ExhibitItem>();
                }
            }


            _service.Save(
                settings);


            CatalogService.Save(
                _sourceCategories,
                _sourceCatalog);


            DialogResult =
                true;


            Close();
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


    public class CategoryEditorItem :
        INotifyPropertyChanged
    {
        private string _name =
            string.Empty;

        private string _subtitle =
            string.Empty;

        private string _imagePath =
            string.Empty;


        public string Key
            { get; set; } =
            string.Empty;


        public string Icon
            { get; set; } =
            string.Empty;


        public string Name
        {
            get => _name;

            set
            {
                _name =
                    value ?? string.Empty;

                OnPropertyChanged();
            }
        }


        public string Subtitle
        {
            get => _subtitle;

            set
            {
                _subtitle =
                    value ?? string.Empty;

                OnPropertyChanged();
            }
        }


        public string ImagePath
        {
            get => _imagePath;

            set
            {
                _imagePath =
                    value ?? string.Empty;

                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler?
            PropertyChanged;


        private void OnPropertyChanged(
            [CallerMemberName]
            string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(
                    propertyName));
        }
    }
}

