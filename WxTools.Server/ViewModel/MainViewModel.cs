using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using log4net;
using WxTools.Server.Annotations;
using WxTools.Server.Dal;
using WxTools.Theme;

namespace WxTools.Server.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public static MainViewModel Instance { get; } = new MainViewModel();

        private readonly ILog _log = LogManager.GetLogger(typeof(MainViewModel));
        private TcpServerDal _tcpServerDal;
        private string _url;
        private int _wxCount;
        public ObservableCollection<ClientInfo> ClientInfos { get; set; }

        public int WxCount
        {
            get => _wxCount;
            set
            {
                if (value == _wxCount) return;
                _wxCount = value;
                OnPropertyChanged();
            }
        }
        /* {
            get
            {
                var count = 0;
                foreach (var clientInfo in ClientInfos)
                    count += clientInfo.WxCount;
                return count;
            }
        }*/

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
            Url =
                "https://mp.weixin.qq.com/s?src=11&timestamp=1507368444&ver=438&signature=82XVDEO4Ms7JSHg6-iPZuLdSG6NhGCiYzmzZSTBn9TLYd-RXmYK8gV9wRcgq9feHnm1fXs5zB1KZnHxzOuoL*ORFxwngLoLo9zWVRAXVOjuwrARnlkFvOcj2cFFcN7qS&new=1";

            InitTcp();
            StartHeartbeatThread();
        }

        public RelayCommand SendUrlCommand => new RelayCommand(() =>
        {
            _tcpServerDal.SendUrl(Url);
        });

        private void InitTcp()
        {
            ClientInfos = new ObservableCollection<ClientInfo>();
            _tcpServerDal = new TcpServerDal(ClientInfos);
            _tcpServerDal.StartServer();
        }

        private void StartHeartbeatThread()
        {
            //心跳包线程
            new Thread(() =>
                {
                    var list = new List<ClientInfo>();
                    while (true)
                    {
                        foreach (var client in ClientInfos)
                        {
                            if ((client.HeartbeatTime - DateTime.Now).TotalSeconds >= 60)
                            {
                                _log.Warn("客户端超时" + client.Ip);
                                list.Add(client);
                            }
                        }
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            foreach (var info in list)
                            {
                                ClientInfos.Remove(info);
                            }
                        });
                        Thread.Sleep(2000);
                        _tcpServerDal.SendHeartbeat();
                    }
                })
                { IsBackground = true, Name = "心跳包线程" }.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
