using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class QueryStatGameReportApiRequestParameterModel
    {
        public List<int> MerchantIDs { get; set; }

        public List<int> GameIDs { get; set; }

        public DateTime? Begin { get; set; }

        public DateTime? End { get; set; }


        /// <summary>
        /// 請參考 StatReportSearchColumnType
        /// </summary>
        public List<int> IncludeExtraSearchInfo { get; set; }




    }



    public class StatGameReportViewModel
    {

        public string ServerTimeTW { get; set; }


        public bool IsShowGoToAgentReportHint { get; set; }


        public bool IsQuerySummaryByHierarchyMode { get; set; }


        public List<StatGameReportMerchantItemViewModel> Merchants { get; set; }


    }

    public class StatGameReportMerchantItemViewModel
    {
        public int MerchantID { get; set; }

        public string MerchantName { get; set; }

        public List<StatGameReportItemViewModel> Items { get; set; }

        public StatGameReportItemViewModel TotalSummary { get; set; }

        public List<StatGameReportItemViewModel> CategoryReportItems { get; set; }
        

    }



    public class StatGameReportItemViewModel
    {
        public int GameID { get; set; }
        public string GameName { get; set; }
        public string GameNameEN { get; set; }
        public int GameType { get; set; }
        public string GameTypeText { get; set; }


        public int TotalRound { get; set; }
        public int OnlinePlayer { get; set; }

        /// <summary>
        /// 商戶盈利
        /// </summary>
        public decimal MerchantWinLoss { get; set; }

        /// <summary>
        /// 玩家輸贏
        /// </summary>
        public decimal UpdateAmount { get; set; }

        public decimal WinAmount { get; set; }
        public decimal LoseAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal FlowAmount { get; set; }
        public decimal BetAmount { get; set; }
        public decimal ValidAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal RTP { get; set; }




        public decimal JackpotPoolAmount { get; set; }

    }


}
