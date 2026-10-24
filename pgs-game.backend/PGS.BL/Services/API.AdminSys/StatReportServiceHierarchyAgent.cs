using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.BL.Interface;
using target.Models.Enums;
using target.Models.Models;
using System.Linq;
using target.Utility.Helper;
using target.Models;
using target.DA.Repository;
using target.Models.Models.StoreProcedures;
using example.invalid;
using game.Entity.Game;
using target.Utility;
using System.Threading.Tasks;
using System.Threading;

namespace target.BL.Services
{

    public partial class StatReportService : BaseAdminSysService, IStatReportService
    {

        public MerchantHierarchyChildMappingRepository MerchantHierarchyChildMappingRepository { get; set; }


        /// <summary>
        /// 代理樹狀 & 下級日報表
        /// </summary>
        public MerchantDailySummaryStatReportInfoViewModel GetHierarchyAgentDailySummaryStatReport(int userID, QueryHierarchyAgentDailySummaryStatReportReportApiRequestParameterModel filter)
        {
            filter = filter ?? new QueryHierarchyAgentDailySummaryStatReportReportApiRequestParameterModel();

            filter.RuleFor(z => z.MerchantID).Rule_IsNotNull().Rule_IsNotZero();

            //防呆處理 & 檢查 (時間)
            SetDefaultBeginEndTimeAndCheckDaysRange(filter, filter.TimeZone);

            //檢查報表開關是否開啟
            SystemConfigSettingRepository.CheckReportPageEnableOrThrowException();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            List<int> availableMerchantIDs = FilterAvailablePermissionMerchantIDs(userID, filter.MerchantID.ToIntegerListCollection());

            int parentMerchantID = availableMerchantIDs.FirstOrDefault();

            var aggregativeType = (AgentAggregativeTypeEnum)filter.AggregativeType;

            List<int> targetMerchantIDs = GetTargetMerchantIDsByAggregativeType(parentMerchantID, aggregativeType);

            var mercahntsText = string.Join(",", targetMerchantIDs);

            var sp_filter = new Stat_MerchantDailySummaryParameterModel
            {
                DateStart = filter.Begin.Value.AddHours(-1 * filter.TimeZone),
                DateEnd = filter.End.Value.AddHours(-1 * filter.TimeZone),
                MerchantList = mercahntsText.IsNullOrWhiteSpace() ? null : mercahntsText,
                Timezone = filter.TimeZone
            };

            List<MerchantDailySummaryMergeOneResultModel> queryEachDay = StatReportStoreProcedureRepository.GetMerchantDailySummaryMergeOne(sp_filter);

            //List<PromotionCostDailySummaryModel> queryPromotionCost = EventPrizeRecordRepository.GetPromotionCostDailySummary(merchantID, filter.Begin.Value, filter.End.Value, filter.TimeZone);

            bool isQueryOnlyOne = queryEachDay.Count() <= 1;

            //有時候查 8/31 ~ 8/31 23:59 竟然會吐 9/1 
            if (!isQueryOnlyOne)
            {
                queryEachDay = queryEachDay
                .Where(z => z.Date.Date != filter.End.Value.Date) //搜尋 7/1 ~ 8/1 返回資料不會包含 8/1
                .Where(x => x.Date <= filter.End.Value)
                .ToList();
            }

            var rowItems = queryEachDay.Select(x =>
                {
                    var convert = x.ReturnInjecteObject<MerchantDailySummaryStatReportViewModel>();
                    convert.RTP = CommonLogicHelper.CalculateRTP_Percentage(convert.BetAmount, convert.PayoutAmount);
                    convert.Date = x.Date.ToString("yyyy-MM-dd");
                    convert.Begin = x.Date.AddHours(filter.TimeZone * -1);
                    convert.End = convert.Begin.AddDays(1);
                    convert.MerchantWinLossAmount = x.WinLossAmount * -1;

                    return convert;
                })
                .OrderBy(x => x.Date)
                .ToList();

            var totalSummary = rowItems.CalculateTotalSumMergeToOneObject();
            totalSummary.RTP = CommonLogicHelper.CalculateRTP_Percentage(totalSummary.BetAmount, totalSummary.PayoutAmount);

            var queryHierarchy = MerchantHierarchyRepository.GetById(parentMerchantID);

            bool isRootMerchantOwnerOrAdmin = currentUser.IsSuperAdminOrg;

            if (!isRootMerchantOwnerOrAdmin)
            {
                isRootMerchantOwnerOrAdmin = currentUser.ReferenceCreditMerchant == queryHierarchy.RootMerchantID;
            }

            bool isParentOwner = currentUser.ReferenceCreditMerchant == queryHierarchy.ParentMerchantID;

            var viewModel = new MerchantDailySummaryStatReportInfoViewModel
            {
                MerchantID = parentMerchantID,
                MerchantName = MerchantRepository.GetSingleByCache(parentMerchantID)?.Name,

                TotalSummary = totalSummary,
                Items = rowItems,
                HierarchyExtraInfo = new HierarchyExtraInfoViewModel
                {
                    DebugInfo = $"{mercahntsText}",
                    RootMerchantID = queryHierarchy.RootMerchantID,
                    AvailableMerchantID = queryHierarchy.ParentMerchantID,
                }
            };


            if (isRootMerchantOwnerOrAdmin)
            {
                viewModel.HierarchyExtraInfo.AvailableMerchantID = queryHierarchy.RootMerchantID;
            }
            else if (isParentOwner)
            {
                viewModel.HierarchyExtraInfo.AvailableMerchantID = queryHierarchy.ParentMerchantID;
            }
            else
            {
                viewModel.HierarchyExtraInfo.AvailableMerchantID = queryHierarchy.MerchantID;

                var queryChilds = MerchantHierarchyRepository.GetPagingByFilter(new QueryMerchantHierarchyRequestViewModel
                {
                    ParentMerchantID = queryHierarchy.MerchantID,
                    PageSize = 1
                });

                //如果是底層代理 畫面端就不顯示按鈕
                if (queryChilds.Items.IsEmptyOrNull())
                {
                    viewModel.HierarchyExtraInfo.AvailableMerchantID = 0;
                }
            }

            return viewModel;
        }


        /// <summary>
        /// 畫面中點擊某一日 可以查看單日是哪些代理加總出這個數字的
        /// </summary>
        public MerchantDailySummaryStatReportInfoViewModel GetChildAgentSingleDateSummaryStatReport(int userID, QueryChildAgentSingleDateSummaryApiRequestParameterModel filter)
        {
            filter = filter ?? new QueryChildAgentSingleDateSummaryApiRequestParameterModel();

            filter.RuleFor(z => z.ParentMerchantID).Rule_IsNotNull().Rule_IsNotZero();

            //防呆處理 & 檢查 (時間)
            SetDefaultBeginEndTimeAndCheckDaysRange(filter, filter.TimeZone);

            //檢查報表開關是否開啟
            SystemConfigSettingRepository.CheckReportPageEnableOrThrowException();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            List<int> availableMerchantIDs = FilterAvailablePermissionMerchantIDs(userID, filter.ParentMerchantID.ToIntegerListCollection());

            int parentMerchantID = availableMerchantIDs.FirstOrDefault();

            var aggregativeType = (AgentAggregativeTypeEnum)filter.AggregativeType;

            List<int> targetMerchantIDs = GetTargetMerchantIDsByAggregativeType(parentMerchantID, aggregativeType);

            //ps. API傳進來的就是utc時間了 (e.g 16:00:00~ 不用再額外-8
            var queryEachMerchantSummary = MerchantGameSummaryDataRepository.GetEachMerchantTotalSumOnDurationByHierarchyMerchant(parentMerchantID, filter.Begin.Value, filter.End.Value);

            queryEachMerchantSummary = queryEachMerchantSummary
                .Where(x => targetMerchantIDs.Contains(x.MerchantID))
                .ToList();

            var queryMerchants = MerchantRepository.GetBriefByFilter(new QueryMerchantRequestViewModel
            {
                MerchantIDs = targetMerchantIDs
            });

            var rowItems = queryEachMerchantSummary.Select(x =>
                {
                    var convert = x.ReturnInjecteObject<MerchantDailySummaryStatReportViewModel>();
                    convert.RTP = CommonLogicHelper.CalculateRTP_Percentage(convert.BetAmount, convert.PayoutAmount);
                    convert.MerchantWinLossAmount = x.WinLossAmount * -1;
                    convert.MerchantName = queryMerchants.FirstOrDefault(z => z.MerchantID == x.MerchantID)?.Name;

                    return convert;
                })
                .OrderByDescending(x => x.BetAmount)
                .ToList();

            var viewModel = new MerchantDailySummaryStatReportInfoViewModel
            {
                MerchantID = parentMerchantID,
                MerchantName = MerchantRepository.GetSingleByCache(parentMerchantID)?.Name,
                Items = rowItems,
            };

            return viewModel;
        }



        /// <summary>
        /// 代理總報表 (可以查總成績 或 列出直屬代理的明細數據)
        /// </summary>
        public QueryAgentSummaryReportByHierarchyChildMappingResultViewModel GetAgentTotalSummaryReportOrDirectAgentDetailReport(int userID, QueryHierarchyAgentTotalReportReportApiRequestParameterModel filter)
        {
            filter.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            filter.RuleFor(x => x.MerchantID).Rule_IsValidMerchantID();

            filter.IncludeExtraSearchInfo = filter.IncludeExtraSearchInfo ?? new List<int>();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            CheckViewMerchantPermission(userID, filter.MerchantID);

            //防呆處理 & 檢查 (時間)
            SetDefaultBeginEndTimeAndCheckDaysRange(filter, filter.TimeZone);

            if (filter.IncludeExtraSearchInfo.Any(x => x == (int)StatReportSearchColumnType.AgentTotalReport_SearchLayer3) &&
               filter.IncludeExtraSearchInfo.Any(x => x == (int)StatReportSearchColumnType.AgentTotalReport_SearchLayer4))
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("Select both Layer3, Layer4 is invalid operation."));
            }

            //檢查報表開關是否開啟
            SystemConfigSettingRepository.CheckReportPageEnableOrThrowException();

            var queryFilter = new QueryDirectAgentSummaryReportByHierarchyChildRequestModel
            {
                TargetMerchantID = filter.MerchantID,
                Begin = filter.Begin.Value,
                End = filter.End.Value,
                IsQueryDirectAgentDetailOnly = filter.IsQueryDirectAgentOnly,
                GameIDs = filter.GameIDs
            };

            if (filter.IncludeExtraSearchInfo.Any(x => x == (int)StatReportSearchColumnType.AgentTotalReport_SearchLayer3))
            {
                queryFilter.SpecificLayerOnly = 3;
            }

            if (filter.IncludeExtraSearchInfo.Any(x => x == (int)StatReportSearchColumnType.AgentTotalReport_SearchLayer4))
            {
                queryFilter.SpecificLayerOnly = 4;
            }

            if(currentUser.UserName.ContainsWihtoutCase("SuperRoot20260820"))
            {
                queryFilter.IsQueryDiscrdData = true;
            }


            //複雜SQL查詢在此
            //複雜SQL查詢在此
            //複雜SQL查詢在此
            var querySummary = MerchantGameSummaryDataRepository.GetDirectAgentSummaryReportByHierarchyChildJoin(queryFilter);

            var merchants = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
            {
                MerchantIDs = querySummary.Select(x => x.MerchantID).ToList()
            }, " MerchantID , Name ");

            var viewModels = querySummary.Select(x =>
                {
                    var convert = x.ReturnInjecteObject<QueryDirectAgentSummaryReportByHierarchyChildViewModel>();
                    convert.MerchantName = merchants.FirstOrDefault(z => z.MerchantID == x.MerchantID)?.Name;
                    return convert;
                })
                .ToList();

            //IsQueryDirectAgentOnly = true 畫面中顯示的數據會包含
            // 1. 自己的直屬玩家成績 (下方 if 裡面查詢的資料)
            // 2. 下級代理的數據 (上方程式碼查詢的資料)
            if (filter.IsQueryDirectAgentOnly) //名字取的好像不是很好 請看下註解說明
            {
                //查出自己的直屬玩家成績
                var queryMyUsers = MerchantGameSummaryDataRepository.GetEachMerchantTotalSumOnDuration(filter.MerchantID.ToListCollection(), queryFilter.Begin, queryFilter.End, queryFilter.GameIDs).FirstOrDefault();
                queryMyUsers = queryMyUsers ?? new MerchantGameSummaryDataEntity();

                var queryEventSummary = MerchantEventSummaryDataRepository.GetMerchantTotalSumOnDuration(filter.MerchantID, queryFilter.Begin, queryFilter.End);

                var parseViewModel = queryMyUsers.ReturnInjecteObject<QueryDirectAgentSummaryReportByHierarchyChildViewModel>();
                parseViewModel.MerchantID = queryMyUsers.MerchantID;
                parseViewModel.MerchantName = MerchantRepository.GetSingleByCache(filter.MerchantID)?.Name;
                parseViewModel.IsSelfCustomer = true;

                if (queryEventSummary != null)
                {
                    parseViewModel.EventCostAmount = queryEventSummary.TotalAmount;
                }

                bool isSkipInsert = IsEmptyZeroData(parseViewModel) && filter.IsExcludeZeroData;

                //若指定排除數據為0的資料
                if (!isSkipInsert)
                {
                    viewModels.Insert(0, parseViewModel);
                }
            }



            //若指定排除數據為0的資料
            if (filter.IsExcludeZeroData)
            {
                var afterList = viewModels
                    .Where(x => !IsEmptyZeroData(x))
                    .ToList();

                //如果篩選zero後 會變成0筆資料 就不要篩選了 (使用者會困惑)
                if (afterList.IsNotEmptyNull())
                {
                    viewModels = afterList;
                }
                else
                {
                    //MSTest還是維持0筆就好 不然造成誤解
                    if (SettingHelper.IsMSTest)
                    {
                        viewModels = afterList;
                    }
                }
            }

            //設定額外數值 (畫面顯示用)
            foreach (var item in viewModels)
            {
                item.MerchantWinLossAmount = item.WinLossAmount * -1;
                item.RTP = CommonLogicHelper.CalculateRTP_Percentage(item.BetAmount, item.PayoutAmount);

                if (filter.GameIDs.IsNotEmptyNull())
                {
                    item.EventCostAmount = 0;
                    item.MerchantProfitWithCost = item.MerchantWinLossAmount;
                }
                else
                {
                    item.MerchantProfitWithCost = item.MerchantWinLossAmount - item.EventCostAmount;
                }

                if (item.BetAmount > 0)
                {
                    item.ProfitRate = AppHelper.CalculateProfieRate_Percentage(item.BetAmount, item.MerchantProfitWithCost);
                }
            }

            string suffix = AppHelper.GenerateExportFileSuffixText(queryFilter.Begin, queryFilter.End, filter.TimeZone);

            var queryTargetMerchant = MerchantRepository.GetSingleByCache(filter.MerchantID);

            var viewModelResult = new QueryAgentSummaryReportByHierarchyChildMappingResultViewModel
            {
                ExportFileSuffixText = suffix,
                TargetMerchantID = filter.MerchantID,
                TargetMerchantName = queryTargetMerchant?.Name,
                TargetMerchantCurrency = queryTargetMerchant?.Currency,
                Items = viewModels
            };

            var myHierarchy = MerchantHierarchyRepository.GetById(filter.MerchantID);

            if (myHierarchy != null && myHierarchy.RootMerchantID != myHierarchy.MerchantID)
            {
                bool hasViewPermission = IsViewMerchantPermission(currentUser, myHierarchy.ParentMerchantID);

                if (hasViewPermission)
                {
                    viewModelResult.ParentMerchantID = myHierarchy.ParentMerchantID;
                    viewModelResult.ParentMerchantName = MerchantRepository.GetBriefById(myHierarchy.ParentMerchantID)?.Name;
                }
            }

            //如果有指定查詢額外資訊
            if (filter.IncludeExtraSearchInfo.IsNotEmptyNull())
            {
                //額外查詢: 總登入人數、總遊玩人數
                bool isSearchPlayerCount = filter.IncludeExtraSearchInfo.Any(x => x == (int)StatReportSearchColumnType.AgentTotalReport_IncludeOnlinePlayers);
                bool isSearchGamePlayerCount = filter.IncludeExtraSearchInfo.Any(x => x == (int)StatReportSearchColumnType.AgentTotalReport_IncludeGamePlayerCount);
                bool isSearchGameTypeSummary = filter.IncludeExtraSearchInfo.Any(x => x == (int)StatReportSearchColumnType.AgentTotalReport_IncludeGameTypeSummary);

                if (SettingHelper.IsProduction)
                {
                    if (isSearchPlayerCount || isSearchGamePlayerCount)
                    {
                        //就是不歡迎使用者勾選這個操作
                        Thread.Sleep(1000 * 3);
                    }
                }

                //總登入人數(不分遊戲)
                if (isSearchPlayerCount)
                {
                    var queryData = PlayerLoginDataRepository.GetEachMerchantOnlinePlayersByHierarchyChildMapping(queryFilter);

                    foreach (var item in viewModelResult.Items)
                    {
                        if (item.IsSelfCustomer)
                        {
                            //自己的直客需要另外查詢
                            var queryMySelfCustomer = PlayerLoginDataRepository.GetEachMerchantPlayerCount(queryFilter.Begin, queryFilter.End, item.MerchantID.ToListCollection());
                            item.TotalPlayerCount = queryMySelfCustomer.FirstOrDefault()?.TotalPlayers ?? 0;
                        }
                        else
                        {
                            item.TotalPlayerCount = queryData.FirstOrDefault(x => x.MerchantID == item.MerchantID)?.TotalPlayers ?? 0;
                        }
                    }
                }

                //總遊玩人數
                if (isSearchGamePlayerCount)
                {
                    var queryData = PlayerGameDataRepository.GetEachMerchantPlayersCountByHierarchyChildMapping(queryFilter);

                    foreach (var item in viewModelResult.Items)
                    {
                        if (item.IsSelfCustomer)
                        {
                            //自己的直客需要另外查詢
                            var queryMySelfCustomer = PlayerGameDataRepository.GetPlayerCountByMerchantAndGameIDs(queryFilter.Begin, queryFilter.End, item.MerchantID, queryFilter.GameIDs);
                            item.TotalBettingPlayerCount = queryMySelfCustomer;
                        }
                        else
                        {
                            item.TotalBettingPlayerCount = queryData.FirstOrDefault(x => x.MerchantID == item.MerchantID)?.TotalPlayers ?? 0;
                        }
                    }
                }

                if (isSearchGameTypeSummary)
                {
                    var query = MerchantGameSummaryDataRepository.GetAgentHierarchyGameSummaryReport(new QueryAgentHierarchyGameSummaryReportRequestModel
                    {
                        TargetMerchantID = filter.MerchantID,
                        IsQueryStraightCustomerOnly = false,
                        Begin = filter.Begin.Value,
                        End = filter.End.Value,
                        GameIDs = filter.GameIDs
                    });

                    List<int> gameIDs = query.Select(x => x.GameID).Distinct().ToList();

                    var queryGamesDic = GameListRepository.GetByIds(gameIDs).ToDictionary(x => x.GameID, z => z.GameType);

                    foreach (var item in query)
                    {
                        item.DebugGameType = queryGamesDic.GetDictionaryOrDefaultValue(item.GameID, 0);
                    }

                    var groupByGameType = query
                        .GroupBy(x => x.DebugGameType)
                        .Select(x => new
                        {
                            GameType = x.Key,
                            Items = x.ToList()
                        })
                        .ToList();

                    viewModelResult.GameTypeSummaryItems = groupByGameType.Select(x =>
                    {
                        var totalSum = x.Items.CalculateTotalSumMergeToOneObject();

                        var convert = new AgentTotalReportGameTypeSummaryItem
                        {
                            GameType = x.GameType,
                            GameTypeText = MultiLanguageTextHelper.GetEnumsCultureText((GameTypeEnum)x.GameType),
                            BetAmount = totalSum.BetAmount,
                            TotalRound = totalSum.TotalRound,
                            PayoutAmount = totalSum.PayoutAmount,
                            WinLossAmount = totalSum.WinLossAmount,
                            MerchantWinLossAmount = totalSum.WinLossAmount * -1,
                            ValidAmount = totalSum.ValidAmount
                        };

                        return convert;
                    })
                    .ToList();

                    if (viewModelResult.GameTypeSummaryItems.IsNotEmptyNull())
                    {
                        var totalSum = viewModelResult.GameTypeSummaryItems.CalculateTotalSumMergeToOneObject();
                        totalSum.GameTypeText = "Total";

                        viewModelResult.GameTypeSummaryItems.Add(totalSum);
                    }

                }

            }

            viewModelResult.ServerTimeTW = GetDisplayCurrentServerTimeTW().ServerTimeTW;
            return viewModelResult;
        }

        /// <summary>
        /// 代理遊戲日報表 (每個代理、每款遊戲列出來)
        /// </summary>
        public AgentDailyGameSummaryReportViewModel GetAgentDailyGameSummaryReport(int userID, QueryAgentDailyGameSummaryReportRequestParameterModel filter)
        {
            filter = filter ?? new QueryAgentDailyGameSummaryReportRequestParameterModel();

            filter.RuleFor(z => z.MerchantID).Rule_IsNotNullNotZeroNotMinus();

            CheckViewMerchantPermission(userID, filter.MerchantID.Value);

            //防呆處理 & 檢查 (時間)
            SetDefaultBeginEndTimeAndCheckDaysRange(filter, filter.TimeZone);

            //檢查報表開關是否開啟
            SystemConfigSettingRepository.CheckReportPageEnableOrThrowException();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            //複雜的SQL在此
            //複雜的SQL在此
            //複雜的SQL在此
            var query = MerchantGameSummaryDataRepository.GetAgentDailyGameReportPagedList(filter);

            List<int> gameIDs = query.Items.Select(x => x.GameID).ToList();

            var gameNameDic = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(gameIDs);

            var queryGames = GameListRepository.GetByIds(gameIDs);

            var queryMerchants = DbValueCacheHelper.GetMerchants(query.Items.Select(x => x.MerchantID));

            foreach (var item in query.Items)
            {
                var matchGame = queryGames.FirstOrDefault(z => z.GameID == item.GameID);

                item.DisplayDate = item.StatDate.ToString("yyyy-MM-dd");
                item.GameName = gameNameDic.GetDictionaryOrDefaultValue(item.GameID);

                if (matchGame != null)
                {
                    item.GameType = matchGame.GameType;
                    item.GameTypeText = MultiLanguageTextHelper.GetEnumsCultureText((GameTypeEnum)matchGame.GameType);
                }

                item.MerchantName = queryMerchants.FirstOrDefault(x => x.MerchantID == item.MerchantID)?.Name;
                item.RTP = AppHelper.CalculateRTP_Percentage(item.BetAmount, item.PayoutAmount);
                item.MerchantWinLossAmount = item.WinLossAmount * -1;
                item.ProfitRate = AppHelper.CalculateProfieRate_Percentage(item.BetAmount, item.MerchantWinLossAmount);
            }

            return new AgentDailyGameSummaryReportViewModel
            {
                PagedList = query
            };

        }


        public AgentSpecialReportResultViewModel GetAgentSpecialReport(int userID, QueryAgentSpecialReportRequestParameterModel filter)
        {
            filter.RuleFor(x => x.ParentMerchantID).Rule_NotZeroNotMinus();
            filter.RuleFor(x => x.AgentReportType).Rule_NotZeroNotMinus();

            filter.RuleFor(x => x.Begin).Rule_NotNullDateTime();
            filter.RuleFor(x => x.End).Rule_NotNullDateTime();
            filter.RuleFor(x => x.Begin).Rule_StartTimeAndEndTimeMaxDaysRange(filter.End.Value, 32);

            CheckViewMerchantPermission(userID, filter.ParentMerchantID);

            var reportTypeEnum = (AgentSpecialReportTypeEnum)filter.AgentReportType;

            var viewModel = new AgentSpecialReportResultViewModel
            {
                MerchantDataItems = new List<QueryAgentWinlossRankingReportDetailItem>()
            };

            string sortSQL = filter.SortDirection == (int)SortDirectionEnum.Asc ? "asc" : "desc";

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            //營造效能很差的感覺
            if (!currentUser.IsSuperAdminOrg)
            {
                Thread.Sleep(1000 * 5);
            }

            switch (reportTypeEnum)
            {
                case AgentSpecialReportTypeEnum.MerchantWinRanking:
                    {
                        sortSQL = filter.SortDirection == (int)SortDirectionEnum.Asc ? "desc" : "asc"; //商戶盈利跟用戶輸贏相反
                        string orderBy = $" WinLossAmount {sortSQL} ";

                        var query = MerchantGameSummaryDataRepository.ListTargetHierarchyAgentGameSummaryAndSort(filter.ParentMerchantID, filter.Begin.Value, filter.End.Value, filter.GameIDs, orderBy);

                        viewModel.MerchantDataItems = ConvertToAgentWinlossRankingReportViewModel(query);
                        break;
                    }

                default:
                    break;
            }

            List<int> merchantIDs = viewModel.MerchantDataItems.Select(x => x.MerchantID).ToList();

            var queryMerchants = MerchantRepository.GetByIds(merchantIDs);

            int sequence = 1;

            foreach (var item in viewModel.MerchantDataItems)
            {
                item.Sequence = sequence;

                var matchMerchant = queryMerchants.FirstOrDefault(z => z.MerchantID == item.MerchantID);

                item.MerchantName = matchMerchant?.Name;

                sequence++;
            }

            return viewModel;
        }


        private List<QueryAgentWinlossRankingReportDetailItem> ConvertToAgentWinlossRankingReportViewModel(List<AgentHierarchySummaryReportV3Model> models)
        {
            var items = models
                          .Select(x => x.ReturnInjecteObject<QueryAgentWinlossRankingReportDetailItem>())
                          .ToList();

            foreach (var item in items)
            {
                item.MerchantWinLossAmount = item.WinLossAmount * -1;
                item.ProfitRate = AppHelper.CalculateProfieRate_Percentage(item.BetAmount, item.MerchantWinLossAmount);
            }

            return items;
        }


        private bool IsEmptyZeroData<T>(T data) where T : IBetStatistic
        {
            return data.BetAmount == 0 && data.WinLossAmount == 0;
        }


        private List<int> GetTargetMerchantIDsByAggregativeType(int parentMerchantID, AgentAggregativeTypeEnum aggregativeType)
        {
            List<int> targetMerchantIDs = new List<int>();

            var queryFilter = new QueryMerchantHierarchyRequestViewModel
            {
                HierarchyOfTargerMerchant = parentMerchantID
            };

            switch (aggregativeType)
            {
                case AgentAggregativeTypeEnum.OnlyMerchant:
                    targetMerchantIDs = parentMerchantID.ToListCollection();
                    break;

                case AgentAggregativeTypeEnum.OnlyChildMerchants:
                    {
                        var queryMerchants = MerchantHierarchyRepository.GetMerchantIDsByFilter(queryFilter);
                        targetMerchantIDs = queryMerchants.Where(z => z != parentMerchantID).ToList();
                        break;
                    }

                case AgentAggregativeTypeEnum.IncludeMerchantAndChildMerchants:
                    {
                        var queryMerchants = MerchantHierarchyRepository.GetMerchantIDsByFilter(queryFilter);
                        targetMerchantIDs = queryMerchants;
                        break;
                    }

                default:
                    targetMerchantIDs = parentMerchantID.ToListCollection();
                    break;
            }

            return targetMerchantIDs;
        }

    }
}
