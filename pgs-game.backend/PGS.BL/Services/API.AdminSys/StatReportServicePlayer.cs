using DocumentFormat.OpenXml.Wordprocessing;
using game.Entity.Game;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Models.StoreProcedures;
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
using example.invalid;
using System.Linq;
using System.Threading.Tasks;


namespace target.BL.Services
{


    public partial class StatReportService : BaseAdminSysService, IStatReportService
    {


        public PlayerGameSummaryStatReportViewModel GetPlayerGameReport(int userID, QueryPlayerGameSummaryStatReportReportApiRequestParameterModel filter)
        {
            if (!filter.Begin.HasValue)
                filter.Begin = DateTimeHelper.GetCurrentTime().To_00_00_00();

            if (!filter.End.HasValue)
                filter.End = DateTimeHelper.GetCurrentTime().AddDays(1).To_23_59_59();

            SetDefaultBeginEndTimeAndCheckDaysRange(filter, 8, 62);

            var defaultResult = new PlayerGameSummaryStatReportViewModel();

            List<int> playerIDs = filter.PlayerID.ToIntegerListCollection();

            if (!string.IsNullOrWhiteSpace(filter.PlayerName))
            {
                var playerIDResult = GetPlayerIDsByPlayerName(userID, filter.PlayerName, playerIDs);

                if (playerIDResult.IsPlayerNotFound)
                    return defaultResult;

                if (playerIDResult.PlayerIDs.Count() > 1)
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_MultiplePlayerNameMatch_PleaseUsePlayerID);

                filter.PlayerID = playerIDResult.PlayerIDs.FirstOrDefault();
            }

            if (!filter.PlayerID.HasValue || filter.PlayerID <= 0)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            int targetPlayerID = filter.PlayerID.Value;

            var queryPlayer = PlayerRepository.GetSingleByCache(targetPlayerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            //有可能不小心輸入到別家的ID (或誤把帳號輸入到playerID)
            CheckViewMerchantPermission(userID, queryPlayer.MerchantID, isTriggerRiskScoreProcess: false);

            string merchantName = MerchantRepository.GetSingleByCache(queryPlayer.MerchantID)?.Name;

            var querySummary = PlayerGameDataRepository.GetGameSummaryGroupByPlayerAndGame(new QueryPlayerGameStatisticParameter
            {
                MerchantID = queryPlayer.MerchantID,
                PlayerID = targetPlayerID,
                Begin = filter.Begin.Value,
                End = filter.End.Value
            });

            if (querySummary.IsEmptyOrNull())
                return defaultResult;

            if (HasPermission_OnlyVisibleIDGGames(userID))
            {
                querySummary = querySummary
                    .Where(x => AppHelper.IsIDGVisibleGames(x.GameID))
                    .ToList();
            }

            var convertViewModel = querySummary.Select(z =>
                {
                    var convert = z.ReturnInjecteObject<PlayerGameSummaryStatItemViewModel>();
                    convert.GameName = DbMultiLanguageTextHelper.GetGameDisplayName(z.GameID);
                    convert.PlayerName = queryPlayer.PlayerName;
                    convert.MerchantID = queryPlayer.MerchantID;
                    convert.MerchantName = merchantName;
                    convert.RTP = CommonLogicHelper.CalculateRTP_Percentage(convert.BetAmount, convert.PayoutAmount);

                    return convert;
                })
                .ToList();

            var totalRow = convertViewModel.CalculateTotalSumMergeToOneObject();
            totalRow.RTP = CommonLogicHelper.CalculateRTP_Percentage(totalRow.BetAmount, totalRow.PayoutAmount);

            var viewModel = new PlayerGameSummaryStatReportViewModel
            {
                MerchantID = queryPlayer.MerchantID,
                PlayerID = queryPlayer.PlayerID,
                PlayerName = queryPlayer.PlayerName,
                PlayerGameSummary = convertViewModel,
                TotalSummary = totalRow
            };

            viewModel.ServerTimeTW = GetDisplayCurrentServerTimeTW().ServerTimeTW;

            QueryPlayerEventBonusSummary(viewModel, filter.Begin.Value, filter.End.Value);
            return viewModel;
        }



        private void QueryPlayerEventBonusSummary(PlayerGameSummaryStatReportViewModel viewModel, DateTime begin, DateTime end)
        {
            //預期筆數應該不至於太誇張 (頂多是捕魚)
            var queryEventSummary = EventPrizeRecordRepository.GetPlayerEventSummary(viewModel.PlayerID, begin, end);

            if (queryEventSummary.IsEmptyOrNull())
                return;

            var queryMerchant = MerchantRepository.GetSingleByCache(viewModel.MerchantID);

            var convert = queryEventSummary.Select(x => new PlayerEventSummaryStatItemViewModel
            {
                PlayerID = viewModel.PlayerID,
                PlayerName = viewModel.PlayerName,
                MerchantID = viewModel.MerchantID,
                MerchantName = queryMerchant?.Name,
                EventID = x.EventID,
                EventName = x.EventName,
                TotalAmount = x.TotalAmount,
                TotalCount = x.TotalCount
            })
                .ToList();

            viewModel.EventBonusSummary = convert;
            viewModel.TotalEventSummary = convert.CalculateTotalSumMergeToOneObject();
            return;
        }



        public PlayerGameSummaryReportFilterByVendorConfigViewModel GetPlayerWinLossRankingReport(int userID, QueryPlayerGameSummaryFilterByVendorConfigApiRequestParameterModel filter)
        {
            if (!filter.MerchantID.HasValue)
            {
                filter.RuleFor(x => x.VendorConfigID).Rule_IsNotNullNotZeroNotMinus();
            }

            if (!filter.Begin.HasValue)
                filter.Begin = DateTimeHelper.GetCurrentTime().To_00_00_00();

            if (!filter.End.HasValue)
                filter.End = DateTimeHelper.GetCurrentTime().AddDays(1).To_23_59_59();

            if ((filter.End.Value - filter.Begin.Value).TotalDays > 62)
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_DateTimeRangeTooLarge);
            }

            if (filter.VendorConfigID.HasValue)
            {
                var queryVendorConfig = VendorConfigRepository.GetById(filter.VendorConfigID.Value);
                filter.GameVendorID = queryVendorConfig.VendorID;
            }

            if (!string.IsNullOrWhiteSpace(filter.PlayerName))
            {
                var playerIDResult = GetPlayerIDsByPlayerName(userID, filter.PlayerName, new List<int>());

                if (playerIDResult.IsPlayerNotFound)
                    throw new TerproException(ApiErrorCode.NotFoundPlayer);

                if (playerIDResult.PlayerIDs.Count() > 1)
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_MultiplePlayerNameMatch_PleaseUsePlayerID);

                filter.PlayerID = playerIDResult.PlayerIDs.FirstOrDefault();
            }

            var viewModel = new PlayerGameSummaryReportFilterByVendorConfigViewModel();
            viewModel.ServerTimeTW = GetDisplayCurrentServerTimeTW().ServerTimeTW;

            var query = PlayerGameDataRepository.GetPlayerWinLossRankingReport(filter);

            var pagingResult = ConvertToNewTypePagingObject<PlayerWinLossWinRateReportModel, PlayerGameSummaryReportFilterByVendorConfigDetailItemModel>(query);
            pagingResult.Items = query.Items.Select(x =>
                {
                    var convert = x.ReturnInjecteObject<PlayerGameSummaryReportFilterByVendorConfigDetailItemModel>();
                    return convert;
                })
                .ToList();

            viewModel.PagingItems = pagingResult;

            SetPlayerWinRateReportExtraProperty(viewModel);
            return viewModel;
        }


        public QueryFreespinStatReportViewModel GetAGGEventSummaryReport(int userID, QueryFreespinStatReportApiRequestParameter filter)
        {
            filter.RuleFor(x => x.VendorConfigID).Rule_IsNotNullNotZeroNotMinus();

            //filter.RuleFor(x => x.EventPrizeType).Rule_IsNotNullNotZeroNotMinus();

            var query = VendorConfigRepository.GetByFilter(new QueryVendorConfigRequestViewModel
            {
                VendorIDs = GameVendorTypeEnum.AGG.ToInteger().ToListCollection(),
                VendorConfigIDs = filter.VendorConfigID.Value.ToListCollection()
            })
            .FirstOrDefault();

            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundVendorConfig);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            List<int> prizeTypes = EventDefinitionHelper.PrizeTypes_AGGReport_Page();

            if(filter.EventPrizeType.HasValue)
            {
                prizeTypes = new List<int>
                {
                    filter.EventPrizeType.Value
                };
            }

            var queryFilter = new EachMerchantEventPrizeTotalSummaryQueryParameter
            {
                EventPrizeTypes = prizeTypes,
                BeginUTC = filter.Begin.Value,
                EndUTC = filter.End.Value,
                VendorConfigID = query.VendorConfigID,
                MerchantID = filter.MerchantID,
                GameID = filter.GameID
            };

            if (!currentUser.IsSuperAdminOrg)
            {
                queryFilter.FilterMerchantIDsByMultipleHierarchyMerchant = currentUser.MyMultipleHierarchyMerchantIDs;
            }

            var queryEachSummaryItems = EventPrizeRecordRepository.GetEachMerchantEventPrizeTotalSumByDetailTable(queryFilter);

            var items = new List<EachMerchantAGGEventPrizeTotalSummaryViewModel>();

            var queryMerchants = DbValueCacheHelper.GetMerchants(queryEachSummaryItems.Select(x => x.MerchantID));
            var queryGamesDic = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(queryEachSummaryItems.Select(x => x.GameID));

            foreach (var item in queryEachSummaryItems)
            {
                var matchM = queryMerchants.FirstOrDefault(x => x.MerchantID == item.MerchantID);

                var myInstance = item.ReturnInjecteObject<EachMerchantAGGEventPrizeTotalSummaryViewModel>();
                myInstance.MerchantName = matchM?.Name;
                myInstance.Currency = matchM?.Currency;
                myInstance.GameName = queryGamesDic.GetDictionaryOrDefaultValue(item.GameID);
                myInstance.EventPrizeTypeName = MultiLanguageTextHelper.GetEnumsCultureText((EventPrizeTypeEnum)item.EventPrizeType);

                items.Add(myInstance);
            }

            items = items.OrderBy(x => x.MerchantID).ThenBy(x => x.GameID).ToList();

            var viewModel = new QueryFreespinStatReportViewModel
            {
                Items = items,
            };

            viewModel.TotalSummary = items.CalculateTotalSumMergeToOneObject();
            return viewModel;
        }

        public PagedList<AGGFreespinStatusListItemViewModel> GetFreespinStatusList(int userID, QueryFreespinStatusListRequestParameter filter)
        {
            filter.RuleFor(x => x.VendorConfigID).Rule_IsNotNullNotZeroNotMinus();
            filter.RuleFor(x => x.Begin).Rule_NotNullDateTime();
            filter.RuleFor(x => x.End).Rule_NotNullDateTime();

            bool isHighPermission = HasSuperAdminPermission(userID) || HasTargetPermission(userID, "Special_AM_Permission");

            //非管理員 merchantID是必填
            if (!isHighPermission)
            {
                filter.RuleFor(x => x.MerchantID).Rule_IsNotNullNotZeroNotMinus();
            }

            var query = VendorConfigRepository.GetByFilter(new QueryVendorConfigRequestViewModel
            {
                VendorIDs = GameVendorTypeEnum.AGG.ToInteger().ToListCollection(),
                VendorConfigIDs = filter.VendorConfigID.Value.ToListCollection()
            })
            .FirstOrDefault();

            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundVendorConfig);

            var aggService = new AGGIntegrationService();

            var apiFilter = new AGGAPIFreespinStatusReqeustParameter
            {
                VendorConfigID = filter.VendorConfigID,
                VendorAPIParam = new VendorAPIFreespinStatusReqeustParameter
                {
                    MerchantID = filter.MerchantID.ParseNullToZero(),
                    StartTime = filter.Begin.Value,
                    EndTime = filter.End.Value,
                    FreeSpinID = filter.FreespinID,
                    PageIndex = filter.PageIndex,
                    PageSize = filter.PageSize,
                    freeSpinUseStatus = filter.FreeSpinUseStatus
                }
            };

            if (!string.IsNullOrWhiteSpace(filter.PlayerName))
            {
                var queryMatchPlayer = GetPlayerIDsByPlayerName(userID, filter.PlayerName, null, filter.MerchantID);

                if (queryMatchPlayer.PlayerIDs.CountOrZero() >= 2)
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter_PleaseSelectMerchantID, new ErrorDetailModel("Multiple player match"));
                }

                if (queryMatchPlayer.PlayerIDs.IsEmptyOrNull())
                {
                    throw new TerproException(ApiErrorCode.NotFoundPlayer);
                }

                apiFilter.UserID = queryMatchPlayer.PlayerIDs.FirstOrDefault();
            }

            var queryEachSummaryItems = aggService.Freespin_Status(apiFilter);

            var viewModel = new PagedList<AGGFreespinStatusListItemViewModel>();
            viewModel.TotalCount = queryEachSummaryItems.total;
            viewModel.TotalPage = AppHelper.CalculateTotalPage(viewModel.TotalCount, filter.PageSize);
            viewModel.PageSize = filter.PageSize;
            viewModel.PageIndex = filter.PageIndex;
            viewModel.Items = new List<AGGFreespinStatusListItemViewModel>();

            foreach (var item in queryEachSummaryItems.freeSpinStatuses)
            {
                var instance = item.ReturnInjecteObject<AGGFreespinStatusListItemViewModel>();
                instance.PlayerID = Int32.Parse(item.UserId);
                instance.CreateTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(item.CreateDate);

                var targetEnumValue = ExtensionOfEnum.GetEnumByTextValue(item.FreeSpinUseStatus , AGGFreespinStatusEnum.Undefined);
                instance.FreeSpinUseStatus = MultiLanguageTextHelper.GetEnumsCultureText(targetEnumValue);

                if (!string.IsNullOrWhiteSpace(item.ClientId))
                {
                    instance.MerchantID = Int32.Parse(item.ClientId);
                }

                viewModel.Items.Add(instance);
            }

            List<int> playerIDs = viewModel.Items.Select(x => x.PlayerID).ToList();

            var queryPlayers = PlayerRepository.GetPlayerJoinInfo(playerIDs);

            foreach (var item in viewModel.Items)
            {
                var matchInfo = queryPlayers.FirstOrDefault(x => x.PlayerID == item.PlayerID);

                if (matchInfo != null)
                {
                    item.PlayerName = matchInfo.PlayerName;
                    item.MerchantName = matchInfo.MerchantName;
                }
            }

            //避免客戶疑惑來詢問
            if (!string.IsNullOrWhiteSpace(filter.FreespinID) && viewModel.Items.IsEmptyOrNull())
            {
                var queryFreespinList = aggService.Freespin_GetList(new AGGGameFreespinListReqeustParameter
                {
                    StartTime = DateTimeHelper.GetCurrentTime().AddDays(-14),
                    EndTime = DateTimeHelper.GetCurrentTime().AddDays(14),
                    PageSize = 10,
                    VendorConfigID = filter.VendorConfigID,
                    FreespinID = filter.FreespinID,
                });

                if(queryFreespinList.freeSpinCampaigns.IsNotEmptyNull())
                {
                    string msg = $"The freespin has been sent, but has not yet been used. Status records are unavailable." +
                        $"该笔freespin已发送。但尚未使用。无法顯示status纪录 (freespinID: {filter.FreespinID})";

                    throw new TerproException(ApiErrorCode.NotFoundData, msg);
                }
            }

            return viewModel;
        }



        public RetentionReportViewModel GetRetentionReport(int userID, QueryRetentionReportRequestParameterModel filter)
        {
            if (!filter.Begin.HasValue)
                filter.Begin = DateTimeHelper.GetCurrentTime().To_00_00_00();

            filter.RuleFor(z => z.MerchantID).Rule_NotZeroNotMinus();

            CheckViewMerchantPermission(userID, filter.MerchantID);

            List<DateTime> dateList = new List<DateTime>();

            DateTime now = DateTimeHelper.GetCurrentTime();
            DateTime beginDateWithTimezone = filter.Begin.Value.AddHours(filter.TimeZone).Date;

            int totalDays = 7;

            Dictionary<int, List<int>> eachDayNewPlayerIDs = new Dictionary<int, List<int>>();

            DateTime minRegisterTimeUTC = beginDateWithTimezone.Date.AddHours(-1 * filter.TimeZone);
            DateTime maxRegisterTimeUTC = minRegisterTimeUTC.AddDays(totalDays);

            var queryPlayers = PlayerRepository.GetByFilter(new QueryPlayerRequestViewModel
            {
                MerchantIDs = filter.MerchantID.ToListCollection(),
                CreateTimeBegin = minRegisterTimeUTC,
                CreateTimeEnd = maxRegisterTimeUTC
            }, " PlayerID , CreateTime ");


            //每一天的新註冊玩家塞入Dic
            for (int day = 0; day < totalDays; day++)
            {
                DateTime beginTimeUTC = beginDateWithTimezone.Date.AddDays(day).AddHours(-1 * filter.TimeZone);
                DateTime endTimeUTC = beginTimeUTC.AddDays(1);

                var players = queryPlayers.Where(z => z.CreateTime >= beginTimeUTC && z.CreateTime < endTimeUTC).Select(z => z.PlayerID).ToList();

                eachDayNewPlayerIDs.Add(day, players);
            }

            DateTime minBetTime = filter.Begin.Value;
            DateTime maxBetTime = minBetTime.AddDays(30 + totalDays); //多查一天 避免少算

            //先一口氣查出 一段時間區間內 每一天的投注玩家
            var eachDateActivePlayersMapping = PlayerGameDataRepository.GetEachDatePlayerIDsByMerchant(minBetTime, maxBetTime, filter.MerchantID, filter.TimeZone);

            List<RetentionReportItemViewModel> items = new List<RetentionReportItemViewModel>();

            List<int> targetDaysRetention = new List<int>
                {
                    1,
                    3,
                    7,
                    14,
                    30
                };

            foreach (var item in eachDayNewPlayerIDs)
            {
                int dayOfThisItem = item.Key;
                List<int> newPlayerIDsOfThisDay = item.Value;

                DateTime BeginOfRow = beginDateWithTimezone.AddDays(dayOfThisItem);
                DateTime EndOfRow = BeginOfRow.AddDays(1);

                var myInstance = new RetentionReportItemViewModel
                {
                    Date = BeginOfRow,
                    DateText = BeginOfRow.ToString("yyyy-MM-dd"),
                    NewPlayerCount = item.Value.Count(),
                    DaysRetentionMapping = new List<RetentionRateDayMapping>(),
                    DaysRetentionRate = new List<decimal?>()
                };

                foreach (int eachTargetDay in targetDaysRetention)
                {
                    DateTime referenceDate = BeginOfRow.AddDays(eachTargetDay);

                    //查出指定日期 還有活躍的玩家
                    int mergeMatchActivePlayerCount = eachDateActivePlayersMapping[referenceDate].Where(z => newPlayerIDsOfThisDay.Contains(z)).Count();

                    decimal retentionOfThisDay = CalculateRetentionRate(mergeMatchActivePlayerCount, newPlayerIDsOfThisDay.Count());

                    var para = new RetentionRateDayMapping
                    {
                        TargetDay = eachTargetDay,
                        RetentionRate = retentionOfThisDay,
                        RetentionPlayerCount = mergeMatchActivePlayerCount,
                        IsFutureDate = mergeMatchActivePlayerCount == 0 && referenceDate > now,
                    };

                    myInstance.DaysRetentionMapping.Add(para);
                }

                items.Add(myInstance);
            }

            //為了前端方便撈資料
            foreach (var item in items)
            {
                item.Day1 = item.DaysRetentionMapping.FirstOrDefault(z => z.TargetDay == 1);
                item.Day3 = item.DaysRetentionMapping.FirstOrDefault(z => z.TargetDay == 3);
                item.Day7 = item.DaysRetentionMapping.FirstOrDefault(z => z.TargetDay == 7);
                item.Day14 = item.DaysRetentionMapping.FirstOrDefault(z => z.TargetDay == 14);
                item.Day30 = item.DaysRetentionMapping.FirstOrDefault(z => z.TargetDay == 30);
            }

            return new RetentionReportViewModel
            {
                Items = items
            };
        }


        public MerchantAllPlayerWinLossStatReportInfoViewModel GetMerchantAllPlayerWinLossReport(int userID, QueryMerchantAllPlayerWinLossReportApiRequestParameterModel filter)
        {
            filter.RuleFor(z => z.Begin).Rule_NotNullDateTime();
            filter.RuleFor(z => z.End).Rule_NotNullDateTime();
            filter.RuleFor(z => z.MerchantID).Rule_NotMinus();

            SetDefaultBeginEndTimeAndCheckDaysRange(filter, 8, 62);

            var defaultResult = new MerchantAllPlayerWinLossStatReportInfoViewModel();

            if (!string.IsNullOrWhiteSpace(filter.PlayerName))
            {
                var playerIDResult = GetPlayerIDsByPlayerName(userID, filter.PlayerName, new List<int>());

                if (playerIDResult.IsPlayerNotFound)
                    return defaultResult;

                if (playerIDResult.PlayerIDs.Count() > 1)
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_MultiplePlayerNameMatch_PleaseUsePlayerID);

                filter.PlayerID = playerIDResult.PlayerIDs.FirstOrDefault();
            }

            if (filter.PlayerID.HasValue)
            {
                var queryPlayer = PlayerRepository.GetById(filter.PlayerID.Value);

                if (queryPlayer == null)
                    throw new TerproException(ApiErrorCode.NotFoundPlayer);

                CheckViewPlayerPermission(userID, queryPlayer.PlayerID, isTriggerRiskScoreProcess: false);

                filter.MerchantID = queryPlayer.MerchantID;
            }
            else
            {
                filter.RuleFor(z => z.MerchantID).Rule_NotZeroNotMinus();
            }

            CheckViewMerchantPermission(userID, filter.MerchantID);

            var param = new Stat_MerchantAllPlayerWinLossParameterModel
            {
                DateStart = filter.Begin.Value,
                DateEnd = filter.End.Value,
                MerchantID = filter.MerchantID
            };

            if (!filter.PlayerID.HasValue)
            {
                //若被標記為大資料商戶 時間範圍禁止太大的
                bool isLargeDate = MerchantCustomizedSettingRepository.IsYesValue(filter.MerchantID, MerchantCustomizedSettingKeyEnum.IsLargeDataMerchant);

                int myMaxDays = isLargeDate ? 7 : DefaultMaxDays;

                //如果沒過濾playerID。時間區間最多只能N天
                filter.RuleFor(x => x.Begin).Rule_StartTimeAndEndTimeMaxDaysRange(filter.End, myMaxDays);
            }

            var querySP = StatReportStoreProcedureRepository.GetMerchantAllPlayerWinLoss(param);

            //若指定只需要特定玩家資料
            if (filter.PlayerID > 0)
            {
                querySP = querySP
                    .Where(z => z.PlayerID == filter.PlayerID)
                    .ToList();
            }

            var vieModelItems = querySP.Select(x =>
                {
                    var convert = x.ReturnInjecteObject<MerchantAllPlayerWinLossStatReportViewModel>();
                    convert.MerchantID = filter.MerchantID > 0 ? filter.MerchantID : 0;
                    convert.RTP = CommonLogicHelper.CalculateRTP_Percentage(convert.BetAmount, convert.PayoutAmount);
                    return convert;
                })
                .ToList();

            var totalSummary = vieModelItems.CalculateTotalSumMergeToOneObject();
            totalSummary.RTP = CommonLogicHelper.CalculateRTP_Percentage(totalSummary.BetAmount, totalSummary.PayoutAmount);

            var viewModel = new MerchantAllPlayerWinLossStatReportInfoViewModel
            {
                ServerTimeTW = GetDisplayCurrentServerTimeTW().ServerTimeTW,
                TotalSummary = totalSummary,
                Items = vieModelItems
            };

            if (totalSummary.BetAmount == 0)
            {
                viewModel.IsShowGoToAgentReportHint = IsChildAgentExist(filter.MerchantID);
            }

            return viewModel;
        }



        private decimal CalculateRetentionRate(int numberOfBetPlayer, int numberOfNewPlayer)
        {
            if (numberOfNewPlayer <= 0)
                return 0;

            return (decimal)Math.Round(((decimal)numberOfBetPlayer / (decimal)numberOfNewPlayer) * 100, 2);
        }


        private void SetPlayerWinRateReportExtraProperty(PlayerGameSummaryReportFilterByVendorConfigViewModel viewModel)
        {
            if (viewModel.PagingItems.Items.IsEmptyOrNull())
                return;

            var items = viewModel.PagingItems.Items;

            var gameNameDic = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(items.Select(x => x.GameID));

            var queryMerchants = DbValueCacheHelper.GetMerchants(items.Select(x => x.MerchantID));

            foreach (var item in viewModel.PagingItems.Items)
            {
                item.GameName = gameNameDic.GetDictionaryOrDefaultValue(item.GameID);

                if (item.GameID == 0)
                {
                    item.GameName = "All";
                }

                item.MerchantName = queryMerchants.FirstOrDefault(x => x.MerchantID == item.MerchantID)?.Name;
            }
        }


    }
}
