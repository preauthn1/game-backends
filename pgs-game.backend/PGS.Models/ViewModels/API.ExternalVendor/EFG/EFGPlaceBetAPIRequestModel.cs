namespace target.Models.ViewModels
{

    public class EFGAPIEncyptRequestModel
    {
        public string Data { get; set; }
    }




    public class EFGPlaceBetAPIRequestModel : EFGTransactionAPIBaseModel
    {
        public int isBuyFeature { get; set; }

    }


    public class EFGSettleAPIRequestModel : EFGTransactionAPIBaseModel
    {        
        public int isBuyFeature { get; set; }
    }


    public class EFGCancelAPIRequestModel : EFGTransactionAPIBaseModel
    {
      


    }

    public class EFGPlaceBalanceAPIRequestModel 
    {
        public string uuid { get; set; }

        public string username { get; set; }
    }





    public class EFGTransactionAPIBaseModel
    {
        public string uuid { get; set; }

        public string betId { get; set; }

        public string gameCode { get; set; }

        public string username { get; set; }

        public decimal amount { get; set; }

        public long updateTime { get; set; }
    }




    public class EFGBetTransactionResponseModel
    {
        public string uuid { get; set; }

        public string betId { get; set; }

        public string gameCode { get; set; }

        public string username { get; set; }

        public decimal amount { get; set; }
    }




}
