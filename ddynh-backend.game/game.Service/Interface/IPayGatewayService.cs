using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Users;
using game.Entity.Game;
using game.Entity.Payment;

namespace game.Service
{

    public interface IPayGatewayService
    {

        PayGatewayEntity GetByID(int id);


        List<PayGatewayEntity> GetByIDs(List<int> ids);


        List<PayGatewayEntity> GetByFilter(QueryPayGatewayRequestViewModel filter);


        PagedList<PayGatewayEntity> GetPagingByFilter(QueryPayGatewayRequestViewModel filter);


        CRUDResultModel Create(PayGatewayEntity model);


        CRUDResultModel Edit(PayGatewayEntity model);



    }
}
