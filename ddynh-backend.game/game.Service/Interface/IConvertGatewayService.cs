using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Users;
using game.Entity.Game;

namespace game.Service
{

    public interface IConvertGatewayService
    {

        ConvertGatewayEntity GetByID(int id);


        List<ConvertGatewayEntity> GetByIDs(List<int> ids);


        List<ConvertGatewayEntity> GetByFilter(QueryConvertGatewayRequestViewModel filter);


        PagedList<ConvertGatewayEntity> GetPagingByFilter(QueryConvertGatewayRequestViewModel filter);


        CRUDResultModel Create(ConvertGatewayEntity model);


        CRUDResultModel Edit(ConvertGatewayEntity model);



    }
}
