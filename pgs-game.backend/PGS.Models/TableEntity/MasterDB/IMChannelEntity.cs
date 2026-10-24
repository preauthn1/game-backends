using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class IMChannelEntity
    {
        [Key]
        public int IMChannelID { get; set; }
        public int Type { get; set; }
        public string DisplayName { get; set; }
        public string Memo { get; set; }
        public string ThirdPartyChatID { get; set; }
        public string TGThreadID { get; set; }
        public int Status { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public DateTime UpdateTime { get; set; }


    }
}
