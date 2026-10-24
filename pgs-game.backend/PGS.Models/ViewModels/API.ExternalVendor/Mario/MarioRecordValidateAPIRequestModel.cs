namespace target.Models.ViewModels
{
    public class MarioRecordValidateAPIRequestModel : IMarioSeamlessRequest
    {
        public string api_id { get; set; }

        public long timestamp { get; set; }

        public string sign { get; set; }

        public int user_id { get; set; }

        public string log_id { get; set; }
    }


    public class MarioTransactionValidateAPIResponseModel : MarioAPICommonResponse
    {
        public decimal beginBalance { get; set; }

        public decimal endBalance { get; set; }

        public string transactionDate { get; set; }

        public decimal beginWallet { get; set; }

        public decimal endWallet { get; set; }

        public string remark { get; set; }
    }

}
