using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class IPAddressListTemplateEntity
    {
        [Key]
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string IPAddressList { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public int? ReferIMChannelID { get; set; }
        public bool IsForbiddenAppend { get; set; }

    }

    public class IPAddressListTemplateBriefModel
    {       
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string IPAddressList { get; set; }
    }
}
