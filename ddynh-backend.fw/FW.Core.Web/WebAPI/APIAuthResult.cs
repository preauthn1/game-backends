using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Web.WebAPI
{
    public class APIAuthResult
    {
        public bool IsSuccess
        {
            get;
            set;
        }

        public string Token
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }
    }
}
