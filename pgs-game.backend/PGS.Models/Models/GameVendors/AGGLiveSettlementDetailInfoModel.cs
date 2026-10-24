using System.Collections.Generic;

namespace target.Models.ViewModels
{

    public class AGGLiveSettlementDetailInfoModel
    {
        public string serverId { get; set; }
        public string roundId { get; set; }
        public string roundString { get; set; }
        public int betIndex { get; set; }
        public int betLength { get; set; }
        public List<AGGLiveSettlementDetailBetListModel> BetList { get; set; }
        public object roundResult { get; set; }
        public bool isPlayerOffline { get; set; }
        public int returnReason { get; set; }
    }



    public class AGGLiveSettlementDetailBetListModel
    {
        public int type { get; set; }
        public int index { get; set; }
        public decimal amount { get; set; }
        public decimal exchangedAmount { get; set; }
        public decimal win { get; set; }
        public bool noCommission { get; set; }
        public decimal prebet { get; set; }
        public decimal tax { get; set; }
    }
}
