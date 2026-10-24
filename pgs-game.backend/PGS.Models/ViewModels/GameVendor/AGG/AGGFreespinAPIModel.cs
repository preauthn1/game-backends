using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels.GameVendor
{
    public class AGGGameFreespinListReqeustParameter : IMerchantIDVendorConfigID
    {
        public int? MerchantID { get; set; }
        public int? VendorConfigID { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string FreespinID { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    public class AGGGameFreespinSetReqeustParameter : IMerchantIDVendorConfigID
    {
        public int? MerchantID { get; set; }
        public int? VendorConfigID { get; set; }
        public List<int> PlayerIDs { get; set; }
        public VendorAPIFreespinSetReqeustParameter VendorAPIParam { get; set; }

    }



    public class AGGAPIFreespinDisableReqeustParameter : IMerchantIDVendorConfigID
    {
        public int? MerchantID { get; set; }
        public int? VendorConfigID { get; set; }
        public string FreeSpinID { get; set; }  

    }



    public class AGGAPIFreespinStatusReqeustParameter : IMerchantIDVendorConfigID
    {
        public int? MerchantID { get; set; }
        public int? VendorConfigID { get; set; }
        public int? UserID { get; set; }
        public VendorAPIFreespinStatusReqeustParameter VendorAPIParam { get; set; }

    }


    public class AGGAPIFreespinRecordRequestParameter : IMerchantIDVendorConfigID
    {
        public int? MerchantID { get; set; }
        public int? VendorConfigID { get; set; }
        public VendorAPIFreespinRecordReqeustParameter VendorAPIParam { get; set; }
    }


    public class AGGAPIFreespinSummaryRequestParameter : IMerchantIDVendorConfigID
    {
        public int? MerchantID { get; set; }
        public int? VendorConfigID { get; set; }
        public VendorAPIFreespinSummaryReqeustParameter VendorAPIParam { get; set; }
    }


    public class AGGAPIFreespinStatusResponseModel
    {
        public List<AGGAPIFreespinStatusDetailItemModel> freeSpinStatuses { get; set; }
        public int total { get; set; }
    }


    public class AGGAPIFreespinStatusDetailItemModel
    {
        public string UserId { get; set; }
        public string freespinID { get; set; }
        public string ClientId { get; set; }
        public int LastRounds { get; set; }
        public int TotalRounds { get; set; }
        public decimal TotalWin { get; set; }
        public string FreeSpinUseStatus { get; set; }
        public string CreateDate { get; set; }
        public string ModifyDate { get; set; }
    }



    public class AGGAPIFreespinRecordResponseModel
    {


    }



    public class AGGAPIFreespinSummaryResponseModel
    {
        public List<AGGAPIFreespinSummaryDetailItemModel> freeSpinStatisticals { get; set; }

    }


    public class AGGAPIFreespinSummaryDetailItemModel
    {
        public string StatisticalTime { get; set; }
        public string FreeSpinId { get; set; }
        public string GameId { get; set; }
        public decimal TotalBet { get; set; }
        public decimal TotalWin { get; set; }
        public decimal TotalUser { get; set; }
        public decimal TotalRounds { get; set; } 
    }


    public class AGGFreespinGameListItemViewModel
    {
        public int GameID { get; set; }

        public decimal Bet { get; set; }

        [SwaggerIgnoreOutputWhenPagedList]
        [SwaggerIgnore]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ThirdGameID { get; set; }
    }


    public class AGGFreespinGameListItem
    {
        [JsonProperty(PropertyName = "gameId")]
        public string GameID { get; set; }

        [JsonProperty(PropertyName = "bet")]
        public decimal Bet { get; set; }
    }
       

    public interface IMerchantIDVendorConfigID
    {
        int? MerchantID { get; set; }
        int? VendorConfigID { get; set; }
    }


    public class AGGFreespinListAPIResponseModel
    {
        public string requestId { get; set; }
        public List<AGGFreespinCampaignDetailModel> freeSpinCampaigns { get; set; }
        public int total { get; set; }
    }


    public class AGGFreespinSetAPIResponseModel
    {
        public string requestId { get; set; }

        public string message { get; set; }

        public AGGFreespinCampaignDetailModel freeSpinCampaign { get; set; }

    }


    public class AGGFreespinDisableAPIResponseModel
    {
        public string requestId { get; set; }

        public AGGFreespinCampaignDetailModel freeSpinCampaign { get; set; }

    }


    public class AGGFreespinCampaignDetailModel
    {
        public string freeSpinId { get; set; }
        public decimal creditRate { get; set; }
        public string clientId { get; set; }
        public int rounds { get; set; }
        public List<AGGFreespinGameListItem> gameList { get; set; }
        public List<string> userIdList { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string useEndDate { get; set; }
        public string enable { get; set; }
        public string createDate { get; set; }
        public string modifyDate { get; set; }
        public string freeSpinStatus { get; set; }
    }


    public class AGGAPIFreespinBudgetListRequestParameter : IMerchantIDVendorConfigID
    {
        public int? MerchantID { get; set; }
        public int? VendorConfigID { get; set; }
        public VendorAPIFreespinBudgetListReqeustParameter VendorAPIParam { get; set; }
    }

    public class AGGAPIFreespinBudgetListResponseModel
    {

        public List<AGGAPIFreespinBudgetSetDetailInfoModel> FreeSpinBudgets { get;set; }

    }


    public class AGGAPIFreespinBudgetSetRequestParameter : IMerchantIDVendorConfigID
    {
        public int? MerchantID { get; set; }
        public int? VendorConfigID { get; set; }
        public VendorAPIFreespinBudgetSetReqeustParameter VendorAPIParam { get; set; }
    }


    public class AGGAPIFreespinBudgetSetResponseModel
    {
        public string message { get; set; }

        public AGGAPIFreespinBudgetSetDetailInfoModel FreeSpinBudget { get; set; }

    }


    public class AGGAPIFreespinBudgetSetDetailInfoModel
    {
        public string BudgetId { get; set; }
        public bool Enable { get; set; }
        public string Name { get; set; }
        public decimal RemainAmount { get; set; }
        public long RemainRounds { get; set; }
        public string CreatedTime { get; set; }
        //public string freeSpinIds { get; set; }
        public string ClientId { get; set; }
        public string Remark { get; set; }
    }









    public class AGGAPIFreespinBudgetDepositRequestParameter : IMerchantIDVendorConfigID
    {
        public int? MerchantID { get; set; }
        public int? VendorConfigID { get; set; }
        public VendorAPIFreespinBudgetDepositReqeustParameter VendorAPIParam { get; set; }
    }

    public class AGGAPIFreespinBudgetDepositResponseModel
    {
        public AGGAPIFreespinBudgetBalanceUpdateDetailResponseModel FreeSpinBudgetWallet { get; set; }

    }


    public class AGGAPIFreespinBudgetWithdrawalRequestParameter : IMerchantIDVendorConfigID
    {
        public int? MerchantID { get; set; }
        public int? VendorConfigID { get; set; }
        public VendorAPIFreespinBudgetWithdrawalReqeustParameter VendorAPIParam { get; set; }
    }


    public class AGGAPIFreespinBudgetBalanceUpdateDetailResponseModel
    {
        public decimal remainAmount { get; set; }

        public long remainRounds { get; set; }

    }

}
