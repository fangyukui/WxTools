using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using log4net;
using OAUS.Core;
using WxTools.Client.Model;
using WxTools.Client.ViewModel;
using WxTools.Theme;

namespace WxTools.Client
{
    public class Common
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Common));

        //消息通知
        public static Messenger Messenger = new Messenger();

        //微信的窗体大小
        public static int Width = 888;

        public static int Height = 625;

        public static bool? HasNewVersion()
        {
            try
            {
                return VersionHelper.HasNewVersion(MainViewModel.Instance.TcpIp, 4540);
            }
            catch (Exception e)
            {
                Log.Error("HasNewVersion", e);
                return null;
            }
        }

        public static async Task StartUpdate()
        {
            await Task.Run(async() =>
            {
                var has = HasNewVersion();
                if (has == true)
                {
                    Log.Info("进入自动更新");
                    //这个延时可以防止TCP连接网络出错
                    await Task.Delay(3000);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        string updateExePath = AppDomain.CurrentDomain.BaseDirectory + "AutoUpdater\\AutoUpdater.exe";
                        System.Diagnostics.Process.Start(updateExePath);
                        System.Windows.Application.Current.Shutdown();
                    });
                }
            });
        }
    }
}
