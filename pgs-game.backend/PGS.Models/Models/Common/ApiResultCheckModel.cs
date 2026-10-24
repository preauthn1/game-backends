using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace target.Models.Models
{
    public class ApiResultCheckModel
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        public string ResponseText { get; set; }

        public string DebugUUID { get; set; }

    }
}
