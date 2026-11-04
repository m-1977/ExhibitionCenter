using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ExhibitionCenter.Models;

namespace ExhibitionCenter
{
    public partial class MainWindow : Window
    {
        private readonly Dictionary<string, List<ExhibitItem>> _catalog;

        private readonly List<CategoryItem> _categories;

        private List<ExhibitItem> _currentItems = new();

        private ExhibitItem? _selectedItem;


        // Slider
        private Point _dragStartPoint;
        private double _dragStartOffset;
        private bool _isDragging;
        private ScrollViewer? _scrollViewer;

        // Home category slider
        private Point _categoryDragStartPoint;
        private double _categoryDragStartOffset;
        private bool _isCategoryDragging;

        // Admin category slider
        private Point _adminCategoryDragStart;
        private double _adminCategoryStartOffset;
        private bool _adminCategoryDragging;
        private ScrollViewer? _adminCategoryScrollViewer;

        // Hidden Admin
        private readonly DispatcherTimer _adminHoldTimer = new();
        private string _pinBuffer = string.Empty;
        private const string AdminPin = "1234";


        public MainWindow()
        {
            InitializeComponent();

            InitializeTouchClickSound();

            InitializePowerSettings();

            InitializeTouchscreenSettings();

            InitializeClockScreenRuntime();

            LoadBackground();

            _catalog = CreateDemoCatalog();


            // -------------------------------------------------
            // Extra demo categories so we can test Home Slider.
            // Later these come from Admin Panel / Database.
            // -------------------------------------------------

            _catalog["Televisions"] = new()
            {
                NewItem(
                    "Televisions",
                    "📺",
                    "Sony Trinitron",
                    "1968",
                    "Sony",
                    "CRT Television",
                    "بلاستيك وزجاج",
                    "أحد أشهر أجهزة التلفزيون الملون",
                    "تميزت سلسلة Trinitron بجودة الصورة وأصبحت من أشهر أجهزة التلفزيون في عصرها."),

                NewItem(
                    "Televisions",
                    "📺",
                    "Philips K9",
                    "1972",
                    "Philips",
                    "Color Television",
                    "خشب وبلاستيك",
                    "التلفزيون الملون الكلاسيكي",
                    "يمثل هذا الجهاز مرحلة مهمة في انتشار التلفزيون الملون داخل المنازل."),

                NewItem(
                    "Televisions",
                    "📺",
                    "Panasonic Quintrix",
                    "1974",
                    "Panasonic",
                    "CRT Television",
                    "بلاستيك وزجاج",
                    "تصميم ياباني كلاسيكي",
                    "من أجهزة التلفزيون المنزلية التي اشتهرت خلال السبعينيات.")
            };


            _catalog["Radios"] = new()
            {
                NewItem(
                    "Radios",
                    "📻",
                    "Philips BX490A",
                    "1949",
                    "Philips",
                    "Valve Radio",
                    "خشب ومعدن",
                    "الراديو المنزلي الكلاسيكي",
                    "جهاز راديو يعتمد على الصمامات ويعكس تصميم أجهزة الاستقبال في منتصف القرن العشرين."),

                NewItem(
                    "Radios",
                    "📻",
                    "Grundig 4090",
                    "1956",
                    "Grundig",
                    "Table Radio",
                    "خشب وقماش",
                    "صوت الراديو الأوروبي",
                    "من أجهزة الراديو الألمانية التي اشتهرت بتصميمها وجودة الصوت."),

                NewItem(
                    "Radios",
                    "📻",
                    "Sony TR-63",
                    "1957",
                    "Sony",
                    "Transistor Radio",
                    "بلاستيك ومعدن",
                    "الراديو أصبح محمولاً",
                    "من أجهزة الترانزستور الصغيرة التي ساهمت في انتشار الراديو المحمول.")
            };


            _categories = CreateCategories();

            CategoryStrip.ItemsSource = _categories;


                        OpenInitialSelection();

            _adminHoldTimer.Interval = TimeSpan.FromSeconds(3);
            _adminHoldTimer.Tick += AdminHoldTimer_Tick;

            RefreshAdminDashboard();
        }


        // =====================================================
        // DEMO DATA
        // Later this will come from Admin / Database
        // =====================================================

        private List<CategoryItem> CreateCategories()
        {
            return new List<CategoryItem>
            {
                new CategoryItem
                {
                    Key = "Cameras",
                    Icon = "📷",
                    Name = "الكاميرات",
                    Subtitle = "لحظات صنعت التاريخ"
                },

                new CategoryItem
                {
                    Key = "Recorders",
                    Icon = "📼",
                    Name = "المسجلات",
                    Subtitle = "أصوات من زمن مضى"
                },

                new CategoryItem
                {
                    Key = "Speakers",
                    Icon = "🔊",
                    Name = "السماعات",
                    Subtitle = "صوت يتجاوز الزمن"
                },

                new CategoryItem
                {
                    Key = "Phones",
                    Icon = "☎",
                    Name = "الهواتف",
                    Subtitle = "التواصل عبر الأجيال"
                },

                // Demo only
                new CategoryItem
                {
                    Key = "Televisions",
                    Icon = "📺",
                    Name = "التلفزيونات",
                    Subtitle = "الصورة التي دخلت كل بيت"
                },

                // Demo only
                new CategoryItem
                {
                    Key = "Radios",
                    Icon = "📻",
                    Name = "الراديو",
                    Subtitle = "أصوات عبر الأثير"
                }
            };
        }

        private Dictionary<string, List<ExhibitItem>> CreateDemoCatalog()
        {
            return new Dictionary<string, List<ExhibitItem>>
            {
                ["Cameras"] = new()
                {
                    NewItem("Cameras","📷","Nikon F","1959","Nikon","SLR 35mm","معدن وجلد",
                        "أيقونة التصوير في القرن العشرين",
                        "كاميرا احترافية شهيرة ساهمت في تشكيل تاريخ التصوير الحديث."),

                    NewItem("Cameras","📷","Leica M3","1954","Leica","Rangefinder","معدن وزجاج",
                        "تصميم ألماني كلاسيكي",
                        "من أشهر كاميرات التصوير الكلاسيكية وأكثرها تقديراً."),

                    NewItem("Cameras","📷","Polaroid SX-70","1972","Polaroid","Instant Camera","معدن وبلاستيك",
                        "الصورة الفورية",
                        "قدمت تجربة التصوير الفوري بتصميم قابل للطي أصبح أيقونياً."),

                    NewItem("Cameras","📷","Canon AE-1","1976","Canon","SLR 35mm","معدن وبلاستيك",
                        "جيل جديد من الكاميرات",
                        "من أشهر كاميرات السبعينيات وساهمت في انتشار التصوير الإلكتروني."),

                    NewItem("Cameras","📷","Kodak No. 1","1888","Kodak","Box Camera","خشب ومعدن",
                        "بداية التصوير للجميع",
                        "ساعدت في جعل التصوير أكثر سهولة وانتشاراً."),

                    NewItem("Cameras","📷","Rolleiflex 2.8F","1960","Rollei","Twin Lens","معدن وجلد",
                        "العدستان الكلاسيكيتان",
                        "كاميرا شهيرة بتصميم العدستين."),

                    NewItem("Cameras","📷","Hasselblad 500C","1957","Hasselblad","Medium Format","معدن وجلد",
                        "جودة احترافية عالية",
                        "كاميرا معروفة بجودة الصور العالية."),

                    NewItem("Cameras","📷","Olympus OM-1","1972","Olympus","SLR 35mm","معدن",
                        "كاميرا صغيرة وخفيفة",
                        "تميزت بحجمها الصغير وتصميمها العملي.")
                },


                ["Recorders"] = new()
                {
                    NewItem("Recorders","📼","Sony TC-377","1971","Sony","Reel-to-Reel","معدن وبلاستيك",
                        "صوت الشريط المغناطيسي",
                        "مسجل بكرات كلاسيكي من أجهزة الصوت المنزلية القديمة."),

                    NewItem("Recorders","📼","Akai GX-4000D","1970","Akai","Reel-to-Reel","معدن",
                        "التسجيل المنزلي الاحترافي",
                        "من أشهر أجهزة التسجيل بالبكرات في تلك الفترة."),

                    NewItem("Recorders","📼","Philips EL3300","1963","Philips","Cassette Recorder","بلاستيك ومعدن",
                        "بداية الكاسيت المحمول",
                        "من الأجهزة المهمة في انتشار شريط الكاسيت."),

                    NewItem("Recorders","📼","Panasonic RQ-2102","1975","Panasonic","Cassette Recorder","بلاستيك",
                        "مسجل محمول",
                        "جهاز تسجيل بسيط انتشر للاستخدام المنزلي والتعليمي."),

                    NewItem("Recorders","📼","Sony Walkman TPS-L2","1979","Sony","Portable Cassette","معدن وبلاستيك",
                        "الموسيقى أصبحت محمولة",
                        "أحد أشهر الأجهزة التي غيرت طريقة الاستماع للموسيقى."),

                    NewItem("Recorders","📼","Grundig TK 46","1965","Grundig","Tape Recorder","معدن وخشب",
                        "تسجيل كلاسيكي",
                        "جهاز تسجيل أوروبي بتصميم مميز."),

                    NewItem("Recorders","📼","Uher 4000","1961","Uher","Portable Reel","معدن",
                        "التسجيل الميداني",
                        "استخدم في التسجيلات الصحفية والميدانية."),

                    NewItem("Recorders","📼","National RQ-309S","1974","National","Cassette","بلاستيك ومعدن",
                        "مسجل منزلي",
                        "مثال كلاسيكي على أجهزة الكاسيت القديمة.")
                },


                ["Speakers"] = new()
                {
                    NewItem("Speakers","🔊","JBL L100","1970","JBL","Loudspeaker","خشب وقماش",
                        "صوت السبعينيات",
                        "من أشهر السماعات المنزلية الكلاسيكية."),

                    NewItem("Speakers","🔊","Tannoy Monitor Gold","1967","Tannoy","Studio Speaker","خشب ومعدن",
                        "صوت الاستوديو",
                        "استخدمت في الاستوديوهات وأنظمة الصوت عالية الجودة."),

                    NewItem("Speakers","🔊","Bose 901","1968","Bose","Direct Reflecting","خشب ومعدن",
                        "تصميم صوتي مختلف",
                        "استخدمت أسلوباً مختلفاً في توزيع الصوت داخل الغرفة."),

                    NewItem("Speakers","🔊","KEF 104","1973","KEF","Hi-Fi Speaker","خشب",
                        "الصوت البريطاني",
                        "إحدى سماعات Hi-Fi المعروفة في السبعينيات."),

                    NewItem("Speakers","🔊","Altec Lansing 604","1944","Altec Lansing","Coaxial","معدن",
                        "سماعة استوديو تاريخية",
                        "من التصاميم المؤثرة في أنظمة الصوت الاحترافية."),

                    NewItem("Speakers","🔊","Pioneer CS-99A","1971","Pioneer","Hi-Fi","خشب وقماش",
                        "تصميم كلاسيكي",
                        "سماعة منزلية من عصر أجهزة الصوت الكبيرة."),

                    NewItem("Speakers","🔊","Sansui SP-3500","1972","Sansui","Hi-Fi","خشب",
                        "شبك خشبي مميز",
                        "اشتهرت بتصميمها الياباني الكلاسيكي."),

                    NewItem("Speakers","🔊","Wharfedale Denton","1970","Wharfedale","Bookshelf","خشب",
                        "صوت صغير بحضور كبير",
                        "تصميم بريطاني كلاسيكي للسماعات المنزلية.")
                },


                ["Phones"] = new()
                {
                    NewItem("Phones","☎","Rotary Telephone","1950","Various","Rotary Phone","بلاستيك ومعدن",
                        "الهاتف الدوار",
                        "من أشهر أشكال الهواتف المنزلية في القرن العشرين."),

                    NewItem("Phones","☎","Ericsson Dialog","1962","Ericsson","Desk Phone","بلاستيك",
                        "تصميم اسكندنافي",
                        "هاتف مكتبي بسيط وأنيق انتشر لسنوات طويلة."),

                    NewItem("Phones","☎","Trimphone","1965","GPO","Rotary Phone","بلاستيك",
                        "هاتف بتصميم مستقبلي",
                        "تميز بحجمه الصغير وشكله المختلف عن الهواتف التقليدية."),

                    NewItem("Phones","☎","Motorola DynaTAC","1983","Motorola","Mobile Phone","بلاستيك",
                        "بداية الهاتف المحمول",
                        "من أوائل الهواتف المحمولة التجارية المعروفة."),

                    NewItem("Phones","☎","Nokia 1011","1992","Nokia","GSM Phone","بلاستيك",
                        "جيل GSM",
                        "من أوائل هواتف GSM التي دخلت الأسواق."),

                    NewItem("Phones","☎","Nokia 8110","1996","Nokia","Mobile Phone","بلاستيك",
                        "تصميم منحني مميز",
                        "أحد أشهر تصاميم هواتف نوكيا في التسعينيات."),

                    NewItem("Phones","☎","Motorola StarTAC","1996","Motorola","Flip Phone","بلاستيك",
                        "الهاتف القابل للطي",
                        "ساهم في انتشار تصميم الهواتف القابلة للطي."),

                    NewItem("Phones","☎","Nokia 3310","2000","Nokia","Mobile Phone","بلاستيك",
                        "أيقونة بداية الألفية",
                        "أحد أشهر الهواتف المحمولة وأكثرها انتشاراً.")
                }
            };
        }


        private ExhibitItem NewItem(
            string category,
            string icon,
            string name,
            string year,
            string company,
            string type,
            string material,
            string subtitle,
            string description)
        {
            return new ExhibitItem
            {
                Category = category,
                Icon = icon,
                Name = name,
                Year = year,
                Company = company,
                Type = type,
                Material = material,
                Subtitle = subtitle,
                Description = description
            };
        }


        // =====================================================
        // CATEGORY BUTTONS
        // =====================================================

        private void OpenCategoryCard_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            if (button.Tag is not string categoryKey)
                return;

            PlayTouchClick();

            OpenCategory(categoryKey);
        }

        private void OpenCameras_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenCategory("Cameras");
        }


        private void OpenRecorders_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenCategory("Recorders");
        }


        private void OpenSpeakers_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenCategory("Speakers");
        }


        private void OpenPhones_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenCategory("Phones");
        }


        private void OpenCategory(string category)
        {
            StopVideo();

            if (!_catalog.TryGetValue(category, out var items))
                return;

            _currentItems = items;

            CameraStrip.ItemsSource = null;
            CameraStrip.ItemsSource = _currentItems;

            HomeContent.Visibility = Visibility.Collapsed;
            CamerasContent.Visibility = Visibility.Visible;

            _scrollViewer ??=
                FindVisualChild<ScrollViewer>(CameraStrip);

            _scrollViewer?.ScrollToHorizontalOffset(0);

            if (_currentItems.Count > 0)
            {
                CameraStrip.SelectedIndex = 0;
            }
        }


        private void OpenInitialSelection()
        {
            CamerasContent.Visibility = Visibility.Collapsed;
            HomeContent.Visibility = Visibility.Visible;
        }


        // =====================================================
        // BACKGROUND
        // =====================================================

        private void LoadBackground()
        {
            string backgroundPath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Assets",
                    "Backgrounds",
                    "MuseumBackground.png");

            if (File.Exists(backgroundPath))
            {
                BackgroundImage.Source =
                    new BitmapImage(
                        new Uri(backgroundPath, UriKind.Absolute));
            }
        }


        // =====================================================
        // BACK
        // =====================================================

        private void BackHome_Click(
            object sender,
            RoutedEventArgs e)
        {
            PlayTouchClick();

            StopVideo();

            CamerasContent.Visibility = Visibility.Collapsed;
            HomeContent.Visibility = Visibility.Visible;
        }


        // =====================================================
        // ITEM SELECTION
        // =====================================================

        private void CameraStrip_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (CameraStrip.SelectedItem is not ExhibitItem item)
                return;

            _selectedItem = item;

            CameraTitle.Text = item.Name;
            CameraSubtitle.Text = item.Subtitle;
            CameraDescription.Text = item.Description;

            CameraYear.Text = item.Year;
            CameraCompany.Text = item.Company;
            CameraType.Text = item.Type;
            CameraMaterial.Text = item.Material;

            PosterCameraName.Text = item.Name;

            PosterIcon.Text = item.Icon;

            StopVideo();

            LoadPoster(item);
        }


        // =====================================================
        // IMAGE
        // =====================================================

        private void LoadPoster(ExhibitItem item)
        {
            PosterImage.Source = null;

            PosterImage.Visibility = Visibility.Collapsed;
            PosterPlaceholder.Visibility = Visibility.Visible;

            if (string.IsNullOrWhiteSpace(item.ImagePath))
                return;

            if (!File.Exists(item.ImagePath))
                return;

            try
            {
                PosterImage.Source =
                    new BitmapImage(
                        new Uri(item.ImagePath, UriKind.Absolute));

                PosterImage.Visibility = Visibility.Visible;
                PosterPlaceholder.Visibility = Visibility.Collapsed;
            }
            catch
            {
                PosterImage.Source = null;
                PosterImage.Visibility = Visibility.Collapsed;
                PosterPlaceholder.Visibility = Visibility.Visible;
            }
        }


        // =====================================================
        // VIDEO
        // =====================================================

        private void PlayVideo_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_selectedItem is null)
                return;


            if (string.IsNullOrWhiteSpace(
                    _selectedItem.VideoPath) ||
                !File.Exists(
                    _selectedItem.VideoPath))
            {
                MediaStatusText.Text =
                    "لا يوجد فيديو مضاف لهذا الجهاز حالياً";


                MediaStatusText.Visibility =
                    Visibility.Visible;


                return;
            }


            try
            {
                MediaStatusText.Visibility =
                    Visibility.Collapsed;


                PosterImage.Visibility =
                    Visibility.Collapsed;


                PosterPlaceholder.Visibility =
                    Visibility.Collapsed;


                VideoPlayer.Visibility =
                    Visibility.Collapsed;


                VideoSurface.Visibility =
                    Visibility.Visible;


                PlayButton.Visibility =
                    Visibility.Collapsed;


                PlayRoundedVideo(
                    _selectedItem.VideoPath);
            }
            catch
            {
                VideoSurface.Visibility =
                    Visibility.Collapsed;


                MediaStatusText.Text =
                    "تعذر تشغيل الفيديو";


                MediaStatusText.Visibility =
                    Visibility.Visible;


                PlayButton.Visibility =
                    Visibility.Visible;


                LoadPoster(
                    _selectedItem);
            }
        }

        private void VideoPlayer_MediaEnded(
            object sender,
            RoutedEventArgs e)
        {
            StopVideo();
        }


        private void StopVideo()
        {
            StopRoundedVideo();


            VideoSurface.Visibility =
                Visibility.Collapsed;


            VideoPlayer.Visibility =
                Visibility.Collapsed;


            PlayButton.Visibility =
                Visibility.Visible;


            MediaStatusText.Visibility =
                Visibility.Collapsed;


            if (_selectedItem is not null)
            {
                LoadPoster(
                    _selectedItem);
            }
        }

        // =====================================================
        // TOUCH + MOUSE SLIDER
        // =====================================================

        // =====================================================
        // HOME CATEGORY SLIDER
        // Touch Swipe + Mouse Drag + Mouse Wheel
        // =====================================================

        private void CategorySlider_PreviewMouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            _categoryDragStartPoint =
                e.GetPosition(CategoryScrollViewer);

            _categoryDragStartOffset =
                CategoryScrollViewer.HorizontalOffset;

            _isCategoryDragging = false;
        }


        private void CategorySlider_PreviewMouseMove(
            object sender,
            MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            Point currentPoint =
                e.GetPosition(CategoryScrollViewer);

            double delta =
                currentPoint.X -
                _categoryDragStartPoint.X;


            if (!_isCategoryDragging &&
                Math.Abs(delta) < 7)
            {
                return;
            }


            _isCategoryDragging = true;


            if (!CategoryScrollViewer.IsMouseCaptured)
            {
                CategoryScrollViewer.CaptureMouse();
            }


            CategoryScrollViewer.ScrollToHorizontalOffset(
                _categoryDragStartOffset - delta);


            e.Handled = true;
        }


        private void CategorySlider_PreviewMouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            if (CategoryScrollViewer.IsMouseCaptured)
            {
                CategoryScrollViewer.ReleaseMouseCapture();
            }


            if (_isCategoryDragging)
            {
                // Prevent opening a category when user was only dragging.
                e.Handled = true;
            }


            _isCategoryDragging = false;
        }


        private void CategorySlider_PreviewMouseWheel(
            object sender,
            MouseWheelEventArgs e)
        {
            double newOffset =
                CategoryScrollViewer.HorizontalOffset -
                (e.Delta * 0.70);


            CategoryScrollViewer.ScrollToHorizontalOffset(
                newOffset);


            e.Handled = true;
        }

        private void CameraStrip_PreviewMouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            _scrollViewer ??=
                FindVisualChild<ScrollViewer>(CameraStrip);

            if (_scrollViewer is null)
                return;

            _dragStartPoint =
                e.GetPosition(CameraStrip);

            _dragStartOffset =
                _scrollViewer.HorizontalOffset;

            _isDragging = false;
        }


        private void CameraStrip_PreviewMouseMove(
            object sender,
            MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            _scrollViewer ??=
                FindVisualChild<ScrollViewer>(CameraStrip);

            if (_scrollViewer is null)
                return;

            Point currentPoint =
                e.GetPosition(CameraStrip);

            double delta =
                currentPoint.X -
                _dragStartPoint.X;

            if (!_isDragging &&
                Math.Abs(delta) < 7)
            {
                return;
            }

            _isDragging = true;

            _scrollViewer.ScrollToHorizontalOffset(
                _dragStartOffset - delta);

            e.Handled = true;
        }


        private void CameraStrip_PreviewMouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                e.Handled = true;
            }

            _isDragging = false;
        }


        private void CameraStrip_PreviewMouseWheel(
            object sender,
            MouseWheelEventArgs e)
        {
            _scrollViewer ??=
                FindVisualChild<ScrollViewer>(CameraStrip);

            if (_scrollViewer is null)
                return;

            double newOffset =
                _scrollViewer.HorizontalOffset -
                (e.Delta * 0.65);

            _scrollViewer.ScrollToHorizontalOffset(
                newOffset);

            e.Handled = true;
        }


        private static T? FindVisualChild<T>(
            DependencyObject parent)
            where T : DependencyObject
        {
            int count =
                VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < count; i++)
            {
                DependencyObject child =
                    VisualTreeHelper.GetChild(parent, i);

                if (child is T result)
                    return result;

                T? nested =
                    FindVisualChild<T>(child);

                if (nested is not null)
                    return nested;
            }

            return null;
        }

        // =====================================================
        // HIDDEN ADMIN ACCESS
        // Hold title for 3 seconds
        // =====================================================

        private void StartAdminHold()
        {
            _adminHoldTimer.Stop();
            _adminHoldTimer.Start();
        }


        private void CancelAdminHold()
        {
            _adminHoldTimer.Stop();
        }


        private void AdminHotspot_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            StartAdminHold();
        }


        private void AdminHotspot_MouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            CancelAdminHold();
        }


        private void AdminHotspot_MouseLeave(
            object sender,
            MouseEventArgs e)
        {
            CancelAdminHold();
        }


        private void AdminHotspot_TouchDown(
            object sender,
            TouchEventArgs e)
        {
            StartAdminHold();
        }


        private void AdminHotspot_TouchUp(
            object sender,
            TouchEventArgs e)
        {
            CancelAdminHold();
        }


        private void AdminHoldTimer_Tick(
            object? sender,
            EventArgs e)
        {
            _adminHoldTimer.Stop();

            _pinBuffer = string.Empty;
            UpdatePinDisplay();

            PinOverlay.Visibility = Visibility.Visible;
        }


        // =====================================================
        // PIN
        // =====================================================

        private void PinDigit_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            if (button.Tag is not string digit)
                return;

            if (_pinBuffer.Length >= 8)
                return;

            _pinBuffer += digit;

            UpdatePinDisplay();
        }


        private void PinClear_Click(
            object sender,
            RoutedEventArgs e)
        {
            _pinBuffer = string.Empty;
            UpdatePinDisplay();
        }


        private void PinBackspace_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_pinBuffer.Length == 0)
                return;

            _pinBuffer =
                _pinBuffer.Substring(
                    0,
                    _pinBuffer.Length - 1);

            UpdatePinDisplay();
        }


        private void PinCancel_Click(
            object sender,
            RoutedEventArgs e)
        {
            _pinBuffer = string.Empty;

            PinOverlay.Visibility =
                Visibility.Collapsed;
        }


        private void PinEnter_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_pinBuffer != AdminPin)
            {
                PinDisplay.Text = "PIN غير صحيح";

                _pinBuffer = string.Empty;

                return;
            }

            PinOverlay.Visibility =
                Visibility.Collapsed;

            HomeContent.Visibility =
                Visibility.Collapsed;

            CamerasContent.Visibility =
                Visibility.Collapsed;

            AdminContent.Visibility =
                Visibility.Visible;

            RefreshAdminDashboard();
        }


        private void UpdatePinDisplay()
        {
            PinDisplay.Text =
                new string('●', _pinBuffer.Length);
        }


        // =====================================================
        // ADMIN DASHBOARD
        // =====================================================

        private void RefreshAdminDashboard()
        {
            if (AdminCategoryList is null)
                return;

            AdminCategoryList.ItemsSource = null;
            AdminCategoryList.ItemsSource = _categories;

            AdminCategoryCount.Text =
                _categories.Count.ToString();

            int totalItems = 0;

            foreach (var category in _catalog)
            {
                totalItems += category.Value.Count;
            }

            AdminItemCount.Text =
                totalItems.ToString();
        }


        private void ExitAdmin_Click(
            object sender,
            RoutedEventArgs e)
        {
            AdminContent.Visibility =
                Visibility.Collapsed;

            HomeContent.Visibility =
                Visibility.Visible;
        }


        private void AdminAddCategory_Click(
            object sender,
            RoutedEventArgs e)
        {
            MessageBox.Show(
                "الخطوة التالية: إضافة قسم جديد من داخل البرنامج.",
                "Exhibition Center",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }


        // =====================================================
        // ADMIN CATEGORY SLIDER
        // Touch + Mouse Drag + Mouse Wheel
        // =====================================================

        private void AdminCategorySlider_MouseDown(
            object sender,
            MouseButtonEventArgs e)
        {
            _adminCategoryScrollViewer ??=
                FindVisualChild<ScrollViewer>(AdminCategoryList);

            if (_adminCategoryScrollViewer is null)
                return;

            _adminCategoryDragStart =
                e.GetPosition(AdminCategoryList);

            _adminCategoryStartOffset =
                _adminCategoryScrollViewer.HorizontalOffset;

            _adminCategoryDragging = false;
        }


        private void AdminCategorySlider_MouseMove(
            object sender,
            MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            _adminCategoryScrollViewer ??=
                FindVisualChild<ScrollViewer>(AdminCategoryList);

            if (_adminCategoryScrollViewer is null)
                return;

            Point current =
                e.GetPosition(AdminCategoryList);

            double delta =
                current.X -
                _adminCategoryDragStart.X;

            if (!_adminCategoryDragging &&
                Math.Abs(delta) < 7)
            {
                return;
            }

            _adminCategoryDragging = true;

            _adminCategoryScrollViewer.ScrollToHorizontalOffset(
                _adminCategoryStartOffset - delta);

            e.Handled = true;
        }


        private void AdminCategorySlider_MouseUp(
            object sender,
            MouseButtonEventArgs e)
        {
            if (_adminCategoryDragging)
            {
                e.Handled = true;
            }

            _adminCategoryDragging = false;
        }


        private void AdminCategorySlider_MouseWheel(
            object sender,
            MouseWheelEventArgs e)
        {
            _adminCategoryScrollViewer ??=
                FindVisualChild<ScrollViewer>(AdminCategoryList);

            if (_adminCategoryScrollViewer is null)
                return;

            double offset =
                _adminCategoryScrollViewer.HorizontalOffset -
                (e.Delta * 0.65);

            _adminCategoryScrollViewer.ScrollToHorizontalOffset(
                offset);

            e.Handled = true;
        }

}
}






