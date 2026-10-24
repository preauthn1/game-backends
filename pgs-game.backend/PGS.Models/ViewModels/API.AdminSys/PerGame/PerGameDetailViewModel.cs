using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    /// <summary>
    /// 回應參數
    /// </summary>
    public class PerGameDetailViewModel
    {
        public long PerGameDetailLogID { get; set; }

        public long PerGameLogID { get; set; }

        public int ServerID { get; set; }

        public string ServerName { get; set; }

        public int GameID { get; set; }

        public int GameVendorID { get; set; }

        public string GameName { get; set; }

        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public int MerchantID { get; set; }

        public string MerchantName { get; set; }


        public decimal? AmountBeforeBet { get; set; }


        /// <summary>
        /// 投注前金幣 (還沒扣掉betAmount)
        /// </summary>
        public decimal CurrentAmount { get; set; }

        /// <summary>
        /// 派彩後金幣
        /// </summary>
        public decimal AfterAmount { get; set; }

        public decimal UpdateAmount { get; set; }

        public decimal BetAmount { get; set; }

        public decimal PayoutAmount { get; set; }

        public decimal ValidAmount { get; set; }

        public decimal JackpotAmount { get; set; }

        public decimal TaxAmount { get; set; }


        public DateTime GameTime { get; set; }

        public bool IsExternalVendorGame { get; set; }

        public string ThirdPartyBetRecordNo { get; set; }

        public string ThirdPartyGameCode { get; set; }

        public bool IsSlotGame { get; set; }

        public bool IsHideAfterAmountInfo { get; set; }

        public bool IsSupportExternalLink { get; set; }

        public bool IsSupportExperimentFeatureForInternalUsers { get; set; }

        public bool IsSupportFishGameDetailRecord { get; set; }

        public bool IsAccountingIgnoreData { get; set; }

        public bool IsAnyPrizeRecordReference { get; set; }

        public bool IsAnyTransferRecordReference { get; set; }

        public string VendorName { get; set; }

        public string TransactionNo { get; set; }

        public decimal ConvertRate { get; set; }

        /// <summary>
        /// 是否延遲結算 投注時間 >>> 結算時間 時間差很遠
        /// </summary>
        public bool IsDelaySettle { get; set; }

        /// <summary>
        /// AGG捕魚特殊使用
        /// </summary>
        public bool IsShowFishGameMoneyRecordButton { get; set; }

        public int? VendorConfigID { get; set; }
    }



}
