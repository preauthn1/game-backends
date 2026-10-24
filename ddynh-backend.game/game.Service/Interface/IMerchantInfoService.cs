using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Game;
using game.Entity.Users;

namespace game.Service
{

    public interface IMerchantInfoService
    {
        MerchantInfoEntity GetByID(int id);

        List<MerchantInfoEntity> GetByIDs(List<int> ids);

        List<MerchantInfoEntity> GetByFilter(QueryMerchantInfoRequestViewModel filter);

    }
}
