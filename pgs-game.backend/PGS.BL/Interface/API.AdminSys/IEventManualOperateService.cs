using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.API.AdminSys;

namespace target.BL.Interface
{

    public interface IEventManualOperateService
    {
        EventRankPreviewViewModel PreviewEventPlayerRank(int userID, EventRankPreviewRequestModel filter);

        void ManualTriggerRankPrize(int userID, EventSimpleRequestModel model);

    }
}
