namespace target.Models.ViewModels
{
    public class AGGLiveBalanceApiResponseDataModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public AGGLiveBalanceApiResponseDetailDataModel result { get; set; }
    }


    public class AGGLiveBalanceApiResponseDetailDataModel
    {
        public decimal balance { get; set; }
    }




}
