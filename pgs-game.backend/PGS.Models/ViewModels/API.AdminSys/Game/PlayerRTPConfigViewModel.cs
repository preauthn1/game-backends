using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class PlayerRTPConfigViewModel
    {
        public int Id { get; set; }
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
        public int GameID { get; set; }
        public string GameName { get; set; }
        public decimal RTP { get; set; }
        public bool IsRiskControl { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UpdateBy { get; set; }
    }

    public class UpdatePlayerRTPRequestModel
    {
        public int PlayerID { get; set; }

        public int GameID { get; set; }

        public List<int> GameIDs { get; set; }

        /// <summary>
        /// 目標數值
        /// </summary>
        public decimal RTP { get; set; }


        /// <summary>
        /// 勾選 重製成預設RTP 
        /// </summary>
        public bool IsReset { get; set; }

    }


    public class ResetAllPlayerRTPRequestModel
    {
        public decimal TargetRTPValue { get; set; }

    }

    public class ResetAllPlayerRTPByEventRequestModel
    {
        public int EventID { get; set; }

        public string Memo { get; set; }

    }


}
