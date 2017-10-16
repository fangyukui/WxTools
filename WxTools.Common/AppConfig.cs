using System;
using System.Configuration;

namespace WxTools.Common
{
    /// <summary>  
    /// 配置信息维护  
    /// </summary>  
    public class AppConfig
    {
        public static Configuration Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        /// <summary>  
        /// 获取配置值  
        /// </summary>  
        /// <param name="key">配置标识</param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>  
        public static T GetValue<T>(string key,T defaultValue)
        {
            string result = string.Empty;
            if (Config.AppSettings.Settings[key] != null)
                result = Config.AppSettings.Settings[key].Value;
            var value = (T)Convert.ChangeType(result, typeof(T));
            if (value != null) return value;
            return defaultValue;
        }

        /// <summary>  
        /// 修改或增加配置值  
        /// </summary>  
        /// <param name="key">配置标识</param>  
        /// <param name="value">配置值</param>  
        public static void SetValue(string key, string value)
        {
            if (Config.AppSettings.Settings[key] != null)
                Config.AppSettings.Settings[key].Value = value;
            else
                Config.AppSettings.Settings.Add(key, value);
            Config.Save(ConfigurationSaveMode.Modified);
        }

        /// <summary>  
        /// 删除配置值  
        /// </summary>  
        /// <param name="key">配置标识</param>  
        public static void DeleteValue(string key)
        {
            Config.AppSettings.Settings.Remove(key);
        }
    }
}
