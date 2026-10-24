using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class AGGLiveDetailJsonEntity
    {
        [Key]
        public string BetRecordNoPGS { get; set; }


        /// <summary>
        /// 請參考 AGGLiveSettlementDetailInfoModel 物件
        /// </summary>
        public string DetailJson { get; set; }

        public string RoundID { get; set; }

        public decimal? RoundTotalFlowAmount { get; set; }

        public DateTime CreateTime { get; set; }

    }
}
