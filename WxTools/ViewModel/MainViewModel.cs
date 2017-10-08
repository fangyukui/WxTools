using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using log4net;
using WxTools.Annotations;
using WxTools.Client.Helper;
using WxTools.Client.Model;
using WxTools.Theme;

namespace WxTools.Client.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region 字段

        public static MainViewModel Instance { get; } = new MainViewModel();

        public ObservableCollection<OperaDal> Operas
        {
            get => _operas;
            set
            {
                if (Equals(value, _operas)) return;
                _operas = value;
                OnPropertyChanged();
            }
        }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (value == _isChecked) return;
                _isChecked = value;
                OnPropertyChanged();
            }
        }

        private readonly ILog _log = LogManager.GetLogger(typeof(MainViewModel));

        private ObservableCollection<OperaDal> _operas;

        private bool _isChecked;

        private Thread _thread;

        private DateTime _lasTime;

        public TcpDal TcpDal { get; } = new TcpDal();

        #endregion

        #region 构造函数

        public MainViewModel()
        {
            Operas = new ObservableCollection<OperaDal>();
            RegisterMessenger();
            CheckUpdate();
            TcpDal.Connect();
        }

        #endregion

        #region 命令

        /// <summary>
        /// 窗体关闭事件
        /// </summary>
        public RelayCommand ClosedCommand { get; } = new RelayCommand(() =>
        {
            Instance.IsChecked = false;
            foreach (var opera in Instance.Operas)
            {
                opera.StopThread();
                opera.Dispose();
            }
            Instance.TcpDal.Dispose();
            Instance.Operas.Clear();
            LwFactory.Clear();
        });

        public RelayCommand OpenWeixinCommand { get; } = new RelayCommand(() =>
        {
            var path = @"C:\Program Files (x86)\Tencent\WeChat\WeChat.exe";
            if (File.Exists(path))
            {
                try
                {
                    var strCmd = new StringBuilder("cd \"C:\\Program Files (x86)\\Tencent\\WeChat\"&start ");
                    for (int i = 0; i < 5; i++)
                    {
                        strCmd.Append("WeChat.exe&");
                    }
                    Process myProcess = new Process();
                    ProcessStartInfo myProcessStartInfo = new ProcessStartInfo("cmd.exe")
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true
                    };
                    myProcess.StartInfo = myProcessStartInfo;
                    myProcessStartInfo.Arguments = "/c " + strCmd.ToString().TrimEnd('&');
                    myProcess.Start();
                    myProcess.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        });

        public RelayCommand RunCommand { get; } = new RelayCommand(() =>
        {
            Instance.Run();
        });

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region 方法

        //注册消息监听
        private void RegisterMessenger()
        {
            Client.Common.Messenger.Register("CefWebViewWnd", () =>
            {
                _lasTime = DateTime.Now;
                //Interlocked.Increment(ref Common.SessionCount);
                Client.Common.SessionCount += 1;

                if (Client.Common.SessionCount >= Client.Common.MaxSessionCount || Client.Common.SessionCount == Operas.Count)
                {
                    _log.Info($"清理一次窗口; SessionCount:{Client.Common.SessionCount};" +
                              $" Operas.Count:{Operas.Count}; MaxSessionCount:{Client.Common.MaxSessionCount};");
                    //超过窗口数 或者 正好
                    Thread.Sleep(4000);
                    Client.Common.SessionCount = 0;
                    CloseCefWebViewWnd();
                }
            });
        }

        //关闭CefWebViewWnd窗口
        private void CloseCefWebViewWnd()
        {
            var hwndstr = LwFactory.GetDefault().EnumWindow("微信", "CefWebViewWnd", null);
            if (hwndstr != null)
            {
                var hwnds = hwndstr.Split(',');
                foreach (var hwnd in hwnds)
                {
                    WinApi.SendMessage(new IntPtr(int.Parse(hwnd)), 0x0010, 0, 0);
                }
                Console.WriteLine("All完成，全部关闭");
            }
        }

        //监测窗体，变量状态,自动更新 线程
        private void CheckHwndStateThread()
        {
            if (_thread != null) return;
            _thread = new Thread(() =>
            {
                _log.Info("监测线程启动成功");
                Thread.Sleep(8000);
                var list = new List<OperaDal>();
                while (true)
                {
                    Thread.Sleep(1000);
                    if (IsChecked)
                    {
                        foreach (var opera in Operas)
                        {
                            if (opera.Lw.GetWindowState(opera.Hwnd, 0) == 0)
                            {
                                //窗体不存在
                                list.Add(opera);
                            }
                            else
                            {
                                if (opera.Lw.GetWindowState(opera.Hwnd, 3) == 1)
                                {
                                    //窗体最小化了
                                    _log.Warn($"微信被窗体最小化了，已经恢复，hwnd={opera.Hwnd}");
                                    opera.Lw.SetWindowState(opera.Hwnd, 7);
                                }
                                if (opera.Lw.GetClientSize(opera.Hwnd) == 1)
                                {
                                    if (opera.Lw.X() != Client.Common.Width || opera.Lw.Y() != Client.Common.Height)
                                    {
                                        opera.Lw.SetWindowSize(opera.Hwnd, Client.Common.Width, Client.Common.Height);
                                        _log.Warn("微信窗体大小被用户拖动，已经恢复");
                                    }
                                }
                            }
                        }
                        foreach (var dal in list)
                        {
                            _log.Warn($"微信窗口出错了，hwnd={dal.Hwnd}");
                            dal.Dispose();
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Operas.Remove(dal);
                            });
                        }

                        //防止文章窗口打开过久
                        if (Client.Common.SessionCount > 0 && (DateTime.Now - _lasTime).Seconds > 30)
                        {
                            _log.Warn($"文章窗口打开过久，已经关闭");
                            Client.Common.SessionCount = 0;
                            CloseCefWebViewWnd();
                        }
                    }
                }
            })
            {
                IsBackground = true,
                Name = "检测状态改变线程"
            };
            _thread.Start();
        }

        //检测程序更新
        private void CheckUpdate()
        {
            new Thread(() =>
            {
                while (true)
                {
                    if (Operas != null && Operas.All(o => o.RunState == RunState.Idle))
                    {
                        Client.Common.StartUpdate();
                    }
                    Thread.Sleep(60000);
                }
            }) {IsBackground = true}.Start();
        }

        //执行线程启动
        private void Run()
        {
            if (IsChecked)
            {
                var lw = LwFactory.GetDefault();
                var hwndstr = lw.EnumWindow(null, "WeChatMainWndForPC", null);
                var hwnds = hwndstr?.Split(',');
                if (hwnds == null || hwnds.Length == 0)
                {
                    IsChecked = false;
                    _log.Info("未找到微信(先运行微信)");
                    MessageBox.Show("未找到微信(先运行微信)", "提示");
                    return;
                }
                //默认添加一个
                if (Operas.Count == 0)
                    Operas.Add(new OperaDal(LwFactory.GetDefault()));
                if (hwnds.Length != Operas.Count)
                {
                    for (int i = 0; i < hwnds.Length - 1; i++)
                    {
                        Operas.Add(new OperaDal(LwFactory.GetNew()));
                    }
                }
                var j = 0;
                foreach (var opera in Operas)
                {
                    var mythread = new Thread(() =>
                    {
                        try
                        {
                            opera.Load(int.Parse(hwnds[j++]));
                            opera.Lw.SetWindowState(opera.Hwnd, 1);
                            if (!opera.RunThread())
                            {
                                _log.Error($"{opera.Hwnd}启动线程出现错误，被跳过");
                            }
                        }
                        catch (Exception e)
                        {
                            _log.Error($"{opera.Hwnd}加载出错", e);
                            MessageBox.Show($"{opera.Hwnd}加载出错", "提示");
                        }
                    });
                    mythread.SetApartmentState(ApartmentState.STA);
                    mythread.Start();
                }
                //开启状态监测线程
                CheckHwndStateThread();
                _log.Info("启动完成");
            }
            else
            {
                foreach (var opera in Operas)
                {
                    opera.StopThread();
                    opera.Dispose();
                }
                Operas.Clear();
                LwFactory.Clear();
                GC.Collect();
                _log.Info("关闭完成");
            }
        }

        public void ExecuteUrl(string url)
        {
            foreach (var opera in Operas)
            {
                opera.GoOnceAction(() => opera.SendMyMessage(url));
            }
        }

        #endregion
    }
}
