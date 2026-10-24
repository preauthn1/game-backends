using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.AdminSystem
{
    public static class CheckHelper
    {
        /// <summary>
        /// 是否包含特殊字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool ContainsInvalidChar(string str) {

            string[] charString = new string[] { "{", "}", "[", "]", "\"","\\","'", "=", "null" };

            str = str.ToLower();
            foreach (var c in charString) {
                if (str.Contains(c) == true) {
                    return true;                    
                }
            }
            return false;
        }
      

    }
}