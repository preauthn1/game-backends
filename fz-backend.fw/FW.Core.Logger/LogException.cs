using System;

namespace FW.Core.Logger
{
    public class LogException : System.Exception
    {
        public LogException()
        { }

        public LogException(string message)
            : base(message)
        { }

        public LogException(string message, System.Exception ex)
        { }

    }
}
