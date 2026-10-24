using target.Models.Table;

namespace target.Models.ViewModels
{
    public class GameVendorCopyInfoExtraTextModel
    {
        public VendorSessionTokenEntity LastSessionTokenQuery { get; set; }
        public string VendorConfigInfo { get; set; }
        public string LastTokenText { get; set; }
        public string WalletModeInfo { get; set; }
        public string BetRecordNoPGS { get; set; }
        public string ThirdPartyBetRecordNo { get; set; }
    }


}
