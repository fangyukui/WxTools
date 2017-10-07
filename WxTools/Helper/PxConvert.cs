using System.Windows;

namespace WxTools.Helper
{
    public class PxConvert
    {
        //调试下的宽高
        public static Rect DebugPoint = new Rect(0, 0, 888, 625);

        public static Point To(Point p, Rect rect)
        {
            var x = rect.Width * p.X / DebugPoint.Width;
            var y = rect.Height * p.Y / DebugPoint.Height;
            return new Point(x, y);
        }

        public static Point To(int x, int y, Rect rect)
        {
            return To(new Point(x, y), rect);
        }
    }
}
