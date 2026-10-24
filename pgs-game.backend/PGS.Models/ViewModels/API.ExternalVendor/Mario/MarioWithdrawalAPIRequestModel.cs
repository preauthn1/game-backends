namespace target.Models.ViewModels
{
    public class MarioWithdrawalAPIRequestModel: IMarioSeamlessRequest
    {
        public string api_id { get; set; }

        public long timestamp { get; set; }

        public string sign { get; set; }

        public int user_id { get; set; }

        public decimal amount { get; set; }

        public long log_id { get; set; }

    }

}
