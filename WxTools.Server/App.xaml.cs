using System;
using System.IO;
using System.Reflection;
using System.Windows;
using log4net;
using log4net.Config;

namespace WxTools.Server
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(App));

        public App()
        {
            Startup += Application_Startup;
            InitLog4Net();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.Current.DispatcherUnhandledException += Application_DispatcherUnhandledException;
        }

        private static void InitLog4Net()
        {
            var appName = Path.GetFileName(Assembly.GetEntryAssembly().GetName().Name);
            var logCfg = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + appName + ".exe.config");
            XmlConfigurator.ConfigureAndWatch(logCfg);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _log.Error(e.Exception);
            MessageBox.Show("我们很抱歉，当前应用程序遇到一些问题.." + e.Exception,
                "意外的操作", MessageBoxButton.OK, MessageBoxImage.Information);//这里通常需要给用户一些较为友好的提示，并且后续可能的操作
            e.Handled = true;//使用这一行代码告诉运行时，该异常被处理了，不再作为UnhandledException抛出了。
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _log.Error(e.ExceptionObject);
            MessageBox.Show("我们很抱歉，当前应用程序遇到一些问题.请联系管理员." + e.ExceptionObject,
                "意外的操作", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
