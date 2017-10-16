using System;
using System.Collections.Generic;
using System.Windows;
using LwSoft;
using LwSoft.Enums;
using WxTools.Client.Model;

namespace WxTools.Client.Helper
{
    public static class LwExtension
    {
        public static int FindWindow(this Lwsoft3 lw, List<WindowInfo> infos, int parent = 0)
        {
            foreach (var info in infos)
            {
                var intptr = lw.FindWindow(info.Title, info.ClassName, null, 0, parent);
                if (intptr <= 0) return 0;
                parent = intptr;
            }
            return parent;
        }

        public static WindowInfo GetWindowInfo(this Lwsoft3 lw, int hwnd)
        {
            var info = new WindowInfo
            {
                ClassName = lw.GetWindowClass(hwnd),
                Title = lw.GetWindowTitle(hwnd)
            };
            lw.GetWindowSize(hwnd);
            info.WindowRect = new Rect(new Point(0, 0), new Point(lw.X(), lw.Y()));
            return info;
        }

        public static IDisposable BindWindow(this Lwsoft3 lw, int hwnd,
            DisplayBindKey display, MouseBindKey mouse, KeypadBindKey keypad, int added)
        {
            lw.BindWindow(hwnd, display, mouse, keypad, added, 0);
            return new BindingDisposable(lw);
        }

        public static Lwsoft3 ClickOnce(this Lwsoft3 lw, int x, int y)
        {
            lw.MoveTo(x, y);
            lw.LeftClick();
            return lw;
        }

        public static Lwsoft3 ClickOnce(this Lwsoft3 lw, Point point)
        {
            return ClickOnce(lw, (int)point.X, (int)point.Y);
        }

        public static Lwsoft3 ClickDouble(this Lwsoft3 lw, int x, int y)
        {
            ClickOnce(lw, x, y);
            lw.LeftClick();
            return lw;
        }

        public static Lwsoft3 ClickDouble(this Lwsoft3 lw, Point point)
        {
            return ClickDouble(lw, (int)point.X, (int)point.Y);
        }

        public static Lwsoft3 MoveTo(this Lwsoft3 lw, Point point)
        {
            lw.MoveTo((int)point.X, (int)point.Y);
            return lw;
        }

        public static Lwsoft3 MoveR(this Lwsoft3 lw, Point point)
        {
            lw.MoveR((int)point.X, (int)point.Y);
            return lw;
        }
    }

    public class BindingDisposable : IDisposable
    {
        private readonly Lwsoft3 _lw;
        public BindingDisposable(Lwsoft3 lw)
        {
            _lw = lw;
        }
        public void Dispose()
        {
            _lw.UnBindWindow();
        }
    }
}
