using System.Collections.Generic;

namespace target.Models.ViewModels
{
        

    public class AGGBonusSummaryRecord
    {
        public decimal totalWin { get; set; }
        public int count { get; set; }
        public string bonusChannel { get; set; }
    }

    public class AGGBonusSummaryAPIResponse
    {
        public List<AGGBonusSummaryRecord> records { get; set; }
        public string requestId { get; set; }
    }


}
