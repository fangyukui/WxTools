using System;
using System.ComponentModel;
using System.Drawing;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using WxTools.Common.Enums;
using WxTools.Server.Annotations;

namespace WxTools.Server
{
    public class ClientInfo : INotifyPropertyChanged
    {
        private string _pcName;
        private Point _screen;
        private string _osName;
        private string _logs;
        private string _ip;
        public DateTime HeartbeatTime;
        private int _wxCount;
        private RunState _taskState;

        public string Ip
        {
            get => _ip;
            set
            {
                if (value == _ip) return;
                _ip = value;
                OnPropertyChanged();
            }
        }

        public TcpClient Client { get; set; }

        public string PcName
        {
            get => _pcName;
            set
            {
                if (value == _pcName) return;
                _pcName = value;
                OnPropertyChanged();
            }
        }

        public string OsName
        {
            get => _osName;
            set
            {
                if (value == _osName) return;
                _osName = value;
                OnPropertyChanged();
            }
        }

        public Point Screen
        {
            get => _screen;
            set
            {
                if (value.Equals(_screen)) return;
                _screen = value;
                OnPropertyChanged();
            }
        }

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

        public RunState TaskState
        {
            get => _taskState;
            set
            {
                if (value == _taskState) return;
                _taskState = value;
                OnPropertyChanged();
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
