using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using SimpleTCP;
using WxTools.Annotations;
using WxTools.Client.Helper;
using WxTools.Client.ViewModel;
using WxTools.Common;

namespace WxTools.Client
{
    public class TcpDal : INotifyPropertyChanged,IDisposable
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(TcpDal));
        private SimpleTcpClient _client;
        private bool _connected;

        public bool Connected
        {
            get => _connected;
            set
            {
                if (value == _connected) return;
                _connected = value;
                OnPropertyChanged();
            }
        }

        public void Connect()
        {
            Task.Factory.StartNew(() =>
            {
                while (_client == null || !_client.TcpClient.Connected)
                {
                    try
                    {
                        _client = new SimpleTcpClient().Connect("127.0.0.1", 8910);
                        _client.DataReceived += Received;
                        SendLogin();
                        Connected = true;
                        Console.WriteLine("登录成功");
                        break;
                    }
                    catch (Exception)
                    {
                        Connected = false;
                        Console.WriteLine("登录失败");
                    }
                    Thread.Sleep(2000);
                }
            });
        }

        public void SendLogin()
        {
            try
            {
                Computer computer = new Computer();
                var lw = LwFactory.GetDefault();
                var tcpmsg = new TcpMessage
                {
                    MsgType = MsgType.Login,
                    Action = ActionType.None,
                    IsServer = false,
                    Ip= lw.GetNetIp(),
                    PcName = computer.GetComputerName(),
                    OsName = computer.GetSystemType(),
                    Screen = new Point(lw.GetScreenWidth(), lw.GetScreenHeight())
                };
                _client.Write(JsonConvert.SerializeObject(tcpmsg));
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

        public void SendLogout()
        {
            try
            {
                var tcpmsg = new TcpMessage
                {
                    Ip = LwFactory.GetDefault().GetNetIp(),
                    MsgType = MsgType.Logout,
                    Action = ActionType.None,
                    IsServer = false,
                };
                _client.Write(JsonConvert.SerializeObject(tcpmsg));
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

        public void SendLog(string log)
        {
            try
            {
                var tcpmsg = new TcpMessage
                {
                    Ip = LwFactory.GetDefault().GetNetIp(),
                    MsgType = MsgType.Log,
                    Action = ActionType.None,
                    IsServer = false,
                    Msg = log
                };
                _client.Write(JsonConvert.SerializeObject(tcpmsg));
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

        public void Received(object sender, Message msg)
        {
            try
            {
                var tcpmsg = JsonConvert.DeserializeObject<TcpMessage>(msg.MessageString);
                _log.Info("收到一条消息");
                switch (tcpmsg.MsgType)
                {
                    case MsgType.Url:
                        if (tcpmsg.Action == ActionType.Execute)
                        {
                            _log.Info("执行URl");
                            MainViewModel.Instance.ExecuteUrl(tcpmsg.Msg);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

        public void Dispose()
        {
            if (_client != null)
            {
                SendLogout();
                _client?.Disconnect();
                _client?.Dispose();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
