namespace target.Utility.Utility.ThirdParty
{
    public class TeamsTokenGenerateApiResponse
    {
        public string error { get; set; }

        public string error_description { get; set; }

        public int expires_in { get; set; }

        public string token_type { get; set; }

        public string access_token { get; set; }
    }

   
}
