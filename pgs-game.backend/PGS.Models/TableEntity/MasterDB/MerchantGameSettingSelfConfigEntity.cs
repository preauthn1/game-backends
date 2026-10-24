using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    [IsNotAutoKeyTable]
    public class MerchantGameSettingSelfConfigEntity
    {
        [Key]
        public int MerchantID { get; set; }

        [Key]
        public int GameID { get; set; }

        public bool IsLobbyVisible { get; set; }

        public bool IsSubAgentDefaultEnableGame { get; set; }

        public bool IsHot { get; set; }

        public bool IsNew { get; set; }

        public DateTime UpdateTime { get; set; }

        public string UpdateBy { get; set; }


    }
}
