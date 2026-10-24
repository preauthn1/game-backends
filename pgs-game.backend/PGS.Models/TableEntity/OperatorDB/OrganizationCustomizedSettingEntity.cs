using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class OrganizationCustomizedSettingEntity
    {
       
        [Key]
        public int OrganizationID { get; set; }

        [Key]
        public int SettingKey { get; set; }


        public string SettingValue { get; set; }

    }
}
