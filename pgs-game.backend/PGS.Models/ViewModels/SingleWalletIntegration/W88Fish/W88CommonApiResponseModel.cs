namespace game.Entity.Game
{
    public class W88CommonApiResponseModel
    {
        public int status { get; set; }

        public string message { get; set; }
    }


    public class W88BalanceApiResponseModel : W88CommonApiResponseModel
    {
        public decimal balance { get; set; }

        public string userid { get; set; }
    }


    public class W88AuthTokenApiResponseModel : W88CommonApiResponseModel
    {
        public decimal balance { get; set; }

        public string username { get; set; }

        public string token { get; set; }

        public string userid { get; set; }
    }



    public class W88DebitApiResponseModel : W88CommonApiResponseModel
    {
        public decimal balance { get; set; }

        public string userid { get; set; }

        public string wallettrxid { get; set; }
    }

    public class W88TransferApiResultModel
    {
        public decimal AfterBalance { get; set; }

        public string W88TransactionNo { get; set; }

    }


}
