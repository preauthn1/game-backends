using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class AGGFreespinCampaignEntity
    {
        [Key]
        public int Id { get; set; }
        public string FreespinID { get; set; }
        public int MerchantID { get; set; }


        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreateTime { get; set; }
        public string Memo { get; set; }


    }
}
