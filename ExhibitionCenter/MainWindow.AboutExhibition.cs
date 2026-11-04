using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using ExhibitionCenter.Models;
using ExhibitionCenter.Services;

namespace ExhibitionCenter
{
    public partial class MainWindow
    {
        private readonly
            AboutExhibitionSettingsService
            _aboutExhibitionService =
                new();


        private bool
            _aboutScrollSync;


        private void AboutExhibition_Click(
            object sender,
            MouseButtonEventArgs e)
        {
            ShowAboutExhibition();


            e.Handled =
                true;
        }


        private void ShowAboutExhibition()
        {
            AboutExhibitionSettings settings =
                _aboutExhibitionService.Load();


            Brush accent =
                GetAboutAccentBrush(
                    settings.AccentColor);


            AboutPopupBorder.BorderBrush =
                accent;

            AboutCloseButton.BorderBrush =
                accent;

            AboutCloseButton.Foreground =
                accent;

            AboutImageFrame.BorderBrush =
                accent;

            AboutTitleText.Foreground =
                accent;

            AboutInfoIcon.Foreground =
                accent;

            AboutGoldLine.Background =
                accent;

            AboutScrollBar.Foreground =
                accent;


            bool english =
                LanguageService.Current ==
                AppLanguage.English;


            string title =
                english
                    ? PickAboutText(
                        settings.TitleEn,
                        settings.TitleAr)
                    : PickAboutText(settings.TitleAr, settings.TitleEn);


            AboutTitleText.Text =
                title;


            AboutTitleText.FlowDirection =
                english
                    ? FlowDirection.LeftToRight
                    : FlowDirection.RightToLeft;


            AboutTitleText.TextAlignment =
                english
                    ? TextAlignment.Left
                    : TextAlignment.Right;


            AboutInfoIcon.HorizontalAlignment =
                english
                    ? HorizontalAlignment.Left
                    : HorizontalAlignment.Right;


            AboutNotesPanel.FlowDirection =
                english
                    ? FlowDirection.LeftToRight
                    : FlowDirection.RightToLeft;


            BuildAboutNotes(
                settings,
                english,
                accent);


            LoadAboutImage(
                settings.ImagePath);


            AboutTextScrollViewer.ScrollToTop();


            AboutOverlay.Visibility =
                Visibility.Visible;


            AboutOverlay.Opacity =
                0;


            AboutPopupScale.ScaleX =
                0.96;

            AboutPopupScale.ScaleY =
                0.96;


            var fade =
                new DoubleAnimation(
                    0,
                    1,
                    TimeSpan.FromMilliseconds(
                        190));


            var scale =
                new DoubleAnimation(
                    0.96,
                    1,
                    TimeSpan.FromMilliseconds(
                        220))
                {
                    EasingFunction =
                        new QuadraticEase
                        {
                            EasingMode =
                                EasingMode.EaseOut
                        }
                };


            AboutOverlay.BeginAnimation(
                OpacityProperty,
                fade);


            AboutPopupScale.BeginAnimation(
                ScaleTransform.ScaleXProperty,
                scale);


            AboutPopupScale.BeginAnimation(
                ScaleTransform.ScaleYProperty,
                scale);


            Dispatcher.BeginInvoke(
                new Action(
                    UpdateAboutScrollBar));
        }


        private void BuildAboutNotes(
            AboutExhibitionSettings settings,
            bool english,
            Brush accent)
        {
            AboutNotesPanel.Children.Clear();


            foreach (AboutExhibitionNote note
                     in settings.Notes)
            {
                string heading =
                    english
                        ? PickAboutText(
                            note.TitleEn,
                            note.TitleAr)
                        : PickAboutText(note.TitleAr, note.TitleEn);


                string body =
                    english
                        ? PickAboutText(
                            note.BodyEn,
                            note.BodyAr)
                        : PickAboutText(note.BodyAr, note.BodyEn);


                if (string.IsNullOrWhiteSpace(
                        heading) &&
                    string.IsNullOrWhiteSpace(
                        body))
                {
                    continue;
                }


                var section =
                    new StackPanel
                    {
                        Margin =
                            new Thickness(
                                0,
                                0,
                                0,
                                34)
                    };


                if (!string.IsNullOrWhiteSpace(
                        heading))
                {
                    section.Children.Add(
                        new TextBlock
                        {
                            Text =
                                heading,

                            Foreground =
                                accent,

                            FontSize =
                                26,

                            FontWeight =
                                FontWeights.SemiBold,

                            TextWrapping =
                                TextWrapping.Wrap,

                            FlowDirection =
                                english
                                    ? FlowDirection.LeftToRight
                                    : FlowDirection.RightToLeft,

                            TextAlignment =
                                english
                                    ? TextAlignment.Left
                                    : TextAlignment.Right
                        });
                }


                if (!string.IsNullOrWhiteSpace(
                        body))
                {
                    section.Children.Add(
                        new TextBlock
                        {
                            Text =
                                body,

                            Foreground =
                                Brushes.White,

                            FontSize =
                                20,

                            LineHeight =
                                35,

                            Margin =
                                new Thickness(
                                    0,
                                    10,
                                    0,
                                    0),

                            TextWrapping =
                                TextWrapping.Wrap,

                            FlowDirection =
                                english
                                    ? FlowDirection.LeftToRight
                                    : FlowDirection.RightToLeft,

                            TextAlignment =
                                english
                                    ? TextAlignment.Left
                                    : TextAlignment.Right
                        });
                }


                AboutNotesPanel.Children.Add(
                    section);
            }
        }


        private static string PickAboutText(
            string preferred,
            string fallback)
        {
            return
                string.IsNullOrWhiteSpace(
                    preferred)
                    ? fallback
                    : preferred;
        }


        private static Brush GetAboutAccentBrush(
            string value)
        {
            try
            {
                Brush brush =
                    (Brush)new BrushConverter()
                        .ConvertFromString(
                            value)!;


                if (brush.CanFreeze)
                {
                    brush.Freeze();
                }


                return brush;
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


        private void LoadAboutImage(
            string path)
        {
            AboutImageSurface.Fill =
                new SolidColorBrush(
                    Color.FromRgb(
                        24,
                        19,
                        15));


            AboutImagePlaceholder.Visibility =
                Visibility.Visible;


            if (string.IsNullOrWhiteSpace(
                    path) ||
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


                AboutImageSurface.Fill =
                    new ImageBrush(
                        bitmap)
                    {
                        Stretch =
                            Stretch.UniformToFill,

                        AlignmentX =
                            AlignmentX.Center,

                        AlignmentY =
                            AlignmentY.Center
                    };


                AboutImagePlaceholder.Visibility =
                    Visibility.Collapsed;
            }
            catch
            {
            }
        }


        private void AboutClose_Click(
            object sender,
            RoutedEventArgs e)
        {
            AboutOverlay.Visibility =
                Visibility.Collapsed;
        }


        private void AboutTextScrollViewer_ScrollChanged(
            object sender,
            ScrollChangedEventArgs e)
        {
            UpdateAboutScrollBar();
        }


        private void UpdateAboutScrollBar()
        {
            if (_aboutScrollSync)
            {
                return;
            }


            _aboutScrollSync =
                true;


            AboutScrollBar.Maximum =
                Math.Max(
                    0,
                    AboutTextScrollViewer
                        .ScrollableHeight);


            AboutScrollBar.ViewportSize =
                Math.Max(
                    1,
                    AboutTextScrollViewer
                        .ViewportHeight);


            AboutScrollBar.Value =
                AboutTextScrollViewer
                    .VerticalOffset;


            AboutScrollBar.Visibility =
                AboutTextScrollViewer
                    .ScrollableHeight > 1
                    ? Visibility.Visible
                    : Visibility.Hidden;


            _aboutScrollSync =
                false;
        }


        private void AboutScrollBar_ValueChanged(
            object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (_aboutScrollSync)
            {
                return;
            }


            _aboutScrollSync =
                true;


            AboutTextScrollViewer
                .ScrollToVerticalOffset(
                    e.NewValue);


            _aboutScrollSync =
                false;
        }
    }
}

