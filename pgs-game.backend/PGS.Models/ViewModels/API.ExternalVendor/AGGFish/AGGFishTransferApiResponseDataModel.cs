namespace target.Models.ViewModels
{

    public class AGGFishTransferApiRequestaModel
    {
        /// <summary>
        /// 如果是service端值接呼叫 不需此參數
        /// </summary>
        public string Token { get; set; }

        public decimal Amount { get; set; }

        /// <summary>
        /// 在某些情境下。會等於pgs的transactionNo
        /// </summary>
        public string TxtID { get; set; }
    }


    public class AGGFishTransferApiDetailDataModel
    {
        public string PlatformOrderNo { get; set; }

        public string TxtID { get; set; }

        public decimal AfterBalance { get; set; }
    }


    public class AGGFishW88BalanceApiRequestaModel
    {
        public string MerchantID { get; set; }

        public string Token { get; set; }
    }

    public class AGGFishW88BalanceApiDetailDataModel
    {
        public decimal Balance { get; set; }
    }

}
