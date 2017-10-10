using System;
using System.Threading.Tasks;
using System.Windows;
using log4net;
using OAUS.Core;
using WxTools.Theme;

namespace WxTools.Client
{
    public class Common
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Common));

        //消息通知
        public static Messenger Messenger = new Messenger();

        //微信的窗体大小，统一
        public static int Width = 888;

        public static int Height = 625;

        //当前打开的文章窗口个数
        public static int SessionCount;

        //最大支持同时打开{MaxSessionCount}个文章窗口
        public static int MaxSessionCount = 10;

        public static int MaxThreadCount = 20;

        //public static string TcpIp = "127.0.0.1";
        public static string TcpIp = "49.4.133.41";

        public static bool? HasNewVersion()
        {
            try
            {
                return VersionHelper.HasNewVersion("121.201.110.147", 4540);
            }
            catch (Exception e)
            {
                Log.Error("HasNewVersion", e);
                return null;
            }
        }

        public static void StartUpdate()
        {
            Task.Factory.StartNew(() =>
            {
                var has = HasNewVersion();
                if (has == true)
                {
                    Log.Info("进入自动更新");
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
