using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class AGGSingleWalletAPITransactionRequestModel
    {

        public string RequestId { get; set; }

        /// <summary>
        /// 對應我方的playerID
        /// </summary>

        public int UserID { get; set; }

        public string TransactionId { get; set; }

        public string AccountToken { get; set; }

        public string UserToken { get; set; }

        public string GameSessionId { get; set; }

        public string GameId { get; set; }

        public string WagerId { get; set; }

        public decimal? Amount { get; set; }

        public string FreeSpinId { get; set; }

        public decimal? FreeSpinAmount { get; set; }


        /// <summary>
        /// V2版本 詳細資訊主要存這邊
        /// </summary>
        public AGGSingleWalletTransactionAPIMetaModel Meta { get; set; }


        /// <summary>
        /// V1版本 詳細資訊主要存這邊
        /// </summary>
        public List<AGGSingleWalletOperationRequestModel> Operations { get; set; }


        public string ExecutionTime { get; set; }

    }


    public class AGGSingleWalletTransactionAPIMetaModel
    {
        public List<AGGSingleWalletOperationRequestModel> Operations { get; set; }
    }



    public class AGGSingleWalletAPIBonusWinRequestModel
    {

        public string RequestId { get; set; }

        /// <summary>
        /// 對應我方的playerID
        /// </summary>

        public int UserID { get; set; }

        public string TransactionId { get; set; }

        public decimal Amount { get; set; }

        public AGGSingleWalletCurrencyItemViewModel Currency { get; set; }

        public string BonusChannel { get; set; }

        public string BonusId { get; set; }

        public string Remark { get; set; }

        public string ExecutionTime { get; set; }
    }



    public class AGGSingleWalletOperationRequestModel
    {
        public string OperationType { get; set; }

        public string ItemId { get; set; }

        public string TagType { get; set; }

        public string Action { get; set; }

        public decimal Withdraw { get; set; }

        public decimal Deposit { get; set; }

        public string ExecutionTime { get; set; }

        public AggTagModel Tag { get; set; }

        public AggCodeModel Code { get; set; }

    }


    public class AggFreespinCampainDetailModel
    {

        /// <summary>
        /// 等同於freespinID
        /// </summary>
        public string CampaignId { get; set; }

        /// <summary>
        /// freespin的本金是多少
        /// </summary>
        public decimal Bet { get; set; }
    }



    public class AggCodeModel
    {
        public AggfeatureItemModel FeatureItem { get; set; }
    }



    public class AggfeatureItemModel
    {
        public string Name { get; set; }

        public decimal bet { get; set; }

        public decimal cost { get; set; }
    }


    public class AggTagModel
    {
        public decimal? RTP { get; set; }

        public string Feature { get; set; }

        public AGGItemTagInfo Item { get; set; }

        public AGGBuyFeatureSetting BuyFeatureSetting { get; set; }

        public List<AggTagItemModel> Actions { get; set; }

        public AggFreespinCampainDetailModel FreeSpinItem { get; set; }

    }


    public class AGGItemTagInfo
    {
        public string Id { get; set; }
    }


    public class AGGBuyFeatureSetting
    {
        public decimal CostLimit { get; set; }
    }


    public class AggTagItemModel
    {
        public string Action { get; set; }

        public decimal MoneyAmount { get; set; }
    }


    public class AGGSingleWalletAPIPlaceBetResponseViewModel
    {

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RequestId { get; set; }

        public List<AGGSingleWalletEachBalanceItemViewModel> Balances { get; set; }

        public string StatusCode { get; set; }

    }


    public class AGGSingleWalletCashBonusResponseViewModel
    {

        public string RequestId { get; set; }

        public List<AGGSingleWalletEachBalanceItemViewModel> Balances { get; set; }

        public string StatusCode { get; set; }

        public string ReferenceId { get; set; }

    }





}
