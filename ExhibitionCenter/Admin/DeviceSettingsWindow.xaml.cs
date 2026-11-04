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
using ExhibitionCenter.Models;
using ExhibitionCenter.Dialogs;
using ExhibitionCenter.Services;
using Microsoft.Win32;

namespace ExhibitionCenter.Admin
{
    public partial class DeviceSettingsWindow : Window
    {
        private readonly DeviceSettingsService _service =
            new();


        private readonly ObservableCollection<DeviceEditorItem>
            _allDevices;


        private readonly List<CategoryItem>
            _sourceCategories;


        private readonly IDictionary<string, List<ExhibitItem>>
            _sourceCatalog;


        private readonly HashSet<string>
            _deletedDeviceIds =
                new(StringComparer.OrdinalIgnoreCase);


        private readonly Dictionary<ListBox, Point>
            _dragStarts = new();


        private readonly Dictionary<ListBox, double>
            _dragOffsets = new();


        private readonly Dictionary<ListBox, ScrollViewer>
            _scrollViewers = new();


        private readonly HashSet<ListBox>
            _draggingLists = new();


        public DeviceSettingsWindow(
            IEnumerable<CategoryItem> categories,
            IDictionary<string, List<ExhibitItem>> catalog)
        {
            InitializeComponent();


            _sourceCategories =
                categories.ToList();


            _sourceCatalog =
                catalog;


            CategoryList.ItemsSource =
                _sourceCategories;


            _allDevices =
                new ObservableCollection<DeviceEditorItem>(
                    catalog
                        .SelectMany(x => x.Value)
                        .Select(
                            item =>
                            {
                                string id =
                                    string.IsNullOrWhiteSpace(item.Id)
                                    ? DeviceSettingsService.MakeStableId(
                                        item.Category,
                                        item.Name)
                                    : item.Id;


                                return new DeviceEditorItem
                                {
                                    Id = id,
                                    Category = item.Category,
                                    Icon = item.Icon,
                                    Name = item.Name,
                                    Subtitle = item.Subtitle,
                                    Description = item.Description,
                                    Year = item.Year,
                                    Company = item.Company,
                                    Type = item.Type,
                                    Material = item.Material,
                                    ImagePath = item.ImagePath,
                                    VideoPath = item.VideoPath
                                };
                            }));


            if (CategoryList.Items.Count > 0)
            {
                CategoryList.SelectedIndex = 0;
            }
        }


        private void CategoryList_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (CategoryList.SelectedItem
                is not CategoryItem category)
            {
                DeviceList.ItemsSource = null;

                return;
            }


            DeviceList.ItemsSource =
                _allDevices
                    .Where(
                        x => string.Equals(
                            x.Category,
                            category.Key,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();


            if (DeviceList.Items.Count > 0)
            {
                DeviceList.SelectedIndex = 0;
            }
        }



        // =====================================================
        // DEVICE CRUD
        // =====================================================

        private void AddDevice_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (CategoryList.SelectedItem
                is not CategoryItem category)
            {
                return;
            }


            var item =
                new DeviceEditorItem
                {
                    Id =
                        Guid.NewGuid()
                            .ToString("N"),

                    Category =
                        category.Key,

                    Icon =
                        category.Icon,

                    Name =
                        "جهاز جديد",

                    Subtitle =
                        "اكتب العنوان الفرعي",

                    Description =
                        "اكتب وصف الجهاز",

                    Year =
                        string.Empty,

                    Company =
                        string.Empty,

                    Type =
                        string.Empty,

                    Material =
                        string.Empty,

                    ImagePath =
                        string.Empty,

                    VideoPath =
                        string.Empty
                };


            _allDevices.Add(
                item);


            RefreshDeviceList(
                category.Key,
                item);
        }


        private void DeleteDevice_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (DeviceList.SelectedItem
                is not DeviceEditorItem item)
            {
                return;
            }


            var confirm =
                new ConfirmActionWindow(
                    "حذف الجهاز؟",
                    $"هل تريد حذف «{item.Name}» من المعرض؟")
                {
                    Owner =
                        this
                };


            if (confirm.ShowDialog() != true)
            {
                return;
            }


            string categoryKey =
                item.Category;


            if (!string.IsNullOrWhiteSpace(
                    item.Id))
            {
                _deletedDeviceIds.Add(
                    item.Id);
            }


            _allDevices.Remove(
                item);


            RefreshDeviceList(
                categoryKey,
                null);
        }


        private void RefreshDeviceList(
            string categoryKey,
            DeviceEditorItem? selectItem)
        {
            var items =
                _allDevices
                    .Where(
                        x => string.Equals(
                            x.Category,
                            categoryKey,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();


            DeviceList.ItemsSource =
                null;


            DeviceList.ItemsSource =
                items;


            if (selectItem is not null &&
                items.Contains(selectItem))
            {
                DeviceList.SelectedItem =
                    selectItem;

                return;
            }


            if (items.Count > 0)
            {
                DeviceList.SelectedIndex =
                    0;
            }
        }

        // =====================================================
        // IMAGE
        // =====================================================

        private void ChooseImage_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (DeviceList.SelectedItem
                is not DeviceEditorItem item)
            {
                return;
            }


            var dialog =
                new OpenFileDialog
                {
                    Title = "اختيار صورة الجهاز",

                    Filter =
                        "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.webp|JPEG|*.jpg;*.jpeg|PNG|*.png|Bitmap|*.bmp",

                    Multiselect = false
                };


            if (dialog.ShowDialog(this) == true)
            {
                item.ImagePath =
                    dialog.FileName;
            }
        }


        private void RemoveImage_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (DeviceList.SelectedItem
                is DeviceEditorItem item)
            {
                item.ImagePath =
                    string.Empty;
            }
        }


        // =====================================================
        // VIDEO
        // =====================================================

        private void ChooseVideo_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (DeviceList.SelectedItem
                is not DeviceEditorItem item)
            {
                return;
            }


            var dialog =
                new OpenFileDialog
                {
                    Title = "اختيار فيديو الجهاز",

                    Filter =
                        "Video Files|*.mp4;*.wmv;*.avi;*.m4v|MP4 Video|*.mp4|Windows Media Video|*.wmv|AVI Video|*.avi",

                    Multiselect = false
                };


            if (dialog.ShowDialog(this) == true)
            {
                item.VideoPath =
                    dialog.FileName;
            }
        }


        private void RemoveVideo_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (DeviceList.SelectedItem
                is DeviceEditorItem item)
            {
                item.VideoPath =
                    string.Empty;
            }
        }


        // =====================================================
        // VERTICAL SLIDERS
        // =====================================================

        private void VerticalList_MouseDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (sender is not ListBox list)
            {
                return;
            }


            ScrollViewer? viewer =
                FindVisualChild<ScrollViewer>(
                    list);


            if (viewer is null)
            {
                return;
            }


            _scrollViewers[list] =
                viewer;


            _dragStarts[list] =
                e.GetPosition(list);


            _dragOffsets[list] =
                viewer.VerticalOffset;


            _draggingLists.Remove(list);
        }


        private void VerticalList_MouseMove(
            object sender,
            MouseEventArgs e)
        {
            if (sender is not ListBox list)
            {
                return;
            }


            if (e.LeftButton !=
                MouseButtonState.Pressed)
            {
                return;
            }


            if (!_scrollViewers.TryGetValue(
                    list,
                    out ScrollViewer? viewer))
            {
                return;
            }


            if (!_dragStarts.TryGetValue(
                    list,
                    out Point start))
            {
                return;
            }


            Point current =
                e.GetPosition(list);


            double delta =
                current.Y -
                start.Y;


            if (Math.Abs(delta) <= 5)
            {
                return;
            }


            _draggingLists.Add(list);


            Mouse.Capture(list);


            viewer.ScrollToVerticalOffset(
                _dragOffsets[list] -
                delta);


            e.Handled = true;
        }


        private void VerticalList_MouseUp(
            object sender,
            MouseButtonEventArgs e)
        {
            if (sender is not ListBox list)
            {
                return;
            }


            if (_draggingLists.Contains(list))
            {
                Mouse.Capture(null);

                _draggingLists.Remove(list);

                e.Handled = true;

                return;
            }


            DependencyObject? source =
                e.OriginalSource
                as DependencyObject;


            if (source is not null)
            {
                ListBoxItem? clickedItem =
                    ItemsControl.ContainerFromElement(
                        list,
                        source)
                    as ListBoxItem;


                if (clickedItem is not null)
                {
                    list.SelectedItem =
                        clickedItem.DataContext;

                    clickedItem.IsSelected =
                        true;
                }
            }
        }


        private void VerticalList_MouseWheel(
            object sender,
            MouseWheelEventArgs e)
        {
            if (sender is not ListBox list)
            {
                return;
            }


            ScrollViewer? viewer =
                FindVisualChild<ScrollViewer>(
                    list);


            if (viewer is null)
            {
                return;
            }


            viewer.ScrollToVerticalOffset(
                viewer.VerticalOffset -
                e.Delta);


            e.Handled = true;
        }


        private static T?
            FindVisualChild<T>(
                DependencyObject parent)
            where T : DependencyObject
        {
            for (int i = 0;
                 i < VisualTreeHelper.GetChildrenCount(parent);
                 i++)
            {
                DependencyObject child =
                    VisualTreeHelper.GetChild(
                        parent,
                        i);


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
        // DEVICE ORDER
        // =====================================================

        private void MoveDeviceUp_Click(
            object sender,
            RoutedEventArgs e)
        {
            MoveSelectedDevice(-1);
        }


        private void MoveDeviceDown_Click(
            object sender,
            RoutedEventArgs e)
        {
            MoveSelectedDevice(1);
        }


        private void MoveSelectedDevice(
            int direction)
        {
            if (DeviceList.SelectedItem
                is not DeviceEditorItem selected)
            {
                return;
            }


            var categoryDevices =
                _allDevices
                    .Where(
                        x => string.Equals(
                            x.Category,
                            selected.Category,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();


            int localIndex =
                categoryDevices.IndexOf(
                    selected);


            int targetLocalIndex =
                localIndex + direction;


            if (localIndex < 0 ||
                targetLocalIndex < 0 ||
                targetLocalIndex >= categoryDevices.Count)
            {
                return;
            }


            DeviceEditorItem target =
                categoryDevices[
                    targetLocalIndex];


            int oldGlobalIndex =
                _allDevices.IndexOf(
                    selected);


            int targetGlobalIndex =
                _allDevices.IndexOf(
                    target);


            if (oldGlobalIndex < 0 ||
                targetGlobalIndex < 0)
            {
                return;
            }


            _allDevices.Move(
                oldGlobalIndex,
                targetGlobalIndex);


            RefreshDeviceList(
                selected.Category,
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
                new DeviceSettings();


            foreach (DeviceEditorItem item
                     in _allDevices)
            {
                string imagePath =
                    item.ImagePath;


                if (string.IsNullOrWhiteSpace(
                        imagePath))
                {
                    DeviceMediaStorage.RemoveImage(
                        item.Id);


                    imagePath =
                        string.Empty;
                }
                else if (File.Exists(imagePath))
                {
                    imagePath =
                        DeviceMediaStorage.StoreImage(
                            item.Id,
                            imagePath);
                }


                string videoPath =
                    item.VideoPath;


                if (string.IsNullOrWhiteSpace(
                        videoPath))
                {
                    DeviceMediaStorage.RemoveVideo(
                        item.Id);


                    videoPath =
                        string.Empty;
                }
                else if (File.Exists(videoPath))
                {
                    videoPath =
                        DeviceMediaStorage.StoreVideo(
                            item.Id,
                            videoPath);
                }


                item.ImagePath =
                    imagePath;


                item.VideoPath =
                    videoPath;


                settings.Devices[item.Id] =
                    new DeviceItemSetting
                    {
                        Name =
                            item.Name.Trim(),

                        Subtitle =
                            item.Subtitle.Trim(),

                        Description =
                            item.Description.Trim(),

                        Year =
                            item.Year.Trim(),

                        Company =
                            item.Company.Trim(),

                        Type =
                            item.Type.Trim(),

                        Material =
                            item.Material.Trim(),

                        ImagePath =
                            imagePath,

                        VideoPath =
                            videoPath
                    };
            }


            _service.Save(settings);


            // ---------------------------------------------
            // Permanently remove media for deleted devices
            // ---------------------------------------------

            foreach (string id
                     in _deletedDeviceIds)
            {
                DeviceMediaStorage.RemoveImage(
                    id);

                DeviceMediaStorage.RemoveVideo(
                    id);
            }


            // ---------------------------------------------
            // Rebuild the live catalog
            // ---------------------------------------------

            foreach (var category
                     in _sourceCategories)
            {
                if (!_sourceCatalog.ContainsKey(
                        category.Key))
                {
                    _sourceCatalog[
                        category.Key] =
                        new List<ExhibitItem>();
                }


                _sourceCatalog[
                    category.Key]
                    .Clear();
            }


            foreach (DeviceEditorItem item
                     in _allDevices)
            {
                if (!_sourceCatalog.TryGetValue(
                        item.Category,
                        out var list))
                {
                    continue;
                }


                list.Add(
                    new ExhibitItem
                    {
                        Id =
                            item.Id,

                        Category =
                            item.Category,

                        Icon =
                            item.Icon,

                        Name =
                            item.Name.Trim(),

                        Subtitle =
                            item.Subtitle.Trim(),

                        Description =
                            item.Description.Trim(),

                        Year =
                            item.Year.Trim(),

                        Company =
                            item.Company.Trim(),

                        Type =
                            item.Type.Trim(),

                        Material =
                            item.Material.Trim(),

                        ImagePath =
                            item.ImagePath,

                        VideoPath =
                            item.VideoPath
                    });
            }


            // ---------------------------------------------
            // Save permanent catalog
            // ---------------------------------------------

            CatalogService.Save(
                _sourceCategories,
                _sourceCatalog);


            DialogResult = true;

            Close();
        }


        private void Cancel_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;

            Close();
        }
    }


    public class DeviceEditorItem :
        INotifyPropertyChanged
    {
        private string _name =
            string.Empty;

        private string _subtitle =
            string.Empty;

        private string _description =
            string.Empty;

        private string _year =
            string.Empty;

        private string _company =
            string.Empty;

        private string _type =
            string.Empty;

        private string _material =
            string.Empty;

        private string _imagePath =
            string.Empty;

        private string _videoPath =
            string.Empty;


        public string Id { get; set; } =
            string.Empty;


        public string Category { get; set; } =
            string.Empty;


        public string Icon { get; set; } =
            string.Empty;


        public string Name
        {
            get => _name;

            set
            {
                _name = value ?? string.Empty;
                OnPropertyChanged();
            }
        }


        public string Subtitle
        {
            get => _subtitle;

            set
            {
                _subtitle = value ?? string.Empty;
                OnPropertyChanged();
            }
        }


        public string Description
        {
            get => _description;

            set
            {
                _description = value ?? string.Empty;
                OnPropertyChanged();
            }
        }


        public string Year
        {
            get => _year;

            set
            {
                _year = value ?? string.Empty;
                OnPropertyChanged();
            }
        }


        public string Company
        {
            get => _company;

            set
            {
                _company = value ?? string.Empty;
                OnPropertyChanged();
            }
        }


        public string Type
        {
            get => _type;

            set
            {
                _type = value ?? string.Empty;
                OnPropertyChanged();
            }
        }


        public string Material
        {
            get => _material;

            set
            {
                _material = value ?? string.Empty;
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


        public string VideoPath
        {
            get => _videoPath;

            set
            {
                _videoPath =
                    value ?? string.Empty;

                OnPropertyChanged();

                OnPropertyChanged(
                    nameof(VideoFileName));
            }
        }


        public string VideoFileName =>
            string.IsNullOrWhiteSpace(
                VideoPath)
            ? "لا يوجد فيديو"
            : Path.GetFileName(
                VideoPath);


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


