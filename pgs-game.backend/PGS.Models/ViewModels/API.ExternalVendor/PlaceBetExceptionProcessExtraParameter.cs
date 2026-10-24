using target.Models.Enums;
using target.Models.Table;
using System;

namespace target.Models.ViewModels
{
    public class PlaceBetExceptionProcessExtraParameter
    {
        public GameVendorTypeEnum VendorID { get; set; }

        public GameVendorPlaceBetCallbackRequestParameter CallbackParam { get; set; }

        public bool IsDelayTooMuchTime { get; set; }

        public PlayerBriefModel QueryPlayer { get; set; }

        public MerchantValidateInfoCacheModel QueryMerchant { get; set; }

        public GameListIdInfoBrief QueryGame { get; set; }

        public string TransactionNoPGS { get; set; }

        public decimal BetAmount { get; set; }

        public Exception Exception { get; set; }
    }

}
