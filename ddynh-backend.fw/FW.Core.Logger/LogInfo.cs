using System;

namespace FW.Core.Logger
{
    public class LogInfo
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="level"></param>
        /// <param name="content"></param>
        public LogInfo(LogLevel level, string content)
        {
            LogLevel = level;
            Content = content;
            LogTime = DateTime.Now;
        }

        /// <summary>
        /// 日志级别
        /// </summary>
        public LogLevel LogLevel
        {
            get;
            set;
        }

        /// <summary>
        /// 日志内容
        /// </summary>
        public string Content
        {
            get;
            set;
        }

        /// <summary>
        /// 日志时间
        /// </summary>
        public DateTime LogTime
        {
            get;
            set;
        }

        public override string ToString()
        {
            return String.Format("LogLevel:{0},Content:{1},LogTime:{2}", LogLevel, Content, LogTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

    }
}
