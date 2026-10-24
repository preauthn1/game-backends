namespace target.Models.ViewModels
{

    public class MarioDepositAPIRequestModel : IMarioSeamlessRequest
    {
        public string api_id { get; set; }

        public long timestamp { get; set; }

        public string sign { get; set; }

        public int user_id { get; set; }

        public decimal amount { get; set; }

        public string log_id { get; set; }

    }

    public class MarioDepositWithdrawalAPIResponseModel : MarioAPICommonResponse
    {
        /// <summary>
        /// 如果是結算API。此欄位為投注前的金額
        /// </summary>
        public decimal beginBalance { get; set; }

        public decimal endBalance { get; set; }

    }

}
