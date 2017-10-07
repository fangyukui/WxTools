using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using WxTools.Theme.Annotations;

namespace WxTools.Theme.Control
{
    /// <summary>
    /// InputBoxX.xaml 的交互逻辑
    /// </summary>
    public partial class InputBoxPassword : INotifyPropertyChanged
    {
        private string _questionText;
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

        public InputBoxPassword()
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

        public static Tuple<bool, string> Show(string questionText, string title)
        {
            var res = true;
            var pass = "";
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                InputBoxPassword nb = new InputBoxPassword
                {
                    Title = title,
                    QuestionText = questionText,
                    Owner = ControlHelper.GetTopWindow()
                };
                nb.ShowDialog();
                pass = nb.InputTextBox.Password;
                res = nb.Result;
            }));
            return new Tuple<bool, string>(res, pass);
        }

        private void InputTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Result = true;
                this.Close();
                e.Handled = true;
            }
        }
    }
}
