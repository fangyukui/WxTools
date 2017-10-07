namespace LwSoft.Enums
{
    public enum MouseBindKey
    {
        /// <summary>
        /// 正常模式,平常我们用的前台鼠标模式
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Windows模式,采取模拟windows消息方式 同按键自带后台插件
        /// </summary>
        Windows = 1,
        /// <summary>
        ///Windows3模式,采取模拟windows消息方式 同按键的后台插件，如果上一个模式不能正常按组合键，可能尝试此模式
        /// </summary>
        Windows3 = 2,
        /// <summary>
        /// dx模式,采用模拟dx后台键盘模式。有些窗口在此模式下绑定时，需要先激活窗口再绑定(或者绑定以后激活)，否则可能会出现绑定后键盘无效的情况
        /// </summary>
        Dx = 3
    }
}
