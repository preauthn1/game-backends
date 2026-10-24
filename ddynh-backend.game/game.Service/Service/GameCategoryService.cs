using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Game;
using game.Service.DAO;
using game.Infrastructure;

namespace game.Service.Service
{
    public class GameCategoryService : BaseService, IGameCategoryService
    {

        public GameCategoryRepository GameCategoryRepository { get; set; }


        public GameCategoryEntity GetByID(int id)
        {
            var query = GameCategoryRepository.GetById(id);
            return query;
        }

        public List<GameCategoryEntity> GetByIDs(List<int> ids)
        {
            var query = GameCategoryRepository.GetByIds(ids);
            return query;
        }

        public List<GameCategoryEntity> GetByFilter(QueryGameCategoryRequestViewModel filter)
        {
            var query = GameCategoryRepository.GetByFilter(filter);
            return query;
        }


        public PagedList<GameCategoryEntity> GetPagingByFilter(QueryGameCategoryRequestViewModel filter)
        {
            var query = GameCategoryRepository.GetPagingByFilter(filter);
            return query;
        }


        public CRUDResultModel Create(GameCategoryEntity model)
        {
            List<CheckResultModel> valids = new List<CheckResultModel>
                {
                    ValidateHelper.CheckTextLength_IsNotEmpty_IsValidLength(model.Name , TargetColumnEnum.Name , 25),
                };

            var result = ValidateHelper.CheckAllSuccessToCRUDResult(valids);

            if (!result.IsSuccess)
            {
                return result;
            }

            var resultId = GameCategoryRepository.Create(model);
            return new CRUDResultModel
            {
                ResultId = resultId,
                IsSuccess = true,
                Message = "新增成功",
            };
        }


        public CRUDResultModel EditMultiple(List<GameCategoryEntity> models)
        {
            foreach(var item in models)
            {
                List<CheckResultModel> valids = new List<CheckResultModel>
                {
                    ValidateHelper.CheckIsNotZeroOrMinus(item.Id , TargetColumnEnum.Id),
                    ValidateHelper.CheckTextLength_IsNotEmpty_IsValidLength(item.Name , TargetColumnEnum.Name , 25),
                };

                var result = ValidateHelper.CheckAllSuccessToCRUDResult(valids);

                if (!result.IsSuccess)
                {
                    return result;
                }
            }

            GameCategoryRepository.UpdateMany(models);
            return new CRUDResultModel(true);
        }





    }

}
