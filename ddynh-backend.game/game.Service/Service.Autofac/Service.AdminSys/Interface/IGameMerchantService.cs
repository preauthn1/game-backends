using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Game;
using game.Entity.ViewModels.Query;

namespace game.Service
{

    public interface IGameMerchantService
    {

        GameMerchantRawEntity GetByID(int id);


        List<GameMerchantRawEntity> GetByIDs(List<int> ids);


        List<GameMerchantRawEntity> GetByFilter(QueryGameMerchantRequestViewModel filter);


        PagedList<GameMerchantRawEntity> GetPagingByFilter(QueryGameMerchantRequestViewModel filter);


        CRUDResultModel Create(GameMerchantRawEntity model);


        CRUDResultModel Edit(GameMerchantRawEntity model);


        CRUDResultModel Delete(int targetID);

    }
}
