﻿using ZenlessZoneZero_Launcher_plus.Service;
using ZenlessZoneZero_Launcher_plus.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Windows.Input;
using ZenlessZoneZero_Launcher_plus.Service.IService;
using MahApps.Metro.Controls.Dialogs;

namespace ZenlessZoneZero_Launcher_plus.ViewModels
{
    /// <summary>
    /// UsersPage的ViewModel 
    /// 集成了UsersPage的部分操作实现逻辑
    /// </summary>
    public class UsersPageViewModel : ObservableObject
    {
        private IDialogCoordinator dialogCoordinator;
        public UsersPageViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;
            SaveUserDataCommand = new RelayCommand(SaveUserData);
            RemoveThisPageCommand = new RelayCommand(RemoveThisPage);

            _registryService = new RegistryService();
            _userDataService = new UserDataService();
        }

        public bool IsSaveGameConfig { get; set; }

        private IRegistryService _registryService;
        public IRegistryService RegistryService { get => _registryService; }

        private IUserDataService _userDataService;
        public IUserDataService UserDataService { get => _userDataService; }

        public LanguageModel languages { get => App.Current.Language; }

        public string? Name { get; set; }

        public ICommand SaveUserDataCommand { get; set; }
        private async void SaveUserData()
        {
            //判断YuanShen.exe是否存在，存在则为False，否则为True
            bool isGlobal = !File.Exists(Path.Combine(App.Current.DataModel.GamePath, "YuanShen.exe"));
            //判断isGlobal值，为True时为Cn，否则为Global
            string gamePort = isGlobal ? "Global" : "CN";
            if (Name != null && Name != string.Empty)
            {
                string userdata = RegistryService.GetFromRegistry(Name, gamePort,IsSaveGameConfig);
                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "UserData", Name), userdata);
                App.Current.NoticeOverAllBase.UserLists = UserDataService.ReadUserList();
                RemoveThisPage();
            }
            else
            {
                await dialogCoordinator.ShowMessageAsync(
                    this, languages.Error,
                    "保存的账号名字不能为空！",
                    MessageDialogStyle.Affirmative,
                    new MetroDialogSettings()
                    { AffirmativeButtonText = languages.Determine });
            }
        }

        public ICommand RemoveThisPageCommand { get; set; }
        private void RemoveThisPage()
        {
            App.Current.NoticeOverAllBase.MainPagesIndex = 0;
        }
    }
}
