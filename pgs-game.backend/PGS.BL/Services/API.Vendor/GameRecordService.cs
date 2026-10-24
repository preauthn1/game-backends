using System;
using System.Collections.Generic;
using target.BL.Interface;
using target.BL.Services;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Models.Models;
using System.Linq;
using target.DA.Repository;
using target.Utility;
using target.Utility.Helper;
using target.Models.ViewModels.GameVendor;
using target.Models.Enums;
using target.Utility.Extensions.Web;
using Lck.Utility.Extensions.Helper;
using target.Utility.Utility.ThirdParty;

namespace target.BL.Services
{

    public partial class GameRecordService : BaseVendorService, IGameRecordService
    {
        public PlayerRepository PlayerRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public EazyTempValueRepository EazyTempValueRepository { get; set; }

        public AGGLiveDetailJsonRepository AGGLiveDetailJsonRepository { get; set; }

        public MerchantEazyLogRepository MerchantEazyLogRepository { get; set; }

        public BetRecordTransactionNoMappingRepository BetRecordTransactionNoMappingRepository { get; set; }

        public MerchantEventSummaryDataRepository MerchantEventSummaryDataRepository { get; set; }

        public MerchantGameSummaryDataRepository MerchantGameSummaryDataRepository { get; set; }

        public PlayerGameDataRepository PlayerGameDataRepository { get; set; }

        public PerGameDetailLogMerchantJoinRepository PerGameDetailLogMerchantJoinRepository { get; set; }

        public PerGameDetailChangeLogRepository PerGameDetailChangeLogRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }


        private const int MAX_QUERY_SIZE = 10000;


        /// <summary>
        /// 取得投注紀錄、流水紀錄
        /// </summary>
        public PagedList<GameRecordViewModel> GetBetRecords(GameRecordRequestModel model)
        {
            model.RuleFor(x => x.StartTime).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.EndTime).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.StartTime).Rule_StartTimeMustSmallThanEndTime(model.EndTime);

            double hoursRange = (int)(model.EndTime - model.StartTime).TotalHours;

            bool isPrduction = !SettingHelper.IsDev;

            var queryMerchant = this.GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            if (isPrduction)
            {
                ValidateStartEndTimeRange(model, queryMerchant);
            }

            //檢查 VendorAPI設置維護
            CheckIsVendorAPIMaintain();

            var now = DateTimeHelper.GetCurrentTime();

            //臨時debug用
            if (hoursRange > 2 && now <= new DateTime(2024, 5, 12) && model.PageIndex == 1)
            {
                //紀錄哪些商戶會把時間range拉很長的
                MerchantEazyLogRepository.CreateTryCatch(new MerchantEazyLogEntity
                {
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    MerchantID = model.MerchantID,
                    Time1 = model.StartTime,
                    Time2 = model.EndTime,
                    Memo = $"{hoursRange} hours",
                    Type = 1000
                });
            }

            //針對UFA商戶的特殊處理 (減少一點資源浪費)
            if (model.PageIndex <= 1 && IsReturnEmptyResultByPreQueryTop1(model, queryMerchant.Type))
            {
                var empty = new PagedList<GameRecordViewModel>();
                empty.Items = new List<GameRecordViewModel>();
                empty.PageIndex = model.PageIndex;
                empty.PageSize = model.PageSize;
                return empty;
            }

            //最多一次撈N筆
            if (model.PageSize >= MAX_QUERY_SIZE)
                model.PageSize = MAX_QUERY_SIZE;

            var queryFilter = model.ReturnInjecteObject<QueryDetailLogRequestViewModel>();
            queryFilter.MerchantIDs = model.MerchantID.ToListCollection();
            queryFilter.GameStartTimeBegin = model.StartTime;
            queryFilter.GameStartTimeEnd = model.EndTime;

            if (queryFilter.TagID.HasValue)
            {
                queryFilter.TagID = queryFilter.TagID;
            }

            //用字串搜尋成本太高，會反查IDs再用playerID過濾
            if (!string.IsNullOrWhiteSpace(model.PlayerName))
            {
                queryFilter.PlayerIDs = GetMerchantPlayerIDByPlayerName(model.MerchantID, model.PlayerName);
            }

            queryFilter.OrderByColumn = $"{nameof(PerGameDetailLogEntity.GameTime)}"; //partition欄位排序會比較快
            queryFilter.DebugSourceMethod = "VendorFetchWagers";

            //精簡查詢的欄位。即使index那邊沒有客製。網路流量的浪費還是有差異的 (e.g. 一次查1萬筆)
            string selectColumns = @" Id, GameID, PlayerID, PlayerName, CurrentAmount, UpdateAmount , " +
                @" GameTime, BetAmount, PayoutAmount, ValidAmount, TaxAmount, JackpotAmount , TransactionNo , AmountBeforeBet ";

            PagedList<PerGameDetailLogEntity> query = null;

            string timeText = DateTimeHelper.GetCurrentTimeYYYYMMDDHH();

            //開始實驗用新版查法
            if (IsQueryByBriefJoinTable(model))
            {
                query = PerGameDetailLogRepository.GetPagingByJoinBriefTable(queryFilter, selectColumns);
                //觀察數據SQL (2024/05/21 新的結論。假如第一個查詢1.5秒 第二個查詢查詢的資料本來就可能會多1.5秒的資料量...查詢結果有誤差是必然)
            }
            else
            {
                query = PerGameDetailLogRepository.GetPagingByFilter(queryFilter, selectColumns);
            }


            var pageModels = ConvertToNewTypePagingObject<PerGameDetailLogEntity, GameRecordViewModel>(query);
            pageModels.Items = query.Items.Select(perGame =>
                    {
                        var convert = new GameRecordViewModel
                        {
                            //如果有加欄位 記得改上面的 selectColumns
                            BetTime = perGame.GameTime,
                            PlayerName = perGame.PlayerName,
                            GameID = perGame.GameID,
                            BeforeAmount = perGame.CurrentAmount,
                            WinLossAmount = perGame.UpdateAmount,
                            BetAmount = perGame.BetAmount,
                            ValidAmount = perGame.ValidAmount,
                            PayoutAmount = perGame.PayoutAmount,
                            TaxAmount = perGame.TaxAmount,
                            BetID = perGame.Id.ToString(),
                            RoundStartTime = perGame.GameTime,
                            RoundEndTime = perGame.GameTime,
                            TransactionNo = perGame.TransactionNo
                        };

                        //過渡期 預計9/17 或 9/24 就會移除此欄位
                        if (perGame.GameTime >= new DateTime(2024, 9, 4))
                        {
                            //原本是存放 PerGameLogID 此欄位已棄用
                            convert.RoundID = perGame.Id.ToString();
                        }
                        else
                        {
                            //原本是存放 PerGameLogID 此欄位已棄用
                            if (perGame.PerGameLogID == 0)
                            {
                                convert.RoundID = perGame.Id.ToString();
                            }
                            else
                            {
                                convert.RoundID = perGame.PerGameLogID.ToString();
                            }
                        }

                        if (convert.BeforeAmount <= -1 && perGame.AmountBeforeBet.HasValue)
                        {
                            convert.BeforeAmount = perGame.AmountBeforeBet.Value;
                        }

                        //如果有指定參數 會查詢額外資訊
                        if (model.IsSearchExtraDetail)
                        {
                            convert.ExtraInfo = new GameRecordExtraInfoViewModel
                            {
                                JackpotAmount = perGame.JackpotAmount
                            };
                        }

                        return convert;
                    })
                    .ToList();

            //確認有接需求 才提供
            SetExtraProperty(model.MerchantID, pageModels.Items, model);

            //if (SettingHelper.IsLocalhostWebSiteName)
            //{
            //    var debug1 = pageModels.Items.Select(x => x.BetAmount).Sum();
            //    var debug2 = pageModels.Items.Select(x => x.WinLossAmount).Sum();
            //    var debug3 = pageModels.Items.Select(x => x.PayoutAmount).Sum();
            //    string qqqq = "";
            //}

            return pageModels;
        }


        public void CheckVendorAPIMaintainAndThrow()
        {
            //檢查 VendorAPI設置維護
            CheckIsVendorAPIMaintain();
        }



        private void ValidateStartEndTimeRange(GameRecordRequestModel model, MerchantEntity queryMerchant)
        {
            model.RuleFor(x => x.StartTime).Rule_DateTimeMustLargeThanPreviousDays(90);

            double hoursRange = (int)(model.EndTime - model.StartTime).TotalHours;

            if (hoursRange > 8)
            {
                bool isTestMerchant = queryMerchant.Type == (int)MerchantTypeEnum.Plaform_DevelopEnvironment;

                if (isTestMerchant)
                {
                    //Dev環境直接禁止拉太長
                    int maxHourRangeDev = 8;
                    if (hoursRange > maxHourRangeDev)
                    {
                        model.RuleFor(x => x.StartTime).Rule_StartTimeEndTimeHourRange(model.EndTime, (maxHourRangeDev + 1));
                    }
                }
                else
                {
                    //正式環境根據配置決定
                    var queryHourRange = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(model.MerchantID, MerchantCustomizedSettingKeyEnum.DataQuery_GameRecordAPI_MaxHoursRange);

                    int maxHourRange = queryHourRange.SettingValueOfInt + 1;

                    model.RuleFor(x => x.StartTime).Rule_StartTimeEndTimeHourRange(model.EndTime, maxHourRange);
                }
            }


            if (SettingHelper.IsProduction || SettingHelper.IsLocalhostWebSiteName)
            {
                //檢查短時間請求太多次 (避免廠商誤寫了無窮迴圈)
                CheckRequestTooManyTimes(model.MerchantID, model.ConvertToJsonString().MD5Hash(), "GetBetRecords");

                if (hoursRange > 8)
                {
                    //預計三周(可能6月初?) 後更新調整
                    CheckRequestTooManyTimes(model.MerchantID, model.ConvertToJsonString().MD5Hash(), "GetBetRecords_8Hours", 6, "Query interval > 8 hours");
                }
            }
        }

        public bool IsQueryByBriefJoinTable<T>(T model) where T : IQueryWagerRequest
        {
            if (model.PlayerName.IsNotEmptyTextNotNull())
            {
                return false;
            }

            if (model.TagID.NotNullNotZeroNotMinus())
            {
                return false;
            }

            //預估是超過14天的資料就會清除了
            DateTime data_lastKeepTime = DateTimeHelper.GetCurrentTime().AddDays(-13);

            if (model.StartTime < data_lastKeepTime)
            {
                return false;
            }

            double secondsRange = (int)(model.EndTime - model.StartTime).TotalSeconds;
            double minutesRange = (int)(model.EndTime - model.StartTime).TotalMinutes;

            //間隔太短的 不宜使用
            if (secondsRange < 60)
            {
                return false;
            }

            DateTime validEndTime = DateTimeHelper.GetCurrentTime().AddSeconds(-1 * 15);

            bool isQueryValidTimeRange = model.EndTime < validEndTime;
            bool isQueryLargeMinutesRange = minutesRange >= 60;

            if (isQueryValidTimeRange)
                return true;

            if (isQueryLargeMinutesRange)
                return true;

            //常用商戶直接return 減少浪費查詢cache
            List<int> trustMerchantIDs = new List<int>
            {
                175774, //商戶:WinChat 30秒一次
                        
                //ION系列
                175038,
                175079,
                175080,

                //Vworld系列
                176064,
                176043,
                176045,
                176177,
                175026,
            };

            if (trustMerchantIDs.Contains(model.MerchantID))
            {
                return true;
            }

            //根據配置
            var querySetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(model.MerchantID, MerchantCustomizedSettingKeyEnum.DataQuery_GameRecordAPI_ForceQueryBriefJoin);

            if (querySetting.SettingValueOfBoolean)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// 紀錄執行次數 作為觀察
        /// </summary>
        private void AddCounterByKey(string counter_identity)
        {
            try
            {
                string cacheKey = CacheKey.MyEazyCounter(counter_identity);

                var counter = CacheHelper.GetCacheValue<EazyCounterModel>(cacheKey);

                if (counter == null)
                {
                    counter = new EazyCounterModel
                    {
                        TotalCount = 1
                    };
                }
                else
                {
                    counter.TotalCount = counter.TotalCount + 1;
                }

                TimeSpan timeSpan = new TimeSpan(48, 0, 0);
                CacheHelper.SetCacheValue(cacheKey, counter, timeSpan);
            }
            catch
            {

            }
        }


        /// <summary>
        /// 取得捕魚遊戲詳細記錄
        /// </summary>
        public PagedList<FishGameDetailRecordViewModel> GetFishGameDetailRecords(FishDetailRecordRequestModel model)
        {
            return new PagedList<FishGameDetailRecordViewModel>
            {
                Items = new List<FishGameDetailRecordViewModel>()
            };

            //model.RuleFor(x => x.RoundSettleTimeBegin).Rule_NotEmptyDateTime();
            //model.RuleFor(x => x.RoundSettleTimeEnd).Rule_NotEmptyDateTime();

            //if (!SettingHelper.IsDev)
            //{
            //    model.RuleFor(x => x.RoundSettleTimeBegin).Rule_StartTimeEndTimeRange(model.RoundSettleTimeEnd, 30);
            //    model.RuleFor(x => x.RoundSettleTimeEnd).Rule_DateTimeMustLargeThanPreviousDays(90);
            //}

            //this.GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            ////檢查 VendorAPI設置維護
            //CheckIsVendorAPIMaintain();

            //var qeuryMerchant = MerchantRepository.GetSingleByCache(model.MerchantID);

            ////最多一次撈N筆
            //if (model.PageSize >= MAX_QUERY_SIZE)
            //    model.PageSize = MAX_QUERY_SIZE;

            //var filter = model.ReturnInjecteObject<QueryAGGFishDetailRecordRequestViewModel>();
            //filter.MerchantIDs = model.MerchantID.ToListCollection();
            //filter.RoundSettleTimeBegin = model.RoundSettleTimeBegin;
            //filter.RoundSettleTimeEnd = model.RoundSettleTimeEnd;

            //var query = AGGFishDetailRecordRepository.GetPagingByFilter(filter);

            //var pageModels = ConvertToNewTypePagingObject<AGGFishDetailRecordEntity, FishGameDetailRecordViewModel>(query);
            //pageModels.Items = query.Items.Select(x => new FishGameDetailRecordViewModel
            //{
            //    BetTime = x.BetTime,
            //    PlayerName = x.PlayerName,
            //    GameID = x.GameID,
            //    BetAmount = x.BetAmount,
            //    ValidAmount = x.BetAmount,
            //    PayoutAmount = x.PayoutAmount,
            //    WinLossAmount = x.WinLossAmount,
            //    WagerID = x.WagerID.ToString(),
            //    RoundID = x.PgsRoundID.ToString(),
            //    FishType = x.FishType,
            //    RoundSettleTime = x.RoundSettleTime,
            //})
            //    .ToList();

            ////從其他Table補充資訊
            //return pageModels;
        }


        /// <summary>
        /// 做一些基本檢查。後面會呼叫另個method 查資料
        /// </summary>
        public void ValidateScreenShotApiParameter(BetRecordDetailInfoLinkGenerateRequestModel model)
        {
            var merchantInfo = this.GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            if (model.TransactionNo.IsNotEmptyTextNotNull())
            {
            }
            else if (model.BetID.HasValue)
            {
                if (merchantInfo.Type == (int)MerchantTypeEnum.Plaform_DevelopEnvironment)
                {
                    throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel("BetID no longer support"));
                }

                if (merchantInfo.CreateTime >= new DateTime(2024, 9, 1))
                {
                    throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel("BetID no longer support"));
                }
            }
            else
            {
                throw new TerproException(ApiErrorCode.InvalidParameter);
            }
        }



        /// <summary>
        /// (單錢包模式) 根據投注單號取得資料
        /// </summary>
        public SingleWalletGameRecordViewModel GetBetRecordByTransactionNo(SingleWalletGameRecordRequestModel model)
        {
            model.RuleFor(x => x.TransactionNo).Rule_NotEmptyText();

            this.GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            //檢查 VendorAPI設置維護
            CheckIsVendorAPIMaintain();

            var queryTransactionNoMapping = BetRecordTransactionNoMappingRepository.GetByTransactionNoPGS(model.TransactionNo);

            SingleWalletGameRecordViewModel result = null;

            if (queryTransactionNoMapping == null)
            {
                var eazyTemp = GetRecordByEazyTempTable(model.TransactionNo);
                result = ConvertToViewModel(eazyTemp);
            }
            else
            {
                var query = GetByPgsTransactionNo(queryTransactionNoMapping.GameVendorID, queryTransactionNoMapping.BetRecordNoPGS);
                result = ConvertToViewModel(query);
            }

            if (result == null)
                return null;

            //權限不合法
            if (result.MerchantID != model.MerchantID)
                return null;

            return result;
        }


        /// <summary>
        /// 取得總報表
        /// </summary>
        public MerchantReportSummaryViewModel GetReportSummary(VendorReportSummaryRequestModel model)
        {
            model.RuleFor(x => x.StartTime).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.EndTime).Rule_NotEmptyDateTime();

            //正式環境才需要檢查range不得太大
            if (!SettingHelper.IsDev)
            {
                model.RuleFor(x => x.StartTime).Rule_StartTimeEndTimeRange(model.EndTime, 32);
            }

            this.GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            //檢查 VendorAPI設置維護
            CheckIsVendorAPIMaintain();

            var query = MerchantGameSummaryDataRepository.GetEachGameTotalSumOnDuration(model.MerchantID.ToListCollection(), null, model.StartTime, model.EndTime);

            var queryPromotion = MerchantEventSummaryDataRepository.GetEachMerchantTotalSumOnDuration(model.MerchantID.ToListCollection(), model.StartTime, model.EndTime);

            decimal rate_to_original = 1.0M;

            if (model.IsExchangeToOriginalRate)
            {
                rate_to_original = GetExchangeToOriginalRate(model.IsExchangeToOriginalRate, model.MerchantID);
            }

            var result = new MerchantReportSummaryViewModel
            {
                TotalRound = query.Select(x => x.TotalRound).Sum(),
                TotalBetAmount = query.Select(x => x.BetAmount).Sum() / rate_to_original,
                TotalValidAmount = query.Select(x => x.ValidAmount).Sum() / rate_to_original,
                TotalTaxAmount = query.Select(x => x.TaxAmount).Sum() / rate_to_original,
                TotalPayoutAmount = query.Select(x => x.PayoutAmount).Sum() / rate_to_original,
                TotalWinLossAmount = query.Select(x => x.WinLossAmount).Sum() / rate_to_original,
                GameSummary = query
                    .Select(x => new MerchantTotalSummaryItemViewModel
                    {
                        GameID = x.GameID,
                        TotalRound = x.TotalRound,
                        TotalBetAmount = x.BetAmount / rate_to_original,
                        TotalTaxAmount = x.TaxAmount / rate_to_original,
                        TotalWinLossAmount = x.WinLossAmount / rate_to_original,
                        TotalPayoutAmount = x.PayoutAmount / rate_to_original,
                        TotalValidAmount = x.ValidAmount / rate_to_original
                    })
                    .ToList()
            };

            if (queryPromotion.IsNotEmptyNull())
            {
                result.TotalPromotionBonus = queryPromotion.Select(x => x.TotalAmount).Sum() / rate_to_original;
            }

            return result;
        }


        /// <summary>
        /// 取得玩家輸贏報表(待處理)
        /// </summary>
        public PagedList<PlayerWinLossReportSummaryDetailItemModel> GetPlayerWinLossReport(PlayerWinLossReportSummaryRequestModel filter)
        {
            filter.RuleFor(x => x.StartTime).Rule_NotEmptyDateTime();
            filter.RuleFor(x => x.EndTime).Rule_NotEmptyDateTime();

            //正式環境才需要檢查range不得太大
            if (!SettingHelper.IsDev)
            {
                filter.RuleFor(x => x.StartTime).Rule_StartTimeEndTimeRange(filter.EndTime, 32);
            }

            filter.PageIndex = filter.PageIndex <= 1 ? 1 : filter.PageIndex;
            filter.PageSize = filter.PageSize >= 10000 ? 10000 : filter.PageSize;

            this.GetMerchantAndCheckIsValid(filter.MerchantID, filter.MerchantSecretKey);

            //檢查 VendorAPI設置維護
            CheckIsVendorAPIMaintain();

            var searchFilter = new QueryPlayerGameSummaryFilterByVendorConfigApiRequestParameterModel
            {
                PageSize = filter.PageSize,
                PageIndex = filter.PageIndex,
                MerchantID = filter.MerchantID,
                Begin = filter.StartTime,
                End = filter.EndTime,
                GameID = filter.GameID,
                OrderByColumn = "WinLossAmount",
            };

            if (!string.IsNullOrWhiteSpace(filter.PlayerName))
            {
                var matchPlayer = PlayerRepository.GetByPlayerNameAndMerchantID(filter.PlayerName, filter.MerchantID);

                if (matchPlayer == null)
                {
                    throw new TerproException(ApiErrorCode.NotFoundPlayer);
                }

                searchFilter.PlayerID = matchPlayer.PlayerID;
            }

            var query = PlayerGameDataRepository.GetPlayerWinLossRankingReport(searchFilter);

            decimal rate_to_original = 1.0M;

            if (filter.IsExchangeToOriginalRate)
            {
                rate_to_original = GetExchangeToOriginalRate(filter.IsExchangeToOriginalRate, filter.MerchantID);
            }

            var pageModels = ConvertToNewTypePagingObject<PlayerWinLossWinRateReportModel, PlayerWinLossReportSummaryDetailItemModel>(query);

            pageModels.Items = query.Items.Select(x =>
                {
                    var convert = x.ReturnInjecteObject<PlayerWinLossReportSummaryDetailItemModel>();
                    convert.BetAmount = convert.BetAmount / rate_to_original;
                    convert.PayoutAmount = convert.PayoutAmount / rate_to_original;
                    convert.WinLossAmount = convert.WinLossAmount / rate_to_original;

                    return convert;
                })
                .ToList();

            SetWinLossRankingReportExtraProperty(pageModels.Items);
            return pageModels;
        }


        /// <summary>
        /// 取得結算數字有更改過的注單
        /// </summary>
        public PagedList<SettleInfoChangedGameRecordViewModel> GetSettleChangeRecords(SettleInfoChangedGameRecordRequestModel filter)
        {
            filter.RuleFor(x => x.StartTime).Rule_NotEmptyDateTime();
            filter.RuleFor(x => x.EndTime).Rule_NotEmptyDateTime();

            filter.PageSize = filter.PageSize >= MAX_QUERY_SIZE ? MAX_QUERY_SIZE : filter.PageSize;

            //正式環境才需要檢查range不得太大
            if (!SettingHelper.IsDev)
            {
                filter.RuleFor(x => x.StartTime).Rule_StartTimeEndTimeRange(filter.EndTime, 62);
            }

            this.GetMerchantAndCheckIsValid(filter.MerchantID, filter.MerchantSecretKey);

            //檢查 VendorAPI設置維護
            CheckIsVendorAPIMaintain();

            var queryFilter = new QueryPerGameDetailChangeLogRequestViewModel
            {
                MerchantIDs = filter.MerchantID.ToListCollection(),
                CreateTimeBegin = filter.StartTime,
                CreateTimeEnd = filter.EndTime,
                PageSize = filter.PageSize,
                PageIndex = filter.PageIndex
            };

            var query = PerGameDetailChangeLogRepository.GetPagingByFilter(queryFilter);

            var pageModels = ConvertToNewTypePagingObject<PerGameDetailChangeLogEntity, SettleInfoChangedGameRecordViewModel>(query);
            pageModels.Items = query.Items.Select(x =>
            {
                var parsingModel = PerGameDetailChangeLogRepository.ConvertToParseModel(x);

                var convert = x.ReturnInjecteObject<SettleInfoChangedGameRecordViewModel>();

                convert.BetID = x.PerGameDetailLogID;
                convert.Before = parsingModel.BeforeObject.ReturnInjecteObject<PerGameChangeSettleInfoBeforeAfterViewModel>();
                convert.Before.WinLossAmount = parsingModel.BeforeObject.UpdateAmount;
                convert.After = parsingModel.AfterObject.ReturnInjecteObject<PerGameChangeSettleInfoBeforeAfterViewModel>();
                convert.After.WinLossAmount = parsingModel.AfterObject.UpdateAmount;

                return convert;

            }).ToList();

            return pageModels;
        }


        private BetRecordGeneralColumnModel GetByPgsTransactionNo(int gameVendorID, string pgsTransactionNo)
        {
            var service = ServiceInstanceHelper.InstanceVendorService(gameVendorID);
            var result = service.GetByPGSTransactionNo(pgsTransactionNo);

            if (result != null)
                return result;

            var tempData = GetRecordByEazyTempTable(pgsTransactionNo);
            return tempData;
        }


        private BetRecordGeneralColumnModel GetRecordByEazyTempTable(string pgsTransactionNo)
        {
            //如果查不到資料 嘗試從eazyTempValue 資料表查詢
            var queryByTempTable = EazyTempValueRepository.GetByTransactionNo(pgsTransactionNo, EazyTempValueTypeEnum.PlaceBetTooMuchTime.ToInteger());

            if (queryByTempTable == null)
                return null;

            return new BetRecordGeneralColumnModel
            {
                Status = BetRecordStatusEnum.Cancel.ToInteger(),
                BetRecordNoPGS = pgsTransactionNo,
                CreateTime = queryByTempTable.CreateTime,
                GameID = queryByTempTable.GameID.GetValueOrDefault(),
                BetAmount = queryByTempTable.Amount1.GetValueOrNullZero(),
                PlayerID = queryByTempTable.PlayerID ?? 0,
                PlayerName = queryByTempTable.PlayerName,
                ValidAmount = 0,
                PayoutAmount = 0,
                WinLossAmount = 0,
            };
        }

        private SingleWalletGameRecordViewModel ConvertToViewModel(BetRecordGeneralColumnModel query)
        {
            if (query == null)
                return null;

            return new SingleWalletGameRecordViewModel
            {
                BetAmount = query.BetAmount,
                GameID = query.GameID,
                MerchantID = query.MerchantID,
                BetTime = query.CreateTime,
                GameName = DbMultiLanguageTextHelper.GetGameDisplayName(query.GameID),
                ValidAmount = query.ValidAmount,
                PayoutAmount = query.PayoutAmount,
                TransactionNo = query.BetRecordNoPGS,
                WinLossAmount = query.WinLossAmount,
                TaxAmount = query.TaxAmount,
                Status = query.Status,
                StatusText = MultiLanguageTextHelper.GetEnumsCultureText((BetRecordStatusEnum)query.Status),
                SettlementTime = query.SettlementTime,
                PlayerName = query.PlayerName,
            };
        }



        private void SetExtraProperty(int merchantID, List<GameRecordViewModel> viewModels, GameRecordRequestModel filter)
        {
            if (viewModels.IsEmptyOrNull())
                return;

            var extraInfoSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.DataQuery_SearchExtraInfoInVendorRecordsAPI);

            bool isEnableExtraInfo = extraInfoSetting.SettingValueOfBoolean;

            if (isEnableExtraInfo || filter.IsSearchExtraDetail)
            {
                List<int> gameIDs = viewModels.Select(z => z.GameID).Distinct().ToList();
                string columns = " GameID , GameType ";

                var queryGameDic = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
                {
                    GameIDs = gameIDs,

                }, columns)
                    .ToDictionary(z => z.GameID, z => z.GameType);

                foreach (var item in viewModels)
                {
                    item.ExtraInfo = item.ExtraInfo ?? new GameRecordExtraInfoViewModel();

                    if (queryGameDic.ContainsKey(item.GameID))
                    {
                        item.ExtraInfo.GameType = queryGameDic[item.GameID];
                    }
                }
            }

            //如果有參數指定
            if (filter.IsSearchExtraDetail)
            {
                List<string> liveGameBetRecordNos = viewModels
                    .Where(x => AppHelper.IsLiveCasinoGameVendors(x.GameID))
                    .Select(x => x.TransactionNo)
                    .ToList();

                bool isAnyLiveCasinoGame = liveGameBetRecordNos.IsNotEmptyNull();

                if (isAnyLiveCasinoGame)
                {
                    string selectCoumns = " BetRecordNoPGS , RoundID , RoundTotalFlowAmount ";

                    var queryRecord = AGGLiveDetailJsonRepository.GetByRecordNoList(liveGameBetRecordNos, selectCoumns);

                    if (queryRecord.IsNotEmptyNull())
                    {
                        foreach (var item in viewModels)
                        {
                            if (!AppHelper.IsLiveCasinoGameVendors(item.GameID))
                                continue;

                            item.ExtraInfo = item.ExtraInfo ?? new GameRecordExtraInfoViewModel();

                            var match = queryRecord.FirstOrDefault(x => x.BetRecordNoPGS == item.TransactionNo);

                            if (match != null)
                            {
                                item.ExtraInfo.RoundID = match.RoundID;
                                item.ExtraInfo.RoundTotalFlowAmount = match.RoundTotalFlowAmount.GetValueOrNullZero();
                            }
                        }
                    }
                }
            }


            //指定轉換貨幣....
            if (filter.IsExchangeToOriginalRate)
            {
                var rate_to_original = GetExchangeToOriginalRate(filter.IsExchangeToOriginalRate, merchantID);

                //還是判斷下1 可以節省一點迴圈運算浪費
                if (rate_to_original != 1 && rate_to_original != 0)
                {
                    foreach (var item in viewModels)
                    {
                        item.BetAmount = item.BetAmount / rate_to_original;
                        item.PayoutAmount = item.PayoutAmount / rate_to_original;
                        item.ValidAmount = item.ValidAmount / rate_to_original;
                        item.TaxAmount = item.TaxAmount / rate_to_original;
                        item.WinLossAmount = item.WinLossAmount / rate_to_original;
                        item.BeforeAmount = item.BeforeAmount / rate_to_original;

                        if (item.ExtraInfo != null)
                        {
                            item.ExtraInfo.JackpotAmount = item.ExtraInfo.JackpotAmount / rate_to_original;
                            item.ExtraInfo.RoundTotalFlowAmount = item.ExtraInfo.RoundTotalFlowAmount / rate_to_original;
                        }
                    }
                }
            }

            return;
        }


        private void SetWinLossRankingReportExtraProperty(List<PlayerWinLossReportSummaryDetailItemModel> items)
        {
            var gamesDic = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(items.Select(x => x.GameID));

            foreach (var item in items)
            {
                item.GameName = gamesDic.GetDictionaryOrDefaultValue(item.GameID);
            }
        }


        /// <summary>
        /// 針對UFA商戶的特殊流程 (可以節省一點資源浪費)
        /// 在執行分頁查詢之前 (分頁較耗資源) 先嘗試查詢是否有至少一筆資料
        /// </summary>
        public bool IsReturnEmptyResultByPreQueryTop1<T>(T model, int merchantType) where T : IQueryWagerRequest
        {
            int merchantID = model.MerchantID;
            DateTime beginTime = model.StartTime;
            DateTime endTime = model.EndTime;

            //基本上 這邊指定的就是局數比較大的幾個商戶 直接跳過這個檢查(不採用小規模查詢) 減少一點redis查詢存取
            List<int> skipVipMerchants = new List<int>
            {
                175080,
                175038,
                176130,
                176386,
                176252,
                176403,
                176236,
                175079,
                176043,
                175768,
                175087,
                175064,
                175204,
                175774,
                176229,
                176237,
                176177,
                175823,
                176390,
                175251,
                176272,
                175226,
                175425,
                175270,
                175084,
                176437,
                176239,
                175268,
                175323,
                175846,
                175338,
                175996,
                176455,
                175272,
                175163
            };

            if (SettingHelper.IsProduction && skipVipMerchants.Contains(merchantID))
                return false;

            #region 舊版寫法(標記量很小的商戶)

            //基本上都是UFA商戶
            //List<int> merchantIDs = new List<int>
            //{
            //    //175445,  //MYR有數據
            //    175446,
            //    175447,
            //    175448,
            //    175449,
            //    176084,
            //    175106,
            //    176092
            //};
            #endregion

            //兩種狀況會執行小規模查詢
            // 1. 商戶配置有打開 (ps. 量大的商戶會在上面的code先被排除了)
            // 2. 測試用商戶 (dev環境)
            bool isEnableSettingOrDevMerchant = IsEnableQueryTop1BeforePagingQuery(merchantID) || merchantType == (int)MerchantTypeEnum.Plaform_DevelopEnvironment;

            if (!isEnableSettingOrDevMerchant)
                return false;

            //先確認至少有1筆數據 才執行後續的分頁查詢 (量很小的商戶。通常有大量空窗時段 或 根本沒量...會浪費查詢資源)
            QueryDetailLogRequestViewModel filter = new QueryDetailLogRequestViewModel();
            filter.MerchantID = merchantID;
            filter.GameStartTimeBegin = beginTime;
            filter.GameStartTimeEnd = endTime;

            bool isReturnEmpty = false;

            if (IsQueryByBriefJoinTable(model))
            {
                var query = PerGameDetailLogMerchantJoinRepository.GetByFilter(filter, " Top 1 PerGameDetailLogID ");
                isReturnEmpty = query.IsEmptyOrNull();
            }
            else
            {
                var query = PerGameDetailLogRepository.GetByFilter(filter, " Top 1 Id ");
                isReturnEmpty = query.IsEmptyOrNull();
            }

            return isReturnEmpty;
        }


        protected virtual bool IsEnableQueryTop1BeforePagingQuery(int merchantID)
        {
            var queryTop1 = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.DataQuery_QueryTop1_BeforePagingQuery);
            return queryTop1.SettingValueOfInt == NormalActiveStatus.Enable.ToInteger();
        }


    }
}
