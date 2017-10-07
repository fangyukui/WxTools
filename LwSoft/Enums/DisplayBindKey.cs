namespace LwSoft.Enums
{
    public enum DisplayBindKey
    {
        /// <summary>
        /// 正常模式,平常我们用的前台截屏模式
        /// </summary>
        Normal = 0,
        /// <summary>
        /// gdi模式,用于窗口采用GDI方式刷新时. 此模式占用CPU较大
        /// </summary>
        Gdi = 1,
        /// <summary>
        /// gdi2模式,此模式兼容性较强,但是速度比gdi模式要慢许多,如果gdi模式发现后台不刷新时,可以考虑用gdi2模式.
        /// </summary>
        Gdi2 = 2,
        /// <summary>
        /// dx2模式,用于窗口采用dx模式刷新,如果dx方式会出现窗口所在进程崩溃的状况,可以考虑采用这种.采用这种方式要保证窗口有一部分在屏幕外.win7或者vista不需要移动也可后台.此模式占用CPU较大.
        /// </summary>
        Dx2 = 3,
        /// <summary>
        /// dx模式,注意此模式需要管理员权限
        /// </summary>
        Dx = 4,
        /// <summary>
        /// 用于窗口采用Opengl模式刷新。注意此模式需要管理员权限
        /// </summary>
        Opengl = 5,
        /// <summary>
        /// opengl2,同上，如果上一个模式不能正常截图，可以采用此模式，注意此模式需要管理员权限。
        /// </summary>
        Opengl2 = 6
    }
}
