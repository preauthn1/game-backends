using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class GameRTPConfigEntity
    {
        [Key]
        public int Id { get; set; }
        public int VendorConfigID { get; set; }
        public int GameID { get; set; }
        public decimal RTP { get; set; }
        public decimal? WinLimit { get; set; }
        public string AvailableRTP { get; set; }
        public string DebugInfo { get; set; }
        public string Memo { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public DateTime CreateTime { get; set; }


    }
}
