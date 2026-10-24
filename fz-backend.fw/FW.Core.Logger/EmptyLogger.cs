using System;

namespace FW.Core.Logger
{
    public class EmptyLogger : ILogger
    {
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="message"></param>
        public void WriteLog(string message)
        {
            
        }

        public string LoggerName
        {
            get 
            { 
                return "Empty"; 
            }
        }

        public void Dispose()
        {
            
        }
    }
}
