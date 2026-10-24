using target.Models.Paging;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class QueryAgentSpecialReportRequestParameterModel : PaginationParameter
    {

        /// <summary>
        /// 參考 AgentSpecialReportTypeEnum
        /// </summary>
        public int AgentReportType { get; set; }


        public int ParentMerchantID { get; set; }


        public List<int> GameIDs { get; set; }


        public DateTime? Begin { get; set; }


        public DateTime? End { get; set; }


        public int SortDirection { get; set; }        


    }



    public class AgentSpecialReportResultViewModel
    {
        public List<QueryAgentWinlossRankingReportDetailItem> MerchantDataItems { get; set; }

    }



    public class QueryAgentWinlossRankingReportDetailItem
    {
        public int Sequence { get; set; }

        public int MerchantID { get; set; }

        public string MerchantName { get; set; }

        //如果有不同類型的報表 (以商戶為主體) 欄位就共用吧 懶得設計多物件結構了
        //如果有不同類型的報表 (以商戶為主體) 欄位就共用吧 懶得設計多物件結構了
        //如果有不同類型的報表 (以商戶為主體) 欄位就共用吧 懶得設計多物件結構了

        public int TotalRound { get; set; }

        public decimal BetAmount { get; set; }

        public decimal PayoutAmount { get; set; }

        public decimal WinLossAmount { get; set; }


        public decimal MerchantWinLossAmount { get; set; }


        public decimal ProfitRate { get; set; }

     
        //亏损排行，需要有可以筛选的日期，搜索数据结果会显示，各代理账号的，总玩，总得，总盈利，和盈利率																							

    }








}
