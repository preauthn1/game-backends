using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class GameVendorRTPChangeRecordEntity
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// 參考 GameVendorRTPChangeTypeEnum
        /// </summary>
        public int Type { get; set; }

        public int VendorConfigID { get; set; }
        public int GameID { get; set; }
        public int PlayerID { get; set; }
        public decimal RTP { get; set; }
        public string DebugInfo { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }


    }
}
