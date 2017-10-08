using System.Windows;
using System.Windows.Media;

namespace WxTools.Server
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private DrawingBrush _gridBrush;
        public MainWindow()
        {
            InitializeComponent();

            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new Server.ViewModel.MainViewModel();
            }
        }

        private void GridRoot_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_gridBrush == null)
            {
                _gridBrush = new DrawingBrush(new GeometryDrawing(
                    new SolidColorBrush(Colors.White),
                    new Pen(new SolidColorBrush(Color.FromRgb(246, 247, 245)), 1),
                    new RectangleGeometry(new Rect(0, 0, 40, 40))))
                {
                    Stretch = Stretch.None,
                    TileMode = TileMode.Tile,
                    Viewport = new Rect(0.0, 0.0, 40, 40),
                    ViewportUnits = BrushMappingMode.Absolute
                };
                GridRoot.Background = _gridBrush;
            }
        }
    }
}
