namespace target.Models.ViewModels
{
    public class JiliLiveGameBetAPIResponseModel : JiliAPICommonResponse
    {
        public string Username { get; set; }

        public string Currency { get; set; }

        public decimal Balance { get; set; }

    }
}
