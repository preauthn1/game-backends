using System;


namespace FW.Core.Cache
{
    public class CacheException : System.Exception
    {
        public CacheException()
        { }

        public CacheException(string message)
            : base(message)
        { }

        public CacheException(string message, System.Exception innerEx)
            : base(message, innerEx)
        { }

    }
}
