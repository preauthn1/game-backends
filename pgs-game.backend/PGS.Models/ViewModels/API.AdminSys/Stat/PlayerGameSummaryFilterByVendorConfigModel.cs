using target.Models.Paging;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryPlayerGameSummaryFilterByVendorConfigApiRequestParameterModel : PaginationParameter
    {
        public int? VendorConfigID { get; set; }

        public int? PlayerID { get; set; }

        public int? MerchantID { get; set; }

        public int? GameID { get; set; }

        public int? GameVendorID { get; set; }

        public string PlayerName { get; set; }

        /// <summary>
        /// 不分遊戲(全部group by加總成一筆)
        /// </summary>
        public bool IsGroupAllGameOne { get; set; }

        public DateTime? Begin { get; set; }

        public DateTime? End { get; set; }
    }





    public class PlayerGameSummaryReportFilterByVendorConfigViewModel
    {
        public PlayerGameSummaryReportFilterByVendorConfigViewModel()
        {
        }

        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public PagedList<PlayerGameSummaryReportFilterByVendorConfigDetailItemModel> PagingItems { get; set; }

        public string ServerTimeTW { get; set; }


        //public PlayerGameSummaryReportFilterByVendorConfigDetailItemModel TotalSummary { get; set; }

    }


    public class PlayerGameSummaryReportFilterByVendorConfigDetailItemModel
    {

        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public int MerchantID { get; set; }

        public string MerchantName { get; set; }

        public int GameID { get; set; }

        public string GameName { get; set; }

        public int TotalRound { get; set; }

        public decimal BetAmount { get; set; }

        public decimal PayoutAmount { get; set; }

        public decimal WinLossAmount { get; set; }

        public decimal RTP { get; set; }

        public decimal WinRate { get; set; }
    }





}
