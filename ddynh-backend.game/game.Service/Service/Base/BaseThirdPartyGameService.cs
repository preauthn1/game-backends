using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Money;
using game.Service.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using game.Infrastructure.Extensions;
using FW.Core.Engine;
using game.Entity.Game;
using game.Entity.ViewModels.Query;
using target.Models.Table;
using game.Infrastructure.Helper;
using game.Infrastructure;
using game.Infrastructure.Utility;
using game.Entity.RawEntity;

namespace game.Service.Service
{
    public class BaseThirdPartyGameService : BaseService
    {

        public GameMerchantRepository GameMerchantRepository { get; set; }

        public GameMerchantPlatformConfigRepository GameMerchantPlatformConfigRepository { get; set; }

        public ApiRequestLog_ExternalApiRepository ApiRequestLog_ExternalApiRepository { get; set; }



        public virtual int GameMerchantID { get; }


        protected void WriteThirdPartyRequestLog(RequestInfoTemporaryModel model, string category)
        {
            var insertModel = new ExternalApiRequestLogEntity
            {
                CreateTime = DateTimeHelper.GetCurrentTime(),
                Category = category,
                ResponseText = model.ResponseContent,
                ResponseTime = model.ResponseTime,
                ResponseStatusCode = model.ResponseStatusCode,
                RequestMethod = model.RequestParameters.Method.ToString().Replace("http" , "").ToUpper(),

                RequestContent = model.RequestContent,
                RequestBody = string.Empty, //多開一個欄位了 不需要紀錄兩次
                RequestUrl = (model.RequestParameters.BaseUrl + model.RequestParameters.ApiUrl).ReplaceDuplicateUrlChar(),
                Memo = model.RequestParameters.LogIdentityKey,
                WebSiteName = SettingHelper.WebSiteName
            };

            //先用最基本款的資料庫紀錄log吧 (也許未來會調整成外部storage或nosql什麼的)
            var entity = insertModel.ReturnInjecteObject<ApiRequestLog_ExternalApiRawEntity>();
            ApiRequestLog_ExternalApiRepository.Create(entity);
        }


        protected string GetGameMerchantName()
        {
            return ((GameMerchantEnum)GameMerchantID).GetDescription();
        }        

        protected void CheckGameMerchantExistAndStatusIsOK()
        {
            var queryGameMerchant = GameMerchantRepository.GetById(GameMerchantID);

            if (queryGameMerchant == null)
                throw new TerproException(ApiErrorCode.NotFoundGameMerchant);

            if (queryGameMerchant.Status == (int)GameMerchantStatusEnum.Maintain)
            {
                string msg = queryGameMerchant.MaintainMsg;

                if(string.IsNullOrWhiteSpace(msg))
                    throw new TerproException(ApiErrorCode.InvalidStatus_StatusIsMaintain);                
                    
                throw new TerproException(ApiErrorCode.InvalidStatus_StatusIsMaintain , msg);
            }

            if (queryGameMerchant.Status != (int)GameMerchantStatusEnum.Active)
            {
                throw new TerproException(ApiErrorCode.InvalidStatus);
            }
        }


        protected GameMerchantPlatformConfigRawEntity GetGameMerchantConfigDataAndCheckStatus(int platformID)
        {           
            var queryMerchantSetting = GameMerchantPlatformConfigRepository.GetByFilter(new QueryGameMerchantPlatformConfigRequestViewModel
                {
                    GameMerchantIDs = GameMerchantID.ToListCollection(),
                    PlatformIDs = platformID.ToListCollection()
                })
                .FirstOrDefault();

            if (queryMerchantSetting == null)
                throw new TerproException(ApiErrorCode.ThirdPartyError_SettingDataNotCompleted);

            if (queryMerchantSetting.Status != (int)NormalActiveStatusEnum.Enable)
                throw new TerproException(ApiErrorCode.ThirdPartyError_SettingDataNotCompleted);

            return queryMerchantSetting;

        }

    }
}
