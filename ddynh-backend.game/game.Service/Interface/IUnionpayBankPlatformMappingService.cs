using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Users;
using game.Entity.Game;

namespace game.Service
{

    public interface IUnionpayBankPlatformMappingService
    {

        UnionpayBankPlatformMappingEntity GetByID(int id);


        List<UnionpayBankPlatformMappingEntity> GetByIDs(List<int> ids);


        List<UnionpayBankPlatformMappingEntity> GetByFilter(QueryUnionpayBankPlatformMappingRequestViewModel filter);


        CRUDResultModel Edit(UnionpayBankPlatformUpdateViewModel model);


    }
}
