using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class AnnouncementEntity
    {
        [Key] 
        public int AnnouncementID { get; set; }
        public Guid AnnouncementGuid { get; set; }
        public int Type { get; set; }
        public string Memo { get; set; }
        public int Status { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UpdateBy { get; set; }

        public int? MerchantSelectTemplateID { get; set; }


    }
}
