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
    public class GameMerchantPlatformConfigService : BaseService, IGameMerchantPlatformConfigService
    {

        public GameMerchantPlatformConfigRepository GameMerchantPlatformConfigRepository { get; set; }


        public GameMerchantPlatformConfigRawEntity GetByID(int id)
        {
            var query = GameMerchantPlatformConfigRepository.GetById(id);
            return query;
        }

        public List<GameMerchantPlatformConfigRawEntity> GetByIDs(List<int> ids)
        {
            var query = GameMerchantPlatformConfigRepository.GetByIds(ids);
            return query;
        }

        public List<GameMerchantPlatformConfigRawEntity> GetByFilter(QueryGameMerchantPlatformConfigRequestViewModel filter)
        {
            var query = GameMerchantPlatformConfigRepository.GetByFilter(filter);
            return query;
        }


        public PagedList<GameMerchantPlatformConfigRawEntity> GetPagingByFilter(QueryGameMerchantPlatformConfigRequestViewModel filter)
        {
            var query = GameMerchantPlatformConfigRepository.GetPagingByFilter(filter);
            return query;
        }


        public CRUDResultModel Create(GameMerchantPlatformConfigRawEntity model)
        {
            model.RuleFor(x => x.GameMerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.PlatformID).Rule_NotZeroNotMinus();

            model.CreateTime = DateTimeHelper.GetCurrentTime();
            model.UpdateTime = DateTimeHelper.GetCurrentTime();
            model.Status = NormalActiveStatusEnum.Enable.ToInteger();

            var queryExist = GameMerchantPlatformConfigRepository.GetByFilter(new QueryGameMerchantPlatformConfigRequestViewModel
            {
                PlatformIDs = model.PlatformID.ToListCollection(),
                GameMerchantIDs = model.GameMerchantID.ToListCollection(),
            });

            if (queryExist.IsNotEmptyNull())
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);

            //如果有自己指定值 不會透過此function覆蓋
            if(string.IsNullOrWhiteSpace(model.ConfigJsonValues))
            {
                model.ConfigJsonValues = GetEmptyJsonSettingValues(model.GameMerchantID);
            }

            var resultId = GameMerchantPlatformConfigRepository.Create(model);
            return new CRUDResultModel
            {
                ResultId = resultId,
                IsSuccess = true,
                Message = "新增成功",
            };
        }



        public CRUDResultModel Edit(GameMerchantPlatformConfigRawEntity model)
        {
            model.RuleFor(x => x.ID).Rule_NotZeroNotMinus();

            var queryExsit = GameMerchantPlatformConfigRepository.GetById(model.ID);

            if (queryExsit == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            queryExsit.MerchantSecretPublic = model.MerchantSecretPublic;
            queryExsit.MerchantSecretPrivate = model.MerchantSecretPrivate;
            queryExsit.MerchantApiHost = model.MerchantApiHost;
            queryExsit.MerchantIdentity = model.MerchantIdentity;
            queryExsit.ConfigJsonValues = model.ConfigJsonValues;
            queryExsit.AdminWebSiteUrl = model.AdminWebSiteUrl;
            queryExsit.UpdateBy = model.UpdateBy;
            model.CreateTime = DateTimeHelper.GetCurrentTime();
            model.UpdateTime = DateTimeHelper.GetCurrentTime();

            GameMerchantPlatformConfigRepository.Update(queryExsit);
            return new CRUDResultModel(true);
        }


        public CRUDResultModel Delete(int targetID)
        {
            var queryExsit = GameMerchantPlatformConfigRepository.GetById(targetID);

            if (queryExsit == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            queryExsit.Status = (int)NormalActiveStatusEnum.PermanentInvisible;

            GameMerchantPlatformConfigRepository.Update(queryExsit);
            return new CRUDResultModel(true);
        }
        

        public CRUDResultModel CopySettingFromPlatform(int gameMerchantID , int templatePlatformID , int targetPlatformID , string adminUserName)
        {
            if (templatePlatformID == targetPlatformID)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SourceAndTargetAreEqual);

            var queryExist = GameMerchantPlatformConfigRepository.GetByFilter(new QueryGameMerchantPlatformConfigRequestViewModel
                {
                    PlatformIDs = templatePlatformID.ToListCollection(),
                    GameMerchantIDs = gameMerchantID.ToListCollection(),
                })
                .FirstOrDefault();

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            var queryTargetExist = GameMerchantPlatformConfigRepository.GetByFilter(new QueryGameMerchantPlatformConfigRequestViewModel
                {
                    PlatformIDs = targetPlatformID.ToListCollection(),
                    GameMerchantIDs = gameMerchantID.ToListCollection(),
                })
               .FirstOrDefault();
            
            if(queryTargetExist != null)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);

            var now = DateTimeHelper.GetCurrentTime();
            queryExist.PlatformID = targetPlatformID;
            queryExist.UpdateBy = adminUserName;
            queryExist.CreateTime = now;
            queryExist.UpdateTime = now;


            var result = Create(queryExist);
            return result;
        }



        private string GetEmptyJsonSettingValues(int gameMerchantID)
        {
            var gameMerchant = (GameMerchantEnum)gameMerchantID;

            if (!gameMerchant.IsDefinedValue())
                return string.Empty;

            var value = ExtensionOfEnum.GetAttributeFromEnum<EnumSettingValue>(gameMerchant);

            if (value == null || value.Value.IsEmptyOrNull())
                return string.Empty;

            //如果有透過enums去定義特殊的參數要設置
            var split = value.Value.Split(',').Select(z => z.Trim()).ToList();
            return split.ToDictionary(x => x, x => "").ConvertToJsonString();
        }


    }

}
