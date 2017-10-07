using System;
using System.ComponentModel;
using System.Windows;
using WxTools.Theme.Annotations;

namespace WxTools.Theme.Control
{
    /// <summary>
    /// InputBoxX.xaml 的交互逻辑
    /// </summary>
    public partial class InputBoxX :INotifyPropertyChanged
    {
        private string _questionText;
        private string _numText = "0";
        private NumTextBox.Type _type;

        /// <summary>
        /// 结果，用户点击确定Result=true;
        /// </summary>
        public bool Result { get; private set; }

        public string QuestionText
        {
            get { return _questionText; }
            set
            {
                if (value == _questionText) return;
                _questionText = value;
                OnPropertyChanged(nameof(QuestionText));
            }
        }

        public string NumText
        {
            get { return _numText; }
            set
            {
                if (value == _numText) return;
                _numText = value;
                OnPropertyChanged(nameof(NumText));
            }
        }

        public NumTextBox.Type Type
        {
            get { return _type; }
            set
            {
                if (value == _type) return;
                _type = value;
                OnPropertyChanged(nameof(Type));
            }
        }


        public InputBoxX()
        {
            InitializeComponent();
            DataContext = this;
            InputTextBox.Focus();
            Loaded += (s, e) => InputTextBox.SelectAll();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Result = true;
            this.Close();
            e.Handled = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = false;
            this.Close();
            e.Handled = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static Tuple<bool, T> Show<T>(string questionText, string title, T defaultValue, NumTextBox.Type type = NumTextBox.Type.Int)
        {
            var res = true;
            T t = default(T);
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                InputBoxX nb = new InputBoxX
                {
                    Title = title,
                    QuestionText = questionText,
                    Type = type,
                    NumText = defaultValue.ToString(),
                    Owner = ControlHelper.GetTopWindow()
                };
                nb.ShowDialog();
                try
                {
                    t = (T)Convert.ChangeType(nb.NumText, typeof(T));
                }
                catch (FormatException) {}
                res = nb.Result;
            }));
            return new Tuple<bool, T>(res, t);
        }
    }
}
