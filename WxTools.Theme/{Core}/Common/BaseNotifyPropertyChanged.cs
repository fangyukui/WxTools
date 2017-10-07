using System;
using System.Linq.Expressions;

namespace WxTools.Theme.Common
{
    /// <summary>
    /// 实现了属性更改通知的基类
    /// </summary>
    public class BaseNotifyPropertyChanged : System.ComponentModel.INotifyPropertyChanged
    {
        /// <summary>
        /// 属性值变化时发生
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 属性值变化时发生
        /// </summary>
        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression != null)
            {
                var propertyName = memberExpression.Member.Name;
                this.OnPropertyChanged(propertyName);
            }
        }

        public virtual event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}