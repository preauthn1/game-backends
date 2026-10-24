using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Language
{
    public class LanguageException : System.Exception
    {
        public LanguageException()
            :base()
        { }

        public LanguageException(string message)
            : base(message)
        { }

        public LanguageException(string message, System.Exception innerEx)
            : base(message, innerEx)
        { }

    }
}
