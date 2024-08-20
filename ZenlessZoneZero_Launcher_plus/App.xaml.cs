using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using ZenlessZoneZero_Launcher_plus.Core;
using ZenlessZoneZero_Launcher_plus.Models;
using ZenlessZoneZero_Launcher_plus.Server;

namespace ZenlessZoneZero_Launcher_plus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly SingleInstanceCheck _singleInstanceCheck = new("ZenlessZoneZeroLauncherPlus");

        public App()
        {
            LoadProgramCore = new();

        }

        public new static App Current => (App)Application.Current;
        public LoadProgramCore LoadProgramCore { get; set; }
        public DataModel DataModel { get; set; }
        public NoticeOverAllBase NoticeOverAllBase { get; set; }
        public LanguageModel Language { get; set; }
    }
}
