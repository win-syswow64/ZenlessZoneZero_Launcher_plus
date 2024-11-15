﻿namespace ZenlessZoneZero_Launcher_plus.Models
{
    public class UpdateModel
    {
        public string Version { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string DownloadUrl { get; set; }
        public string GlobalDownloadUrl { get; set; }
        public string PkgVersion { get; set; }
        public bool RequisiteUpdate { get; set; }

    }

    public class PkgUpdataModel
    {
        public string PkgVersion { get; set; }
    }

    public class BackgroundModel
    {
        public string BackgroundUrl { get; set; }
    }
    public class NoticeModel
    {
        public int Code { get; set; }
        public string NoticeMsg { get; set; }
    }
}
