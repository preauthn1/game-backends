using System;

namespace FW.Core.Repository
{
    public class RepositoryException : System.Exception
    {
        public RepositoryException()
        { }

        public RepositoryException(string message)
            : base(message)
        { }

        public RepositoryException(string message, System.Exception innerEx)
            : base(message, innerEx)
        { }

    }
}
