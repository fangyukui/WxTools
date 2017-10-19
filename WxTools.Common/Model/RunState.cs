using System.ComponentModel;

namespace WxTools.Common.Enums
{
    public enum RunState
    {
        [Description("准备中")]
        Ready,
        [Description("空闲")]
        Idle,
        [Description("忙碌")]
        Busy,
        [Description("异常")]
        Error
    }
}
