using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Game;
using game.Entity.ViewModels.Query;

namespace game.Service
{

    public interface IGameListService
    {

        GameListRawEntity GetByID(int id);


        List<GameListRawEntity> GetByIDs(List<int> ids);


        List<GameListRawEntity> GetByFilter(QueryGameListRequestViewModel filter);


        PagedList<GameListRawEntity> GetPagingByFilter(QueryGameListRequestViewModel filter);


        CRUDResultModel Create(GameListRawEntity model);


        CRUDResultModel Edit(GameListRawEntity model);


    }
}
