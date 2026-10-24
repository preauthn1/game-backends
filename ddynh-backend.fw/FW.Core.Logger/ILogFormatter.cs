using System;

namespace FW.Core.Logger
{
    public interface ILogFormatter
    {
        /// <summary>
        /// 格式化日志
        /// </summary>
        /// <param name="loginfo"></param>
        /// <returns></returns>
        string Format(LogInfo loginfo);
    }
}
