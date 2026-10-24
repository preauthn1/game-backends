namespace target.Models.ViewModels
{
    public class GalaxyBalanceAPIRequestModel
    {
        public string PlayerID { get; set; }

        public string Action { get; set; }
    }


    public class GalaxyBalanceResponseModel 
    {
        public decimal Amount { get; set; }
    }


}
