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
    public class GameMerchantService : BaseService, IGameMerchantService
    {

        public GameMerchantRepository GameMerchantRepository { get; set; }


        public GameMerchantRawEntity GetByID(int id)
        {
            var query = GameMerchantRepository.GetById(id);
            return query;
        }

        public List<GameMerchantRawEntity> GetByIDs(List<int> ids)
        {
            var query = GameMerchantRepository.GetByIds(ids);
            return query;
        }

        public List<GameMerchantRawEntity> GetByFilter(QueryGameMerchantRequestViewModel filter)
        {
            var query = GameMerchantRepository.GetByFilter(filter);
            return query;
        }


        public PagedList<GameMerchantRawEntity> GetPagingByFilter(QueryGameMerchantRequestViewModel filter)
        {
            var query = GameMerchantRepository.GetPagingByFilter(filter);
            return query;
        }


        public CRUDResultModel Create(GameMerchantRawEntity model)
        {
            model.RuleFor(x => x.GameMerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.GameMerchantName).Rule_NotEmptyText();
            model.RuleFor(x => x.GameMerchantCode).Rule_NotEmptyText();
            model.RuleFor(x => x.Status).Rule_NotMinus();
            

            var queryExist = GameMerchantRepository.GetById(model.GameMerchantID);

            if (queryExist != null)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);

            var queryExistCode = GameMerchantRepository.GetByFilter(new QueryGameMerchantRequestViewModel
            {
                GameMerchantCode = model.GameMerchantCode
            });

            if (queryExistCode.IsNotEmptyNull())
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);

            model.CreateTime = DateTimeHelper.GetCurrentTime();
            model.UpdateTime = DateTimeHelper.GetCurrentTime();

            var resultId = GameMerchantRepository.Create(model);
            return new CRUDResultModel
            {
                ResultId = resultId,
                IsSuccess = true,
                Message = "新增成功",
            };
        }


        public CRUDResultModel Edit(GameMerchantRawEntity model)
        {
            model.RuleFor(x => x.GameMerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.GameMerchantName).Rule_NotEmptyText();
            model.RuleFor(x => x.GameMerchantCode).Rule_NotEmptyText();
            model.RuleFor(x => x.Status).Rule_NotMinus();

            var queryExsit = GameMerchantRepository.GetById(model.GameMerchantID);

            if (queryExsit == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            queryExsit.GameMerchantName = model.GameMerchantName;
            queryExsit.GameMerchantCode = model.GameMerchantCode;
            queryExsit.Memo = model.Memo;
            queryExsit.MaintainMsg = model.MaintainMsg;
            queryExsit.Status = model.Status;
            queryExsit.UpdateBy = model.UpdateBy;
            queryExsit.CreateTime = DateTimeHelper.GetCurrentTime();
            queryExsit.UpdateTime = DateTimeHelper.GetCurrentTime();

            GameMerchantRepository.Update(queryExsit);
            return new CRUDResultModel(true);
        }


        public CRUDResultModel Delete(int targetID)
        {
            var queryExsit = GameMerchantRepository.GetById(targetID);

            if (queryExsit == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            queryExsit.Status = (int)GameMerchantStatusEnum.PermanentInvisible;

            GameMerchantRepository.Update(queryExsit);
            return new CRUDResultModel(true);
        }



    }

}
