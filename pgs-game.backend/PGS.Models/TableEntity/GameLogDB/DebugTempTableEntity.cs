using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class DebugTempTableEntity
    {
        [Key]
        public long Id { get; set; }
        public string Category { get; set; }
        public string BetRecordNoPGS { get; set; }
        public string BetRecordNo { get; set; }
        public string Text { get; set; }
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
        public int GameID { get; set; }
        public int MerchantID { get; set; }
        public decimal Amount { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }


    }
}
