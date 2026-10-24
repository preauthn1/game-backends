using target.Models.Enums;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels.GameVendor
{



    public class ExternalVendorGetReportRequestParameter
    {

        public int VendorConfigID { get; set; }

        /// <summary>
        /// 請統一傳UTC過來，再根據遊戲商不同 自行轉成對方接受的時區
        /// 若查詢 2020-01-01 當日的報表，StartTimeUTC 會帶入 2020-01-01 00:00:00
        /// </summary>
        public DateTime StartTimeUTC { get; set; }


        /// <summary>
        /// 若查詢 2020-01-01 當日的報表，EndTimeUTC 會帶入 2020-01-02 00:00:00
        /// 請注意各遊戲商API接收的endTime格式要再自行做轉換 (e.g. 起始結束 要同一日)
        /// </summary>
        public DateTime EndTimeUTC { get; set; }


        /// <summary>
        /// 純粹測試用 指定特定gameIDs
        /// </summary>
        public List<int> Debug_GameIDs { get; set; }



        public string DebugRequestUUID { get; set; }

    }



    public class ExternalVendorGetReportResultModel
    {
        public ExternalVendorGetReportResultModel()
        {
            this.EachGameSummary = new List<GameVendorAccountingSummaryReportModel>();
        }

        public string ResponseText { get; set; }

        public string AssignVendorID { get; set; }

        public List<GameVendorAccountingSummaryReportModel> EachGameSummary { get; set; }

        public BetRecordSummaryReportModel DbSummary { get; set; }

        public BetRecordSummaryReportModel ApiSummary { get; set; }

    }


    public class BetRecordSummaryReportModel
    {

        public int TotalBetCount { get; set; }
        public decimal TotalBetAmount { get; set; }
        public decimal TotalValidBetAmount { get; set; }
        public decimal TotalPayoutAmount { get; set; }
        public decimal TotalWinLoss { get; set; }


        /// <summary>
        /// 定義哪幾個欄位要做對帳檢查
        /// </summary>
        public List<GameVendorReportApiAvailableColumnsEnum> AvailableColumns { get; set; }

    }



    public class GameVendorAccountingSummaryReportModel
    {

        /// <summary>
        /// 請填寫 +8時區的 日期00:00:00
        /// </summary>
        public DateTime StatDate { get; set; }

        /// <summary>
        /// 三方遊戲代碼
        /// </summary>
        public string ThirdPartyGameCode { get; set; }
        public int TotalBetCount { get; set; }
        public decimal TotalBetAmount { get; set; }
        public decimal TotalValidBetAmount { get; set; }
        public decimal TotalPayoutAmount { get; set; }

        /// <summary>
        /// 玩家視角的輸贏
        /// </summary>
        public decimal TotalWinLoss { get; set; }

        /// <summary>
        /// debug額外資訊
        /// </summary>
        public string Debug_Memo { get; set; }

        public string Debug_AgentID { get; set; }

    }





}
