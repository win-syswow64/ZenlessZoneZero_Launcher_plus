using System.Windows;
using System.Windows.Controls;
using ZenlessZoneZero_Launcher_plus.ViewModels;
using MahApps.Metro.Controls.Dialogs;

namespace ZenlessZoneZero_Launcher_plus.Views
{
    /// <summary>
    /// AddUsersPage.xaml 的交互逻辑
    /// </summary>
    public partial class UsersPage : UserControl
    {
        public UsersPage()
        {
            InitializeComponent();
            DataContext = new UsersPageViewModel(DialogCoordinator.Instance);
        }
    }
}
