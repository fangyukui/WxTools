using System.Windows;
using WxTools.Helper;

namespace WxTools.Model
{
    public class WxPoints
    {
        /// <summary>
        /// 第一行
        /// </summary>
        public static Point FirstRow { get; } = PxConvert.To(225, 120);

        /// <summary>
        /// 最后一行
        /// </summary>
        public static Point EndRow { get; } = PxConvert.To(225, 588);

        /// <summary>
        /// 微信
        /// </summary>
        public static Point WeiXin { get; } = PxConvert.To(35, 110);

        public static Point WeiXinHeader { get; } = PxConvert.To(210, 70);

        /// <summary>
        /// 通讯录
        /// </summary>
        public static Point AddressBook { get; } = PxConvert.To(35, 180);

        /// <summary>
        /// 发现
        /// </summary>
        public static Point Discover { get; } = PxConvert.To(186, 470);
    }
}
