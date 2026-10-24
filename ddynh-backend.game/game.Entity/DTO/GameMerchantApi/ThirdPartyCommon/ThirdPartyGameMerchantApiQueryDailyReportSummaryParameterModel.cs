using System;

namespace game.Entity.ViewModels
{
    public class ThirdPartyGameMerchantApiQueryDailyReportSummaryParameterModel
    {
        public int PlatformID { get; set; }

        public DateTime BeginDateUTC { get; set; }

        public DateTime EndDateUTC { get; set; }

        public string GameCode { get; set; }
    }


}
