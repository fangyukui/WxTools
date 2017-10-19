using System;
using System.ComponentModel;
using System.Reflection;

namespace WxTools.Common
{
    public static class Util
    {
        /// <summary>  
        /// 根据值得到中文备注  
        /// </summary>  
        /// <param name="e"></param>  
        /// <param name="value"></param>  
        /// <returns></returns>  
        public static String GetEnumDesc(this Type e, int? value)
        {
            FieldInfo[] fields = e.GetFields();
            for (int i = 1, count = fields.Length; i < count; i++)
            {
                if ((int)System.Enum.Parse(e, fields[i].Name) == value)
                {
                    DescriptionAttribute[] EnumAttributes = (DescriptionAttribute[])fields[i].
                        GetCustomAttributes(typeof(DescriptionAttribute), false);
                    if (EnumAttributes.Length > 0)
                    {
                        return EnumAttributes[0].Description;
                    }
                }
            }
            return "";
        }
    }
}
