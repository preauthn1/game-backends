using System;
using System.Collections;


namespace FW.Core.Helper
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// 设置集合键值，存在则修改，不存在则添加
        /// </summary>
        /// <param name="dct"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set(this IDictionary dct, object key, object value)
        {
            if (dct.Contains(key))
            {
                dct[key] = value;
            }
            else
            {
                dct.Add(key,value);
            }
        }

        /// <summary>
        /// 移除键值
        /// </summary>
        /// <param name="dct"></param>
        /// <param name="key"></param>
        public static void Remove(this IDictionary dct, object key)
        {
            if (dct.Contains(key))
            {
                dct.Remove(key);
            }
        }

    }
}
