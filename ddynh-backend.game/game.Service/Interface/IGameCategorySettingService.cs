using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Users;
using game.Entity.Game;

namespace game.Service
{

    public interface IGameCategorySettingService
    {

        GameCategorySettingEntity GetByID(int id);


        List<GameCategorySettingEntity> GetByIDs(List<int> ids);


        List<GameCategorySettingEntity> GetByFilter(QueryGameCategorySettingRequestViewModel filter);


        CRUDResultModel EditMultiple(List<GameCategorySettingEntity> models);



    }
}
