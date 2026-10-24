namespace game.Entity.Game
{
    public class BasOPSRequestResponseModel
    {

        public string Message { get; set; }

        public string Description { get; set; }

        public long Timestamp { get; set; }
    }


    public class OPSAuthResponseModel : BasOPSRequestResponseModel
    {
        public string Token { get; set; }

        public string Player { get; set; }

        public string Currency { get; set; }

    }

    public class OPSBalanceResponseModel : BasOPSRequestResponseModel
    {
        public decimal Balance { get; set; }

    }

    public class OPSBetAPIResponseModel : BasOPSRequestResponseModel
    {
        public decimal Balance { get; set; }
        public string id { get; set; }

    }

}
