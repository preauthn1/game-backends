using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using target.Models.CoreLibs;

namespace target.Models.Table
{
    [IsNotAutoKeyTable]
    public class PlayerLoginDeviceDataEntity
    {
        [Key]
        public DateTime StatTime { get; set; }

        [Key]
        public int MerchantID { get; set; }

        [Key]
        public int PlayerID { get; set; }

        [Key]
        public int HDType { get; set; }

        public int LoginCount { get; set; }

        public DateTime UpdateTime { get; set; }


    }
}
