using System.Windows;
using System.Windows.Input;
using ExhibitionCenter.Services;

namespace ExhibitionCenter.Dialogs
{
    public partial class ExitConfirmationWindow : Window
    {
        public bool ExitRequested
            { get; private set; }


        public ExitConfirmationWindow()
        {
            InitializeComponent();


            ApplyLanguage();


            Loaded += (_, _) =>
            {
                Activate();

                Focus();
            };
        }


        private void ApplyLanguage()
        {
            if (LanguageService.Current ==
                AppLanguage.English)
            {
                FlowDirection =
                    FlowDirection.LeftToRight;


                ExitTitleText.Text =
                    "Do you want to exit the application?";


                ExitSubtitleText.Text =
                    "Exhibition Center will close";


                ExitNoButton.Content =
                    "No";


                ExitYesButton.Content =
                    "Yes, Exit";
            }
            else
            {
                FlowDirection =
                    FlowDirection.RightToLeft;


                ExitTitleText.Text =
                    "هل تريد الخروج من البرنامج؟";


                ExitSubtitleText.Text =
                    "سيتم إغلاق Exhibition Center";


                ExitNoButton.Content =
                    "لا";


                ExitYesButton.Content =
                    "نعم، خروج";
            }
        }


        private void Yes_Click(
            object sender,
            RoutedEventArgs e)
        {
            ExitRequested =
                true;


            DialogResult =
                true;


            Close();
        }


        private void No_Click(
            object sender,
            RoutedEventArgs e)
        {
            ExitRequested =
                false;


            DialogResult =
                false;


            Close();
        }


        private void Window_PreviewKeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.Key ==
                Key.Escape)
            {
                ExitRequested =
                    false;


                DialogResult =
                    false;


                Close();


                e.Handled =
                    true;
            }
        }
    }
}
