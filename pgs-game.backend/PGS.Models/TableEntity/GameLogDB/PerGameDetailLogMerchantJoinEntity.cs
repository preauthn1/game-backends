using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    [IsNotAutoKeyTable]
    public class PerGameDetailLogMerchantJoinEntity
    {
        [Key]
        public int MerchantID { get; set; }

        [Key]
        public DateTime GameTime { get; set; }

        [Key]
        public long PerGameDetailLogID { get; set; }

    }
}
