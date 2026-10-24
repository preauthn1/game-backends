using System;

namespace target.Models.ViewModels
{
    public class PlayerGameSummaryUpdateParameter
    {
        public int PlayerID { get; set; }
        public int GameID { get; set; }

        public decimal WinLossAmount { get; set; }
        public decimal BetAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal ValidAmount { get; set; }
        public decimal TaxAmount { get; set; }

    }

    public class PlayerGameSummaryGroupByGameModel
    {
        public int PlayerID { get; set; }
        public int GameID { get; set; }
        public int TotalRound { get; set; }
        public decimal PlayerWinLoss { get; set; }
        public decimal BetAmount { get; set; }
        public decimal PayoutAmount { get; set; }



        //以下是透過程式碼額外查詢的欄位
        public string VendorConfigName { get; set; }
        public int VendorConfigID { get; set; }
        public int GameVendorID { get; set; }
        public string GameName { get; set; }
        public decimal RTP { get; set; }
        public string ExtraMemo { get; set; }




    }


}
