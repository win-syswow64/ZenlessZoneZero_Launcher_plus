using ZenlessZoneZero_Launcher_plus.Models;
using ZenlessZoneZero_Launcher_plus.Service.IService;
using System.Collections.Generic;
using System.IO;

namespace ZenlessZoneZero_Launcher_plus.Service
{
    public class UserDataService : IUserDataService
    {
        /// <summary>
        /// 读取用户数据列表
        /// </summary>
        /// <returns></returns>
        public List<UserListModel> ReadUserList()
        {
            List<UserListModel> list = new();
            DirectoryInfo TheFolder = new(@"UserData");
            foreach (FileInfo NextFile in TheFolder.GetFiles())
            {
                list.Add(new UserListModel { UserName = NextFile.Name });
            }
            return list;
        }
    }
}