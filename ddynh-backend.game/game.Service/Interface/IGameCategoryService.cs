using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Users;
using game.Entity.Game;

namespace game.Service
{

    public interface IGameCategoryService
    {

        GameCategoryEntity GetByID(int id);


        List<GameCategoryEntity> GetByIDs(List<int> ids);


        List<GameCategoryEntity> GetByFilter(QueryGameCategoryRequestViewModel filter);


        PagedList<GameCategoryEntity> GetPagingByFilter(QueryGameCategoryRequestViewModel filter);


        CRUDResultModel Create(GameCategoryEntity model);


        CRUDResultModel EditMultiple(List<GameCategoryEntity> model);



    }
}
