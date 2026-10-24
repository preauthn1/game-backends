using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.API.AdminSys;

namespace target.BL.Interface
{

    public interface IEventLuckyDrawService
    {
        LuckyDrawResultViewModel UseTicketDrawRandomPrize(int playerID, int eventID);

        LuckyDrawResultViewModel UseEventPointDrawRandomPrize(int playerID, int eventID);

        LuckyDrawResultViewModel UpdateDrawPrizeStatusAndSendBalance(int playerID, int eventID);

        LuckyDrawResultViewModel DrawRandomPrize(LuckyDrawRequestModel model);

    }
}
