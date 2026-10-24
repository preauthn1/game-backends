using target.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{


    public class QueryPlayerGameDataStatisticModel
    {
        public int PlayerID { get; set; }
        public int GameID { get; set; }
        public int TotalRound { get; set; }
        public decimal BetAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal ValidAmount { get; set; }
        public decimal WinLossAmount { get; set; }
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// 特定情況才會查詢此欄位
        /// </summary>
        public string PlayerName { get; set; }
    }

    public class QueryPlayerGameStatisticParameter
    {
        public int? PlayerID { get; set; }
        public int? GameID { get; set; }
        public int? MerchantID { get; set; }
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }

        /// <summary>
        /// 若指定true。會額外join查詢playerName
        /// </summary>
        public bool IsJoinPlayerName { get; set; }
    }


    public class PlayerWinLossWinRateReportModel
    {
        public int MerchantID { get; set; }
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
        public int GameID { get; set; }
        public int TotalRound { get; set; }
        public decimal WinRate { get; set; }
        public decimal BetAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal WinLossAmount { get; set; }
        public decimal RTP { get; set; }


        //   SELECT TOP 5000 m.MerchantID , m.Name ,  g.GameID , g.GameName , p.PlayerID , p.PlayerName ,
        //sub.WinCount , (cast(sub.WinCount as decimal(18,4)) / cast(sub.TotalRound as decimal(18,4))) * 100  as WinRate , sub.TotalRound , sub.WinLossAmount , sub.RTP  FROM 

    }


}
