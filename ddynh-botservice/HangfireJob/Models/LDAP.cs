using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HangfireJob.Models
{
    public class LDAP
    {
        public string Server { get; set; }
        public string Port { get; set; }
        public string UserDomain { get; set; }
    }
}
