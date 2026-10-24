using Newtonsoft.Json;

namespace target.Models.ViewModels
{


    public class EFGBetTransactionAPIResponseModel : EFGBaseResponseModel
    {

        public EFGBetTransactionAPIResponseDetailModel data { get; set; }

    }



    public class EFGBalanceAPIResponseModel : EFGBaseResponseModel
    {
        public EFGBalanceAPIResponseDetailModel data { get; set; }

    }



    public class EFGBetTransactionAPIResponseDetailModel 
    {
        public decimal balanceOld { get; set; }

        public decimal balance { get; set; }

    }


    public class EFGBalanceAPIResponseDetailModel
    {
        public decimal balance { get; set; }

    }



    public class EFGBaseResponseModel
    {
        public EFGBaseResponseModel ()
        {
            this.status = 200;
        }


        public int status { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string message { get; set; }

    }



}
