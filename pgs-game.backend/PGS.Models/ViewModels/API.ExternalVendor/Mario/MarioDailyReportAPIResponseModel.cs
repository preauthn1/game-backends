namespace target.Models.ViewModels
{




    public class MarioDailyReportAPIResponseModel : MarioAPICommonResponse
    {
        public string reportDate { get; set; }

        public string reportDateStr { get; set; }

        public decimal winLose { get; set; }

        public decimal betAmount { get; set; }

        public decimal bonusMember { get; set; }

        public decimal bonusAgent { get; set; }

        public string currency { get; set; }
    }

}
