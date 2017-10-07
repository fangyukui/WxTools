using System.Windows;
using System.Windows.Input;

namespace WxTools.Theme.Control
{
    /// <summary>
    /// NumericUpDown.xaml 的交互逻辑
    /// </summary>
    public partial class NumericUpDown
    {
        public NumericUpDown()
        {
            InitializeComponent();
            ValueGrid.DataContext = this;
            ValueText.DataContext = this;
            /*ValueText.MaxValue = MaxValue;
            ValueText.MinValue = MinValue;*/
            NumType = NumTextBox.Type.Int;

            ValueText.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Previous);
                    ValueText?.MoveFocus(request);
                    e.Handled = true;
                }
            };
        }

        public double Value
        {
            get
            {
                double d;
                if (double.TryParse(Text, out d))
                {
                    return d;
                }
                return 0;
            }
            set { Text = value.ToString(); }
        }


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(NumericUpDown),new PropertyMetadata("0"));


        /// <summary>
        /// 设置或获取textbox的输入数据类型
        /// </summary>
        public NumTextBox.Type NumType
        {
            get { return ValueText.NumType; }
            set
            {
                ValueText.NumType = value;
            }
        }

        public static readonly DependencyProperty PointLenthProperty = DependencyProperty.Register("PointLenth", typeof(int), typeof(NumericUpDown));

        /// <summary>
        /// 设置或获取小数点后位数长度
        /// </summary>
        public int PointLenth
        {
            get { return (int)GetValue(PointLenthProperty); }
            set { SetValue(PointLenthProperty, value); }
        }

        public decimal MaxValue
        {
            get { return (decimal)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(decimal), typeof(NumericUpDown), new PropertyMetadata((decimal)100));

        public decimal MinValue
        {
            get { return (decimal)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(decimal), typeof(NumericUpDown), new PropertyMetadata((decimal)0));

        public double Increment { get; set; } = 1;

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            double newValue = Value + Increment;
            Value = newValue > (double)MaxValue ? (double)MaxValue : newValue;
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            double newValue = Value - Increment;
            Value = newValue < (double)MinValue ? (double)MinValue : newValue;
        }

    }
}