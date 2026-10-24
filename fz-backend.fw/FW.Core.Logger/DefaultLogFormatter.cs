using System;
using FW.Core.Helper.Enum;

namespace FW.Core.Logger
{
    public class DefaultLogFormatter : ILogFormatter
    {

        public string Format(LogInfo loginfo)
        {
            return String.Format("{0} {1} {2}", loginfo.LogTime.ToString("yyyy-MM-dd HH:mm:ss"), loginfo.LogLevel.GetDisplayName(), loginfo.Content);
        }
    }
}
