﻿using ZenlessZoneZero_Launcher_plus.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using CommunityToolkit.Mvvm.Input;

namespace ZenlessZoneZero_Launcher_plus.ViewModels
{
    public class GuidePageViewModel : ObservableObject
    {
        private IDialogCoordinator dialogCoordinator;
        public GuidePageViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;
            DirchooseCommand = new RelayCommand(Dirchoose);
        }
        private string _GamePath;
        public string GamePath
        {
            get=> _GamePath;
            set=> SetProperty(ref _GamePath, value);
        }

        public LanguageModel languages { get => App.Current.Language; }

        /// <summary>
        /// 选择游戏目录的命令方法
        /// </summary>
        public ICommand DirchooseCommand { get; set; }
        private void Dirchoose()
        {
            CommonOpenFileDialog dialog = new(App.Current.Language.GameDirMsg);
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                GamePath = dialog.FileName;
                if (!File.Exists(Path.Combine(GamePath, "ZenlessZoneZero.exe")))
                {
                    dialogCoordinator.ShowMessageAsync(
                        this, languages.Error, 
                        languages.PathErrorMessageStr, 
                        MessageDialogStyle.Affirmative,
                        new MetroDialogSettings()
                        { AffirmativeButtonText = languages.Determine });
                }
                else
                {
                    App.Current.DataModel.GamePath = GamePath;
                    App.Current.DataModel.SaveDataToFile();
                    App.Current.DataModel = new();
                    MainWindow mainWindow = new();
                    mainWindow.Show();
                    Application.Current.MainWindow.Close();
                    Application.Current.MainWindow = mainWindow;
                }
            }
        }
    }
}
