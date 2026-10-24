using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace  example.invalid.FastDFS
{
    public class FDFSException : Exception
    {
        public FDFSException(string msg) : base(msg)
        {
            
        }
    }
}
