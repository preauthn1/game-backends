using Microsoft.AspNetCore.Mvc;
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

    /// <summary>
    /// 請求參數
    /// </summary>
    public class GameRecordRequestModel : PaginationParameter, IQueryWagerRequest
    {

        [SwaggerApiRequiredField]
        public string MerchantSecretKey { get; set; }

        [SwaggerApiRequiredField]
        public int MerchantID { get; set; }

        public string PlayerName { get; set; }

        [ModelBinder(typeof(NumberOrBooleanModelBinder))]
        public bool IsSearchExtraDetail { get; set; }

        [ModelBinder(typeof(NumberOrBooleanModelBinder))]
        public bool IsExchangeToOriginalRate { get; set; }
        
        public int? TagID { get; set; }

        [SwaggerApiRequiredField]
        public DateTime StartTime { get; set; }

        [SwaggerApiRequiredField]
        public DateTime EndTime { get; set; }
    }

    public class VendorAPIRequestTimeLockerCache
    {
        /// <summary>
        /// 計數器次數
        /// </summary>
        public int Counter { get; set; }

        public DateTime CreateTime { get; set; }
    }


    public class FishDetailRecordRequestModel : PaginationParameter
    {
        public string MerchantSecretKey { get; set; }

        public int MerchantID { get; set; }

        public string RoundID { get; set; }

        public DateTime RoundSettleTimeBegin { get; set; }

        public DateTime RoundSettleTimeEnd { get; set; }
    }




    /// <summary>
    /// 請求參數
    /// </summary>
    public class VendorReportSummaryRequestModel
    {
        [SwaggerApiRequiredField]
        public string MerchantSecretKey { get; set; }

        [SwaggerApiRequiredField]
        public int MerchantID { get; set; }

        [SwaggerApiRequiredField]
        public DateTime StartTime { get; set; }

        [SwaggerApiRequiredField]
        public DateTime EndTime { get; set; }

        [ModelBinder(typeof(NumberOrBooleanModelBinder))]
        public bool IsExchangeToOriginalRate { get; set; }

    }


    /// <summary>
    /// (VendorAPI) 注單流水資料
    /// </summary>
    public class GameRecordViewModel
    {
        public string PlayerName { get; set; }

        public int GameID { get; set; }


        public decimal WinLossAmount { get; set; }

        /// <summary>
        /// 局號
        /// </summary>
        public string RoundID { get; set; }


        public string TransactionNo { get; set; }


        /// <summary>
        /// 投注識別ID
        /// </summary>
        public string BetID { get; set; }

        public DateTime BetTime { get; set; }


        /// <summary>
        /// 投注金額
        /// </summary>
        public decimal BetAmount { get; set; }

        /// <summary>
        /// 派彩金額
        /// </summary>
        public decimal PayoutAmount { get; set; }


        /// <summary>
        /// 稅收金額
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// 有效流水
        /// </summary>
        public decimal ValidAmount { get; set; }


        public GameRecordExtraInfoViewModel ExtraInfo { get; set; }



        /// <summary>
        /// (已棄用欄位) 文件有標註
        /// </summary>
        public decimal BeforeAmount { get; set; }


        /// <summary>
        /// (已棄用欄位) 文件有標註
        /// </summary>
        public DateTime RoundStartTime { get; set; }

        /// <summary>
        /// (已棄用欄位) 文件有標註
        /// </summary>
        public DateTime RoundEndTime { get; set; }



    }


    public class FishGameDetailRecordViewModel
    {
        public string PlayerName { get; set; }

        public int GameID { get; set; }

        public decimal WinLossAmount { get; set; }

        /// <summary>
        /// 局號
        /// </summary>
        public string RoundID { get; set; }


        public string WagerID { get; set; }

        public int FishType { get; set; }


        public DateTime BetTime { get; set; }


        public DateTime RoundSettleTime { get; set; }


        /// <summary>
        /// 投注金額
        /// </summary>
        public decimal BetAmount { get; set; }

        /// <summary>
        /// 派彩金額
        /// </summary>
        public decimal PayoutAmount { get; set; }

        /// <summary>
        /// 有效流水
        /// </summary>
        public decimal ValidAmount { get; set; }


        public GameRecordExtraInfoViewModel ExtraInfo { get; set; }

    }


    public class GameRecordExtraInfoViewModel
    {
        [SwaggerIgnore]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? GameType { get; set; }

        [SwaggerIgnore]
        public decimal JackpotAmount { get; set; }

        [SwaggerIgnore]
        public string RoundID { get; set; }

        [SwaggerIgnore]
        public decimal RoundTotalFlowAmount { get; set; }
    }


    public class PlayerWinLossReportSummaryRequestModel : PaginationParameter
    {
        [SwaggerApiRequiredField]
        public string MerchantSecretKey { get; set; }


        [SwaggerApiRequiredField]
        public int MerchantID { get; set; }

        public int? GameID { get; set; }

        public string PlayerName { get; set; }

        [SwaggerApiRequiredField]
        public DateTime StartTime { get; set; }

        [SwaggerApiRequiredField]
        public DateTime EndTime { get; set; }


        [ModelBinder(typeof(NumberOrBooleanModelBinder))]
        public bool IsExchangeToOriginalRate { get; set; }
    }




    public class PlayerWinLossReportSummaryDetailItemModel
    {
        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public int GameID { get; set; }

        public string GameName { get; set; }

        public int TotalRound { get; set; }

        public decimal BetAmount { get; set; }

        public decimal PayoutAmount { get; set; }

        public decimal WinLossAmount { get; set; }

        public decimal RTP { get; set; }
    }








}
