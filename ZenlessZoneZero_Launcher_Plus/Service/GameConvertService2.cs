﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;
using ZenlessZoneZero_Launcher_plus.Core;
using ZenlessZoneZero_Launcher_plus.Helper;
using ZenlessZoneZero_Launcher_plus.Models;
using ZenlessZoneZero_Launcher_plus.Service.IService;
using ZenlessZoneZero_Launcher_plus.ViewModels;
using ZenlessZoneZero_Launcher_plus;

namespace ZenlessZoneZero_Launcher_plus.Service
{
    internal class ConvertService : IGameConvertService
    {
        private const string CN_DIRECTORY = "CnFile";
        private const string GLOBAL_DIRECTORY = "GlobalFile";
        private const string ZenlessZoneZero_DATA = "ZenlessZoneZero_Data";
        private const string ZenlessZoneZero_EXE = "ZenlessZoneZero.exe";

        private string GamePath { get; set; }
        private string Scheme { get; set; }
        private string PkgPerfix { get; set; }
        private string GameSource { get; set; }
        private string GameDest { get; set; }
        private string CurrentPath { get; set; }
        private string ReplaceSourceDirectory { get; set; }
        private string RestoreSourceDirectory { get; set; }
        private List<string> GameFileList { get; set; }

        public ConvertService()
        {
            GamePath = App.Current.DataModel.GamePath;
            App.Current.DataModel.Cps = ConfigValue(GamePath, "cps");
            CurrentPath = Environment.CurrentDirectory;
        }

        public string ConfigValue(string URL, string code)
        {
            string iniFilePath = Path.Combine(URL ?? "", "Config.ini");
            if (!File.Exists(iniFilePath))
            {
                if (!Directory.Exists(@"Config"))
                {
                    Directory.CreateDirectory("Config");
                }
                return null;
            }

            try
            {
                using (StreamReader iniFile = new StreamReader(iniFilePath))
                {
                    string strLine;
                    string currentRoot = null;

                    while ((strLine = iniFile.ReadLine()) != null)
                    {
                        strLine = strLine.Trim();
                        if (string.IsNullOrEmpty(strLine)) continue;

                        if (strLine.StartsWith("[") && strLine.EndsWith("]"))
                        {
                            currentRoot = strLine.Substring(1, strLine.Length - 2);
                        }
                        else
                        {
                            string[] keyPair = strLine.Split(new char[] { '=' }, 2);
                            if (keyPair.Length > 0 && keyPair[0] == code)
                            {
                                return keyPair.Length > 1 ? keyPair[1].Trim() : null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 可根据需求进行日志记录或其他处理
                Console.WriteLine($"Error reading config file: {ex.Message}");
            }

            return null;
        }

        public async Task GetFilesNameFromJson(string jsonFilePath)
        {
            try
            {
                if (File.Exists(jsonFilePath))
                {
                    string json = await File.ReadAllTextAsync(jsonFilePath);
                    List<string> gameFileList = JsonSerializer.Deserialize<List<string>>(json);

                    foreach (string file in gameFileList)
                    {
                        // 处理文件路径
                        string temp = file.Replace(Path.Combine(Environment.CurrentDirectory, ReplaceSourceDirectory), "");
                        GameFileList.Add(temp);
                    }

                    // 删除 JSON 文件
                    File.Delete(jsonFilePath);
                }
                else
                {
                    MessageBox.Show("GameFileList.json 文件不存在");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void SetUI(SettingsPageViewModel vm, int Is_Mihoyo)
        {
            switch (Is_Mihoyo)
            {
                case 0:
                    App.Current.NoticeOverAllBase.SwitchPort =
                            $"{App.Current.Language.GameClientStr} : {App.Current.Language.GameClientTypePStr}";
                    App.Current.NoticeOverAllBase.IsGamePortLists = "Visible";
                    App.Current.NoticeOverAllBase.GamePortListIndex = 0;
                    break;
                case 1:
                    App.Current.NoticeOverAllBase.SwitchPort = $"{App.Current.Language.GameClientStr} : {App.Current.Language.GameClientTypeBStr}";
                    App.Current.NoticeOverAllBase.IsGamePortLists = "Visible";
                    App.Current.NoticeOverAllBase.GamePortListIndex = 1;
                    break;
                case 2:
                    App.Current.NoticeOverAllBase.SwitchPort =
                            $"{App.Current.Language.GameClientStr} : {App.Current.Language.GameClientTypeMStr}";
                    App.Current.NoticeOverAllBase.IsGamePortLists = "Hidden";
                    App.Current.NoticeOverAllBase.GamePortListIndex = 2;
                    break;
                default:
                    string temp = ConfigValue(GamePath, "cps");
                    if (!string.IsNullOrEmpty(temp))
                    {
                        App.Current.DataModel.Cps = temp;
                        vm.IsMihoyo = App.Current.DataModel.Cps == "hoyoverse" ? 2 : 0;
                        SetUI(vm, vm.IsMihoyo);
                    }
                    break;
            }
        }

        /// <summary>
        /// 获取所有文件加入到清单
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public async Task GetFilesName(string directory)
        {
            try
            {
                DirectoryInfo directoryInfo = new(directory);
                FileInfo[] files = directoryInfo.GetFiles();
                foreach (FileInfo file in files)
                {
                    string temp = file.FullName.Replace(Path.Combine(Environment.CurrentDirectory, ReplaceSourceDirectory), "");
                    GameFileList.Add(temp);
                }
                DirectoryInfo[] dirs = directoryInfo.GetDirectories();
                if (dirs.Length > 0)
                {
                    foreach (DirectoryInfo dir in dirs)
                    {
                        await GetFilesName(dir.FullName);
                    }
                }

                // 将 GameFileList 序列化为 JSON 并保存到 Config 文件夹中
                string json = JsonSerializer.Serialize(GameFileList);
                string configDirectory = Path.Combine(Environment.CurrentDirectory, "Config");
                if (!Directory.Exists(configDirectory))
                {
                    Directory.CreateDirectory(configDirectory);
                }
                string filePath = Path.Combine(configDirectory, "GameFileList.json");
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 获取当前需要的Pkg前缀
        /// </summary>
        /// <returns></returns>
        /// 
        public string GetCurrentSchemeName()
        {
            return ConfigValue(GamePath, "cps") switch
            {
                "mihoyo1_PC" => CN_DIRECTORY,
                "hoyoverse" => GLOBAL_DIRECTORY,
                "zzz_bilibili_pc" => "zzz_bilibili_pc",
                _ => string.Empty,
            };
        }

        /// <summary>
        /// 检查Pkg版本
        /// </summary>
        /// <param name="scheme"></param>
        /// <param name="vm"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// 

        public async Task<bool> CheckPackageVersionAsync(string scheme, SettingsPageViewModel vm)
        {
            int flag = 0;
            while (string.IsNullOrEmpty(App.Current.PkgUpdataModel?.PkgVersion))
            {
                flag++;
                if (App.Current?.PkgUpdataModel != null)
                {
                    App.Current.PkgUpdataModel.PkgVersion = await HtmlHelper.GetPkgVersionAsync();
                }
                else
                {
                    // 初始化 PkgUpdataModel 或处理 null 情况
                    App.Current.PkgUpdataModel = new PkgUpdataModel();
                    App.Current.PkgUpdataModel.PkgVersion = await HtmlHelper.GetPkgVersionAsync();
                }

                if (!string.IsNullOrEmpty(App.Current.PkgUpdataModel.PkgVersion))
                {
                    break;
                }

                if (flag >=10)
                {
                    vm.ConvertingLog = $"获取PKG版本号失败，请检查你的网络设置。";
                    return false;
                }

                vm.ConvertingLog = $"获取PKG版本号失败，尝试重新获取{flag}";
                await Task.Delay(1000);
            }
            string gameversion = ConfigValue(GamePath, "game_version");
            string pkgversion = ConfigValue(scheme, "game_version");
            if (gameversion != App.Current.PkgUpdataModel.PkgVersion)
            {
                vm.ConvertingLog = $"当前游戏版本过低，请前往米哈游启动器更新游戏。\r\n当前游戏版本号：{gameversion}\r\n当前从API获取的游戏版本号：{App.Current.PkgUpdataModel.PkgVersion}\r\n";
                return false;
            }
            if (pkgversion != App.Current.PkgUpdataModel.PkgVersion)
            {
                vm.ConvertingLog = $"{App.Current.Language.NewPkgVer} : [{pkgversion}]\r\n即将下载最新版本转换包。\r\n请将下载好的转换包移动至本软件软件目录下。";
                await Task.Delay(1000);
                if (scheme == CN_DIRECTORY)
                {
                    ProcessStartInfo info = new()
                    {
                        FileName = "https://download.ganyu.us.kg/now/ZenlessZoneZero/CnFile.pkg",
                        UseShellExecute = true,
                    };
                    Process.Start(info);
                }
                else
                {
                    ProcessStartInfo info = new()
                    {
                        FileName = "https://download.ganyu.us.kg/now/ZenlessZoneZero/GlobalFile.pkg",
                        UseShellExecute = true,
                    };
                    Process.Start(info);
                }
                return false;
            }
            return true;
        }



        /// <summary>
        /// 异步转换游戏任务
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task ConvertGameFileAsync(SettingsPageViewModel vm)
        {
            GameFileList = new List<string>();
            Scheme = GetCurrentSchemeName();
            if(Scheme == "zzz_bilibili_pc")
            {
                App.Current.DataModel.Cps = "mihoyo1_PC";
                App.Current.DataModel.Channel = 1;
                App.Current.DataModel.Sub_channel = 1;
                if (File.Exists(Path.Combine(GamePath, $"ZenlessZoneZero_Data/Plugins/x86_64/PCGameSDK.dll")))
                    File.Delete(Path.Combine(GamePath, $"ZenlessZoneZero_Data/Plugins/x86_64/PCGameSDK.dll"));
                App.Current.NoticeOverAllBase.SwitchPort =
                    $"{App.Current.Language.GameClientStr} : {App.Current.Language.GameClientTypePStr}";
                App.Current.NoticeOverAllBase.IsGamePortLists = "Visible";
                App.Current.NoticeOverAllBase.GamePortListIndex = 0;
                App.Current.DataModel.SaveDataToFile();
                Scheme = CN_DIRECTORY;
            }
            PkgPerfix = Scheme == CN_DIRECTORY ? GLOBAL_DIRECTORY : CN_DIRECTORY;
            ReplaceSourceDirectory = Scheme == CN_DIRECTORY ? GLOBAL_DIRECTORY : CN_DIRECTORY;
            RestoreSourceDirectory = Scheme == CN_DIRECTORY ? CN_DIRECTORY : GLOBAL_DIRECTORY;

            await Task.Run(async () =>
            {
                if (File.Exists(Path.Combine(GamePath, $"{ZenlessZoneZero_EXE}.cn.bak")) ||
                    File.Exists(Path.Combine(GamePath, $"{ZenlessZoneZero_EXE}.global.bak")))
                {
                    //直接从 bak 还原
                    vm.StateIndicator = "正在获取备份清单";
                    string jsonFilePath = Path.Combine(CurrentPath, "Config", "GameFileList.json");
                    if (File.Exists(jsonFilePath))
                    {
                        await GetFilesNameFromJson(jsonFilePath);
                        vm.ConvertingLog += $"正在还原客户端\r\n";
                        await RestoreGameFiles(vm);
                    }
                    else
                    {
                        vm.ConvertingLog += $"转换Pkg副本已丢失，无法还原\r\n";
                        vm.ConvertState = false;
                    }
                }
                else if (Directory.Exists(Path.Combine(CurrentPath, PkgPerfix)))
                {
                    //直接从 pkg解压后的目录 处替换
                    if (await CheckPackageVersionAsync(ReplaceSourceDirectory, vm))
                    {
                        vm.StateIndicator = "正在获取文件清单";
                        await GetFilesName(Path.Combine(CurrentPath, ReplaceSourceDirectory));
                        await ReplaceGameFiles(vm);
                        return;
                    }
                    Directory.Delete($"{CurrentPath}/{ReplaceSourceDirectory}", true);
                    vm.ConvertState = false;
                    return;
                }
                else if (File.Exists(Path.Combine(CurrentPath, $"{PkgPerfix}.pkg")))
                {
                    vm.StateIndicator = "开始解压Pkg文件";
                    //解压 pkg 文件
                    if (Decompress(PkgPerfix))
                    {
                        if(await CheckPackageVersionAsync(ReplaceSourceDirectory, vm))
                        {
                            //直接从 pkg解压后的目录 处替换
                            vm.StateIndicator = "正在获取文件清单";
                            await GetFilesName(Path.Combine(CurrentPath, ReplaceSourceDirectory));
                            await ReplaceGameFiles(vm);
                            return;
                        }
                        Directory.Delete($"{CurrentPath}/{ReplaceSourceDirectory}", true);
                        vm.ConvertState = false;
                        return;
                    }
                    else
                    {
                        vm.ConvertingLog += $"{PkgPerfix}.pkg 文件解压失败\r\n";
                        vm.ConvertState = false;
                    }
                }
                else
                {
                    vm.ConvertingLog += $"{PkgPerfix}.pkg 文件不存在\r\n";
                    vm.ConvertState = false;
                }

                vm.StateIndicator = "无状态";
            });
        }

        /// <summary>
        /// 替换游戏文件逻辑
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        public async Task ReplaceGameFiles(SettingsPageViewModel vm)
        {
            vm.ConvertingLog += "开始备份文件\r\n";
            await BackupGameFile(vm);

            vm.StateIndicator = "开始替换客户端";
            vm.ConvertingLog += "释放Pkg文件至游戏目录\r\n";

            string basePkgPath = Path.Combine(Environment.CurrentDirectory, ReplaceSourceDirectory);
            foreach (string file in GameFileList)
            {
                string relativePath = file.Replace($@"\{ZenlessZoneZero_DATA}", ZenlessZoneZero_DATA);
                string gameFilePath = relativePath.Insert(0, $@"{GamePath}\");
                string pkgFilePath = relativePath.Insert(0, $@"{basePkgPath}\");

                if (File.Exists(pkgFilePath))
                {
                    try
                    {
                        File.Copy(pkgFilePath, gameFilePath, true);
                        vm.ConvertingLog += $"{pkgFilePath} 替换成功\r\n";
                    }
                    catch (Exception ex)
                    {
                        vm.ConvertingLog += $"警告：替换 {pkgFilePath} 时出现错误: {ex.Message}\r\n";
                    }
                }
                else
                {
                    vm.ConvertingLog += $"{gameFilePath} 替换失败，文件缺失\r\n";
                }
            }

            App.Current.DataModel.Cps = Scheme == CN_DIRECTORY ? "hoyoverse" : "mihoyo1_PC"; 
            vm.IsMihoyo = App.Current.DataModel.Cps == "hoyoverse" ? 2 : 0;
            MessageBox.Show($"Cps:{App.Current.DataModel.Cps}，Is:{vm.IsMihoyo}");
            SetUI(vm, vm.IsMihoyo);
            vm.ConvertingLog += $"所有文件替换完成，尽情享受吧...\r\n";
            vm.ConvertState = true;
        }



        /// <summary>
        /// 还原游戏文件
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        public async Task RestoreGameFiles(SettingsPageViewModel vm)
        {
            vm.StateIndicator = "开始还原文件";
            string suffix = GetCurrentSchemeName() == GLOBAL_DIRECTORY ? "cn" : "global";

            vm.ConvertingLog += "开始还原文件\r\n";
            foreach (string file in GameFileList)
            {
                string relativePath = file.Replace(Path.Combine(CurrentPath, RestoreSourceDirectory), "");
                string filePath = relativePath.Insert(0, $@"{GamePath}");

                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                        string backupFilePath = $"{filePath}.{suffix}.bak";
                        if (File.Exists(backupFilePath))
                        {
                            File.Move(backupFilePath, filePath);
                            vm.ConvertingLog += $"{filePath} 还原成功\r\n";
                        }
                        else
                        {
                            vm.ConvertingLog += $"{backupFilePath} 不存在，无法还原\r\n";
                        }
                    }
                    catch (Exception ex)
                    {
                        vm.ConvertingLog += $"警告：还原 {filePath} 时出现错误: {ex.Message}\r\n";
                    }
                }
                else
                {
                    vm.ConvertingLog += $"{filePath} 还原失败，文件不存在\r\n";
                }
            }

            App.Current.DataModel.Cps = Scheme == CN_DIRECTORY ? "hoyoverse" : "mihoyo1_PC"; 
            vm.IsMihoyo = App.Current.DataModel.Cps == "hoyoverse" ? 2 : 0;
            SetUI(vm, vm.IsMihoyo);
            MessageBox.Show($"Cps:{App.Current.DataModel.Cps}，Is:{vm.IsMihoyo}");
            vm.ConvertingLog += $"所有文件还原完成，尽情享受吧...\r\n";
            vm.ConvertState = true;
        }


        /// <summary>
        /// 备份原来的游戏文件
        /// </summary>
        /// <returns></returns>
        public async Task BackupGameFile(SettingsPageViewModel vm)
        {
            vm.StateIndicator = "开始备份文件";
            string suffix = GetCurrentSchemeName() == GLOBAL_DIRECTORY ? "global" : "cn";

            foreach (string file in GameFileList)
            {
                string relativePath = file.Replace($@"\{ZenlessZoneZero_DATA}", ZenlessZoneZero_DATA);
                string filePath = relativePath.Insert(0, $@"{GamePath}\");
                string backupFilePath = $"{filePath}.{suffix}.bak";

                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Move(filePath, backupFilePath);
                        vm.ConvertingLog += $"{filePath} 备份成功 -> {backupFilePath}\r\n";
                    }
                    catch (Exception ex)
                    {
                        vm.ConvertingLog += $"警告：备份 {filePath} 时出现错误: {ex.Message}\r\n";
                    }
                }
                else
                {
                    vm.ConvertingLog += $"{filePath} 备份失败，文件不存在\r\n";
                }
            }

            vm.ConvertingLog += $"原游戏本体路径: {Path.Combine(GamePath, ZenlessZoneZero_EXE)}\r\n";
            vm.ConvertingLog += $"备份游戏本体路径: {Path.Combine(GamePath, $"{ZenlessZoneZero_EXE}.{suffix}.bak")}\r\n";
        }



        /// <summary>
        /// 解压Pkg文件
        /// </summary>
        /// <param name="archiveName"></param>
        /// <returns></returns>
        private bool Decompress(string archiveName)
        {
            try
            {
                ZipFile.ExtractToDirectory($"{CurrentPath}/{archiveName}.pkg", CurrentPath, true);
                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// 保存游戏设置
        /// </summary>
        /// <param name="vm"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void SaveGameConfig(SettingsPageViewModel vm)
        {
            if (File.Exists(Path.Combine(App.Current.DataModel.GamePath, "config.ini")))
            {
                string bilibilisdk = "Plugins/x86_64/PCGameSDK.dll";
                switch (vm.IsMihoyo)
                {
                    case 0:
                        App.Current.DataModel.Cps = "mihoyo1_PC";
                        App.Current.DataModel.Channel = 1;
                        App.Current.DataModel.Sub_channel = 1;
                        if (File.Exists(Path.Combine(GamePath, $"ZenlessZoneZero_Data/{bilibilisdk}")))
                            File.Delete(Path.Combine(GamePath, $"ZenlessZoneZero_Data/{bilibilisdk}"));
                        App.Current.NoticeOverAllBase.SwitchPort =
                            $"{App.Current.Language.GameClientStr} : {App.Current.Language.GameClientTypePStr}";
                        App.Current.NoticeOverAllBase.IsGamePortLists = "Visible";
                        App.Current.NoticeOverAllBase.GamePortListIndex = 0;
                        break;
                    // todo bili
                    case 1:
                        App.Current.DataModel.Cps = "zzz_bilibili_pc";
                        App.Current.DataModel.Channel = 14;
                        App.Current.DataModel.Sub_channel = 0;
                        if (!File.Exists(Path.Combine(GamePath, $"ZenlessZoneZero_Data/{bilibilisdk}")))
                        {
                            try
                            {
                                string sdkPath = Path.Combine(GamePath, $"ZenlessZoneZero_Data/{bilibilisdk}");
                                FileHelper.ExtractEmbededAppResource("StaticRes/PCGameSDK.dll", sdkPath);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        }
                        App.Current.NoticeOverAllBase.SwitchPort = $"{App.Current.Language.GameClientStr} : {App.Current.Language.GameClientTypeBStr}";
                        App.Current.NoticeOverAllBase.IsGamePortLists = "Visible";
                        App.Current.NoticeOverAllBase.GamePortListIndex = 1;

                        break;
                    case 2:
                        App.Current.DataModel.Cps = "hoyoverse";
                        App.Current.DataModel.Channel = 1;
                        App.Current.DataModel.Sub_channel = 1;
                        if (File.Exists(Path.Combine(GamePath, $"GenshinImpact_Data/{bilibilisdk}")))
                            File.Delete(Path.Combine(GamePath, $"GenshinImpact_Data/{bilibilisdk}"));
                        App.Current.NoticeOverAllBase.SwitchPort =
                            $"{App.Current.Language.GameClientStr} : {App.Current.Language.GameClientTypeMStr}";
                        App.Current.NoticeOverAllBase.IsGamePortLists = "Hidden";
                        App.Current.NoticeOverAllBase.GamePortListIndex = 2;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}