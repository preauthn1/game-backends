using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using static example.invalid.WebRequestMethods;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class GroupRTPConfigEntity
    {
        [Key]
        public int SettingGroupID { get; set; }

        [Key]
        public int GameID { get; set; }

        public decimal RTP { get; set; }

        public string Memo { get; set; }

    }
}
