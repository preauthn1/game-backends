using Newtonsoft.Json;
using target.Models.Table;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class Debug_AGGWagerRecordReqeustParameter
    {
        public string GameCode { get; set; }

        /// <summary>
        /// 三方的商戶號
        /// </summary>
        public string AssignVendorID { get; set; }

        public int SplitMinutes { get; set; }

        public string AuthToken { get; set; }


        public int PageSize { get; set; }




        /// <summary>
        /// 請自行帶UTC時間進來
        /// </summary>
        public DateTime Begin { get; set; }


        public DateTime End { get; set; }


        /// <summary>
        /// 使用 internalAPI 或 後台API
        /// </summary>
        public bool IsUseInternalApi { get; set; }

    }


    public class Debug_FishingWagerRecordReqeustParameter
    {       
        public string VendorID { get; set; }

        public int SplitMinutes { get; set; }

        /// <summary>
        /// 請自行帶UTC時間進來
        /// </summary>
        public DateTime Begin { get; set; }


        public DateTime End { get; set; }

    }



    public class AGGBackofficeGameSummaeyApiParameter
    {
        public bool IsExcludeCNYCurrencyData { get; set; }

        public string IntegratorID { get; set; }

        public int VendorConfigID { get; set; }

        public string AuthToken { get; set; }

        public DateTime Begin { get; set; }

        public DateTime End { get; set; }

    }


    public class AGGBackofficeGameSummaryResponseModel
    {

        public decimal totalBet { get; set; }
        public decimal totalLose { get; set; }
        public decimal totalWin { get; set; }
        public decimal totalJpWin { get; set; }
        public decimal count { get; set; }

    }

    public class AGGGameSettingApiResponseModel
    {
        public List<AGGGameSettingApiDetailItemsModel> Settings { get; set; }
    }

    public class AGGGameSettingFlagParameter
    {

        public int MerchantID { get; set; }

        /// <summary>
        /// 不填寫(空值) 代表關閉
        /// </summary>
        public string SettingID { get; set; }


        public AGGPromotionSettingServiceTypeEnum ServiceType { get; set; }


        public bool IsDisable { get; set; }
    }



    /// <summary>
    /// 這裡的enum toString 會等於IDG文件中的代碼
    /// </summary>
    public enum AGGPromotionSettingServiceTypeEnum
    {
        None,

        vip_level_setting,

        vip_reward_setting,

        mission_setting,

        ranking_setting,

        luckydraw_setting,
    }



    public class AGGGameSettingApiDetailItemsModel
    {
        public string Id { get; set; }

        public string SettingId { get; set; }
    }


    public class AGGAccountingRecordsResponseModel
    {
        public List<AGGAccountingWagetItemResponseModel> Wagers { get; set; }
         
        public List<ThirdPartyFetchRecordBriefEntity> WagersV2 { get; set; }

        public int Total { get; set; }

        public string ResponseText { get; set; }
    }



    public class AGGNamespaceCreateResponseModel
    {
     
        public int Total { get; set; }

    }



    public class AGGAccountingWagetItemResponseModel
    {
        public string userId { get; set; }
        public string wagerId { get; set; }
        public string gameId { get; set; }
        public string beginDateTime { get; set; }
        public string endDateTIme { get; set; }
        public decimal totalBet { get; set; }
        public decimal totalWin { get; set; }
        public string totalLose { get; set; }
        public decimal totalJpWin { get; set; }
    }





    public class EachCateogryGameSummaryItem
    {
        public string currency { get; set; }
        public bool isUseItem { get; set; }
        public double timestamp { get; set; }
        public string gameId { get; set; }
        public decimal totalBet { get; set; }
        public decimal totalWin { get; set; }
        public decimal totalJpWin { get; set; }
        public decimal totalLose { get; set; }
        public int totalRounds { get; set; }
    }

    public class AGGBackofficeAPIEachVendorAccountingModel
    {
        public List<string> currencyTypes { get; set; }

        public List<string> gameIds { get; set; }

        public List<EachCateogryGameSummaryItem> data { get; set; }

        public AGGBackofficeAccountingAPIAgentDataResponse agentData { get; set; }        
    }


    public class AGGBackofficeAccountingAPIAgentDataResponse
    {
        [JsonProperty(PropertyName = "default")]
        public List<EachCateogryGameSummaryItem> DefaultData { get; set; }

    }


    public class AGGBackofficeAccountingAPIResponse
    {
        public List<string> integratorIds { get; set; }

        public Dictionary<string , AGGBackofficeAPIEachVendorAccountingModel> data { get; set; }
    }




}
