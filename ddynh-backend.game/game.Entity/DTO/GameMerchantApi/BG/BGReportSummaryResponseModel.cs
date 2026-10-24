namespace game.Entity.DTO.GameMerchantApi.BG
{
    public class BGReportSummaryResponseModel : BGBaseResponseModel
    {

        public BGWalletAmountResultResponseModel result { get; set; }

    }


    public class BGWalletAmountResultResponseModel
    {
        public int orderCount { get; set; }
        public int userCount { get; set; }
        public decimal orderAmount { get; set; }
        public decimal validOrderAmount { get; set; }
        public decimal validBetAmount { get; set; }
        public decimal paymentAmount { get; set; }
    }



}
