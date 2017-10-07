using System.Windows;
using WxTools.Theme;

namespace WxTools
{
    public class Common
    {
        //消息通知
        public static Messenger Messenger = new Messenger();

        //微信的窗体大小，统一
        public static Rect WinRect;

        //当前打开的文章窗口个数
        public static int SessionCount;

        //最大支持同时打开{MaxSessionCount}个文章窗口
        public static int MaxSessionCount = 10;
    }
}
