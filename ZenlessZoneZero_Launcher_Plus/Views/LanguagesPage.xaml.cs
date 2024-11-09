using ZenlessZoneZero_Launcher_plus.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ZenlessZoneZero_Launcher_plus.Views
{
    /// <summary>
    /// SwitchLanguagesPage.xaml 的交互逻辑
    /// </summary>
    public partial class LanguagesPage : UserControl
    {
        public LanguagesPage()
        {
            InitializeComponent();
            DataContext = new LanguagesPageViewModel();
        }

        private void RemoveThisPage(object sender, RoutedEventArgs e)
        {
            App.Current.NoticeOverAllBase.MainPagesIndex = 0;
        }
    }
}
