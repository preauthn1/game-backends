using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FW.Core.Helper
{
    public static class StringHelper
    {
        /// <summary>
        /// 截断字符串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="len"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static string TruncateString(this string str, int len, string suffix = "...")
        {
            if (String.IsNullOrWhiteSpace(str))
            {
                return String.Empty;
            }

            Regex regex = new Regex(@"\<(.*?)\>", RegexOptions.IgnoreCase);

            str = regex.Replace(str, "");//.Replace("&nbsp;", "").Replace("\n", "").Replace("\r", "");

            if (str.Length > len)
            {
                return str.Substring(0, len) + suffix;
            }

            return str;
        }
    }
}
