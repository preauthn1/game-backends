using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class VendorGameAccountMappingEntity
    {
        [Key]
        public int VendorConfigID { get; set; }

        [Key]
        public int PlayerID { get; set; }


        public string Account { get; set; }


        public string Password { get; set; }


        public DateTime CreateTime { get; set; }
    }
}
