using System;
using System.Collections.Generic;
using System.Linq;

namespace FW.Core.Helper.Reflection
{
    public static class TypeHelper
    {
        private static List<Type> _simpleTypes = new List<Type>() {
            typeof(Int32),
            typeof(Int64),
            typeof(Int16),
            typeof(byte),
            typeof(bool),
            typeof(string),
            typeof(char),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(DateTime),
            typeof(TimeSpan)
            };

        /// <summary>
        /// 是否简单类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsSimpleType(Type type)
        {
            return _simpleTypes.Where(t => t == type).FirstOrDefault() != null || type.IsEnum;
        }


        /// <summary>
        /// 是否简单类型
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        public static bool IsSimpleType<TObject>()
        {
            return IsSimpleType(typeof(TObject));
        }
    }
}
