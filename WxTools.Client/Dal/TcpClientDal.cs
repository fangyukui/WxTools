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

namespace WxTools.Client.Dal
{
    public class TcpClientDal : INotifyPropertyChanged, IDisposable
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(TcpClientDal));
        private DateTime _heartbeatTime;
        private SimpleTcpClient _client;
        private bool _connected;
        public Action ConnectedAction;
        private bool _connecting;
        private string _ip;

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

        public bool CheckIsConnected()
        {
            if (_client != null)
            {
                if (_client.TcpClient.Connected)
                {
                    if ((DateTime.Now - _heartbeatTime).TotalSeconds >= 60)
                    {
                        //超时
                        Connected = false;
                        return false;
                    }
                    Connected = true;
                    return true;
                }
            }
            Connected = false;
            return false;
        }

        public void Connect()
        {
            if (_connecting) return;
            _connecting = true;
            Task.Run(async() =>
            {
                while (!Connected)
                {
                    try
                    {
                        _ip = LwFactory.Default.GetNetIp();
                        _client = new SimpleTcpClient().Connect(Common.TcpIp,Common.TcpPort);
                        _client.DelimiterDataReceived -= Received;
                        _client.DelimiterDataReceived += Received;
                        SendLogin();
                        _heartbeatTime = DateTime.Now;
                        Connected = true;
                        if (MainViewModel.Instance.Operas.Count > 0)
                            SendWxCount(MainViewModel.Instance.Operas.Count);
                        SendLog("客户端初始化成功");
                        ConnectedAction?.Invoke();
                        Console.WriteLine("登录成功");
                        break;
                    }
                    catch (Exception e)
                    {
                        Connected = false;
                        _log.Warn("尝试登陆", e);
                        Console.WriteLine("登录失败");
                    }
                    await Task.Delay(2000);
                }
                _connecting = false;
            });
        }

        public void SendLogin()
        {
            try
            {
                Computer computer = new Computer();
                var lw = LwFactory.Default;
                var tcpmsg = new TcpMessage
                {
                    MsgType = MsgType.Login,
                    Action = ActionType.None,
                    IsServer = false,
                    Ip= _ip,
                    PcName = computer.GetComputerName(),
                    OsName = computer.GetSystemType(),
                    Screen = new Point(lw.GetScreenWidth(), lw.GetScreenHeight())
                };
                _client.WriteLine(JsonConvert.SerializeObject(tcpmsg));
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
                    Ip = _ip,
                    MsgType = MsgType.Logout,
                    Action = ActionType.None,
                    IsServer = false,
                };
                _client.WriteLine(JsonConvert.SerializeObject(tcpmsg));
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

        public void SendHeartbeat()
        {
            try
            {
                var tcpmsg = new TcpMessage
                {
                    Ip = _ip,
                    MsgType = MsgType.Heartbeat,
                    Action = ActionType.None,
                    IsServer = false,
                    Msg = ""
                };
                _client.WriteLine(JsonConvert.SerializeObject(tcpmsg));
            }
            catch (Exception)
            {
                //_log.Error(e);
            }
        }

        //微信数发送
        public void SendWxCount(int count)
        {
            try
            {
                var tcpmsg = new TcpMessage
                {
                    Ip = _ip,
                    MsgType = MsgType.WxCount,
                    Action = ActionType.None,
                    IsServer = false,
                    Value = count
                };
                _client.WriteLine(JsonConvert.SerializeObject(tcpmsg));
            }
            catch (Exception)
            {
                //_log.Error(e);
            }
        }

        public void SendLog(string log)
        {
            try
            {
                var tcpmsg = new TcpMessage
                {
                    Ip = _ip,
                    MsgType = MsgType.Log,
                    Action = ActionType.None,
                    IsServer = false,
                    Msg = log
                };
                _client.WriteLine(JsonConvert.SerializeObject(tcpmsg));
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
                //_log.Info("收到一条消息");
                switch (tcpmsg.MsgType)
                {
                    case MsgType.Url:
                        if (tcpmsg.Action == ActionType.Execute)
                        {
                            _log.Info("执行URl");
                            MainViewModel.Instance.ExecuteUrl(tcpmsg.Msg);
                        }
                        break;
                    case MsgType.Heartbeat:
                        _heartbeatTime = DateTime.Now;
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
