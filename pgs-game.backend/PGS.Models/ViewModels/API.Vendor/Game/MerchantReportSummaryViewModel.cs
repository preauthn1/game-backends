using Newtonsoft.Json;
using target.Models.Paging;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{



    public class MerchantReportSummaryViewModel : BaseMerchantTotalSummaryItemViewModel
    {      

        public List<MerchantTotalSummaryItemViewModel> GameSummary { get; set; }

        /// <summary>
        /// 活動獎勵
        /// </summary>
        public decimal TotalPromotionBonus { get; set; }

    }


    public class MerchantTotalSummaryItemViewModel : BaseMerchantTotalSummaryItemViewModel
    {

        public int GameID { get; set; }

    }


    public class BaseMerchantTotalSummaryItemViewModel
    {
        public int TotalRound { get; set; }

        public decimal TotalWinLossAmount { get; set; }

        public decimal TotalValidAmount { get; set; }

        public decimal TotalPayoutAmount { get; set; }

        public decimal TotalBetAmount { get; set; }

        public decimal TotalTaxAmount { get; set; }
    }



}
