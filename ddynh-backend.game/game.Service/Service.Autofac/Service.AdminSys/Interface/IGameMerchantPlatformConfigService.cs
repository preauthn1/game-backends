using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Game;
using game.Entity.ViewModels.Query;

namespace game.Service
{

    public interface IGameMerchantPlatformConfigService
    {

        GameMerchantPlatformConfigRawEntity GetByID(int id);


        List<GameMerchantPlatformConfigRawEntity> GetByIDs(List<int> ids);


        List<GameMerchantPlatformConfigRawEntity> GetByFilter(QueryGameMerchantPlatformConfigRequestViewModel filter);


        PagedList<GameMerchantPlatformConfigRawEntity> GetPagingByFilter(QueryGameMerchantPlatformConfigRequestViewModel filter);


        CRUDResultModel Create(GameMerchantPlatformConfigRawEntity model);


        CRUDResultModel Edit(GameMerchantPlatformConfigRawEntity model);


        CRUDResultModel Delete(int targetID);


        CRUDResultModel CopySettingFromPlatform(int gameMerchantID, int templatePlatformID, int targetPlatformID, string adminUserName);
    }
}
