using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace target.BL.Services
{
    public class ThirdPartyGameVendorService : BaseAdminSysService, IThirdPartyGameVendorService
    {
        public BetRecordTransactionNoMappingRepository BetRecordTransactionNoMappingRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public EazyTokenLogRepository EazyTokenLogRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public override bool IsAutoInstanceAllRepository
        {
            get
            {
                return false;
            }
        }

        public override bool IsAutoInstanceAllService
        {
            get
            {
                return false;
            }
        }


        public ThirdPartyGameVendorBetRecordDetailUrlViewModel GetScreenshotDetailUrlByPegGameDetailLogID(int userID , long perGameDetailID)
        {
            this.InstanceAllRepositoryAndService(true, true);

            var queryPerGame = PerGameDetailLogRepository.GetById(perGameDetailID , isLockQuery: false);

            if (queryPerGame == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            var queryGame = GameListRepository.GetById(queryPerGame.GameID);

            if (queryPerGame.ThirdPartyBetRecordNo.IsNullOrWhiteSpace())
                return new ThirdPartyGameVendorBetRecordDetailUrlViewModel();

            var result = GetReplayInfoByGameVendorApi(userID, queryGame.GameVendorType, queryPerGame.TransactionNo);
            return result;
        }


        public ThirdPartyGameVendorBetRecordDetailUrlViewModel GetDetailUrlByTransactionNo(int userID , string pgsTransactionNo)
        {
            this.InstanceAllRepositoryAndService(true, true);

            if (!RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(pgsTransactionNo).IsPgsOrderNo)
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("TransactionNo"));
            }

            var query = BetRecordTransactionNoMappingRepository.GetByTransactionNoPGS(pgsTransactionNo);

            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecordOrDeleteAlready);

            var result = GetReplayInfoByGameVendorApi(userID, query.GameVendorID, pgsTransactionNo);
            return result;
        }


        private ThirdPartyGameVendorBetRecordDetailUrlViewModel GetReplayInfoByGameVendorApi(int userID  , int gameVendorType , string betRecordNoPGS)
        {
            var vendorService = ServiceInstanceHelper.InstanceVendorService(gameVendorType);

            var queryBetRecord = vendorService.GetByPGSTransactionNo(betRecordNoPGS);

            int targetGameID = 0;
            int targetMerchantID = 0;
            var targetCreateTime = DateTimeHelper.GetCurrentTime();

            if (queryBetRecord == null)
            {
                //如果資料已被清除 嘗試從perGame撈資料
                var queryPerGame = PerGameDetailLogRepository.GetByTransactionNo_WithGameTimeRange(betRecordNoPGS);

                if(queryPerGame == null)
                {
                    throw new TerproException(ApiErrorCode.NotFoundBetRecordOrDeleteAlready);
                }

                targetGameID = queryPerGame.GameID;
                targetMerchantID = queryPerGame.MerchantID;
                targetCreateTime = queryPerGame.CreateTime;
            }
            else
            {
                targetGameID = queryBetRecord.GameID;
                targetMerchantID = queryBetRecord.MerchantID;
                targetCreateTime = queryBetRecord.CreateTime;
            }

            if(queryBetRecord != null && queryBetRecord.Status == (int)BetRecordStatusEnum.Cancel)
            {
                throw new TerproException(ApiErrorCode.InvalidStatus, new ErrorDetailModel("Cancel status"));
            }

            if (targetMerchantID == 0)
            {
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);
            }

            double delayMinutes = 3;

            //若透過VendorAPI使用。會傳入-5566。這個流程下請不要檢查userID
            if (userID != SystemConstant.SkipValidateUserID)
            {
                CheckViewMerchantPermission(userID, targetMerchantID);
            }
            else
            {
                //走進這邊待表示vendorAPI呼叫
                delayMinutes = 5;
            }

            //部分遊戲商至少要5~10分鐘後才能查詢
            var delayMinutesTime = DateTimeHelper.GetCurrentTime().AddMinutes(-1 * delayMinutes);

            if (delayMinutesTime < targetCreateTime)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DataNotSyncSuccess_PleaseWait10Minutes , new ErrorDetailModel($"{delayMinutes} minutes"));
            }

            var isSupportExternalLinkUrl = GameVendorDefinitionHelper.IsSupportExternalLinkUrl(gameVendorType);

            if (!isSupportExternalLinkUrl)
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis); 

            if (gameVendorType == GameVendorTypeEnum.AGGLive.ToInteger() ||
                gameVendorType == GameVendorTypeEnum.target.ToInteger() ||
                gameVendorType == GameVendorTypeEnum.Galaxy.ToInteger() ||
                gameVendorType == GameVendorTypeEnum.RG.ToInteger() ||
                gameVendorType == GameVendorTypeEnum.AGin.ToInteger() ||
                gameVendorType == GameVendorTypeEnum.Slot88.ToInteger())
            {
                if(AppHelper.IsPgsSelfGame(targetGameID))
                {
                    var queryGame = GameListRepository.GetById(targetGameID);

                    if(queryGame.GameType != (int)GameTypeEnum.Slot)
                    {
                        throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis);
                    }
                }

                //aggLive是自製頁面顯示
                var token = [REDACTED];
                var now = DateTimeHelper.GetCurrentTime();

                EazyTokenLogRepository.Create(new EazyTokenLogEntity
                {
                    Token = [REDACTED],
                    CreateTime = now,
                    ExpiredTime = now.AddHours(SystemConstant.ReplayHistoryTokenExpiredHour),
                    Type = 0,
                    ReferenceValue = betRecordNoPGS,
                });

                var publicPageDomain = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.LobbyPageUrl);

                var lang = HeaderValueHelper.GetCurrentLanguage();

                string langCode = "en-us";

                if(AppHelper.IsChineseLang(lang))
                {
                    langCode = LanguageEnum.zh_cn.ToDashLangCode();
                }

                int promoteBrandType = SettingHelper.PromoteBrandType;

                string url = $"{publicPageDomain.SettingValue}?page=historyScreenshot&transactionNo={betRecordNoPGS}&eazyToken=[REDACTED]";

                return new ThirdPartyGameVendorBetRecordDetailUrlViewModel
                {
                    InfoType = ThirdPartyVendorInfoTypeEnum.ExternalLink,
                    BetRecordNoPGS = betRecordNoPGS,
                    MerchantID = targetMerchantID,
                    Url = url
                };
            }
            else
            {
                var result = vendorService.GetExternalBetDetailAndScreenshot(new ExternalVendorBetRecordDetailInfoRequestModel
                {
                    BetRecordNoPGS = betRecordNoPGS,                    
                });

                result.MerchantID = targetMerchantID;
                return result;
            }
        }

    }

}
