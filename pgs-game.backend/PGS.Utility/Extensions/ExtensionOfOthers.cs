using System;
using Newtonsoft.Json;
using System.Reflection;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;

namespace target.Utility.Extensions
{
    /// <summary>
    /// Object 的擴充方法 (比較少使用到的) 
    /// namespace是另一組
    /// </summary>
    public static class ExtensionOfOthers
    {
        public static bool HasAttribute<T>(this PropertyInfo prop)
        {
            if (prop == null)
                return false;

            return prop.GetCustomAttribute(typeof(T), false) != null;
        }

        public static T GetTargetAttribute<T>(this PropertyInfo prop) where T: class
        {
            if (prop == null)
                return default(T);

            dynamic attr = prop.GetCustomAttribute(typeof(T), false);
            return (T)attr;
        }

    }

}