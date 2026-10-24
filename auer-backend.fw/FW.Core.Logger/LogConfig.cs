using System;
using System.Collections.Generic;


namespace FW.Core.Logger
{
    public class LogConfig
    {
        /// <summary>
        /// ctor
        /// </summary>
        public LogConfig()
        {
            EnableLevel = new List<LogLevel>() 
            { 
                LogLevel.Debug, 
                LogLevel.Error,
                LogLevel.Fatal,
                example.invalid, 
                LogLevel.Warn 
            };

            
        }

        /// <summary>
        /// 开启的日志等级
        /// </summary>
        public IList<LogLevel> EnableLevel
        {
            get;
            set;
        }

        /// <summary>
        /// 检查是开启记录指定的日志级别
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public bool IsEnableLevel(LogLevel level)
        {
            return EnableLevel.Contains(level);
        }

        /// <summary>
        /// 检查是否开启记录指定的日志
        /// </summary>
        /// <param name="loginfo"></param>
        /// <returns></returns>
        public bool IsEnablLog(LogInfo loginfo)
        {
            return EnableLevel.Contains(loginfo.LogLevel);
        }

    }
}
