using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using log4net;
using Newtonsoft.Json;
using SimpleTCP;
using WxTools.Common;
using WxTools.Server.Annotations;
using WxTools.Theme;

namespace WxTools.Server.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(MainViewModel));
        private SimpleTcpServer _server;
        private string _url;
        public ObservableCollection<ClientInfo> ClientInfos { get; }

        public string Url
        {
            get => _url;
            set
            {
                if (value == _url) return;
                _url = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            ClientInfos = new ObservableCollection<ClientInfo>();
            StartServer();

            Url =
                "https://mp.weixin.qq.com/s?src=11&timestamp=1507368444&ver=438&signature=82XVDEO4Ms7JSHg6-iPZuLdSG6NhGCiYzmzZSTBn9TLYd-RXmYK8gV9wRcgq9feHnm1fXs5zB1KZnHxzOuoL*ORFxwngLoLo9zWVRAXVOjuwrARnlkFvOcj2cFFcN7qS&new=1";
            SendUrlCommand = new RelayCommand(() =>
            {
                SendUrl(Url);
            });
        }

        public RelayCommand SendUrlCommand { get; } 

        public void StartServer()
        {
            _server = new SimpleTcpServer().Start(8910);
            _server.DataReceived += Received;
        }

        public void SendUrl(string url)
        {
            try
            {
                var tcpmsg = new TcpMessage
                {
                    MsgType = MsgType.Url,
                    Action = ActionType.Execute,
                    IsServer = true,
                    Msg = url
                };
                _server.Broadcast(JsonConvert.SerializeObject(tcpmsg));
                MessageBox.Show("发送成功", "提示");
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
                        break;
                    case MsgType.Log:
                        var info = ClientInfos.FirstOrDefault(c => c.Ip == tcpmsg.Ip);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (info != null) info.Logs += tcpmsg.Msg + "\r\n";
                        });
                        _log.Info("Log");
                        break;
                    case MsgType.Login:
                        _log.Info("Login");
                        if (ClientInfos.All(c => c.Ip != tcpmsg.Ip))
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                ClientInfos.Add(new ClientInfo
                                {
                                    Ip = tcpmsg.Ip,
                                    Client = msg.TcpClient,
                                    PcName = tcpmsg.PcName,
                                    OsName = tcpmsg.OsName,
                                    Screen = tcpmsg.Screen
                                });
                            });
                        }
                        break;
                    case MsgType.Logout:
                        _log.Info("Logout");
                        var logoutInfo = ClientInfos.FirstOrDefault(c => c.Ip == tcpmsg.Ip);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (logoutInfo != null) ClientInfos.Remove(logoutInfo);
                        });
                        break;
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
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
