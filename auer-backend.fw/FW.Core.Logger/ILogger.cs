using System;


namespace FW.Core.Logger
{
    public interface ILogger : IDisposable
    {
        /// <summary>
        /// 日志名
        /// </summary>
        string LoggerName
        {
            get;
        }

        
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="message"></param>
        void WriteLog(string message);

    }
}
