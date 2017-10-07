using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using WxTools.Theme.Annotations;
using WxTools.Theme.Win32;

namespace WxTools.Theme.Control
{
    /// <summary>
    /// MessageTip.xaml 的交互逻辑
    /// </summary>
    public partial class MessageTip : INotifyPropertyChanged
    {
        private string _iconText;
        private string _contentText;
        private int _type;
        private int _offsetTop;

        public string IconText
        {
            get { return _iconText; }
            set
            {
                if (value == _iconText) return;
                _iconText = value;
                OnPropertyChanged(nameof(IconText));
            }
        }

        public string ContentText
        {
            get { return _contentText; }
            set
            {
                if (value == _contentText) return;
                _contentText = value;
                OnPropertyChanged(nameof(ContentText));
            }
        }

        public TimeSpan Delay { get; set; }

        public Point BasePoint { get; set; }

        public int Type
        {
            get { return _type; }
            set
            {
                if (value == _type) return;
                _type = value;
                OnPropertyChanged(nameof(Type));
            }
        }

        public int OffsetTop
        {
            get { return _offsetTop; }
            set
            {
                if (value == _offsetTop) return;
                _offsetTop = value;
                OnPropertyChanged(nameof(OffsetTop));
            }
        }

        public MessageTip()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += (s, e) =>
            {
                var width = SystemParameters.WorkArea.Width;
                var height = SystemParameters.WorkArea.Height;

                /*var top = BasePoint.Y - ActualHeight*2;

                if (top < 0) top = 0;
                if (top + Height > bootom) top = top + Height;

                var left = BasePoint.X - ActualWidth*3;
                if (left < 0) left = 0;
                if (left + Width > right) left = right - Width - 20;

                Left = left;
                Top = top;*/
                Left = width/2 - ActualWidth/2;
                Top = height - ActualHeight - 100;

                var offsettop = (int) Top - new Random().Next(15, 25);
                if (offsettop < 0) offsettop = 0;
                OffsetTop = offsettop;

                var sbd = (Storyboard) Resources["CloseStoryboard"];
                sbd.Completed += delegate
                {
                    Close();
                };
                sbd.Begin(this);
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static string IconOk = "\ue62a";
        public static string IconWarning = "\ue653";
        public static string IconError = "\ue644";

        public static void ShowOk(string text = null, int delay = -1)
        {
            Show(text, IconOk, 1, delay);
        }

        public static void ShowWarning(string text = null, int delay = -1)
        {
            Show(text, IconWarning, 2, delay);
        }

        public static void ShowError(string text = null, int delay = -1)
        {
            Show(text, IconError, 3, delay);
        }

        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="text">消息文本</param>
        /// <param name="icon">图标</param>
        /// <param name="type"></param>
        /// <param name="delay">消息停留时长（毫秒）。指定负数则使用 DefaultDelay</param>
        public static void Show(string text, string icon, int type, int delay = -1)
        {
            Thread thread = new Thread(() =>
            {
                /*NativeMethods.POINT p;
                NativeMethods.GetCursorPos(out p);*/
                new MessageTip
                {
                    ContentText = text,
                    IconText = icon,
                    Delay = delay < 0 ? TimeSpan.FromMilliseconds(800) : TimeSpan.FromMilliseconds(delay),
                    //BasePoint = new Point(p.X, p.Y),
                    Type = type
                }.ShowDialog();
            })
            {
                Name = "MessageTip",
                IsBackground = true
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
    }
}
