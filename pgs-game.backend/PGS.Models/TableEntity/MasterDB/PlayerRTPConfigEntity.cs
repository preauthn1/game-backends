using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class PlayerRTPConfigEntity
    {
        [Key]
        public int Id { get; set; }
        public int PlayerID { get; set; }
        public int GameID { get; set; }
        public decimal RTP { get; set; }
        public bool IsRiskControl { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UpdateBy { get; set; }


    }
}
