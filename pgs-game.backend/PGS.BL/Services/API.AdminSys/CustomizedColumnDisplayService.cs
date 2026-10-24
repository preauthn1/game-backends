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
using System.Linq;
using target.Utility.Helper;
using target.Models;
using target.DA.Repository;
using target.Models.ViewModels.API.AdminSys;
using Microsoft.Extensions.Caching.Memory;
using target.Models.Attributes;
using target.Utility;
using target.Models.Models.StoreProcedures;
using System.Globalization;
using System.Text;
using Lck.Utility.Extensions.Helper;
using MongoDB.Bson;

namespace target.BL.Services
{

    public class CustomizedColumnDisplayService : BaseAdminSysService, ICustomizedColumnDisplayService
    {
        public MerchantRepository MerchantRepository { get; set; }

        public DynamicQueryCommandRepository DynamicQueryCommandRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public PlayerGameDataRepository PlayerGameDataRepository { get; set; }

        public LobbyTokenLogRepository LobbyTokenLogRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public UserOperateRecordRepository UserOperateRecordRepository { get; set; }

        public OrganizationRepository OrganizationRepository { get; set; }

        public ExternalApiRequestLogRepository ExternalApiRequestLogRepository { get; set; }

        public OrganizationCustomizedSettingRepository OrganizationCustomizedSettingRepository { get; set; }

        public GameVendorRTPChangeRecordRepository GameVendorRTPChangeRecordRepository { get; set; }

        public GameVendorAccountingSummaryFetchRepository GameVendorAccountingSummaryFetchRepository { get; set; }

        public StatReportStoreProcedureRepository StatReportStoreProcedureRepository { get; set; }

        public MerchantGameSummaryDataRepository MerchantGameSummaryDataRepository { get; set; }

        public MerchantEventSummaryDataRepository MerchantEventSummaryDataRepository { get; set; }

        public VendorSessionTokenRepository VendorSessionTokenRepository { get; set; }

        public SingleWalletMerchantSessionTokenRepository SingleWalletMerchantSessionTokenRepository { get; set; }

        public BetRecordMerchantNotifyQueueRepository BetRecordMerchantNotifyQueueRepository { get; set; }

        public PermissionRepository PermissionRepository { get; set; }

        public RoleRepository RoleRepository { get; set; }

        public RolePermissionMappingRepository RolePermissionMappingRepository { get; set; }

        public EventRepository EventRepository { get; set; }

        public EventPrizeRecordRepository EventPrizeRecordRepository { get; set; }

        public MerchantGameSettingRepository MerchantGameSettingRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public UserRepository UserRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }

        public GameListCustomizedSettingRepository GameListCustomizedSettingRepository { get; set; }

        public IStatReportService StatReportService { get; set; }


        public CustomizedColumnDisplayService()
        {
            this.StatReportStoreProcedureRepository = new StatReportStoreProcedureRepository();
        }


        public DynamicQueryCommandDropdownItemViewModel GetDynamicDropdownItems(int userID)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var querySqlItem = DynamicQueryCommandRepository.GetAll();

            //非超級管理員 顯示的項目有限
            if (!currentUser.IsSuperAdminOrg)
            {
                int roleID = currentUser.Roles.FirstOrDefault();

                querySqlItem = querySqlItem
                    .Where(z =>
                    {
                        var parseModel = new SystemConfigSettingParseModel(0, z.RestrictRoleIDs);
                        return parseModel.SettingValueOfListInt.Any(z => z == roleID);
                    })
                    .ToList();
            }

            var groupBy = querySqlItem
                .Where(z => z.IsVisible)
                .GroupBy(z => z.DataCateory)
                .Select(z => new DynamicQueryCommandCategoryItemViewModel
                {
                    Category = z.Key,
                    Items = z.Select(z => new DynamicQueryCommandItemViewModel
                    {
                        DisplayName = z.DisplayName,
                        Memo = z.Memo,
                        Id = z.Id
                    })
                    .ToList()
                })
                .ToList();

            return new DynamicQueryCommandDropdownItemViewModel
            {
                EachCategory = groupBy
            };
        }


        public DynamicQueryCommandViewModel GetDynamicQueryCommandSetting(int querySqlID)
        {
            var querySqlItem = DynamicQueryCommandRepository.GetById(querySqlID);

            if (querySqlItem == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            var viewModel = querySqlItem.ReturnInjecteObject<DynamicQueryCommandViewModel>();

            if (!string.IsNullOrWhiteSpace(querySqlItem.FilterFormItems))
            {
                viewModel.AvailableFilterFormItems = querySqlItem.FilterFormItems.Split(',').ToList();
            }

            var extraSetting = ParseToExtraSettingModel(querySqlItem.ExtraSettingJson);
            viewModel.ExtraSetting = extraSetting;

            return viewModel;
        }



        /// <summary>
        /// 為了結果麻煩 這種類型的懶人查詢 不提供分頁
        /// 請控制查詢結果在 500筆以內
        /// </summary>
        public CustomizedColumnDynamicQueryResultViewModel QueryDynamicResult(int userID, CustomizedColumnDisplayRequestModel filter)
        {
            if (filter.BeginTime.HasValue && filter.EndTime.HasValue)
            {
                if ((filter.EndTime.Value - filter.BeginTime.Value).TotalDays >= 62)
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter_DateTimeRangeTooLarge);
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.PlayerName))
            {
                var queryPlayerIDs = PlayerRepository.GetPlayerIDsByPlayerName(filter.PlayerName);

                if (queryPlayerIDs.IsEmptyOrNull())
                {
                    return new CustomizedColumnDynamicQueryResultViewModel
                    {
                        Items = new List<Dictionary<string, object>>(),
                        ExtraObject = new CustomizedColumnExtraInfoViewModel
                        {
                            Columns = new List<CustomizedColumnDisplayColumnViewModel>()
                        }
                    };
                }

                if (queryPlayerIDs.Count() > 1)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_MultiplePlayerNameMatch_PleaseUsePlayerID);
                }

                filter.PlayerID = queryPlayerIDs.FirstOrDefault();
            }


            bool isQueryByCode = filter.QuerySqlID >= 900000;

            DynamicQueryCommandEntity querySqlItem = DynamicQueryCommandRepository.GetById(filter.QuerySqlID);

            // 900XXX 系列可不檢查exception
            if (querySqlItem == null && !isQueryByCode)
                throw new TerproException(ApiErrorCode.NotFoundData);

            if (querySqlItem != null)
            {
                //檢查必填欄位
                CheckRequiredColumns(querySqlItem, filter);
            }

            List<Dictionary<string, object>> queryResult = new List<Dictionary<string, object>>();

            filter.CurrentUserID = userID;

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            //如果不是超級組織 只允許查詢特定幾種
            if (!currentUser.IsSuperAdminOrg)
            {
                CheckWatchDataPermission(userID, filter, querySqlItem);
            }

            if (filter.SortDirection.HasValue)
            {
                var sortDirection = filter.SortDirection.Value == (int)SortDirectionEnum.Asc ? "asc" : "desc";
                querySqlItem.Sql = querySqlItem.Sql.Replace("@SortDirection", sortDirection);
            }

            if (!string.IsNullOrWhiteSpace(filter.SortByColumn))
            {
                querySqlItem.Sql = querySqlItem.Sql.Replace("@SortByColumn", filter.SortByColumn);
            }

            bool isSkipFormatColumns = false;

            //可透過這個json指定額外設定 (e.g. 略過excel
            CustomizedColumnDynamicQueryExtraSettingJson extraSetting = ParseToExtraSettingModel(querySqlItem?.ExtraSettingJson);

            var viewModel = new CustomizedColumnDynamicQueryResultViewModel
            {
                ExtraDisplayName = "",
                Items = new List<Dictionary<string, object>>(),
                ExtraObject = new CustomizedColumnExtraInfoViewModel
                {
                    Columns = new List<CustomizedColumnDisplayColumnViewModel>(),
                    CopyExcel = "",
                }
            };

            if (isQueryByCode)
            {
                // 900000系列的實作在這裡
                // 900000系列的實作在這裡
                // 900000系列的實作在這裡
                // 900000系列的實作在這裡
                // 900000系列的實作在這裡
                var methodResult = QueryByExistServiceOrMethod(userID, filter, extraSetting);

                queryResult = methodResult.ListResult;
                viewModel.ExtraDisplayName = ((CustomerColumnSpecialQueryEnum)filter.QuerySqlID).GetDescription();
                viewModel.ExtraDisplayType = filter.QuerySqlID;
                viewModel.ExtraInfo = methodResult;
                viewModel.ExtraInfo.ListResult = null;

                isSkipFormatColumns = true; //format欄位的行為。會造成速度被拖慢
            }
            else
            {
                queryResult = DynamicQueryCommandRepository.QueryByTargetSql(querySqlItem.Sql, filter, querySqlItem.Type);
            }

            //自動查詢特定欄位
            SetExtraProperty(queryResult);


            if (extraSetting.IsGenerateTotalSum)
            {
                var totalSum = queryResult.CalculateTotalSumMergeToOneDictonaryFormat();
                TryCalculateRtpValue(totalSum);
                queryResult.Add(totalSum);
            }

            if (queryResult.IsEmptyOrNull())
            {
                return viewModel;
            }

            var columns = viewModel.ExtraObject.Columns;

            if (isSkipFormatColumns)
            {

            }
            else
            {
                if (!extraSetting.IsSkipFormatColumns)
                {
                    //將某些型別的欄位 顯示格式調整
                    FormattingColumnValue(queryResult, filter);
                }
            }

            string excelText = string.Empty;

            if (!extraSetting.IsSkipCsvFormat)
            {
                StringBuilder sb = new StringBuilder();

                List<string> myColumns = columns.Select(z => z.ColumnName).ToList();

                foreach (string column in myColumns)
                {
                    sb.Append(column + "\t");
                }

                sb.Append(Environment.NewLine);

                foreach (var item in queryResult)
                {
                    foreach (string column in myColumns)
                    {
                        sb.Append(item[column] + "\t");
                    }
                    sb.Append(Environment.NewLine);
                }

                excelText = sb.ToString();
            }

            viewModel.ExtraObject.Columns = columns ?? new List<CustomizedColumnDisplayColumnViewModel>();
            viewModel.ExtraObject.CopyExcel = excelText;
            viewModel.Items = queryResult;

            //設定columns & 多語系翻譯
            DataModelConvertHelper.ConvertToAllColumnsViewModel(viewModel);

            return viewModel;
        }


        private void CheckWatchDataPermission(int userID, CustomizedColumnDisplayRequestModel filter, DynamicQueryCommandEntity sqlEntity)
        {
            if (filter.MerchantID.HasValue)
            {
                CheckViewMerchantPermission(userID, filter.MerchantID.Value);
            }

            if (filter.PlayerID.HasValue)
            {
                CheckViewPlayerPermission(userID, filter.PlayerID.Value);
            }

            var enumParse = (CustomerColumnSpecialQueryEnum)filter.QuerySqlID;

            if (!enumParse.IsDefinedValue())
            {
                var availableRoleIDs = sqlEntity.RestrictRoleIDs.SeperateToListInt();

                //角色有匹配就允許查詢
                if (availableRoleIDs.IsNotEmptyNull())
                {
                    var myRoleID = GetCurrentOwnPermissionByCache(userID);

                    var isAnyMatch = myRoleID.Roles.Any(myID => availableRoleIDs.Contains(myID));

                    if (!isAnyMatch)
                    {
                        throw new TerproException(ApiErrorCode.Forbidden_WatchDataPermissionNotAllow);
                    }
                }
                else
                {
                    throw new TerproException(ApiErrorCode.Forbidden_WatchDataPermissionNotAllow);
                }
            }
            else
            {
                var isAttrDefine = enumParse.IsAttributeExist<EnableForCommonUser>();

                if (!isAttrDefine)
                {
                    throw new TerproException(ApiErrorCode.Forbidden_WatchDataPermissionNotAllow);
                }
            }
        }

        public virtual CustomizeQueryMethodResultModel QueryByExistServiceOrMethod(int userID, CustomizedColumnDisplayRequestModel filter, CustomizedColumnDynamicQueryExtraSettingJson settingJsong)
        {
            //前面的code有檢查非法參數了 請參考 CheckWatchDataPermission
            //前面的code有檢查非法參數了 請參考 CheckWatchDataPermission
            //前面的code有檢查非法參數了 請參考 CheckWatchDataPermission

            CustomizeQueryMethodResultModel result = null;
            var parseEnum = (CustomerColumnSpecialQueryEnum)filter.QuerySqlID;
            switch (parseEnum)
            {
                case CustomerColumnSpecialQueryEnum.GetGameVendorBetSummary:
                    {
                        result = GetGameVendorBetSummary(userID, filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.QueryRTPChangeLog:
                    {
                        result = QueryRTPChangeLog(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.GameVendorDailyReport_EachMerchant:
                    {
                        result = GameVendorDailyReport_EachMerchant(filter, settingJsong);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.GameVendorDailyReport_EachHierarchyMerchant:
                    {
                        result = GameVendorDailyReport_EachHierarchyMerchant(filter, settingJsong);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.FishGameMoneyChangeLog:
                    {
                        result = FishGameMoneyChangeLog(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.EventDailySummary:
                    {
                        result = EventDailySummary(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.EventDailySummaryByVendorConfigGroup:
                    {
                        result = EventDailySummaryByVendorConfigGroup(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.MerchantEnableGames:
                    {
                        result = MerchantEnableGames(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.QuerySingleWalletNotifyQueueByTranNo:
                    {
                        result = QuerySingleWalletNotifyQueueByTranNo(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.QueryElasticDataByUniqueID:
                    {
                        result = QueryElasticDataByUniqueID(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.AgentHierararchtGameReport:
                    {
                        result = AgentHierararchtGameReport(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.AgentHierararchtEventReport:
                    {
                        result = AgentHierararchtEventReport(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.ShowEachRoleEnableThisPermission:
                    {
                        result = ShowEachRoleEnableThisPermission(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.AGGCampaign_FreespinRecords:
                    {
                        result = AGGCampaign_FreespinRecords(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.Player_QuerySingleWalletToken:
                    {
                        result = Player_QuerySingleWalletToken(filter);
                        break;
                    }


                case CustomerColumnSpecialQueryEnum.Player_QueryExternalAPILog:
                    {
                        result = Player_QueryExternalAPILog(filter);
                        break;
                    }


                case CustomerColumnSpecialQueryEnum.Player_QueryVendorSessionToken:
                    {
                        result = Player_QueryVendorSessionToken(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.Player_QueryLobbyToken:
                    {
                        result = Player_QueryLobbyToken(filter);
                        break;
                    }


                case CustomerColumnSpecialQueryEnum.QueryRoleUserAccounts:
                    {
                        result = QueryRoleUserAccounts(filter);
                        break;
                    }


                case CustomerColumnSpecialQueryEnum.Query_Roles_AssignThisPermission:
                    {
                        result = Query_Roles_AssignThisPermission(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.Query_Record_OperateLogs:
                    {
                        result = Query_Record_OperateLogs(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.Query_Users_AssignThisPermission:
                    {
                        result = Query_Users_AssignThisPermission(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.QueryOrganizationUserAccounts:
                    {
                        result = QueryOrganizationUserAccounts(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.QueryOrganizationCustomizedSetting:
                    {
                        result = QueryOrganizationCustomizedSetting(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.QueryStatistic_GameReport_SpecificGameAllEachPlayer:
                    {
                        result = QueryStatistic_GameReport_SpecificGameAllEachPlayer(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.QueryStatistic_GameServerReport_EachGames:
                    {
                        result = QueryStatistic_GameServerReport_EachGames(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.QueryStatistic_GameServerReport_EachGamesByThirdPartyFetch:
                    {
                        result = QueryStatistic_GameServerReport_EachGamesByThirdPartyFetch(filter);
                        break;
                    }


                case CustomerColumnSpecialQueryEnum.QueryStatistic_GameServerReport_EachMerchant:
                    {
                        result = QueryStatistic_GameServerReport_EachMerchant(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.HomePageReport_EachMerchant_OnlinePlayerCount:
                    {
                        result = HomePageReport_EachMerchant_OnlinePlayerCount(filter);
                        break;
                    }


                case CustomerColumnSpecialQueryEnum.HomePageReport_EachCurrency_OnlinePlayerCount:
                    {
                        result = HomePageReport_EachCurrency_OnlinePlayerCount(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.HomePageReport_EachDay_WagersCount:
                    {
                        result = HomePageReport_EachDay_WagersCount(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.HomePageReport_SingleCurrency_DailyTotalBetAmount:
                    {
                        result = HomePageReport_SingleCurrency_DailyTotalBetAmount(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.HomePageReport_SingleCurrency_DailyTotalRound:
                    {
                        result = HomePageReport_SingleCurrency_DailyTotalRound(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.Merchant_QueryMerchantsByIMChannels:
                    {
                        result = Merchant_QueryMerchantsByIMChannels(filter);
                        break;
                    }

                case CustomerColumnSpecialQueryEnum.HomePageReport_TargetDatePlayerCountAndWagerCount_EachMerchantCount:
                    {
                        result = HomePageReport_TargetDatePlayerCountAndWagerCount_EachMerchantCount(filter);
                        break;
                    }


                default:
                    throw new TerproException(ApiErrorCode.NotImplement);
            }

            if (string.IsNullOrWhiteSpace(result.ExtraMemo))
            {
                result.ExtraMemo = ((CustomerColumnSpecialQueryEnum)filter.QuerySqlID).GetAttributeFromEnum<ExtraAttributeValue>()?.Value;
            }

            return result;
        }

        private CustomizeQueryMethodResultModel AgentHierararchtGameReport(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();
            filter.RuleFor(x => x.MerchantID).Rule_IsNotNullNotZeroNotMinus();
            filter.RuleFor(x => x.BeginTime).Rule_NotNullDateTime();
            filter.RuleFor(x => x.EndTime).Rule_NotNullDateTime();

            DateTime myBegin = filter.BeginTime.Value;
            DateTime myEnd = filter.EndTime.Value;

            var query = MerchantGameSummaryDataRepository.GetAgentHierarchyGameSummaryReport(new QueryAgentHierarchyGameSummaryReportRequestModel
            {
                IsQueryStraightCustomerOnly = filter.IsQueryStraightCustomerOnly,
                TargetMerchantID = filter.MerchantID.Value,
                Begin = myBegin,
                End = myEnd
            });

            var convertList = query
                .OrderBy(x => x.GameID)
                .Select(x => new
                {
                    GameID = x.GameID,
                    GameName = DbMultiLanguageTextHelper.GetGameDisplayName(x.GameID),
                    TotalRound = x.TotalRound,
                    BetAmount = x.BetAmount,
                    PayoutAmount = x.PayoutAmount,
                    WinLossAmount = x.WinLossAmount,
                    RTP = CommonLogicHelper.CalculateRTP_Percentage(x.BetAmount, x.PayoutAmount),
                    MerchantWinLoss = x.WinLossAmount * -1,
                })
               .ToList();

            result.ListResult = ConvertListObjectToListDic(convertList);
            SetTotalSumRowToDic(result.ListResult);
            return result;
        }


        private CustomizeQueryMethodResultModel AgentHierararchtEventReport(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();
            filter.RuleFor(x => x.MerchantID).Rule_IsNotNullNotZeroNotMinus();
            filter.RuleFor(x => x.BeginTime).Rule_NotNullDateTime();
            filter.RuleFor(x => x.EndTime).Rule_NotNullDateTime();

            DateTime myBegin = filter.BeginTime.Value;
            DateTime myEnd = filter.EndTime.Value;

            var queryEventSummary = MerchantEventSummaryDataRepository.GetEachEventSummaryByHierarchyMerchantID(filter.MerchantID.Value, filter.BeginTime.Value, filter.EndTime.Value);

            List<int> eventIDs = queryEventSummary.Select(x => x.EventID).Distinct().ToList();

            var queryEvents = EventRepository.GetByIds(eventIDs);

            var convertList = queryEventSummary
                .OrderBy(x => x.EventID)
                .Select(x =>
                {
                    string displayName = DataModelAndDbValueConvertHelper.GetEventDisplayName(queryEvents.FirstOrDefault(z => z.EventID == x.EventID));

                    var prizeTypes = queryEventSummary.Where(t => t.EventID == x.EventID).Select(x => x.EventPrizeType).Distinct().ToList();

                    //如果同個活動有多個eventPrizeType 額外顯示文字
                    if (prizeTypes.CountOrZero() >= 2)
                    {
                        displayName += $" ({MultiLanguageTextHelper.GetEnumsCultureText((EventPrizeTypeEnum)x.EventPrizeType)})";
                    }

                    var myInstance = new
                    {
                        EventID = x.EventID,
                        EventName = displayName,
                        TotalCount = x.TotalCount,
                        TotalAmount = x.TotalAmount,
                    };

                    return myInstance;
                })
               .ToList();

            result.ListResult = ConvertListObjectToListDic(convertList);
            SetTotalSumRowToDic(result.ListResult);
            return result;
        }


        private CustomizeQueryMethodResultModel ShowEachRoleEnableThisPermission(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();
            filter.RuleFor(x => x.PermissionID).Rule_IsNotNullNotZeroNotMinus();

            var queryRoles = RoleRepository.GetByFilter(new QueryRoleRequestViewModel
            {
                IsIncludeDeleteData = false
            });

            var queryPermissionMapping = RolePermissionMappingRepository.GetByFilter(new QueryRolePermissionMappingRequestViewModel
            {
                PermissionIDs = filter.PermissionID.ToIntegerListCollection()
            });

            var convertList = queryRoles
                .Select(x => new
                {
                    RoleID = x.RoleID,
                    RoleName = x.RoleName,
                    IsEnable = queryPermissionMapping.Any(p => p.RoleID == x.RoleID) ? "v" : ""
                })
               .ToList();

            result.ListResult = ConvertListObjectToListDic(convertList);
            return result;
        }

        private CustomizeQueryMethodResultModel AGGCampaign_FreespinRecords(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();

            filter.RuleFor(x => x.FreespinID).Rule_NotEmptyText();
            filter.RuleFor(x => x.PlayerID).Rule_IsNotNullNotZeroNotMinus();

            var query = EventPrizeRecordRepository.GetByFilter(new QueryEventPrizeRecordRequestViewModel
            {
                FreespinID = filter.FreespinID,
                PlayerIDs = filter.PlayerID.ToIntegerListCollection(),
            });

            var convertList = query
                .OrderByDescending(x=> x.ReceiveTime)
                .Select(x => new
                {
                    FreespinID = filter.FreespinID,
                    ReceiveTime = x.ReceiveTime,
                    PlayerID = x.PlayerID,
                    PlayerName = x.PlayerName,
                    MerchantID = x.MerchantID,
                    TransactionNo = x.PgsTransactionNo,
                    Amount = x.UpdateAmount,
                })
               .ToList();

            result.ListResult = ConvertListObjectToListDic(convertList);
            return result;
        }

        private CustomizeQueryMethodResultModel GetGameVendorBetSummary(int userID, CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();
            var query = StatReportService.GetGameVendorBetSummary(userID, new QueryGameVendorBetSummaryApiRequestParameterModel
            {
                Begin = filter.BeginTime,
                End = filter.EndTime
            });

            List<string> orderColumns = new List<string>
                        {
                            nameof(GameVendorBetSummaryReportItemViewModel.GameVendorType),
                            nameof(GameVendorBetSummaryReportItemViewModel.GameVendorTypeText),
                            nameof(GameVendorBetSummaryReportItemViewModel.ChargeCurrency),
                        };

            result.ListResult = ConvertListObjectToListDic(query.Items, orderColumns.ToArray());
            return result;
        }

        private CustomizeQueryMethodResultModel QueryRTPChangeLog(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();
            filter.RuleFor(x => x.PlayerID).Rule_IsNotNullNotZeroNotMinus();

            var query = GameVendorRTPChangeRecordRepository.GetPagingByFilter(new QueryGameVendorRTPChangeRecordRequestViewModel
            {
                PlayerIDs = filter.PlayerID.ToIntegerListCollection(),
                PageSize = 100
            })
                .Items.Select(x => new
                {
                    CreateTime = x.CreateTime.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss"),
                    GameID = x.GameID,
                    GameName = DbMultiLanguageTextHelper.GetGameDisplayName(x.GameID),
                    Type = x.Type,  //GameVendorRTPChangeTypeEnum
                    TypeText = ((GameVendorRTPChangeTypeEnum)x.Type).ToString(),
                    RTP = x.RTP,
                })
                .ToList();

            result.ListResult = ConvertListObjectToListDic(query);
            return result;
        }

        private CustomizeQueryMethodResultModel QueryStatistic_GameReport_SpecificGameAllEachPlayer(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();

            filter.RuleFor(x => x.MerchantID).Rule_IsNotNullNotZeroNotMinus();
            filter.RuleFor(x => x.GameID).Rule_IsNotNullNotZeroNotMinus();
            filter.RuleFor(x => x.BeginTime).Rule_NotNullDateTime();
            filter.RuleFor(x => x.EndTime).Rule_NotNullDateTime();

            var query = PlayerGameDataRepository.GetGameSummaryGroupByPlayerAndGame(new QueryPlayerGameStatisticParameter
            {
                Begin = filter.BeginTime.Value,
                End = filter.EndTime.Value,
                GameID = filter.GameID,
                MerchantID = filter.MerchantID,
                IsJoinPlayerName = true
            });

            result.ListResult = query
                .OrderByDescending(x => x.BetAmount)
                .Select(x =>
                {
                    var myDic = new Dictionary<string, object>();

                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.PlayerID, x.PlayerID);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.PlayerName, x.PlayerName);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.TotalRound, x.TotalRound);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.BetAmount, x.BetAmount);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.PayoutAmount, x.PayoutAmount);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.WinLossAmount, x.WinLossAmount);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.RTP, CommonLogicHelper.CalculateRTP_Percentage(x.BetAmount, x.PayoutAmount));
                    return myDic;
                })
                .ToList();

            return result;
        }


        private CustomizeQueryMethodResultModel QueryStatistic_GameServerReport_EachGames(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();

            filter.RuleFor(x => x.VendorConfigID).Rule_IsNotNullNotZeroNotMinus();
            filter.RuleFor(x => x.BeginTime).Rule_NotNullDateTime();
            filter.RuleFor(x => x.EndTime).Rule_NotNullDateTime();

            var groupBy = TableColumnEnum.GameID;

            var query = MerchantGameSummaryDataRepository.GetVendorConfigStatReportGroupByTargetCondition(filter.VendorConfigID.Value, filter.BeginTime.Value, filter.EndTime.Value, groupBy);

            List<int> gameIDs = query.Select(x => x.GameID).ToList();

            var gameNameDic = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(gameIDs);

            var queryGames = GameListRepository.GetByIds(gameIDs);

            result.ListResult = query
                .OrderByDescending(x => x.BetAmount)
                .Select(x =>
                {
                    var myDic = new Dictionary<string, object>();

                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.GameID, x.GameID);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.GameName, gameNameDic.GetDictionaryOrDefaultValue(x.GameID));
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.GameCode, queryGames.FirstOrDefault(g => g.GameID == x.GameID)?.ThirdPartyGameCode);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.TotalRound, x.TotalRound);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.BetAmount, x.BetAmount);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.PayoutAmount, x.PayoutAmount);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.WinLossAmount, x.WinLossAmount);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.MerchantWinLoss, x.WinLossAmount * -1);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.RTP, CommonLogicHelper.CalculateRTP_Percentage(x.BetAmount, x.PayoutAmount));
                    return myDic;
                })
                .ToList();

            SetTotalSumRowToDic(result.ListResult);
            result.ExtraMemo = $"{ToTimeText_UTC8(filter.BeginTime.Value)} - {ToTimeText_UTC8(filter.EndTime.Value)}";
            return result;
        }

        private CustomizeQueryMethodResultModel QueryStatistic_GameServerReport_EachGamesByThirdPartyFetch(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();

            filter.RuleFor(x => x.VendorConfigID).Rule_IsNotNullNotZeroNotMinus();
            filter.RuleFor(x => x.BeginTime).Rule_NotNullDateTime();
            filter.RuleFor(x => x.EndTime).Rule_NotNullDateTime();

            var targetDateTW = filter.BeginTime.Value.AddHours(8);

            var queryThird = GameVendorAccountingSummaryFetchRepository.GetEachGameSummary(filter.VendorConfigID.Value, targetDateTW.ToString("yyyy-MM-dd"));

            var queryPGS = MerchantGameSummaryDataRepository.GetVendorConfigStatReportGroupByTargetCondition(filter.VendorConfigID.Value, filter.BeginTime.Value, filter.EndTime.Value, TableColumnEnum.GameID);

            List<int> gameIDs = queryThird.Select(x => x.GameID).ToList();

            var gameNameDic = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(gameIDs);

            result.ListResult = queryThird
                .OrderBy(x =>
                {
                    if (x.GameCode.IsNumeric())
                    {
                        return Int32.Parse(x.GameCode);
                    }
                    else
                    {
                        return 9999999;
                    }
                })
                .ThenBy(x => x.GameCode)
                .Select(x =>
                {
                    var myDic = new Dictionary<string, object>();

                    var matchPGSReport = queryPGS.Where(q => q.GameID == x.GameID).FirstOrDefault();

                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.GameID, x.GameID);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.GameName, gameNameDic.GetDictionaryOrDefaultValue(x.GameID));
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.GameCode, x.GameCode);


                    bool isRoundEqual = x.TotalRound == matchPGSReport?.TotalRound;

                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.TotalRoundText, $"{x.TotalRound} / {matchPGSReport?.TotalRound}{(isRoundEqual ? "" : "⚠️")}");

                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.BetAmount, x.TotalBetAmount);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.WinLossAmount, x.TotalWinLoss);

                    if (matchPGSReport == null)
                    {
                        AddColumnAndValueToDic(myDic, EazyLabelTextEnum.WinLossAmount2, "-");
                        AddColumnAndValueToDic(myDic, EazyLabelTextEnum.DiffAmount, "-");
                    }
                    else
                    {
                        AddColumnAndValueToDic(myDic, EazyLabelTextEnum.WinLossAmount2, matchPGSReport.WinLossAmount);
                        AddColumnAndValueToDic(myDic, EazyLabelTextEnum.DiffAmount, (matchPGSReport.WinLossAmount.ToAbs() - x.TotalWinLoss.ToAbs()).ToAbs());
                    }

                    string url = $"internal/backend-buttons/action" +
                    $"?BackendButtonActionID=7331&vendorConfigID={filter.VendorConfigID}&gameID={matchPGSReport?.GameID}" +
                    $"&begin={filter.BeginTime.Value.ToString("yyyy-MM-dd HH:mm:ss").UrlEncode()}&end={filter.EndTime.Value.ToString("yyyy-MM-dd HH:mm:ss").UrlEncode()}";

                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.FileDonwloadUrl, url);
                    return myDic;
                })
                .ToList();

            SetTotalSumRowToDic(result.ListResult);
            result.ExtraMemo = $"{ToTimeText_UTC8(filter.BeginTime.Value)} - {ToTimeText_UTC8(filter.EndTime.Value)}";
            result.IcontHint1 = $@"
DECLARE @begin datetime = DATEADD(hour , -8 , '{filter.BeginTime.Value.AddHours(8).ToString("yyyy-MM-dd")}')
DECLARE @end   datetime = DATEADD(hour , -8 , '{filter.EndTime.Value.AddHours(8).ToString("yyyy-MM-dd")}')
DECLARE @targetDate date = '{filter.EndTime.Value.AddHours(8).ToString("yyyy-MM-dd")}'
DECLARE @vendorConfgid int = {filter.VendorConfigID}
DECLARE @gameID int = 7777777

SELECT * FROM 
(
	SELECT query.* , fetch_data.WagerID , fetch_data.StatDate FROM 
	(
		SELECT * FROM [PGS_GameLog].[dbo].[PerGameDetailLog] WITH (NOLOCK)
		WHERE GameTime >= DATEADD(hour , -12 , @begin) AND GameTime < DATEADD(hour , 12 , @end) AND ServerID = @vendorConfgid 
	) as query
	LEFT JOIN
	(
		SELECT * FROM [PGS_GameLog].[WagerFetch].[ThirdPartyFetchRecord] WITH (NOLOCK)
		WHERE VendorConfigID = @vendorConfgid 
	) as fetch_data on query.ThirdPartyBetRecordNo = fetch_data.WagerID
	WHERE query.GameID = @gameID AND query.ThirdSettleDate = @targetDate 
) as final_query
WHERE final_query.WagerID is NULL";

            return result;
        }


        private CustomizeQueryMethodResultModel QueryStatistic_GameServerReport_EachMerchant(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();

            filter.RuleFor(x => x.VendorConfigID).Rule_IsNotNullNotZeroNotMinus();
            filter.RuleFor(x => x.BeginTime).Rule_NotNullDateTime();
            filter.RuleFor(x => x.EndTime).Rule_NotNullDateTime();

            var groupBy = TableColumnEnum.MerchantID;
            var query = MerchantGameSummaryDataRepository.GetVendorConfigStatReportGroupByTargetCondition(filter.VendorConfigID.Value, filter.BeginTime.Value, filter.EndTime.Value, groupBy);

            List<int> merchantIDs = query.Select(x => x.MerchantID).ToList();

            var merchants = DbValueCacheHelper.GetMerchants(merchantIDs);

            if(filter.Currency.IsNotEmptyTextNotNull())
            {
                query = query
                    .Where(x =>
                    {
                        var matchMerchant = merchants.FirstOrDefault(m => m.MerchantID == x.MerchantID);
                        return matchMerchant?.Currency == filter.Currency;
                    })
                    .ToList();
            }

            result.ListResult = query
                .OrderByDescending(x => x.BetAmount)
                .Select(x =>
                {
                    var myDic = new Dictionary<string, object>();

                    var matchMerchant = merchants.FirstOrDefault(m => m.MerchantID == x.MerchantID);

                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.MerchantID, x.MerchantID);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.MerchantName, matchMerchant?.Name);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.TotalRound, x.TotalRound);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.BetAmount, x.BetAmount);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.PayoutAmount, x.PayoutAmount);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.WinLossAmount, x.WinLossAmount);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.MerchantWinLoss, x.WinLossAmount * -1);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.RTP, CommonLogicHelper.CalculateRTP_Percentage(x.BetAmount, x.PayoutAmount));
                    return myDic;
                })
                .ToList();

            SetTotalSumRowToDic(result.ListResult);
            result.ExtraMemo = $"{ToTimeText_UTC8(filter.BeginTime.Value)} - {ToTimeText_UTC8(filter.EndTime.Value)}";
            return result;
        }


        private CustomizeQueryMethodResultModel QueryRoleUserAccounts(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();
            filter.RuleFor(x => x.RoleID).Rule_IsNotNullNotZeroNotMinus();

            var queryUsers = UserRepository.GetByFilter(new QueryUserRequestViewModel
            {
                RoleIDs = filter.RoleID.Value.ToListCollection()
            });

            result.ListResult = queryUsers
                .OrderBy(x => x.UserID)
                .Select(x =>
                {
                    var myDic = new Dictionary<string, object>();

                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.Id, x.UserID);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.UserName, x.UserName);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.OrganizationID, x.OrganizationID);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.Status, x.Status);

                    return myDic;
                })
                .ToList();

            return result;
        }



        private CustomizeQueryMethodResultModel Query_Roles_AssignThisPermission(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();
            filter.RuleFor(x => x.PermissionID).Rule_IsNotNullNotZeroNotMinus();

            var queryRoles = RoleRepository.GetByFilter(new QueryRoleRequestViewModel
            {
                AssignPermissionID = filter.PermissionID
            });

            result.ListResult = queryRoles
                .OrderBy(x => x.RoleID)
                .Select(x =>
                {
                    var myDic = new Dictionary<string, object>();

                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.Id, x.RoleID);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.UserName, x.RoleName);

                    return myDic;
                })
                .ToList();

            return result;
        }

        private CustomizeQueryMethodResultModel Query_Record_OperateLogs(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();

            filter.RuleFor(x => x.Type).Rule_IsNotNullNotZeroNotMinus();

            var queryLogs = UserOperateRecordRepository.GetByFilter(new QueryUserOperateRecordRequestViewModel
            {
                OperateTypes = filter.Type.ToIntegerListCollection(),
                TargetKey = filter.TargetKey,
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1 * 90),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime()
            });

            List<int> userIDs = queryLogs.Select(x => x.UserID).ToList();

            var queryUsers = UserRepository.GetByFilter(new QueryUserRequestViewModel
            {
                UserIDs = userIDs
            });

            result.ListResult = queryLogs
                .OrderByDescending(x => x.CreateTime)
                .Select(x =>
                {
                    var myDic = new Dictionary<string, object>();

                    var matchUser = queryUsers.FirstOrDefault(u => u.UserID == x.UserID);

                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.CreateTime, x.CreateTime.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss"));
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.UserName, matchUser?.UserName);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.Type, MultiLanguageTextHelper.GetEnumsCultureText((UserOperateTypeEnum)x.OperateType));
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.Memo, x.Memo);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.Memo2, x.DebugInfo);

                    return myDic;
                })
                .ToList();

            return result;
        }

        private CustomizeQueryMethodResultModel Query_Users_AssignThisPermission(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();
            filter.RuleFor(x => x.PermissionID).Rule_IsNotNullNotZeroNotMinus();

            var queryUsers = UserRepository.GetByFilter(new QueryUserRequestViewModel
            {
                AssignPermissionID = filter.PermissionID
            });

            result.ListResult = queryUsers
                .OrderBy(x => x.UserID)
                .Select(x =>
                {
                    var myDic = new Dictionary<string, object>();

                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.Id, x.UserID);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.UserName, x.UserName);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.OrganizationID, x.OrganizationID);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.Status, x.Status);

                    return myDic;
                })
                .ToList();

            return result;
        }


        private CustomizeQueryMethodResultModel QueryOrganizationUserAccounts(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();
            filter.RuleFor(x => x.OrganizationID).Rule_IsNotNullNotZeroNotMinus();

            var queryUsers = UserRepository.GetByFilter(new QueryUserRequestViewModel
            {
                OrganizationIDs = filter.OrganizationID.Value.ToListCollection()
            });

            result.ListResult = queryUsers
                .OrderBy(x => x.UserID)
                .Select(x =>
                {
                    var myDic = new Dictionary<string, object>();

                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.Id, x.UserID);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.UserName, x.UserName);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.LastLoginTime, x.LastLogonTime.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss"));

                    return myDic;
                })
                .ToList();

            return result;
        }

        private CustomizeQueryMethodResultModel QueryOrganizationCustomizedSetting(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();

            var queryAllSettings = OrganizationCustomizedSettingRepository.GetAll();

            List<int> orgIDs = queryAllSettings.Select(x => x.OrganizationID).Distinct().ToList();

            var queryOrgs = OrganizationRepository.GetByIds(orgIDs);

            result.ListResult = queryAllSettings
                .Select(x =>
                {
                    var myDic = new Dictionary<string, object>();

                    var matchOrg = queryOrgs.FirstOrDefault(o => o.OrganizationID == x.OrganizationID);

                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.OrganizationID, x.OrganizationID);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.OrganizationName, matchOrg?.OrganizationName);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.SettingKey, x.SettingKey);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.DisplayName2, ((OrganizationCustomizedSettingKey)x.SettingKey).ToString());
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.SettingValue, x.SettingValue);

                    return myDic;
                })
                .ToList();

            return result;
        }


        private CustomizeQueryMethodResultModel Player_QuerySingleWalletToken(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();

            filter.RuleFor(x => x.PlayerID).Rule_IsNotNullNotZeroNotMinus();

            var queryPlayerToken = [REDACTED] QuerySingleWalletMerchantSessionTokenRequestViewModel
            {
                PageSize = 20,
                PlayerIDs = filter.PlayerID.ToIntegerListCollection()
            });

            var convert = queryPlayerToken
                .Items
                .Select(x => new
                {
                    CreateTime = x.CreateTime.ToAdminSysDateFormatText(8),
                    PlayerID = x.PlayerID,
                    MerchantID = x.MerchantID,
                    GameID = x.GameID,
                    SessionTokenPGS = x.SessionTokenPGS,
                    SessionTokenRaw = x.SessionTokenRaw,
                })
                .ToList();

            result.ListResult = ConvertListObjectToListDic(convert);
            return result;
        }


        private CustomizeQueryMethodResultModel Player_QueryExternalAPILog(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();

            filter.RuleFor(x => x.PlayerID).Rule_IsNotNullNotZeroNotMinus();

            int hourRange = 8;

            var query = ExternalApiRequestLogRepository.GetByFilter(new QueryExternalApiRequestLogRequestViewModel
            {
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddHours(-1 * hourRange),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime(),
                DebugInfo2 = $"{filter.PlayerID}",
                PageSize = 20,
                OrderByColumn = "CreateTime",
            });

            var convert = query
                .Select(x => new
                {
                    CreateTime = x.CreateTime.ToAdminSysDateFormatText(8),
                    MerchantID = x.SubCategory,
                    Info1 = x.DebugInfo,
                    RequestUrl = x.RequestUrl,
                    RequestContent = x.RequestContent,
                    ResponseText = x.ResponseText,
                    ResponseStatusCode = x.ResponseStatusCode,
                })
                .ToList();

            result.ListResult = ConvertListObjectToListDic(convert);
            result.ExtraMemo = $"近期{hourRange}小時";
            return result;
        }

        private CustomizeQueryMethodResultModel Player_QueryVendorSessionToken(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();

            filter.RuleFor(x => x.PlayerID).Rule_IsNotNullNotZeroNotMinus();

            var querySessionToken = [REDACTED] QueryVendorSessionTokenRequestViewModel
            {
                PageSize = 20,
                PlayerIDs = filter.PlayerID.ToIntegerListCollection()
            });

            var queryPlayer = PlayerRepository.GetById(filter.PlayerID.Value);

            List<int> gameIDs = querySessionToken.Items.Select(x => x.GameID).Distinct().ToList();

            var gameNameDic = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(gameIDs, LanguageEnum.zh_cn);

            var convert = querySessionToken
                .Items
                .Select(x => new
                {
                    Id = x.Id,
                    CreateTime = x.CreateTime.ToAdminSysDateFormatText(8),
                    PlayerID = x.PlayerID,
                    GameID = x.GameID,
                    GameName = gameNameDic.GetDictionaryOrDefaultValue(x.GameID, ""),
                    SessionTokenPGS = x.SessionToken,
                    IPAddress = x.IPAddress,
                    ExtraSettingJson = x.ExtraSettingJson,
                })
                .ToList();

            result.ListResult = ConvertListObjectToListDic(convert);

            string extraText = $"玩家ID：{queryPlayer.PlayerID}" + Environment.NewLine + Environment.NewLine +
                               $"時間｜遊戲名稱｜Token" + Environment.NewLine;


            foreach (var item in querySessionToken.Items.OrderByDescending(x => x.CreateTime).Take(5))
            {
                string dateText = $"{item.CreateTime.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss")}(GMT+8)";
                string gameName = gameNameDic.GetDictionaryOrDefaultValue(item.GameID, "");
                extraText += $"{dateText}\t{item.GameID}｜{gameName}\t{item.SessionToken}" + Environment.NewLine;
            }

            result.ExtraCopyInfo = extraText;
            return result;
        }

        private CustomizeQueryMethodResultModel Player_QueryLobbyToken(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();

            filter.RuleFor(x => x.PlayerID).Rule_IsNotNullNotZeroNotMinus();

            var queryToken = [REDACTED] QueryLobbyTokenLogRequestViewModel
            {
                PageSize = 20,
                PlayerIDs = filter.PlayerID.ToIntegerListCollection()
            });

            double timeZone = 8;
            var convert = queryToken
                .Items
                .Select(x => new
                {
                    Id = x.Id,
                    CreateTime = x.CreateTime.ToAdminSysDateFormatText(timeZone),
                    IdentityGuid = x.IdentityGuid,
                    PlayerID = x.PlayerID,
                    ExpiredTime = x.ExpiredTime.ToAdminSysDateFormatText(timeZone),
                    ExtraSettingJson = x.RawRequestInfo,
                })
                .ToList();

            result.ListResult = ConvertListObjectToListDic(convert);
            return result;
        }


        private CustomizeQueryMethodResultModel MerchantEnableGames(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();
            filter.RuleFor(x => x.MerchantID).Rule_IsNotNullNotZeroNotMinus();

            var queryMapping = MerchantGameSettingRepository.GetByFilter(new QueryMerchantGameSettingRequestViewModel
            {
                MerchantIDs = filter.MerchantID.ToIntegerListCollection(),
                Status = NormalActiveStatus.Enable.ToInteger(),
                EnableGameOnly = true
            });

            List<int> gameIDs = queryMapping.Select(x => x.GameID).ToList();

            var games = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                GameIDs = gameIDs
            });

            var gameNameDic = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(gameIDs);

            result.ListResult = games
                .OrderBy(x => x.GameID)
                .Select(x =>
                {
                    var myDic = new Dictionary<string, object>();

                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.GameID, x.GameID);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.GameName, gameNameDic.GetDictionaryOrDefaultValue(x.GameID));

                    return myDic;
                })
                .ToList();

            return result;
        }


        private CustomizeQueryMethodResultModel QuerySingleWalletNotifyQueueByTranNo(CustomizedColumnDisplayRequestModel filter)
        {
            filter.RuleFor(x => x.TransactionNo).Rule_NotEmptyText();

            var result = new CustomizeQueryMethodResultModel();

            var query = BetRecordMerchantNotifyQueueRepository.GetByFilter(new QueryBetRecordMerchantNotifyQueueRequestViewModel
            {
                BetRecordNoPGS = filter.TransactionNo,
            });

            result.ListResult = query
                .Select(queryEntity =>
                {
                    var myDic = new Dictionary<string, object>();

                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.TransactionNo, queryEntity.BetRecordNoPGS);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.MerchantID, queryEntity.MerchantID);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.PlayerID, queryEntity.PlayerID);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.GameID, queryEntity.GameID);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.NextTime, queryEntity.NextNotifyTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.CreateTime, queryEntity.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.TotalTimes, queryEntity.RetryTimes);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.Status, queryEntity.IsSuccess ? "成功" : "失敗");

                    return myDic;
                })
                .ToList();

            return result;
        }

        private CustomizeQueryMethodResultModel QueryElasticDataByUniqueID(CustomizedColumnDisplayRequestModel filter)
        {
            filter.RuleFor(x => x.TableName).Rule_NotEmptyText();
            filter.RuleFor(x => x.UniqueID).Rule_NotEmptyText();

            var query = ElasticsearchHelper.GetByUniqueID<FullInfoApiRequestLogESModel>(filter.TableName, filter.UniqueID);

            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundData , new ErrorDetailModel("此資料尚未轉移記錄到Elastic"));

            var result = new CustomizeQueryMethodResultModel();

            Dictionary<string, string> keyValues = query.GetType().GetProperties()
                .OrderBy(x=> x.Name)
                .ToDictionary(x => x.Name, x =>
                {
                    var myValue = x.GetValue(query);

                    if (myValue == null)
                        return "";

                    return myValue.ToString();
                });

            result.ListResult = keyValues
                .Select(item =>
                {
                    var myDic = new Dictionary<string, object>();
                    myDic.Add("Column", item.Key);
                    myDic.Add("Value", item.Value);

                    return myDic;
                })
                .ToList();

            return result;
        }


        private CustomizeQueryMethodResultModel EventDailySummary(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();
            filter.RuleFor(x => x.EventID).Rule_IsNotNullNotZeroNotMinus();

            int myEventID = filter.EventID.Value;
            double timeZone = 8.0;

            var queryEvent = EventRepository.GetById(myEventID);

            if (queryEvent == null)
                throw new TerproException(ApiErrorCode.NotFoundEvent);

            result.ExtraMemo = $"{myEventID}｜{queryEvent.DisplayName}";

            var queryPrize = EventPrizeRecordRepository.GetByFilter(new QueryEventPrizeRecordRequestViewModel
            {
                EventIDs = myEventID.ToListCollection()
            });

            result.ListResult = queryPrize
                .Where(x => x.UpdateAmount > 0)
                .GroupBy(x => x.CreateTime.AddHours(timeZone).Date)
                .OrderBy(x => x.Key)
                .Select(x =>
                {
                    var myDic = new Dictionary<string, object>();

                    int totalPrizeCount = x.Count();
                    int notReceiveCount = x.Where(x => x.ReceiveTime == null).Count();
                    decimal notReceiveRate = totalPrizeCount <= 0 ? 0 : 100 * Math.Round(((decimal)notReceiveCount / (decimal)totalPrizeCount), 4);

                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.Date, x.Key.ToString("yyyy-MM-dd"));
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.UniquePlayerCount, x.Select(x => x.PlayerID).Distinct().Count());
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.TotalPrizeCount, totalPrizeCount);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.TotalAmount, ToRound4(x.Select(x => x.UpdateAmount).Sum()));
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.AvgBonusAmount, ToRound4(x.Select(x => x.UpdateAmount).Average()));
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.NotReceiveCount, notReceiveCount);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.NotReceiveRate, notReceiveRate);

                    return myDic;
                })
                .ToList();

            SetTotalSumRowToDic(result.ListResult);
            return result;
        }


        private CustomizeQueryMethodResultModel EventDailySummaryByVendorConfigGroup(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();

            filter.RuleFor(x => x.VendorConfigID).Rule_IsNotNullNotZeroNotMinus();
            filter.RuleFor(x => x.BeginTime).Rule_NotNullDateTime();
            filter.RuleFor(x => x.EndTime).Rule_NotNullDateTime();

            var twDate = DateTimeHelper.GetCurrentTime(8).Date;

            //不允許搜尋當天
            if (filter.EndTime.Value.AddHours(8).Date >= twDate)
            {
                filter.EndTime = DateTimeHelper.GetCurrentTime(8).Date.AddHours(-1 * 8);
            }

            double timeZone = 8.0;

            DateTime myBegin = filter.BeginTime.Value;
            DateTime myEnd = filter.EndTime.Value;

            var queryEventSummary = MerchantEventSummaryDataRepository.GetEachDateEventSummaryByVendorConfigID(filter.VendorConfigID.Value, filter.BeginTime.Value, filter.EndTime.Value);

            List<int> eventIDs = queryEventSummary.Select(x => x.EventID).Distinct().ToList();

            var queryEvents = EventRepository.GetByIds(eventIDs);

            var aggService = new AGGIntegrationService();

            List<object> listOfResult = new List<object>();


            var durationTotal = aggService.FetchBonusSummaryRerport(new Models.ViewModels.GameVendor.AGGReportQueryRequestParameter
            {
                VendorConfigID = filter.VendorConfigID.Value,
                StartTimeUTC = filter.BeginTime.Value,
                EndTimeUTC = filter.EndTime.Value
            });

            if (durationTotal.records.IsNotEmptyNull())
            {
                foreach (var item in durationTotal.records.OrderBy(z => z.count))
                {
                    listOfResult.Add(new
                    {
                        Date = "⭐【AGG】期間加總",
                        EventName = $"獎勵分類: {item.bonusChannel}",
                        TotalCount = item.count,
                        TotalAmount = Math.Round(item.totalWin, 2),
                    });
                }
            }

            //先識別每個活動是否有多重獎項 (顯示名稱會調整)
            var eachEventPrizeCountDic = queryEventSummary
                .GroupBy(x => x.EventID)
                .Select(x =>
                {
                    return new
                    {
                        EventId = x.Key,
                        PrizeTypeCount = x.Select(z => z.EventPrizeType).Distinct().Count()
                    };
                })
                .ToDictionary(z => z.EventId, z => z.PrizeTypeCount);

            //計算PGS這邊 所有日期的加總
            queryEventSummary
                .Where(x => x.EventPrizeType != (int)EventPrizeTypeEnum.AGG_ItemCard)
                .GroupBy(x => x.EventPrizeType)
                .Select(x =>
                {
                    return new
                    {
                        EventPrizeType = x.Key,
                        TotalCount = x.Select(x => x.TotalCount).Sum(),
                        Items = x.ToList()
                    };
                })
                .OrderBy(x => x.TotalCount)
                .ToList()
                .ForEach(x =>
                {
                    var matchFirst = x.Items.FirstOrDefault();

                    string displayName = DataModelAndDbValueConvertHelper.GetEventDisplayName(queryEvents.FirstOrDefault(z => z.EventID == matchFirst.EventID));

                    //如果同個活動有多個eventPrizeType 額外顯示文字
                    if (eachEventPrizeCountDic[matchFirst.EventID] >= 2)
                    {
                        displayName += $" ({MultiLanguageTextHelper.GetEnumsCultureText((EventPrizeTypeEnum)x.EventPrizeType)})";
                    }

                    listOfResult.Add(new
                    {
                        Date = "⭐【target】期間加總",
                        EventName = $"獎勵分類: {displayName}",
                        TotalCount = x.Items.Select(x => x.TotalCount).Sum(),
                        TotalAmount = Math.Round(x.Items.Select(x => x.TotalAmount).Sum(), 2),
                    });
                });

            var convertList = queryEventSummary
                .OrderBy(x => x.Date)
                .ThenBy(x => x.EventID)
                .ThenBy(x => x.EventPrizeType)
                .Select(x =>
                {
                    string displayName = DataModelAndDbValueConvertHelper.GetEventDisplayName(queryEvents.FirstOrDefault(z => z.EventID == x.EventID));

                    var prizeTypes = queryEventSummary.Where(t => t.EventID == x.EventID).Select(x => x.EventPrizeType).Distinct().ToList();

                    //如果同個活動有多個eventPrizeType 額外顯示文字
                    if (prizeTypes.CountOrZero() >= 2)
                    {
                        displayName += $" ({MultiLanguageTextHelper.GetEnumsCultureText((EventPrizeTypeEnum)x.EventPrizeType)})";
                    }

                    var myInstance = new
                    {
                        Date = x.Date.ToString("yyyy-MM-dd"),
                        //EventID = x.EventID,
                        EventName = displayName,
                        TotalCount = x.TotalCount,
                        TotalAmount = Math.Round(x.TotalAmount, 2),
                    };

                    return myInstance;
                })
               .ToList();

            listOfResult.AddRange(convertList);

            result.ListResult = ConvertListObjectToListDic(listOfResult);
            SetTotalSumRowToDic(result.ListResult);
            return result;
        }


        private CustomizeQueryMethodResultModel GameVendorDailyReport_EachMerchant(CustomizedColumnDisplayRequestModel filter, CustomizedColumnDynamicQueryExtraSettingJson settingJson)
        {
            filter.RuleFor(x => x.BeginTime).Rule_NotNullDateTime();
            filter.RuleFor(x => x.EndTime).Rule_NotNullDateTime();

            var queryStatResult = StatReportStoreProcedureRepository.GetGameVendorDailySummaryStatReport(filter.BeginTime.Value, filter.EndTime.Value, filter.MerchantID);

            if (queryStatResult.IsEmptyOrNull())
                return new CustomizeQueryMethodResultModel();

            //設置額外加總數值/商戶信息
            SetExtraColumnValueForGameVendorDailySummary(queryStatResult);

            //900000系列要自己處理format (因為通用型的強制判斷不處理)
            FormattingColumnValue(queryStatResult, filter);

            //在商戶與商戶之間安插分隔線的一列 (比較好閱讀)
            AppendLineRowsForGameVendorDailySummary(filter, queryStatResult, settingJson);

            return new CustomizeQueryMethodResultModel
            {
                ListResult = queryStatResult,
            };
        }

        private CustomizeQueryMethodResultModel GameVendorDailyReport_EachHierarchyMerchant(CustomizedColumnDisplayRequestModel filter, CustomizedColumnDynamicQueryExtraSettingJson settingJson)
        {
            filter.RuleFor(x => x.BeginTime).Rule_NotNullDateTime();
            filter.RuleFor(x => x.EndTime).Rule_NotNullDateTime();

            var queryStatResult = StatReportStoreProcedureRepository.GetGameVendorDailySummaryStatReport_HierarchyMerchants(filter.BeginTime.Value, filter.EndTime.Value, filter.MerchantID);

            if (queryStatResult.IsEmptyOrNull())
                return new CustomizeQueryMethodResultModel();

            //設置額外加總數值/商戶信息
            SetExtraColumnValueForGameVendorDailySummary(queryStatResult);

            //900000系列要自己處理format (因為通用型的強制判斷不處理)
            FormattingColumnValue(queryStatResult, filter);

            //在商戶與商戶之間安插分隔線的一列 (比較好閱讀)
            AppendLineRowsForGameVendorDailySummary(filter, queryStatResult, settingJson);

            return new CustomizeQueryMethodResultModel
            {
                ListResult = queryStatResult,
            };
        }
        private CustomizeQueryMethodResultModel HomePageReport_EachMerchant_OnlinePlayerCount(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();

            DateTime begin = DateTimeHelper.GetCurrentTime().AddMinutes(-1 * SystemConstant.OnlinePlayerQuery_MinutesRange);
            DateTime end = DateTimeHelper.GetCurrentTime();

            int maxSize = 30;
            var query = PerGameDetailLogRepository.GetEachMerchantPlayersCount(begin, end)
                .OrderByDescending(x => x.PlayerCount)
                .Take(30)
                .ToList();

            result.ListResult = query
                .Select(x =>
                {
                    var myDic = new Dictionary<string, object>();

                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.MerchantID, x.MerchantID);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.MerchantName, x.Name);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.Currency, x.Currency);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.UniquePlayerCount, x.PlayerCount);
                    return myDic;
                })
                .ToList();

            result.ExtraMemo = $"最多顯示 {maxSize} 筆";
            return result;
        }


        private CustomizeQueryMethodResultModel HomePageReport_EachCurrency_OnlinePlayerCount(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();

            DateTime begin = DateTimeHelper.GetCurrentTime().AddMinutes(-1 * SystemConstant.OnlinePlayerQuery_MinutesRange);
            DateTime end = DateTimeHelper.GetCurrentTime();

            var query = PerGameDetailLogRepository.GetEachMerchantPlayersCount(begin, end)
                .GroupBy(x => x.Currency)
                .Select(x => new
                {
                    Currency = x.Key,
                    PlayerCount = x.Select(x => x.PlayerCount).Sum()
                })
                .OrderByDescending(z => z.PlayerCount)
                .ToList();

            result.ListResult = query
                .Select(x =>
                {
                    var myDic = new Dictionary<string, object>();

                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.Currency, x.Currency);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.UniquePlayerCount, x.PlayerCount);
                    return myDic;
                })
                .ToList();

            return result;
        }


        private CustomizeQueryMethodResultModel HomePageReport_EachDay_WagersCount(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();

            double timeZone = 8;

            DateTime beginTW = DateTimeHelper.GetCurrentTime(timeZone).To_00_00_00().AddDays(-1 * 30);
            DateTime endTW = DateTimeHelper.GetCurrentTime(timeZone).To_00_00_00();

            DateTime beginUTC = beginTW.AddHours(-1 * timeZone);
            DateTime endUTC = endTW.AddHours(-1 * timeZone);

            var query = MerchantGameSummaryDataRepository.GetDailySumOnDuration(beginUTC, endUTC, timeZone);

            decimal avgRound = Math.Round((decimal)query.Select(x => x.TotalRound).Average(), 0);

            result.ListResult = query
                .OrderByDescending(x => x.StatTime)
                .Select(x =>
                {
                    var myDic = new Dictionary<string, object>();
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.Date, x.StatTime.ToString("yyyy-MM-dd"));
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.TotalRound, x.TotalRound);

                    decimal diff = (x.TotalRound - avgRound) / avgRound;
                    var diffPercentage = Math.Round(diff * 100, 2);

                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.DiffPercentage, diffPercentage.ToString());
                    return myDic;
                })
                .ToList();

            result.ExtraMemo = $"平均值: {avgRound}";
            return result;
        }


        private CustomizeQueryMethodResultModel HomePageReport_SingleCurrency_DailyTotalBetAmount(CustomizedColumnDisplayRequestModel filter)
        {
            filter.RuleFor(x => x.Currency).Rule_NotEmptyText();


            return null;

            //string cacheKey = CacheKey.AdminSys_HomePageReport_EachMerchantSummary(beginUTC, endUTC);

            //var queryEachMerchant = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            //{
            //    var queryDB = MerchantGameSummaryDataRepository.GetEachMerchantDailyReportForFinanceOverview(beginUTC, endUTC);
            //    return queryDB;

            //}, expiredMinute: 24 * 60);

            //List<string> targetCurrency = new List<string>
            //{
            //    "MYR",
            //    "THB",
            //    "IDR",
            //};

            //if (!string.IsNullOrWhiteSpace(filter.Currency))
            //{
            //    targetCurrency = new List<string>
            //    {
            //        filter.Currency
            //    };
            //}

            //List<string> labels = new List<string>();

            //List<DateTime> dateList = beginUTC.AddHours(8).SplitToDateListByHourRange(endUTC.AddHours(8));

            //var myViewModels = new ChartCommonDataStatReportViewModel
            //{
            //    ChartName = "各幣別投注額數據 *已轉換為台幣 (單位: 百萬M)",
            //    Labels = dateList.Select(x => x.ToString("MM-dd")).ToList(),
            //    Groups = targetCurrency,
            //    Datasets = new List<ChartCommonColorDataStatReportItemViewModel>(),
            //};

            //var currencySetting = CurrencyRepository.GetAll();

            //foreach (string currency in targetCurrency)
            //{
            //    var targetCurrencyMerchants = queryEachMerchant.Where(z => z.Currency == currency).ToList();
            //    var chartDataItems = new List<ChartLabelMappingValueViewModel>();

            //    var myCurrencySetting = currencySetting.FirstOrDefault(x => x.Currency == currency);

            //    decimal exchangeRate = myCurrencySetting == null ? 1 : myCurrencySetting.ExchangeRateToTWD;

            //    var groupByDateDic = targetCurrencyMerchants.GroupBy(x => x.Date).ToDictionary(z => z.Key, z => z.ToList());

            //    foreach (var time in dateList)
            //    {
            //        decimal value = 0;

            //        if (groupByDateDic.ContainsKey(time))
            //        {
            //            value = groupByDateDic[time].DefaultIfEmpty().Select(x => x.BetAmount).Sum();
            //        }

            //        string labelName = time.ToString("MM-dd");

            //        chartDataItems.Add(new ChartLabelMappingValueViewModel
            //        {
            //            LabelName = labelName,
            //            Value = Math.Round(value * exchangeRate / (1000 * 1000), 2)
            //        });
            //    }

            //    myViewModels.Datasets.Add(new ChartCommonColorDataStatReportItemViewModel
            //    {
            //        Data = chartDataItems,
            //        DataOfValues = new List<decimal>(),
            //        GroupName = currency
            //    });
            //}

            //var result = new CustomizeQueryMethodResultModel();

            //result.ListResult = query
            //    .Select(x =>
            //    {
            //        var myDic = new Dictionary<string, object>();

            //        AddColumnAndValueToDic(myDic, EazyLabelTextEnum.Currency, x.Currency);
            //        AddColumnAndValueToDic(myDic, EazyLabelTextEnum.UniquePlayerCount, x.PlayerCount);
            //        return myDic;
            //    })
            //    .ToList();

            //return result;
        }


        private CustomizeQueryMethodResultModel HomePageReport_SingleCurrency_DailyTotalRound(CustomizedColumnDisplayRequestModel filter)
        {
            filter.RuleFor(x => x.Currency).Rule_NotEmptyText();
            filter.RuleFor(x => x.BeginDateTW).Rule_NotNullDateTime();

            DateTime beginUTC = filter.BeginDateTW.Value.AddHours(-8);
            DateTime endUTC = filter.BeginDateTW.Value.AddMonths(1).AddHours(-8);

            var now = DateTimeHelper.GetCurrentTime();

            if (endUTC > now)
            {
                endUTC = DateTimeHelper.GetCurrentTime(8).To_00_00_00().AddHours(-8);
            }

            string cacheKey = CacheKey.AdminSys_HomePageReport_EachMerchantTotalRound(beginUTC, endUTC);

            var queryEachMerchant = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var queryDB = MerchantGameSummaryDataRepository.GetEachMerchantDailyReportForFinanceOverview(beginUTC, endUTC);
                return queryDB;

            }, expiredMinute: 24 * 60);

            List<DateTime> dateList = beginUTC.AddHours(8).SplitToDateListByHourRange(endUTC.AddHours(8));

            var result = new CustomizeQueryMethodResultModel();
            result.ListResult = new List<Dictionary<string, object>>();

            var targetCurrencyMerchants = queryEachMerchant.Where(z => z.Currency == filter.Currency).ToList();
            var groupByDateDic = targetCurrencyMerchants.GroupBy(x => x.Date).ToDictionary(z => z.Key, z => z.ToList());

            var eachDateTotalSum = groupByDateDic.Select(x =>
                {
                    return new
                    {
                        Date = x.Key,
                        TotalSum = x.Value.Select(x => x.TotalRound).Sum()
                    };
                })
                .ToList();

            decimal avgRound = (decimal)(eachDateTotalSum.Select(x => x.TotalSum).DefaultIfEmpty().Average());

            foreach (var time in dateList)
            {
                var myDic = new Dictionary<string, object>();

                decimal value = 0;

                if (groupByDateDic.ContainsKey(time))
                {
                    value = groupByDateDic[time].DefaultIfEmpty().Select(x => x.TotalRound).Sum();
                }

                var dayOfWeekText = " (" + GetDayOfWeekTextCN((int)time.DayOfWeek) + ")";

                string labelName = time.ToString("MM-dd");

                if (avgRound == 0)
                {
                    avgRound = 1;
                }

                var diff = (value - avgRound) / avgRound;
                var diffPercentage = Math.Round(diff * 100, 2);

                AddColumnAndValueToDic(myDic, EazyLabelTextEnum.Date, labelName + dayOfWeekText);
                AddColumnAndValueToDic(myDic, EazyLabelTextEnum.TotalRound, value);
                AddColumnAndValueToDic(myDic, EazyLabelTextEnum.DiffPercentage, diffPercentage.ToString());

                result.ListResult.Add(myDic);
            }

            result.ExtraMemo = $"貨幣：{filter.Currency}｜平均值：{(int)avgRound}";
            return result;
        }


        private string GetDayOfWeekTextCN(int dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case 0:
                    return "日";
                case 1:
                    return "一";
                case 2:
                    return "二";
                case 3:
                    return "三";
                case 4:
                    return "四";
                case 5:
                    return "五";
                case 6:
                    return "六";
                default:
                    return "";
            }
        }


        private CustomizeQueryMethodResultModel Merchant_QueryMerchantsByIMChannels(CustomizedColumnDisplayRequestModel filter)
        {
            filter.RuleFor(x => x.IMChannelID).Rule_NotZeroNotMinus();

            var result = new CustomizeQueryMethodResultModel();

            var quetyMerchants = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
            {
                MerchantCustomizedSetting_SettingKey = MerchantCustomizedSettingKeyEnum.Common_IMChannelID.ToInteger(),
                MerchantCustomizedSetting_SettingValue = filter.IMChannelID.ToString(),
            });

            result.ListResult = quetyMerchants
                .Select(x =>
                {
                    var myDic = new Dictionary<string, object>();

                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.MerchantID, x.MerchantID);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.MerchantName, x.Name);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.Currency, x.Currency);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.Type, MultiLanguageTextHelper.GetEnumsCultureText((MerchantTypeEnum)x.Type));
                    return myDic;
                })
                .ToList();

            return result;
        }


        private CustomizeQueryMethodResultModel HomePageReport_TargetDatePlayerCountAndWagerCount_EachMerchantCount(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();

            filter.RuleFor(x => x.TargetDayText).Rule_NotEmptyText();
            filter.RuleFor(x => x.OrderBy).Rule_NotEmptyText();

            string queryDayText = "今日";
            filter.TargetDayText = filter.TargetDayText.ToLower();
            filter.OrderBy = filter.OrderBy.ToLower();

            DateTime begin = DateTimeHelper.GetCurrentTime(8).To_00_00_00().AddHours(-8);

            if (filter.TargetDayText == "yesterday")
            {
                queryDayText = "昨日";
                begin = begin.AddDays(-1);
            }

            DateTime end = begin.AddDays(1);

            int maxSize = 30;
            var query = PlayerGameDataRepository.GetEachMerchantPlayerCountAndTotalRound(begin, end);

            result.ListResult = query
                .OrderByDescending(x =>
                {
                    if (filter.OrderBy == "playercount")
                    {
                        return x.PlayerCount;
                    }
                    else
                    {
                        return x.TotalRound;
                    }
                })
                .Take(maxSize)
                .Select(x =>
                {
                    var myDic = new Dictionary<string, object>();

                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.MerchantID, x.MerchantID);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.MerchantName, x.Name);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.UniquePlayerCount, x.PlayerCount);
                    AddColumnAndValueToDic(myDic, EazyLabelTextEnum.TotalRound, x.TotalRound);
                    return myDic;
                })
                .ToList();

            result.ExtraMemo = $"查詢:{queryDayText}、顯示最多{maxSize}筆";
            return result;
        }


        private CustomizeQueryMethodResultModel FishGameMoneyChangeLog(CustomizedColumnDisplayRequestModel filter)
        {
            var result = new CustomizeQueryMethodResultModel();
            var fishService = new AGGFishIntegrationService();

            filter.RuleFor(x => x.PlayerID).Rule_IsNotNullNotZeroNotMinus();
            filter.RuleFor(x => x.BeginTime).Rule_NotNullDateTime();
            filter.RuleFor(x => x.EndTime).Rule_NotNullDateTime();
            filter.PageSize = filter.PageSize <= 0 ? 50 : filter.PageSize;
            filter.PageIndex = filter.PageIndex <= 0 ? 1 : filter.PageIndex;

            var loop = AppHelper.InstanceLoopTimes(filter.PageSize);

            //呼叫AGGAPI
            int playerID = filter.PlayerID.Value;
            var queryPlayer = PlayerRepository.GetBriefByCache(playerID);

            var vendorConfig = VendorConfigRepository.GetCacheByVendorTypeAndMerchant(queryPlayer.MerchantID, (int)GameVendorTypeEnum.AGGFish);

            DateTime endTime = filter.EndTime.Value;

            var maxTime = DateTimeHelper.GetCurrentTime().AddMinutes(-1 * 15);

            if (endTime >= maxTime)
                endTime = maxTime;

            var apiResult = fishService.FetchMoneyRecord(new AGGFishFetchMoneyRecordRequestParameter
            {
                VendorConfigID = vendorConfig.VendorConfigID,
                PlayerID = playerID,
                PageIndex = filter.PageIndex,
                PageSize = filter.PageSize,
                StartTimeUTC = filter.BeginTime.Value,
                EndTimeUTC = endTime,
                Type = filter.Type
            });

            List<decimal> levelUpAmounts = new List<decimal>();

            var queryFishMapping = GameListCustomizedSettingRepository.GetByFilter(new QueryGameListCustomizedSettingRequestViewModel
            {
                SettingKey = GameListCustomizedSettingKeyEnum.AGGFish_FishIdMapping.ToInteger()
            });

            //如果轉入成功會有referenceNo

            if (apiResult.result.IsNotEmptyNull())
            {
                foreach (var item in apiResult.result)
                {
                    decimal roomEnterAmount = 0;

                    if (item.BeforeAmount.HasValue && item.AfterAmount.HasValue)
                    {
                        roomEnterAmount = (item.BeforeAmount.Value - item.AfterAmount.Value).ToAbs();
                    }

                    if (item.Type == AGGFishMoneyRecordTypeEnum.Exchange_ToFishWallet_Fail.ToInteger())
                    {
                        item.WarningText = "*Warning";
                    }
                    else if (item.Type == AGGFishMoneyRecordTypeEnum.Exchange_ToFishWallet.ToInteger() &&
                            item.Type == AGGFishMoneyRecordTypeEnum.Exchange_ToPlatformWallet.ToInteger())
                    {
                        if (string.IsNullOrWhiteSpace(item.ReferenceNo))
                        {
                            item.WarningText = "*Warning";
                        }
                    }

                    if (item.Type == AGGFishMoneyRecordTypeEnum.LevelUpBonus.ToInteger() && item.UpdateAmount.HasValue)
                    {
                        item.TypeDesc = "升级奖励";
                        levelUpAmounts.Add(item.UpdateAmount.Value);
                    }

                    if (item.Type == AGGFishMoneyRecordTypeEnum.LoginBonus.ToInteger() && item.UpdateAmount.HasValue)
                    {
                        item.TypeDesc = "登入奖励";
                    }

                    if (item.Type == AGGFishMoneyRecordTypeEnum.DepositBalanceToFishRoom.ToInteger())
                    {
                        item.TypeDesc = "进入捕鱼房";
                        item.TempOriginalAmount = item.UpdateAmount;
                        item.UpdateAmount = -1;
                        item.BeforeAmount = roomEnterAmount;
                        item.AfterAmount = roomEnterAmount;
                    }

                    if (item.Type == AGGFishMoneyRecordTypeEnum.WithdrawalBalanceFromFishRoom.ToInteger())
                    {
                        item.TypeDesc = "退出捕鱼房";
                        item.UpdateAmount = -1;
                        item.BeforeAmount = roomEnterAmount;
                        item.AfterAmount = roomEnterAmount;
                    }

                    if (item.UpdateAmount != 0 &&
                        item.BeforeAmount.IsNullOrIsZero() &&
                        item.AfterAmount.IsNullOrIsZero() &&
                        item.Type != AGGFishMoneyRecordTypeEnum.WithdrawalBalanceFromFishRoom.ToInteger())
                    {
                        item.BeforeAmount = -1;
                        item.AfterAmount = -1;
                    }

                    if (item.Type == AGGFishMoneyRecordTypeEnum.Bullet.ToInteger() &&
                       item.Memo.IsNotEmptyTextNotNull() && item.Memo.IsNumeric())
                    {
                        var matchFish = queryFishMapping.FirstOrDefault(x => x.Category1 == item.Memo);
                        item.Memo = matchFish?.SettingValue;
                    }
                }
            }

            apiResult.result = apiResult.result ?? new List<AGGFishMoneyRecordDetailItemModel>();

            //ps. where條件會排除掉
            var listResult = apiResult.result?
                .Where(x => !(x.Type == AGGFishMoneyRecordTypeEnum.DepositBalanceToFishRoom.ToInteger() && x.TempOriginalAmount.HasValue && levelUpAmounts.Contains(x.TempOriginalAmount.Value)))
                .Select(x =>
                {
                    string referenceNo = x.ReferenceNo;

                    if (string.IsNullOrWhiteSpace(referenceNo))
                    {
                        referenceNo = x.Txtid;
                    }

                    List<string> tags = new List<string>();

                    if (!string.IsNullOrWhiteSpace(x.WarningText))
                    {
                        tags.Add(x.WarningText);
                    }

                    var myObject = new
                    {
                        CreateTime = DateTimeHelper.ConvertUnknowFormatTextToUtcDateTime(x.CreateTime).ToAdminSysDateFormatText(filter.TimeZone),
                        PlayerID = x.Username,
                        PlayerName = queryPlayer.PlayerName,
                        Type = x.TypeDesc,
                        Amount = x.UpdateAmount == -1 ? "-" : x.UpdateAmount.ToString(),
                        BeforeAmount = x.BeforeAmount == -1 ? "-" : x.BeforeAmount.ToString(),
                        AfterAmount = x.AfterAmount == -1 ? "-" : x.AfterAmount.ToString(),
                        ReferenceOrderNo = referenceNo,
                        Memo = x.Memo,
                        Tags = tags
                    };

                    return myObject;
                })
                .ToList();

            result.ExtraMemo = $"{vendorConfig.Name} (*数据有15分钟延迟)";
            result.ListResult = ConvertListObjectToListDic(listResult);
            return result;
        }


        /// <summary>
        /// 自動產生totalSum(加總列) 透過Dic裡面的欄位名稱來判斷是否需要加總
        /// </summary>
        /// <param name="rowValues"></param>
        private void SetTotalSumRowToDic(List<Dictionary<string, object>> rowValues)
        {
            if (rowValues.IsEmptyOrNull())
                return;

            if (rowValues.Count() <= 0)
                return;

            var matchFirst = rowValues.FirstOrDefault();

            try
            {
                var totalSumObject = new Dictionary<string, object>();

                var firstRowColumn = matchFirst.Keys.FirstOrDefault();

                foreach (var eachColumn in matchFirst)
                {
                    var columnName = eachColumn.Key.ToLower();
                    var columnValue = eachColumn.Value;

                    if (columnName == firstRowColumn.ToLower())
                    {
                        totalSumObject.Add(eachColumn.Key, "Total");
                    }
                    else if (columnName.Contains("avg") || columnName.Contains("average") || columnName.Contains("rate") || columnName.Contains("rtp"))
                    {
                        var totalSumValue = rowValues.Select(x => decimal.Parse(x[eachColumn.Key].ToString())).ToList().Average();
                        totalSumObject.Add(eachColumn.Key, Math.Round(totalSumValue, 2));
                    }
                    else if (columnName.Contains("count") || columnName.Contains("round"))
                    {
                        var totalSumValue = rowValues.Select(x => Int32.Parse(x[eachColumn.Key].ToString())).ToList().Sum();
                        totalSumObject.Add(eachColumn.Key, totalSumValue);
                    }
                    else if (columnName.Contains("amount") || columnName.Contains("winloss"))
                    {
                        decimal totalSumValue = rowValues.Select(x => decimal.Parse(x[eachColumn.Key].ToString())).ToList().Sum();
                        totalSumObject.Add(eachColumn.Key, totalSumValue);
                    }
                    else
                    {
                        totalSumObject.Add(eachColumn.Key, null);
                    }
                }

                rowValues.Add(totalSumObject);
            }
            catch (Exception ex)
            {
                string msg = "";
            }
        }


        private void AddColumnAndValueToDic(Dictionary<string, object> dic, EazyLabelTextEnum eazyLabel, object myValue)
        {
            //一律轉駝峰式小寫
            dic.Add(eazyLabel.ToString().ToCamelCase(), myValue);
        }


        private decimal ToRound4(decimal? amount)
        {
            if (!amount.HasValue)
                return 0;

            return Math.Round(amount.Value, 4);
        }


        private List<Dictionary<string, object>> ConvertListObjectToListDic<T>(List<T> items, params string[] orderColumns) where T : class
        {
            if (items.IsEmptyOrNull())
                return new List<Dictionary<string, object>>();

            List<string> listOfOrderColumns = orderColumns == null ? new List<string>() : orderColumns.ToList();

            List<Dictionary<string, object>> listOfDic = new List<Dictionary<string, object>>();

            Dictionary<string, int> columnSquqenceMapping = new Dictionary<string, int>();

            List<string> properties = items.FirstOrDefault().GetType().GetProperties().Select(z => z.Name).ToList();

            int loopTimes = 0;

            //讓指定的欄位優先排前面
            foreach (string propName in properties)
            {
                loopTimes++;

                if (listOfOrderColumns.Contains(propName))
                {
                    columnSquqenceMapping.Add(propName, listOfOrderColumns.IndexOf(propName));
                }
                else
                {
                    columnSquqenceMapping.Add(propName, 99999 + loopTimes);
                }
            }

            List<string> propertiesWithOrder = columnSquqenceMapping.OrderBy(z => z.Value).Select(z => z.Key).ToList();

            Type type = items.FirstOrDefault().GetType();

            foreach (var item in items)
            {
                var dic = new Dictionary<string, object>();

                foreach (string propName in propertiesWithOrder)
                {
                    dic.Add(propName, type.GetProperty(propName).GetValue(item));
                }

                listOfDic.Add(dic);
            }

            return listOfDic;
        }


        public void FormattingColumnValue(List<Dictionary<string, object>> queryResult, CustomizedColumnDisplayRequestModel filter)
        {
            foreach (var eachRow in queryResult)
            {
                Dictionary<string, object> modifyValues = new Dictionary<string, object>();

                foreach (var eachColumn in eachRow)
                {
                    string keyToLower = eachColumn.Key.ToLower();
                    string valueToString = eachColumn.Value.ToString();

                    bool isAmount = keyToLower.Contains("amount") || keyToLower == "rtp";
                    bool isTime = keyToLower.Contains("time");
                    bool isDate = keyToLower == "date" || keyToLower == "datetext" || keyToLower == "statdate" || keyToLower == "sdate";
                    bool isDiff = keyToLower.Contains("_diff");
                    bool isZeroAmount = valueToString == "0" || valueToString == "0.00" || valueToString == "0.0000";

                    try
                    {
                        //暫存記住哪幾個欄位要被異動
                        if (isDiff)
                        {
                            //無數字差異的話 顯示空白字串
                            decimal numberValue = decimal.Parse(eachColumn.Value.ToString());
                            bool isZero = numberValue == 0;
                            modifyValues.Add(eachColumn.Key, isZero ? "" : eachColumn.Value.ToString());
                        }
                        else if (isZeroAmount)
                        {
                            modifyValues.Add(eachColumn.Key, "0");
                        }
                        else if (isAmount && !filter.IsSkipFormatAmountColumn)
                        {
                            modifyValues.Add(eachColumn.Key, (decimal.Parse(eachColumn.Value.ToString())).ToMoneyFormat());
                        }
                        else if (isTime)
                        {
                            //反正是自己人用 強制轉+8
                            modifyValues.Add(eachColumn.Key, ((DateTime)eachColumn.Value).AddHours(8).ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                        else if (isDate)
                        {
                            modifyValues.Add(eachColumn.Key, ((DateTime)eachColumn.Value).ToString("yyyy-MM-dd"));
                        }
                    }
                    catch (Exception ex)
                    {
                        //如果轉型失敗就不轉了
                        string msg = "";

                        if (SettingHelper.IsMSTest)
                        {
                            throw;
                        }
                    }
                }

                // 將amount欄位轉千分位符號
                foreach (var newValue in modifyValues)
                {
                    eachRow[newValue.Key] = newValue.Value;
                }
            }

            string enumColumn_GameVendorTypeText = "GameVendorTypeText";
            string enumColumn_RTPChangeType = "RTPChangeType";
            string enumColumn_GameNameEN = "GameNameEN";

            //如果有些顯示文字 希望用enum多語系產生出來 (因為沒有table 只能用join的出來)
            foreach (var eachRow in queryResult)
            {
                //先定義出現怎樣的columnName的時候 需要檢查enums多語系
                Dictionary<string, object> modifyValues = new Dictionary<string, object>();

                foreach (var eachColumn in eachRow)
                {
                    var columnName = eachColumn.Key;
                    var columnNameLower = eachColumn.Key.ToLower();

                    if (eachRow.ContainsKey(enumColumn_GameVendorTypeText))
                    {
                        //嘗試從定義的幾種enums 查出
                        if (columnName == "GameVendorType" ||
                           columnName == "GameVendor" ||
                           columnName == "VendorID" ||
                           columnName == "GameVendorID")
                        {
                            int myValue = Int32.Parse(eachColumn.Value.ToString());
                            modifyValues.Add(enumColumn_GameVendorTypeText, MultiLanguageTextHelper.GetGameVendorDisplayName(myValue));
                        }
                    }

                    if (eachRow.ContainsKey(enumColumn_RTPChangeType))
                    {
                        //嘗試從定義的幾種enums 查出
                        if (columnName == "RTPChangeType")
                        {
                            modifyValues.Add(enumColumn_RTPChangeType, MultiLanguageTextHelper.GetEnumsCultureText((GameVendorRTPChangeTypeEnum)eachColumn.Value));
                        }
                    }

                    if (eachRow.ContainsKey(enumColumn_GameNameEN))
                    {
                        //嘗試從定義的幾種enums 查出
                        if (columnNameLower == "gameid")
                        {
                            int gameID = Int32.Parse(eachColumn.Value.ToString());
                            modifyValues.Add(enumColumn_GameNameEN, DbMultiLanguageTextHelper.GetGameDisplayName(gameID, LanguageEnum.en_us));
                        }
                    }
                }

                // 將amount欄位轉千分位符號
                foreach (var newValue in modifyValues)
                {
                    eachRow[newValue.Key] = newValue.Value;
                }
            }

        }

        private void SetExtraColumnValueForGameVendorDailySummary(List<Dictionary<string, object>> queryStatResult)
        {
            if (queryStatResult.IsEmptyOrNull())
                return;

            string keyOfMerchantID = "MerchantID";
            List<int> merchantIDs = queryStatResult.Select(x => x).Select(z => (int)z[keyOfMerchantID]).Distinct().ToList();

            var queryMerchants = MerchantRepository.GetBriefByFilter(new QueryMerchantRequestViewModel
            {
                MerchantIDs = merchantIDs
            });

            foreach (var eachRow in queryStatResult)
            {
                int merchantID = (int)eachRow[keyOfMerchantID];

                var matchMerchant = queryMerchants.FirstOrDefault(x => x.MerchantID == merchantID);

                eachRow["MerchantName"] = matchMerchant?.Name;
                eachRow["Currency"] = matchMerchant?.Currency;

                decimal totalBetAmount = eachRow.Where(x => x.Key.ToLower().Contains("betamount")).Select(z => decimal.Parse(z.Value.ToString())).Sum();
                decimal totalWinLoss = eachRow.Where(x => x.Key.ToLower().Contains("winloss")).Select(z => decimal.Parse(z.Value.ToString())).Sum();
                var uniqueUsers = eachRow.Where(x => x.Key.ToLower().Contains("uniqueusers")).Select(z => (int)z.Value).OrderByDescending(z => z).ToList();

                eachRow["totalBet"] = totalBetAmount;
                eachRow["totalWinLoss"] = totalWinLoss;

                //這欄位很煩 會影響查詢速度。因此用估算的 第一名的遊戲商人數 + 第二名遊戲商的60%
                eachRow["totalPlayers"] = uniqueUsers[0] + ((int)(uniqueUsers[1] * 0.6));
            }
        }


        private void AppendLineRowsForGameVendorDailySummary(CustomizedColumnDisplayRequestModel filter, List<Dictionary<string, object>> queryStatResult, CustomizedColumnDynamicQueryExtraSettingJson settingJson)
        {
            if (queryStatResult.IsEmptyOrNull())
                return;

            //在商戶與商戶之間安插分隔線的一列 (閱讀上比較好)
            if (settingJson != null && settingJson.IsAppendBreakLine && (filter.EndTime.Value - filter.BeginTime.Value).TotalDays > 1) //如果只有1筆還需要分隔線嗎...?
            {
                List<int> appendPosition = new List<int>();

                //安插分隔線
                for (int i = queryStatResult.Count() - 1; i > 0; i--)
                {
                    int nextIndex = (i - 1) <= 0 ? 0 : (i - 1);
                    int currentMerchantID = (int)queryStatResult[i]["MerchantID"];
                    int nextMerchantID = (int)queryStatResult[nextIndex]["MerchantID"];

                    if (currentMerchantID != nextMerchantID)
                    {
                        appendPosition.Add(i);
                    }
                }

                if (appendPosition.IsNotEmptyNull())
                {
                    var templateRows = queryStatResult.LastOrDefault();

                    Dictionary<string, object> appendLineRow = new Dictionary<string, object>();

                    foreach (var row in templateRows)
                    {
                        appendLineRow.Add(row.Key, "   ");
                    }

                    foreach (int position in appendPosition)
                    {
                        queryStatResult.Insert(position, appendLineRow);
                    }
                }
            }
        }



        private void CheckRequiredColumns(DynamicQueryCommandEntity settingItem, CustomizedColumnDisplayRequestModel request)
        {
            if (settingItem.RequiredColumns.IsNullOrWhiteSpace())
                return;

            var columns = settingItem.RequiredColumns.Split(',').ToList();

            if (columns.IsEmptyOrNull())
                return;

            //檢查必填欄位是否有填寫
            foreach (string columnName in columns)
            {
                string columnNameLower = columnName.ToLower();

                foreach (var prop in request.GetType().GetProperties())
                {
                    string propName = prop.Name;
                    string alias = "";

                    if (columnNameLower == "vendorconfig")
                    {
                        alias = "VendorConfigID".ToLower();
                    }

                    if (prop.Name.ToLower() == columnNameLower || prop.Name.ToLower() == alias)
                    {
                        var value = prop.GetValue(request);

                        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                        {
                            throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel(propName));
                        }
                    }
                }
            }

            //部分配置是存在json裡面
            if (!string.IsNullOrWhiteSpace(settingItem.ExtraSettingJson))
            {
                var parseSetting = settingItem.ExtraSettingJson.JsonConvertToModelIgnoreErrorColumn<CustomizedColumnDynamicQueryExtraSettingJson>();

                if (parseSetting != null)
                {
                    if (parseSetting.Filter_MaxDay.HasValue && request.BeginTime.HasValue && request.EndTime.HasValue)
                    {
                        int totalDays = (int)(request.EndTime.Value - request.BeginTime.Value).TotalDays;

                        if (totalDays > parseSetting.Filter_MaxDay)
                        {
                            throw new TerproException(ApiErrorCode.InvalidParameter_DateTimeRangeTooLarge);
                        }
                    }
                }
            }
        }


        private void SetExtraProperty(List<Dictionary<string, object>> listDic)
        {
            if (listDic.IsEmptyOrNull())
                return;

            List<int> merchantIDs = new List<int>();

            foreach (var eachDic in listDic)
            {
                foreach (var eachValue in eachDic)
                {
                    string columnName = eachValue.Key.ToLower();

                    if (columnName == "merchantid")
                    {
                        string merchantIDText = eachValue.Value.ToString();

                        if (merchantIDText.IsNumeric())
                        {
                            int merchantID = Int32.Parse(merchantIDText);
                            merchantIDs.Add(merchantID);
                        }
                    }
                }
            }

            if (merchantIDs.IsNotEmptyNull())
            {
                var firstElement = listDic.FirstOrDefault();

                var columnsOfMerchantName = firstElement
                    .Select(x => x.Key)
                    .Where(x => x.ToLower() == "merchantname")
                    .ToList();

                var columnsOfCurrency = firstElement
                   .Select(x => x.Key)
                   .Where(x => x.ToLower() == "currency")
                   .ToList();

                var merchantNameColumn = columnsOfMerchantName.FirstOrDefault();
                var currencyColumn = columnsOfCurrency.FirstOrDefault();

                var queryMerchants = MerchantRepository.GetByIds(merchantIDs);

                foreach (var eachDic in listDic)
                {
                    try
                    {
                        int merchantID = 0;

                        MerchantEntity matchMerchant = null;

                        foreach (var eachValue in eachDic)
                        {
                            string columnName = eachValue.Key.ToLower();

                            if (columnName == "merchantid")
                            {
                                merchantID = Int32.Parse(eachValue.Value.ToString());

                                if (merchantID > 0)
                                {
                                    matchMerchant = queryMerchants.FirstOrDefault(x => x.MerchantID == merchantID);
                                }
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(merchantNameColumn) && merchantID > 0)
                        {
                            eachDic[merchantNameColumn] = matchMerchant?.Name;
                        }

                        if (!string.IsNullOrWhiteSpace(currencyColumn) && merchantID > 0)
                        {
                            eachDic[currencyColumn] = matchMerchant?.Currency;
                        }
                    }
                    catch (Exception ex)
                    {
                        string q = "";
                    }
                }
            }



        }



        private void TryCalculateRtpValue(Dictionary<string, object> dic)
        {
            try
            {
                bool isAnyRtpColumn = dic.Select(x => x.Key.ToLower()).Any(col => col == "rtp");

                if (!isAnyRtpColumn)
                {
                    return;
                }

                decimal? betAmount = null;
                decimal? payoutAmount = null;
                decimal? winLossAmount = null;

                foreach (var eachValue in dic)
                {
                    string columnName = eachValue.Key.ToLower();
                    if (columnName == "betamount")
                    {
                        betAmount = decimal.Parse(eachValue.Value.ToString());
                    }
                    else if (columnName == "payoutamount")
                    {
                        payoutAmount = decimal.Parse(eachValue.Value.ToString());
                    }
                    else if (columnName == "winlossamount") //注意這邊的winLoss必須是玩家視角的輸贏 不然RTP會算反
                    {
                        winLossAmount = decimal.Parse(eachValue.Value.ToString());
                    }
                }

                if (winLossAmount.HasValue && !payoutAmount.HasValue)
                {
                    payoutAmount = (-1 * winLossAmount) + betAmount;
                }

                decimal rtp = CommonLogicHelper.CalculateRTP_Percentage(betAmount.Value, payoutAmount.Value);

                if (dic.ContainsKey("RTP"))
                {
                    dic["RTP"] = rtp;
                }
                else if (dic.ContainsKey("Rtp"))
                {
                    dic["Rtp"] = rtp;
                }
                else if (dic.ContainsKey("rtp"))
                {
                    dic["rtp"] = rtp;
                }
            }
            catch
            {

            }
        }


        private string ToTimeText_UTC8(DateTime time)
        {
            return time.AddHours(8).ToString("yyyy-MM-dd HH:mm");
        }


        private CustomizedColumnDynamicQueryExtraSettingJson ParseToExtraSettingModel(string json)
        {
            var defaultEmptyModel = new CustomizedColumnDynamicQueryExtraSettingJson
            {
                IsSkipCsvFormat = false,
                IsSkipFormatColumns = false
            };

            if (string.IsNullOrWhiteSpace(json))
                return defaultEmptyModel;

            try
            {
                var parse = json.JsonConvertToModel<CustomizedColumnDynamicQueryExtraSettingJson>();
                return parse ?? defaultEmptyModel;
            }
            catch
            {

            }

            return defaultEmptyModel;
        }

    }


}
