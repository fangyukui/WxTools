using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using log4net;
using LwSoft;
using LwSoft.Enums;
using WxTools.Annotations;
using WxTools.Client.Helper;
using WxTools.Client.Model;
using WxTools.Client.ViewModel;
using WxTools.Common.Model;

namespace WxTools.Client.Bll
{
    public class OperaBll : INotifyPropertyChanged, IDisposable
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

        private string _logs;
        private string _name;

        private readonly ILog _log = LogManager.GetLogger(typeof(OperaBll));

        public OperaBll(Lwsoft3 lw)
        {
            Lw = lw;
        }

        #region 初始化

        public void Load()
        {
            LoadWindowsHandle(Hwnd);
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
            _log.Info("初始化窗体成功");
            Log("初始化窗体成功", false);
            //绑定
            Lw.BindWindow(this.Hwnd, DisplayBindKey.Gdi, MouseBindKey.Windows,
                KeypadBindKey.Windows, 32);
            //设置窗体大小
            Lw.SetWindowSize(this.Hwnd, Client.Common.Width, Client.Common.Height);
            Lw.SetWindowState(this.Hwnd, 1);
            Log("绑定成功", false);
            //关闭错误消息
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
            _log.Info("解除绑定");
            Log("解除绑定", false);
        }

        //自己发送链接 自己打开
        public async Task SendMyMessage(string message, int index)
        {
            await WaitBusy();

            Log($"[{index}]开始执行链接");
            RunState = RunState.Busy;
            Lw.ClickOnce(WxPoints.WeiXin);
            MoveUpQuick();
            await Task.Delay(200);
            Point row = WxPoints.FirstRow;
            Lw.ClickOnce(WxPoints.WeiXin).ClickOnce(row).Delay();

            await WaitSeesion();
            bool find = false;
            //找到一个能发送的聊天窗口
            for (int i = 0; i < 7; i++)
            {
                if (Lw.FindPic(220, 400, 500, 550, "char2.bmp|char.bmp", "000000", 0.75, 1))
                {
                    find = true;
                    await Task.Delay(500);
                    break;
                }
                row.Offset(0, 80);
                Lw.ClickOnce(row).Delay();
            }
            if (!find) return;
            Lw.ClickOnce(WxPoints.Chat);
            Lw.SendString(message, 3, Hwnd);
            Lw.Delay().KeyPress(13);

            await WaitSeesion();
            if (Lw.FindPic(600, 80, Common.Width, Client.Common.Height - 150, "dh2.bmp", "000000", 0.95, 1, 5000, 1, -100, 13))
            {
                await OpenAction();
                Log($"[{index}]链接执行完毕");
                RunState = RunState.Idle;
                return;
            }
            Log($"[{index}]执行失败");
            RunState = RunState.Idle;
        }

        private async Task OpenAction()
        {
            await Task.Delay(2000);
            var hwnd = Lw.FindWindow("图片查看器", "ImagePreviewWnd", null);
            if (hwnd > 0)
            {
                Log("发现图片/视频");
                WinApi.SendMessage(new IntPtr(hwnd), 0x0010, 0, 0);
            }
            else
            {
                Common.Messenger.Notify("CefWebViewWnd");
            }
        }

        //会话窗口过多，等待处理
        private async Task WaitSeesion()
        {
            while (MainViewModel.Instance.RunState == RunState.Busy)
            {
                //超过窗口数，等待处理
                await Task.Delay(200);
            }
        }

        private async Task WaitBusy()
        {
            while (this.RunState == RunState.Busy || MainViewModel.Instance.RunState == RunState.Busy)
            {
                //超过窗口数，等待处理
                await Task.Delay(200);
            }
        }

        public void Dispose()
        {
            ThreadRun = false;
            UnBindWindow();
        }

        private void Log(string log, bool isUpload = true)
        {
            Logs += $"{DateTime.Now:HH:mm:ss}: {log}\r\n";
            var lines = Logs.Split("\r\n".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 10)
            {
                StringBuilder strb = new StringBuilder();
                for (int i = lines.Length - 10; i < lines.Length; i++)
                {
                    strb.Append(lines[i]);
                }
                Logs = strb.ToString();
            }
            if (isUpload)
                MainViewModel.Instance.TcpClientDal.SendLog(log);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
