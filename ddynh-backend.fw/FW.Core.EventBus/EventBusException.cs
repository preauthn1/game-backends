using System;


namespace FW.Core.EventBus
{
    public class EventBusException : System.Exception
    {
        public EventBusException()
            :base()
        { }

        public EventBusException(string message)
            : base(message)
        { }

        public EventBusException(string message, System.Exception innerEx)
            : base(message, innerEx)
        { }


    }
}
