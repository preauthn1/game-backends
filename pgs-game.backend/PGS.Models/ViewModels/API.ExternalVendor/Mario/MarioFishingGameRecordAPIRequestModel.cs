namespace target.Models.ViewModels
{
    public class MarioFishingGameRecordAPIRequestModel : IMarioSeamlessRequest
    {
        public string api_id { get; set; }

        public long timestamp { get; set; }

        public string sign { get; set; }

        public int user_id { get; set; }

        public string game_round_id { get; set; }

        public string game_code { get; set; }

        public decimal bet_amount { get; set; }

        public decimal win_amount { get; set; }


    }

}
