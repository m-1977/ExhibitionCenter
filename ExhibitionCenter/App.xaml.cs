using System.Configuration;
using System.Data;
using System.Windows;
using ExhibitionCenter.Services;

namespace ExhibitionCenter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                if (!new WindowsPowerService().Release())
                {
                    MessageBox.Show("تعذر تحرير إعداد إبقاء الجهاز والشاشة قيد التشغيل في Windows.",
                        "خطأ في الطاقة والشاشة", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                base.OnExit(e);
            }
        }
    }

}
