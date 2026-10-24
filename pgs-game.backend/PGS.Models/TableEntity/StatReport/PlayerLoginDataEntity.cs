using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class PlayerLoginDataEntity
    {
        [Key]
        public DateTime StatTime { get; set; }

        [Key]
        public int MerchantID { get; set; }

        [Key]
        public int PlayerID { get; set; }

        public int LoginCount { get; set; }
        public DateTime UpdateTime { get; set; }

    }
}
