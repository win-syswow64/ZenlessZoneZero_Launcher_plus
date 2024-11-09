using ZenlessZoneZero_Launcher_plus.Models;
using System.Collections.Generic;

namespace ZenlessZoneZero_Launcher_plus.Service.IService
{
    public interface IUserDataService
    {
        /// <summary>
        /// 读取用户数据文件到List
        /// </summary>
        /// <returns></returns>
        List<UserListModel> ReadUserList();
    }
}
