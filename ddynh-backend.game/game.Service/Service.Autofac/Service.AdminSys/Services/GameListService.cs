using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Service.DAO;
using game.Infrastructure;
using game.Entity.ViewModels.Query;
using game.Entity.Game;
using game.Entity;
using game.Infrastructure.Helper;
using game.Entity.Money;
using game.Infrastructure.Extensions;


namespace game.Service.Service
{
    public class GameListService : BaseService, IGameListService
    {

        public GameListRepository GameListRepository { get; set; }


        public GameListRawEntity GetByID(int id)
        {
            var query = GameListRepository.GetById(id);
            return query;
        }

        public List<GameListRawEntity> GetByIDs(List<int> ids)
        {
            var query = GameListRepository.GetByIds(ids);
            return query;
        }

        public List<GameListRawEntity> GetByFilter(QueryGameListRequestViewModel filter)
        {
            var query = GameListRepository.GetByFilter(filter);
            return query;
        }


        public PagedList<GameListRawEntity> GetPagingByFilter(QueryGameListRequestViewModel filter)
        {
            var query = GameListRepository.GetPagingByFilter(filter);
            return query;
        }


        public CRUDResultModel Create(GameListRawEntity model)
        {
            model.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.GameName).Rule_NotEmptyText();
            model.RuleFor(x => x.ShortName).Rule_NotEmptyText();

            var queryExsit = GameListRepository.GetById(model.GameID);

            if (queryExsit != null)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);

            var resultId = GameListRepository.Create(model);
            return new CRUDResultModel
            {
                ResultId = resultId,
                IsSuccess = true,
                Message = "新增成功",
            };
        }


        public CRUDResultModel Edit(GameListRawEntity model)
        {
            model.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.GameName).Rule_NotEmptyText();
            model.RuleFor(x => x.ShortName).Rule_NotEmptyText();

            var queryExsit = GameListRepository.GetById(model.GameID);

            if (queryExsit == null)
                throw new TerproException(ApiErrorCode.NotFoundData);


            queryExsit.GameName = model.GameName;
            queryExsit.ShortName = model.ShortName;
            queryExsit.Status = model.Status;

            GameListRepository.Update(queryExsit);
            return new CRUDResultModel(true);
        }



    }

}
