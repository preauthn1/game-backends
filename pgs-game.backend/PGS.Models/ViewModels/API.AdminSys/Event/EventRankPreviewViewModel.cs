using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class EventRankPreviewRequestModel
    {
        public int EventID { get; set; }

        public int PageSize { get; set; }

        /// <summary>
        /// EventCounterOrderByEnum
        /// </summary>
        public int OrderByType { get; set; }
    }


    public class EventRankPreviewViewModel
    {
        public int EventID { get; set; }

        public string EventName { get; set; }

        public string BeginTimeText { get; set; }

        public string EndTimeText { get; set; }

        public int DefaultCompetiteType { get; set; }

        public string HintMemo { get; set; }

        public bool IsJobExist { get; set; }

        public bool IsPrizeEmpty { get; set; }

        public List<EventRankPreviewViewItemModel> Players { get; set; }

    }


    public class EventRankPreviewViewItemModel
    {
        public int Sequence { get; set; }

        public int MerchantID { get; set; }

        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public int TotalRound { get; set; }

        public decimal TotalBetAmount { get; set; }

        public decimal TotalPayoutAmount { get; set; }

        public decimal TotalWinLoss { get; set; }

        public decimal MaxPayoutRate { get; set; }
    }


}
