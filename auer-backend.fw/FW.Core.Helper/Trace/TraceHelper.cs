using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Helper.Trace
{
    public static class TraceHelper
    {
        /// <summary>
        /// 输出格式化消息
        /// </summary>
        /// <param name="trace"></param>
        /// <param name="format"></param>
        /// <param name="ps"></param>
        public static void TraceFormat(this ITrace trace, string format, params object[] ps)
        {
            string message = String.Format(format, ps);

            trace.Trace(message);
        }

        /// <summary>
        /// 输出调试信息
        /// </summary>
        /// <param name="trace"></param>
        /// <param name="message"></param>
        public static void TraceDebug(this ITrace trace, string message)
        {
            message = message.Replace("{", "[").Replace("}", "]");
            trace.TraceFormat(string.Format("{1} debug {0}", message, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        }

        /// <summary>
        /// 输出信息
        /// </summary>
        /// <param name="trace"></param>
        /// <param name="message"></param>
        public static void TraceInfo(this ITrace trace, string message)
        {
            message = message.Replace("{", "[").Replace("}", "]");
            trace.TraceFormat(string.Format("{1} info {0}", message, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        }

        /// <summary>
        /// 输出错误信息
        /// </summary>
        /// <param name="trace"></param>
        /// <param name="message"></param>
        public static void TraceError(this ITrace trace, string message)
        {
            message = message.Replace("{", "[").Replace("}", "]");
            trace.TraceFormat(string.Format("{1} error {0}", message, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        }

    }
}
