using System;


namespace FW.Core.Engine
{
    public class EngineException : System.Exception
    {
        public EngineException()
        {}

        public EngineException(string message)
            : base(message)
        { }

        public EngineException(string message, System.Exception innerEx)
            :base(message, innerEx)
        { }

    }
}
