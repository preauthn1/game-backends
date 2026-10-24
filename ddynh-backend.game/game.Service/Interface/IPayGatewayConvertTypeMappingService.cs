using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Users;
using game.Entity.Game;
using game.Entity.Payment;

namespace game.Service
{

    public interface IPayGatewayConvertTypeMappingService
    {

        PayGatewayConvertTypeMappingEntity GetByID(int id);


        List<PayGatewayConvertTypeMappingEntity> GetByIDs(List<int> ids);


        List<PayGatewayConvertTypeMappingEntity> GetByFilter(QueryPayGatewayConvertTypeMappingRequestViewModel filter);
        

        CRUDResultModel EditAndDelete(PayGatewayConvertTypeMappingUpdateRequestModel model);



    }
}
