using target.Models.Table;
using target.Models.ViewModels.GameVendor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class BetRecordQueryRequestModel
    {
        public string TransactionNo { get; set; }
    }

    public class FishGameDetailRecordQueryRequestModel
    {
        public string TransactionNo { get; set; }
    }


    public class BetRecordManualRefundRequestModel
    {
        public string TransactionNo { get; set; }

        public string Memo { get; set; }

        public int? ExtraTagID { get; set; }

    }

    public class BatchManualRefundBetRecordRequestModel
    {
        public List<string> ListOfTransactionNo { get; set; }

        public string AllTransactionNoText { get; set; }

        public string Memo { get; set; }

        public bool IsAsync { get; set; }

        public int DelayMilliseconds { get; set; }
    }


    public class BetRecordGeneralInfoViewModel
    {

        public int GameVendorID { get; set; }

        public string GameVendorName { get; set; }

        public string ExtraInfoText { get; set; }

        public string ExtraInfoReferPrizeType { get; set; }

        public int ExtraReferPrizeType { get; set; }

        public bool IsSupportExternalLink { get; set; }

        public bool IsSupportExperimentFeatureForInternalUsers { get; set; }

        public bool IsAccountingIgnore { get; set; }

        public bool IsSingleWalletMode { get; set; }

        /// <summary>
        /// 是否為虛寶卡投注
        /// </summary>
        public bool IsVirtualItemCard { get; set; }

        /// <summary>
        /// 所有注單共通型欄位
        /// </summary>
        public BetRecordGeneralColumnViewModel GeneralInfo { get; set; }


        public PerGameDetailInfo PerGameLogInfo { get; set; }

        public List<RaiseBetInfoModel> RaiseBetInfo { get; set; }

        public List<ExtraAmountInfoModel> ExtraAmountDetail { get; set; }

        public List<EazyKeyAndTextModel> TagMapping { get; set; }


        public int VendorConfigID { get; set; }

        public string VendorConfigAssignMerchantID { get; set; }

        public string Currency { get; set; }

        public string CopyInfoForGameVendor { get; set; }

    }



    public class RaiseBetInfoModel
    {
        public string RaiseTransactionNo { get; set; }

        public decimal Amount { get; set; }

        public int Status { get; set; }

        public DateTime CreateTime { get; set; }
    }


    public class ExtraAmountInfoModel
    {
        public string ThirdTransactionNo { get; set; }

        public decimal Amount { get; set; }

        public DateTime CreateTime { get; set; }

        public string Memo  { get; set; }
    }



    public class BetRecordGeneralColumnViewModel : BetRecordGeneralColumnModel
    {
        public string MerchantName { get; set; }

        public string ManualSettlementUserName { get; set; }

        public bool IsManualSettlement { get; set; }
    }



    public class PerGameDetailInfo
    {
        public long PerGameDetailLogID { get; set; }

        public long PerGameLogID { get; set; }

        public int ServerID { get; set; }

        public List<string> Tags { get; set; }

    }



    public class BetRecordApiReqeustLogAndDebugSqlViewModel
    {
        public string DebugSQL { get; set; }

        public List<ApiRequestLogDebugModel> LogItems { get; set; }

    }


    public class ApiRequestLogDebugModel
    {
        public long Id { get; set; }
        public string UserIdentity { get; set; }
        public string RequestUrl { get; set; }
        public string Doamin { get; set; }
        public string RelativeUrl { get; set; }
        public string HttpMethod { get; set; }
        public string RequestBody { get; set; }
        public string Headers { get; set; }
        public string ResponseText { get; set; }
        public int ResponseStatusCode { get; set; }
        public string IPAddress { get; set; }
        public string IPLocation { get; set; }
        public string Memo { get; set; }
        public string IdentityKey { get; set; }
        public DateTime CreateTime { get; set; }
        public int CreateTimeCategory { get; set; }
        public string StackTrace { get; set; }
        public string DebugInfo { get; set; }
        public long ResponseLength { get; set; }
        public string DebugInfo2 { get; set; }

        /// <summary>
        /// 在成功的reqeust 這一欄紀錄的時間是 API處理時間
        /// </summary>
        public string DebugInfo3 { get; set; }
        public string WebSiteName { get; set; }
        public int? ExecutionTime { get; set; }
        public bool IsErrorLog { get; set; }
        public string UniqueID { get; set; }

    }



}
