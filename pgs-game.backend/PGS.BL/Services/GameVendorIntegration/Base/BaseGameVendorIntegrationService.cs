using Lck.Utility.Extensions.Helper;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.Attributes;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using target.Utility.Utility;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.BL.Services
{
    public class BaseGameVendorIntegrationService : BaseService
    {
        /// <summary>
        /// 需實作項目，對應哪個vendor列舉
        /// </summary>
        public virtual GameVendorTypeEnum GameVendor { get; }

        /// <summary>
        /// 需實作項目，遊戲商前綴英文 (建議2~4碼 小寫英文)
        /// </summary>
        public virtual string GameVendorPrefix
        {
            get
            {
                return this.GameVendor.ToString().ToLower();
            }
        }

        /// <summary>
        /// 報表API的時區依據
        /// </summary>
        public virtual double TimeZone { get; }

        public virtual double SettlementTimeZone
        {
            get
            {
                return GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(GameVendor).SettlementReportTimezone;
            }
        }


        public const string SelectColumns_BriefQuery = " BetRecordNoPGS , BetRecordNo , Status , ConvertRate , BetAmount , PayoutAmount, CreateTime , PlayerID , MerchantID , GameID  ";

        public const string SelectColumns_PlaceBetCheckDuplicateQuery = " BetRecordNoPGS , Status , BetAmount ,  PlayerID   ";


        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public VendorSessionTokenRepository VendorSessionTokenRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }

        public int GameVendorID
        {
            get
            {
                return this.GameVendor.ToInteger();
            }
        }

        public VendorConfigSettingRepository VendorConfigSettingRepository { get; set; }

        public IExternalApiRequestLogRefService ExternalApiRequestLogRefService { get; set; }


        public int _tempVendorConfigID = 0;

        public List<SystemConfigSettingParseModel> _tempVendorConfigSettings = new List<SystemConfigSettingParseModel>();


        protected void WriteRequestLog(ApiRequestLogTemporaryInfoModel model)
        {
            ExternalApiRequestLogRefService.Create(model);
        }

        public double GetVendorTimeZone()
        {
            return this.TimeZone;
        }


        protected T InstanceInsertBetRecordEntity<T>(InsertRawBetRecordRequiredInfoModel model) where T : IBetRecordEntity, IThirdPartyBetRecordEntity
        {
            var insert = Activator.CreateInstance<T>();
            var now = DateTimeHelper.GetCurrentTime();

            insert.GameID = model.GameID;
            insert.PlayerID = model.PlayerID;
            insert.MerchantID = model.MerchantID;
            insert.BetAmount = model.BetAmount;
            insert.WinLossAmount = 0;
            insert.ValidAmount = 0;
            insert.CreateTime = now;
            insert.UpdateTime = now;
            insert.BetRecordNo = model.BetRecordNo;
            insert.PlayerName = model.PlayerName;
            insert.Status = 0;
            insert.SettlementTime = null;
            insert.BetRecordNoPGS = model.BetRecordNoPGS;
            insert.ConvertRate = model.ConvertRate;
            insert.VendorConfigID = model.VendorConfigID;
            insert.AmountBeforeBet = model.AmountBeforeBet;

            return insert;
        }

        protected virtual DateTime ConvertSettleTimeToThirdSettleDate(DateTime settlementTime)
        {
            return settlementTime.AddHours(SettlementTimeZone).Date;
        }


        public PerGameDetailLogEntity BetRecordConvertToPerGame<T>(T queryBetRecord) where T : IBetRecordEntity, IThirdPartyBetRecordEntity
        {
            var perGame = new PerGameDetailLogEntity
            {
                BetAmount = queryBetRecord.BetAmount,
                ValidAmount = queryBetRecord.ValidAmount,
                UpdateAmount = queryBetRecord.WinLossAmount,
                PayoutAmount = queryBetRecord.PayoutAmount,
                GameID = queryBetRecord.GameID,
                CreateTime = DateTimeHelper.GetCurrentTime(),
                GameTime = queryBetRecord.SettlementTime.Value,
                MerchantID = queryBetRecord.MerchantID,
                PlayerID = queryBetRecord.PlayerID,
                PlayerName = queryBetRecord.PlayerName,
                ThirdPartyBetRecordNo = queryBetRecord.BetRecordNo,
                TransactionNo = queryBetRecord.BetRecordNoPGS,
                ThirdSettleDate = this.ConvertSettleTimeToThirdSettleDate(queryBetRecord.ThirdSettleTime.HasValue ? queryBetRecord.ThirdSettleTime.Value : queryBetRecord.SettlementTime.Value),
                AmountBeforeBet = queryBetRecord.AmountBeforeBet,
                ServerID = queryBetRecord.VendorConfigID ?? 0,

                CurrentAmount = -1, //後半段的code再補上
                TaxAmount = 0,
                //PerGameLogID = 0,
                //TradeID = 0,
            };

            return perGame;
        }


        protected void SetVendorConfigSettingsToTempVariableByVendorConfigID(int vendorConfigID)
        {
            List<SystemConfigSettingParseModel> settingCache = VendorConfigSettingRepository.GetByFilter(new QueryVendorConfigSettingRequestViewModel
            {
                VendorConfigIDs = vendorConfigID.ToListCollection()
            })
                .Select(z => new SystemConfigSettingParseModel(z.SettingKey, z.Value))
                .ToList();

            //放到變數中
            this._tempVendorConfigID = vendorConfigID;
            this._tempVendorConfigSettings = settingCache;
            return;
        }

        public void SetVendorConfigSettingsToTempVariableByMerchantID(int merchantID)
        {
            if(merchantID > 0)
            {
                if(!RegexRuleHelper.IsMerchantID(merchantID))
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter_IsNotValidMerchantIDFormat, new ErrorDetailModel($"{merchantID}"));
                }
            }

            string cacheKey = CacheKey.ExternalVendor_VendorConfigSettings(merchantID, this.GameVendor.ToInteger());

            List<SystemConfigSettingParseModel> settingCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var querySettings = VendorConfigSettingRepository.GetTargetMerchantVendorSettings(merchantID, this.GameVendor);
                return querySettings;

            }, 10);

            //放到變數中
            this._tempVendorConfigSettings = settingCache;

            if (settingCache.IsNotEmptyNull())
            {
                this._tempVendorConfigID = settingCache.FirstOrDefault().OwnerID;
            }

            return;
        }


        protected SystemConfigSettingParseModel GetVendorSettingByTempVariable(VendorConfigSettingEnum settingKey, bool isRequiredException = true)
        {
            var debugID = this._tempVendorConfigID;

            if (this._tempVendorConfigSettings.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, "是否已事先呼叫 SetVendorConfigSettingsToTempVariable 或 配置並未設定？ tempVendorConfigSettings為空");
            }

            var match = this._tempVendorConfigSettings.FirstOrDefault(x => x.SettingKey == (int)settingKey);

            if (match == null || string.IsNullOrWhiteSpace(match.SettingValue))
            {
                if (isRequiredException)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel(settingKey.ToString()));
                }

                //如果沒指定拋exception 會返回預設空model
                return new SystemConfigSettingParseModel
                {
                    SettingKey = settingKey.ToInteger(),
                    SettingValue = settingKey.GetAttributeFromEnum<SettingDefaultValue>()?.Value
                };
            }

            return match;
        }


        protected (bool IsDefine, decimal ConvertRate) GetMerchantConvertRate(int merchantID, GameVendorTypeEnum gameVendor)
        {
            var settingKeyResult = AppHelper.GetConvertRateCustomizedSettingKey(gameVendor);

            if (!settingKeyResult.IsDefine)
                return (false, 1.0M);

            decimal rate = MerchantCustomizedSettingRepository.GetMerchantGamePointConvertRateByCache(merchantID, settingKeyResult.SettingKey);
            return (true, rate);
        }


        protected bool IsBalanceNotEnughError(ApiErrorCode errorCode)
        {
            if (errorCode == ApiErrorCode.BetFail_WalletAmountNotEnough ||
               errorCode == ApiErrorCode.Forbidden_EnterGameError_WalletAmountNotEnough ||
               errorCode == ApiErrorCode.WalletTranferFail_WalletAmountNotEnough
                )
            {
                return true;
            }

            return false;
        }

        protected bool IsAuthenrizeError(ApiErrorCode errorCode)
        {
            if (errorCode == ApiErrorCode.NotFoundSessionToken ||
               errorCode == ApiErrorCode.Authorize_AccessToken_Expired ||
               errorCode == ApiErrorCode.Authorize_AccessToken_NotValid ||
               errorCode == ApiErrorCode.Authorize_TokenHasChanged ||
               errorCode == ApiErrorCode.Authorize_TokenAvailableGameNotMatch ||
               errorCode == ApiErrorCode.ThirdPartyApiFail_SeamlessAPIResponse_AuthError ||
               errorCode == ApiErrorCode.NotFoundPlayer
                )
            {
                return true;
            }

            return false;
        }

        protected decimal ConvertGamePointToMoney(int merchantID, decimal point)
        {
            var rate = GetMerchantConvertRate(merchantID, GameVendor);
            return point / rate.ConvertRate;
        }


        protected bool IsNotFoundBetRecordError(ApiErrorCode errorCode)
        {
            if (errorCode == ApiErrorCode.NotFoundBetRecord ||
                errorCode == ApiErrorCode.NotFoundRecord ||
                errorCode == ApiErrorCode.NotFoundBetRecordWhenCancelProcess
                )
            {
                return true;
            }

            return false;

        }

        protected bool IsInvalidStatusError(ApiErrorCode errorCode)
        {
            if (errorCode == ApiErrorCode.InvalidStatus ||
                errorCode == ApiErrorCode.InvalidStatus_DisableNow ||
                errorCode == ApiErrorCode.InvalidStatus_BetRecordAlreadyCancel ||
                errorCode == ApiErrorCode.InvalidStatus_BetRecordAlreadySettlement)
            {
                return true;
            }

            return false;

        }

        protected bool IsDuplicateDataError(ApiErrorCode errorCode)
        {
            if (errorCode == ApiErrorCode.BusinessLogicDeny_DuplicateOrderNo ||
                errorCode == ApiErrorCode.BusinessLogicDeny_DuplicateDataExist)
            {
                return true;
            }

            return false;
        }


        protected bool IsAlreadySettlementStatusError(ApiErrorCode errorCode)
        {
            if (errorCode == ApiErrorCode.InvalidStatus_BetRecordAlreadySettlement)
            {
                return true;
            }

            return false;
        }

        protected bool IsAlreadyCancelStatusError(ApiErrorCode errorCode)
        {
            if (errorCode == ApiErrorCode.InvalidStatus_BetRecordAlreadyCancel)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// 檢查sessionToken是否合法 並返回playerID
        /// </summary>
        protected int CheckSessionTokenAndPlayer(ExternalVendorInputValidateRequestParameter model)
        {
            //預設要檢查 除非有指定不檢查
            if (!model.IsSkipCheckSessionToken)
            {
                model.RuleFor(x => x.SessionOrToken).Rule_NotEmptyText();

                var query = VendorSessionTokenRepository.GetTokenByCache(model.SessionOrToken);

                if (query == null)
                {
                    throw new TerproException(ApiErrorCode.NotFoundSessionToken);
                }

                if (model.PlayerID != 0 && query.PlayerID != model.PlayerID)
                {
                    throw new TerproException(ApiErrorCode.NotFoundSessionToken);
                }

                if (query.ExpiredTime <= DateTimeHelper.GetCurrentTime())
                {
                    throw new TerproException(ApiErrorCode.Authorize_AccessToken_Expired);
                }

                return query.PlayerID;
            }

            return model.PlayerID;
        }


        protected GameListIdInfoBrief GetGameByThirdGameCode_V2(string thirdGameCode)
        {
            var gameVendor = this.GameVendor.ToInteger();

            var myCacheKey = CacheKey.GetAllGameBriefByGameVendor(gameVendor);

            //儲存的資料變動機率不高 允許cache比較久
            var expiredTime = CacheHelper.InstanceMemoryCacheTime(extendRate: 3);

            var queryMyCache = CacheHelper.GetCacheOrQueryByMemoryCache(myCacheKey, () =>
            {
                string selectColumns = " GameID , GameCode , ThirdPartyGameCode , Status ";
                var queryVendorGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
                {
                    GameVendorTypes = gameVendor.ToListCollection()
                }, selectColumns);

                var convertModels = queryVendorGames.Select(x => x.ReturnInjecteObject<GameListIdInfoBrief>()).ToList();
                return convertModels;

            }, expiredTime);

            var match = queryMyCache.Where(x => x.ThirdPartyGameCode == thirdGameCode).FirstOrDefault();

            if (match != null)
            {
                return match;
            }

            var queryGameByDB = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                ThirdPartyGameCode = thirdGameCode,
                GameVendorTypes = gameVendor.ToListCollection()
            })
                .Select(x => x.ReturnInjecteObject<GameListIdInfoBrief>())
                .ToList();

            if (queryGameByDB.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.NotFoundGame);

            return queryGameByDB.FirstOrDefault();
        }


        /// <summary>
        /// 部分遊戲商為了效能考量。使用betRecordNo查詢。第一次會先篩選近幾個小時以減少資料量 << 90%的單應該都是屬於這種
        /// 第二次、第三次會漸進式拉長時間範圍
        /// </summary>
        public List<QueryBetRecordGeneralRequestViewModel> InstanceLoopTimesQueryTimeRange(BetRecordInfoQueryParameter model, bool isFilterGame = true)
        {
            // Case 1. 如果有指定 BetRecordNoPGS 直接用 BetRecordNoPGS查詢
            if (model.BetRecordNoPGS.IsNotEmptyTextNotNull())
            {
                var queryByPK = new QueryBetRecordGeneralRequestViewModel
                {
                    BetRecordNoPGS = model.BetRecordNoPGS,
                };

                //如果遊戲商不支援partition欄位
                if (GameVendorDefinitionHelper.IsSupportPartitionTable(GameVendor))
                {
                    var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(model.BetRecordNoPGS);
                    queryByPK.PartitionTimeMatch = recover.Time;
                }

                return new List<QueryBetRecordGeneralRequestViewModel>
                {
                    queryByPK
                };
            }


            var multipleQueryFilter = new List<QueryBetRecordGeneralRequestViewModel>();

            // Case 2. 不支援partition的遊戲商
            if (!GameVendorDefinitionHelper.IsSupportPartitionTable(GameVendor))
            {
                multipleQueryFilter.Add(this.InstanceQueryFilter(model, null, null , isFilterGame));
                return multipleQueryFilter;
            }

            var now = DateTimeHelper.GetCurrentTime();

            //若剛跨過小時 有可能partitionTime是上個小時 
            //反之，可以直接認定資料應該都是同一個小時
            bool isQueryCurrentHourData = now.Minute > 5;

            bool isEnablePartitionMatchQuery = model.IsPartitionMatchQuery && isQueryCurrentHourData;

            //投注階段 (使用PartitionMatch 機制匹配)
            if (model.IsPlaceBetProcess && isEnablePartitionMatchQuery)
            {
                var myFilter = this.InstanceQueryFilter(model, null, null, isFilterGame);
                myFilter.PartitionTimeMatch = RandomHelper.GetPgsUniqueDateHourSequenceOrderNoAndTime().TimeWithHour;

                multipleQueryFilter.Add(myFilter);

                return multipleQueryFilter;
            }

            // Case 3. 支援 partition
            if (model.IsQueryRecentRange)
            {
                var defaultFilter = this.InstanceQueryFilter(model, now.AddHours(-1 * SystemConstant.BetRecord_LoopQueryDefaultPartitionHoursRange), now, isFilterGame);

                //若有指定 優先使用match time的查詢 (看執行計畫 感覺cost是三分之一)
                if (isEnablePartitionMatchQuery)
                {
                    var timeMatchFilter = this.InstanceQueryFilter(model, null, null, isFilterGame);
                    timeMatchFilter.PartitionTimeMatch = RandomHelper.GetPgsUniqueDateHourSequenceOrderNoAndTime().TimeWithHour;

                    multipleQueryFilter.Add(timeMatchFilter);
                }

                //默認預設N小時查詢
                multipleQueryFilter.Add(defaultFilter);

                bool isSpecificDateRange = model.MaxQueryHourRange.HasValue;

                //如果有指定範圍上限就直接用上限的值去查
                if (isSpecificDateRange)
                {
                    //如果指定的時間hour跟預設的小時一樣。就不做第二次查詢了
                    if (model.MaxQueryHourRange.Value != SystemConstant.BetRecord_LoopQueryDefaultPartitionHoursRange)
                    {
                        multipleQueryFilter.Add(this.InstanceQueryFilter(model, now.AddHours(-1 * model.MaxQueryHourRange.Value), now, isFilterGame));
                    }
                }
                else
                {
                    multipleQueryFilter.Add(this.InstanceQueryFilter(model, now.AddHours(-1 * 48), now, isFilterGame));

                    //如果該單號是真的建單失敗(資料庫不存在)、這種查詢極度浪費資源...等於是搜尋整個table 卻只為了查一個不存在的資料
                    multipleQueryFilter.Add(this.InstanceQueryFilter(model, null, now, isFilterGame));
                }
            }
            else
            {
                multipleQueryFilter.Add(this.InstanceQueryFilter(model, null, null, isFilterGame));
            }

            return multipleQueryFilter;
        }


        private QueryBetRecordGeneralRequestViewModel InstanceQueryFilter(BetRecordInfoQueryParameter model, DateTime? begin, DateTime? end , bool isFilterGame)
        {
            var filter = new QueryBetRecordGeneralRequestViewModel
            {
                PartitionTimeBegin = begin,
                PartitionTimeEnd = end,
                BetRecordNo = model.BetRecordNo,
                ThirdPartyGameCode = model.GameID.HasValue ? null : model.ThirdPartyGameCode,
                GameID = isFilterGame ? model.GameID : null,
                DebugSourceMethod = model.DebugSourceMethod
            };

            if(!isFilterGame)
            {
                filter.GameID = null;
                filter.ThirdPartyGameCode = null;
            }            
                
            return filter;
        }


        protected ApiResultBriefModel<T> ConvertToBriefModel<T>(ApiRequestResultModel<T> apiResponse)
        {
            return new ApiResultBriefModel<T>
            {
                ResultObject = apiResponse.ResultObject,
                ResponseText = apiResponse.ResponseText
            };
        }



        protected void SendNotifyToChannel(string msg)
        {
            MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
            {
                Message = msg,
                TelegramChannelEnum = TelegramChannelEnum.pgs_externalAPI
            });
        }


    }
}
