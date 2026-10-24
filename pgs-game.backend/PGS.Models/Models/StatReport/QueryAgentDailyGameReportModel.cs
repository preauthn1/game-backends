using System;

namespace target.Models.Table
{
    public class QueryAgentDailyGameReportModel
    {
        public DateTime StatDate { get; set; }
        public int GameID { get; set; }
        public int MerchantID { get; set; }
        public int TotalPlayers { get; set; }
        public int TotalRound { get; set; }
        public decimal BetAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal WinLossAmount { get; set; }


        //有些欄位是後面其他程式碼補上的
        //有些欄位是後面其他程式碼補上的
        //有些欄位是後面其他程式碼補上的

        public string DisplayDate { get; set; }

        public string GameName { get; set; }
        public int GameType { get; set; }
        public string GameTypeText { get; set; }
        public string MerchantName { get; set; }
        public string ParentMerchant { get; set; }

        public decimal MerchantWinLossAmount { get; set; }
        public decimal ProfitRate { get; set; }
        public decimal RTP { get; set; }
    }





}
