using System;


namespace FW.Core.Auth
{
    public class AuthException : System.Exception
    {
        public AuthException()
            : base()
        { }

        public AuthException(string message)
            : base(message)
        { }

        public AuthException(string message, System.Exception innerEx)
            : base(message, innerEx)
        { }


    }
}
