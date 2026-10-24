using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.AdminSystem.Models
{
    public class JQueryDataTableResultViewModel
    {
        
        public int? draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public bool success { get; set; }
        public object data { get; set; }
        
    }
}