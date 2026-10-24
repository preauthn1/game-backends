using target.Models.Enums;

namespace target.Models.ViewModels
{
    public class MerchantWalletTransferRequestModel
    {
        [SwaggerIgnore]
        public WalletTransferTypeEnum Type { get; set; }

        public decimal Amount { get; set; }
    }


    public class MerchantWalletTransferResultViewModel : MerchantWalletInfoViewModel
    {      
        public string PgsOrderNo { get; set; }
    }


    public class PgsTransferResultModel
    {
        public string PgsOrderNo { get; set; }
    }


}
