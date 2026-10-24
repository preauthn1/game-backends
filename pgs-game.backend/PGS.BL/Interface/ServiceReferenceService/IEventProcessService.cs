using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IEventProcessService
    {
        List<EventRuleCheckResultDebugInfoModel> WhenEnterGame(EventProcessModel model);

        List<EventRuleCheckResultDebugInfoModel> WhenSettlement(EventProcessModel model);

        void PreQueryEventBetCounter(int eventID, int playerID);

        EventPrizeBundleCheckResult CheckSpecificPrizeBundlePassAndSendPrize(int eventID, int playerID, EventPrizeBundleViewModel prizeBundle);

        EventPrizeBundleCheckResult CheckSpecificPrizeBundlePassWithoutSendPrize(int eventID, int playerID, EventPrizeBundleViewModel prizeBundle);
    }
}
