using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using log4net;
using LwSoft;
using LwSoft.Enums;
using WxTools.Annotations;
using WxTools.Client.Helper;
using WxTools.Client.Model;

namespace WxTools.Client
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

        public RunState RunState { get; private set; } = RunState.Idle;

        private CancellationTokenSource _source;
        private string _logs;
        private string _name;

        private readonly ILog _log = LogManager.GetLogger(typeof(OperaDal));

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
            Lw.BindWindow(this.Hwnd, DisplayBindKey.Gdi, MouseBindKey.Windows,
                KeypadBindKey.Windows, 32);
            //设置窗体大小
            Lw.SetWindowSize(this.Hwnd, Client.Common.Width, Client.Common.Height);
            Log("绑定成功");
            //关闭错误消息
            Lw.SetShowErrorMsg(0);
            Lw.SetShowErrorMsg(0);
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
            Lw.Capture(0, 0, Client.Common.Width, Client.Common.Height,
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
            Lw.FindPic(0, 0, Client.Common.Width, Client.Common.Height - 50,
                name, "000000", 0.8, dir, 0, isClick ? 1 : 0);
        }

        /// <summary>
        /// 启动消息检测线程
        /// </summary>
        public bool RunThread()
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

            ThreadRun = true;
            _source = new CancellationTokenSource();
            Task.Factory.StartNew(() =>
            {
                //进入聊天首页
                RecoverAction();
                while (!_source.IsCancellationRequested)
                {
                    WaitSeesion();
                    //判断新的消息
                    CheckNewMessage();
                    Thread.Sleep(1000);
                }
                //SendMyMessage("https://mp.weixin.qq.com/s?src=11&timestamp=1507368444&ver=438&signature=82XVDEO4Ms7JSHg6-iPZuLdSG6NhGCiYzmzZSTBn9TLYd-RXmYK8gV9wRcgq9feHnm1fXs5zB1KZnHxzOuoL*ORFxwngLoLo9zWVRAXVOjuwrARnlkFvOcj2cFFcN7qS&new=1");
            }, _source.Token);

            return true;
        }

        private void CheckNewMessage()
        {
            //判断新的消息
            if (Lw.FindPic(70, 50, 160, Client.Common.Height, "new.bmp", "000000", 0.85, 0, 0, 1))
            {
                RunState = RunState.Busy;
                Log("发现新消息");
                //找到会话消息
                Thread.Sleep(2000);
                if (Lw.FindPic(400, 80, 600, Client.Common.Height - 165, "dh.bmp", "000000", 0.95, 1, 0, 1, 100, 13))
                {
                    //Console.WriteLine($"{_lw.X()},{_lw.Y()}");
                    OpenAction();
                }
                RecoverAction(2000);
                RunState = RunState.Idle;
            }
        }

        //自己发送链接 自己打开
        public void SendMyMessage(string message)
        {
            Point row = WxPoints.FirstRow;
            Lw.ClickOnce(WxPoints.WeiXin).ClickOnce(row).Delay();
            //找到一个能发送的聊天窗口
            for (int i = 0; i < 7; i++)
            {
                if (Lw.FindPic(400, 400, 500, 550, "char.bmp", "000000", 0.95, 1))
                {
                    break;
                }
                row.Offset(0, 80);
                Lw.ClickOnce(row).Delay();
            }
            Lw.ClickOnce(WxPoints.Chat);
            //todo 删除有bug,暂时不删除
            //Lw.KeyPressEx(65, 2);
            Lw.SendString(message, 3, Hwnd);
            Lw.Delay().KeyPress(13);

            if (Lw.FindPic(600, 80, 820, Client.Common.Height - 165, "dh2.bmp", "000000", 0.95, 1, 0, 1, -100, 13))
            {
                RunState = RunState.Busy;
                OpenAction();
            }
            RunState = RunState.Idle;
        }

        public void GoOnceAction(Action action)
        {
            Task.Factory.StartNew(() =>
            {
                //进入聊天首页
                RecoverAction();
                WaitSeesion();
                action();
            });
        }

        private void OpenAction()
        {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(2000);
                var hwnd = Lw.FindWindow("图片查看器", "ImagePreviewWnd", null);
                if (hwnd > 0)
                {
                    Log("发现图片，等待关闭");
                    WinApi.SendMessage(new IntPtr(hwnd), 0x0010, 0, 0);
                }
                else
                {
                    WaitSeesion();
                    Client.Common.Messenger.Notify("CefWebViewWnd");
                }
            });
        }

        //恢复到准备状态 聊天首页
        private void RecoverAction(int sleep = 1000)
        {
            Thread.Sleep(sleep);
            Lw.ClickOnce(WxPoints.WeiXin)
                .ClickOnce(WxPoints.EndRow)
                .Delay()
                .ClickOnce(WxPoints.WeiXin);
            MoveUpQuick();
        }

        //会话窗口过多，等待处理
        private void WaitSeesion()
        {
            if (Client.Common.SessionCount > 0)
                while (Client.Common.SessionCount >= Client.Common.MaxSessionCount)
                {
                    _log.Info("窗口数较多，等待处理中");
                    //超过窗口数，等待处理
                    Thread.Sleep(1000);
                }
        }

        public void StopThread()
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
