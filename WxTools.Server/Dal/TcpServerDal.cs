using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using log4net;
using Newtonsoft.Json;
using SimpleTCP;
using WxTools.Common;
using WxTools.Common.Model;
using WxTools.Server.ViewModel;

namespace WxTools.Server.Dal
{
    public class TcpServerDal : IDisposable
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(TcpServerDal));
        private SimpleTcpServer _server;
        private int _port;

        private readonly ObservableCollection<ClientInfo> _clientInfos;

        public TcpServerDal(ObservableCollection<ClientInfo> clientInfos)
        {
            _clientInfos = clientInfos;
        }

        public void StartServer()
        {
            InitData();
            _server = new SimpleTcpServer().Start(_port);
            _server.DelimiterDataReceived += Received;
        }

        private void InitData()
        {
            try
            {
                _port = AppConfig.GetValue("Server_Port", 8911);
            }
            catch (Exception e)
            {
                _log.Error(e);
                MessageBox.Show("配置文件出错，请检查");
            }
        }

        public async Task SendUrl(string url)
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
                await Task.Run(() => _server.BroadcastLine(JsonConvert.SerializeObject(tcpmsg)));
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

        public async Task SendHeartbeat()
        {
            try
            {
                var tcpmsg = new TcpMessage
                {
                    MsgType = MsgType.Heartbeat,
                    Action = ActionType.None,
                    IsServer = false,
                };
                await Task.Run(() => _server.BroadcastLine(JsonConvert.SerializeObject(tcpmsg)));
            }
            catch (Exception)
            {
               // _log.Error(e);
            }
        }

        public void Received(object sender, Message msg)
        {
            try
            {
                var tcpmsg = JsonConvert.DeserializeObject<TcpMessage>(msg.MessageString);
                switch (tcpmsg.MsgType)
                {
                    case MsgType.Url:
                        break;
                    case MsgType.Log:
                        //_log.Info("Log");
                        var info = _clientInfos.FirstOrDefault(c => c.Ip == tcpmsg.Ip);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (info != null)
                            {
                                info.Logs += tcpmsg.Msg + "\r\n";
                                var lines = info.Logs.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                if (lines.Length > 20)
                                {
                                    StringBuilder strb = new StringBuilder();
                                    for (int i = lines.Length - 10; i < lines.Length; i++)
                                    {
                                        strb.AppendLine(lines[i]);
                                    }
                                    info.Logs = strb.ToString();
                                }
                            }
                        });
                        break;
                    case MsgType.Login:
                        _log.Info("Login:"+ tcpmsg.Ip);
                        if (_clientInfos.All(c => c.Ip != tcpmsg.Ip))
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                _clientInfos.Add(new ClientInfo
                                {
                                    Ip = tcpmsg.Ip,
                                    Client = msg.TcpClient,
                                    PcName = tcpmsg.PcName,
                                    OsName = tcpmsg.OsName,
                                    Screen = tcpmsg.Screen,
                                    HeartbeatTime = DateTime.Now
                                });
                            });
                        }
                        break;
                    case MsgType.Logout:
                        _log.Info("Logout:" + tcpmsg.Ip);
                        var logoutInfo = _clientInfos.FirstOrDefault(c => c.Ip == tcpmsg.Ip);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (logoutInfo != null)
                            {
                                _clientInfos.Remove(logoutInfo);
                                RefreshWxCount();
                            }
                        });
                        break;
                    case MsgType.Heartbeat:
                        //_log.Info("Heartbeat");
                        var heartbeatInfo = _clientInfos.FirstOrDefault(c => c.Ip == tcpmsg.Ip);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (heartbeatInfo != null)
                            {
                                heartbeatInfo.HeartbeatTime = DateTime.Now;
                                heartbeatInfo.TaskState = tcpmsg.TaskState;
                            }
                        });
                        break;
                    case MsgType.WxCount:
                        var wxCountInfo = _clientInfos.FirstOrDefault(c => c.Ip == tcpmsg.Ip);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (wxCountInfo != null)
                            {
                                wxCountInfo.WxCount = tcpmsg.Value;
                                RefreshWxCount();
                            }
                        });
                        break;
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

        private void RefreshWxCount()
        {
            var count = 0;
            foreach (var clientInfo in MainViewModel.Instance.ClientInfos)
                count += clientInfo.WxCount;
            MainViewModel.Instance.WxCount = count;
        }

        public void Dispose()
        {
            _server.Stop();
        }
    }
}
