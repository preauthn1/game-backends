using Newtonsoft.Json;

namespace game.Entity.Game
{
    public class ThubRequestResponseModel<T> : BaseThubRequestResponseModel where T: class 
    {

        public T Value { get; set; }

    }

    public class BaseThubRequestResponseModel
    {
        [JsonProperty("Code")]
        public int Code { get; set; }

        [JsonProperty("Message")]
        public string Message { get; set; }

    }


    public class THubBalanceResponseModel 
    {
        public decimal Balance { get; set; }

        public string SessionToken { get; set; }

    }

    public class THubAuthTokenResponseModel
    {

        public string PlayerCode { get; set; }

        public string Currency { get; set; }

        public string SessionToken { get; set; }

    }


    public class THubBetAPIResponseModel 
    {
        public string GameRoundId { get; set; }
        public string TransactionId { get; set; }
        public string ExternalTransactionId { get; set; }
        public string ProcessAt { get; set; }
        public decimal StartBalance { get; set; }
        public decimal EndBalance { get; set; }
    }

}
