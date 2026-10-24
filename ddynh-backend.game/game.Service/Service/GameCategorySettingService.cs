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
    public class GameCategorySettingService : BaseService, IGameCategorySettingService
    {

        public GameCategorySettingRepository GameCategorySettingRepository { get; set; }


        public GameCategorySettingEntity GetByID(int id)
        {
            var query = GameCategorySettingRepository.GetById(id);
            return query;
        }

        public List<GameCategorySettingEntity> GetByIDs(List<int> ids)
        {
            var query = GameCategorySettingRepository.GetByIds(ids);
            return query;
        }

        public List<GameCategorySettingEntity> GetByFilter(QueryGameCategorySettingRequestViewModel filter)
        {
            var query = GameCategorySettingRepository.GetByFilter(filter);
            return query;
        }


        public CRUDResultModel EditMultiple(List<GameCategorySettingEntity> models)
        {
            foreach(var item in models)
            {
                List<CheckResultModel> valids = new List<CheckResultModel>
                {
                    ValidateHelper.CheckIsNotZeroOrMinus(item.GameID , TargetColumnEnum.Id),
                    ValidateHelper.CheckIsNotZeroOrMinus(item.GameCategoryID , TargetColumnEnum.Id),
                };

                var result = ValidateHelper.CheckAllSuccessToCRUDResult(valids);

                if (!result.IsSuccess)
                {
                    return result;
                }
            }           

            GameCategorySettingRepository.DeleteAndAddNew(models);
            return new CRUDResultModel(true);
        }





    }

}
