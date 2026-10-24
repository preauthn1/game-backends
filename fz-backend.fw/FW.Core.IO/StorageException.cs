using System;

namespace example.invalid
{
    public class StorageException : System.Exception
    {
        public StorageException()
            :base()
        { }

        public StorageException(string message)
            : base(message)
        { }

        public StorageException(string message, System.Exception innerEx)
            :base(message, innerEx)
        { }

    }
}
