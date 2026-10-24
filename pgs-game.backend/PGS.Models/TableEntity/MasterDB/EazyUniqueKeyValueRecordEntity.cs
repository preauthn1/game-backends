using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    [IsNotAutoKeyTable]
    public class EazyUniqueKeyValueRecordEntity
    {
        /// <summary>
        /// 參考 EazyUniqueKeyValueTypeEnum
        /// </summary>
        [Key]
        public int Type { get; set; }

        [Key]
        public int OwnerID { get; set; }

        [Key]
        public string SubCategory { get; set; }


        public string Memo { get; set; }


        public DateTime CreateTime { get; set; }



    }
}
