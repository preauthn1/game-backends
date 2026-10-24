using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{


    public class FishGameDetailInfoRecordViewModel
    {
        public List<FishGameDetailRecordItemViewModel> Items { get; set; }
    }

    public class FishGameDetailRecordItemViewModel
    {
        public string PlayerName { get; set; }

        public int GameID { get; set; }

        /// <summary>
        /// 局號
        /// </summary>
        public string RoundID { get; set; }


        public string WagerID { get; set; }


        public int FishType { get; set; }


        public string FishTypeText { get; set; }


        public DateTime BetTime { get; set; }

        public decimal BetAmount { get; set; }

        public decimal PayoutAmount { get; set; }

        public decimal ValidAmount { get; set; }

        public decimal WinLossAmount { get; set; }



    }

}
