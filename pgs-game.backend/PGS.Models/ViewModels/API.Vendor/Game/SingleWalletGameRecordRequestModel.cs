namespace target.Models.ViewModels
{
    /// <summary>
    /// 請求參數
    /// </summary>
    public class SingleWalletGameRecordRequestModel
    {
        [SwaggerApiRequiredField]
        public string MerchantSecretKey { get; set; }

        [SwaggerApiRequiredField]
        public int MerchantID { get; set; }

        [SwaggerApiRequiredField]
        public string TransactionNo { get; set; }
    }


}
