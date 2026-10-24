using System;
using System.Linq;
using System.Reflection;

namespace FW.Core.Helper.Enum
{
    public static class EnumExtensions
    {
        /// <summary>
        /// 获取显示名称
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetDisplayName(this object obj)
        {
            var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in fields)
            {
                if (field.Name == obj.ToString())
                {
                    var attr = field.GetCustomAttribute<Enum.EnumDescriptionAttribute>();
                    if (attr != null)
                    {
                        return attr.DisplayName;
                    }

                    return String.Empty;
                }
            }

            return String.Empty;
        }


        /// <summary>
        /// 是否显示列举值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool GetDisplay(this object obj)
        {
            var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in fields)
            {
                if (field.Name == obj.ToString())
                {
                    var attr = field.GetCustomAttribute<Disabled>();
                    if (attr != null)
                    {
                        return !attr.IsDisplay;
                    }

                    return true;
                }
            }

            return true;
        }
    }
}
