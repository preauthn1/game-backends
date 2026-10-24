namespace target.Models.ViewModels
{

    public class EFGGameLaunchAPIResponse
    {
        public string url { get; set; }

    }


    public class EFGTokenGenAPIResponse
    {
        public string token { get; set; }

    }

    public class EFGGameKeyGenerateAPIResponse
    {
        public string key { get; set; }

    }


    public class EFGRegisterAPIResponse
    {
        public string username { get; set; }

        public string id { get; set; }

    }


    public class EFGReportSummaryAPIResponse
    {
        public decimal totalBet { get; set; }
        public decimal totalWin { get; set; }
        public decimal profit { get; set; }
        public int featureBet { get; set; }
        public int featureWin { get; set; }
        public int totalPlayerCount { get; set; }
        public int newPlayerCount { get; set; }
    }








}
