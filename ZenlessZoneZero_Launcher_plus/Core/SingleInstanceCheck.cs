using System.Windows;

namespace ZenlessZoneZero_Launcher_plus.Core
{
    internal class SingleInstanceCheck
    {
        private readonly string _appName;
        private EventWaitHandle? _eventWaitHandle;

        /// <summary>
        /// 构造新的检测器实例
        /// </summary>
        /// <param name="appName">App的唯一名称标识符</param>
        public SingleInstanceCheck(string appName)
        {
            this._appName = appName;
        }

        /// <summary>
        /// 指示是否由于单例限制而退出
        /// </summary>
        public bool IsExitDueToSingleInstanceRestriction { get; set; }

        /// <summary>
        /// 指示是否在进行验证
        /// </summary>
        public bool IsEnsureingSingleInstance { get; set; }

        /// <summary>
        /// 确保应用程序是否为第一个打开
        /// </summary>
        /// <param name="app"></param>
        public void Ensure(Application app, Action multiInstancePresentAction)
        {
            try
            {
                IsEnsureingSingleInstance = true;
                _eventWaitHandle = EventWaitHandle.OpenExisting(_appName);
                _eventWaitHandle.Set();
                IsExitDueToSingleInstanceRestriction = true;
                app.Shutdown();
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, _appName);
            }
            finally
            {
                IsEnsureingSingleInstance = false;
            }
            new Task(() =>
            {
                while (_eventWaitHandle.WaitOne())
                {
                    App.Current.Dispatcher.Invoke(multiInstancePresentAction);
                }
            }).Start();
        }
    }
}
