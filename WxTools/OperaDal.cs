using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LwSoft;
using LwSoft.Enums;
using WxTools.Annotations;
using WxTools.Helper;
using WxTools.Model;

namespace WxTools
{
    public class OperaDal : INotifyPropertyChanged, IDisposable
    {
        public readonly Lwsoft3 Lw;
        //主窗口
        public int Hwnd;
        //绑定状态
        public bool Bindinged { get; set; }
        //线程启动
        public bool ThreadRun { get; set; }
        //日志
        public string Logs
        {
            get => _logs;
            set
            {
                if (value == _logs) return;
                _logs = value;
                OnPropertyChanged();
            }
        }

        //模拟器名字
        public string Name
        {
            get => _name + $"({Hwnd})";
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        private CancellationTokenSource _source;
        private string _logs;
        private string _name;

        public OperaDal(Lwsoft3 lw)
        {
            Lw = lw;
        }

        #region 初始化

        public void Load(int hwnd)
        {
            LoadWindowsHandle(hwnd);
            Bindinged = true;
        }

        /// <summary>
        /// 初始化窗体信息
        /// </summary>
        private void LoadWindowsHandle(int hwnd)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "imgs");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            Lw.SetPath(path);
            SetWindowsHandle(hwnd);
            Log("初始化窗体成功");
            //绑定
            Lw.BindWindow(this.Hwnd, DisplayBindKey.Gdi, MouseBindKey.Windows, KeypadBindKey.Windows, 1);
            //设置窗体大小
            Lw.SetWindowSize(this.Hwnd, 888, 625);
            //获取窗体大小
            Common.WinRect = Lw.GetWindowInfo(this.Hwnd).WindowRect;
            Log("绑定成功");
            //关闭错误消息
            //Lw.SetShowErrorMsg(0);
        }

        public void SetWindowsHandle(int parent)
        {
            Hwnd = parent;
            Name = Lw.GetWindowTitle(Hwnd);
        }

        #endregion

        #region 移动方法
        /// <summary>
        /// 向上移动一行
        /// </summary>
        public void MoveUpOneRow()
        {
            Lw.MoveTo(WxPoints.FirstRow);
            for (int i = 0; i < 2; i++)
                Lw.WheelUp();
        }

        /// <summary>
        /// 快速向上移动
        /// </summary>
        public void MoveUpQuick()
        {
            Lw.MoveTo(WxPoints.FirstRow);
            for (int i = 0; i < 200; i++)
                Lw.Delay(5).WheelUp();
        }

        /// <summary>
        /// 向下移动一行
        /// </summary>
        public void MoveDownOneRow()
        {
            Lw.MoveTo(WxPoints.FirstRow);
            for (int i = 0; i < 2; i++)
                Lw.WheelDown();
        }

        /// <summary>
        /// 向下快速移动
        /// </summary>
        public void MoveDownQuick()
        {
            Lw.MoveTo(WxPoints.FirstRow);
            for (int i = 0; i < 200; i++)
                Lw.Delay(5).WheelDown();
        }

        #endregion

        /// <summary>
        /// 解除绑定
        /// </summary>
        public void UnBindWindow()
        {
            Lw.UnBindWindow();
            Log("解除绑定");
        }

        /// <summary>
        /// 进入微信聊天第一行
        /// </summary>
        public void ClickFirstRow()
        {
            Lw.ClickOnce(WxPoints.WeiXin)
                .Delay()
                .ClickOnce(WxPoints.FirstRow);
        }

        /// <summary>
        /// 截图
        /// </summary>
        /// <param name="fileName"></param>
        public void Capture(string fileName = null)
        {
            Lw.Capture(0, 0, (int)Common.WinRect.Width, (int)Common.WinRect.Height,
                fileName ?? DateTime.Now.Ticks + ".bmp");
        }

        /// <summary>
        /// 找图并点击
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isClick"></param>
        /// <param name="dir"></param>
        public void FindPic(string name, bool isClick = true, int dir = 0)
        {
            Lw.FindPic(0, 0, (int) Common.WinRect.Width, (int) Common.WinRect.Height - 50,
                name, "000000", 0.8, dir, 0, isClick ? 1 : 0);
        }

        /// <summary>
        /// 启动消息检测线程
        /// </summary>
        public bool CheckNewMessage()
        {
            if (ThreadRun)
            {
                ThreadRun = false;
                _source.Cancel();
                return false;
            }

            if (Lw.IsBind(Hwnd) <= 0)
            {
                ThreadRun = false;
                Log("未绑定！退出");
                return false;
            }

            //恢复到准备状态 聊天首页
            void RecoverAction(int sleep = 1000)
            {
                Thread.Sleep(sleep);
                Lw.ClickOnce(WxPoints.WeiXin)
                    .ClickOnce(WxPoints.EndRow)
                    .Delay()
                    .ClickOnce(WxPoints.WeiXin);
                MoveUpQuick();
            }

            //会话窗口过多，等待处理
            void WaitSeesion()
            {
                if (Common.SessionCount > 0)
                    while (Common.SessionCount >= Common.MaxSessionCount)
                    {
                        //超过窗口数，等待处理
                        Thread.Sleep(1000);
                    }
            }

            ThreadRun = true;
            _source = new CancellationTokenSource();
            Task.Factory.StartNew(() =>
            {
                //进入聊天首页
                RecoverAction();

                var width = (int)Common.WinRect.Width;
                var height = (int)Common.WinRect.Height;
                while (!_source.IsCancellationRequested)
                {
                    //判断新的消息
                    if (Lw.FindPic(70, 50, 160, height, "new.bmp", "000000", 0.85, 0, 0, 1))
                    {
                        Log("发现新消息");
                        //找到会话消息
                        Thread.Sleep(2000);
                        if (Lw.FindPic(400, 80, 600, height - 165, "dh.bmp", "000000", 0.95, 1, 0, 1, 100, 13))
                        {
                            //Console.WriteLine($"{_lw.X()},{_lw.Y()}");
                            Task.Factory.StartNew(() =>
                            {
                                Thread.Sleep(2000);
                                var hwnd = Lw.FindWindow("图片查看器", "ImagePreviewWnd", null);
                                if (hwnd > 0)
                                {
                                    WinApi.SendMessage(new IntPtr(hwnd), 0x0010, 0, 0);
                                }
                                else
                                {
                                    WaitSeesion();
                                    Common.Messenger.Notify("CefWebViewWnd");
                                }
                            });
                        }
                        RecoverAction(2000);
                        WaitSeesion();
                    }
                    Thread.Sleep(1000);
                }
            }, _source.Token);

            return true;
        }

        public void StopCheckNewMessage()
        {
            ThreadRun = false;
            _source?.Cancel();
        }

        public void Dispose()
        {
            ThreadRun = false;
            _source?.Cancel();
            UnBindWindow();
        }

        private void Log(string log)
        {
            Logs += $"{DateTime.Now:HH:mm:ss}: {log}\r\n";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
