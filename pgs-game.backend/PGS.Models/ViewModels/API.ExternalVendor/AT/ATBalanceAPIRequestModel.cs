namespace target.Models.ViewModels
{
    public class ATBalanceAPIRequestModel
    {
        public string Token { get; set; }

        public string Username { get; set; }

        public string ReqId { get; set; }
    }


    public class ATBalanceAPIResponseModel
    {
        public decimal Balance { get; set; }

        public string Currency { get; set; }

    }


}
