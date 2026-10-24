using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class BetRecordTagMappingEntity
    {
        [Key]
        public string BetRecordNoPGS { get; set; }

        /// <summary>
        /// 對應 SystemTagEnum
        /// </summary>
        [Key]
        public int TagID { get; set; }

        public DateTime CreateTime { get; set; }


        /// <summary>
        /// 用來對應 EazyTempValue 資料
        /// </summary>
        public Guid? ReferenceUUID { get; set; }

    }
}
