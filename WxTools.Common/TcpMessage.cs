using System.Drawing;

namespace WxTools.Common
{
    public class TcpMessage
    {
        public string Version { get; set; } = "1.0";
        public bool IsServer { get; set; }
        public MsgType MsgType { get; set; } = MsgType.Log;
        public string PcName { get; set; } = "";
        public string OsName { get; set; } = "";
        public Point Screen { get; set; } = new Point();
        public ActionType Action { get; set; } = ActionType.None;

        public string Msg { get; set; } = "";
        public string MsgExt { get; set; } = "";
        public int State { get; set; } = 0;
        public int Ext1 { get; set; } = 0;
        public int Ext2 { get; set; } = 0;
        public object Data { get; set; }
        public string Ip { get; set; } = "";
    }

    public enum ActionType
    {
        Execute,
        Feedback,
        None,
    }

    public enum MsgType
    {
        Log,
        Url,
        Login,
        Logout
    }
}
