using System;
using FW.Core.Helper.Enum;

namespace FW.Core.Logger
{
    public enum LogLevel
    {
        [EnumDescription("信息")]
        Info,
        [EnumDescription("错误")]
        Error,
        [EnumDescription("调试信息 ")]
        Debug,
        [EnumDescription("警告")]
        Warn,
        [EnumDescription("严重错误")]
        Fatal
    }
}
