using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FW.Core.Helper;
using FW.Core.Repository;

namespace game.Entity.HttpClient
{
    public class Response : BaseEntity
    {
        public Int16 StatusCode { get; set; }
        public string Data { get; set; }
    }
}
