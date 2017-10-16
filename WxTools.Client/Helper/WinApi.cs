using System;
using System.Runtime.InteropServices;

namespace WxTools.Client.Helper
{
    public class WinApi
    {
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    }
}
