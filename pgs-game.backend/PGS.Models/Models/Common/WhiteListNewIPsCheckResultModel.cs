using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace target.Models.Models
{
    public class WhiteListNewIPsCheckResultModel
    {
        public List<string> NewIPs { get; set; }

        public bool IsBigAgent { get; set; }

    }
}
