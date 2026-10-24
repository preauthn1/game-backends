using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity;
using game.Entity.Users;

namespace game.Service
{
   
    public interface IFlowAmountLimitService
    {

        PagedList<UserFlowLimit> GetFlowAmountLimitLog(QueryFlowAmountLimitRequestViewModel filter);


        CRUDResultModel UpdateStatus(CommonUpdateStatusRequestModel model);

    }
}
