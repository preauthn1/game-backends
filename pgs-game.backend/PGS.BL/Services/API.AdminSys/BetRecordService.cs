using DocumentFormat.OpenXml.EMMA;
using game.Entity.Game;
using Lck.Utility.Extensions.Helper;
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
using target.Utility.Helper.Elasticsearch.Model;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace target.BL.Services
{
    public partial class BetRecordService : BaseAdminSysService, IBetRecordService
    {
        public ApiRequestLogV2024Repository ApiRequestLogV2024Repository { get; set; }

        public ApiRequestLogRepository ApiRequestLogRepository { get; set; }

        public BetRecordRepository BetRecordRepository { get; set; }

        public BetRecordExtraInfoRepository BetRecordExtraInfoRepository { get; set; }

        public BetRecordTransactionNoMappingRepository BetRecordTransactionNoMappingRepository { get; set; }

        public BetRecordCustomizedSettingRepository BetRecordCustomizedSettingRepository { get; set; }

        public BetRecordMerchantNotifyQueueRepository BetRecordMerchantNotifyQueueRepository { get; set; }

        public BetRecordTagMappingRepository BetRecordTagMappingRepository { get; set; }

        public BetRecordRawWagerTagMappingRepository BetRecordRawWagerTagMappingRepository { get; set; }

        public CurrencyRepository CurrencyRepository { get; set; }

        public EazyTempValueRepository EazyTempValueRepository { get; set; }

        public EventPrizeRecordRepository EventPrizeRecordRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public MappingTagValuesLargeRepository MappingTagValuesLargeRepository { get; set; }

        public MerchantGameSettingRepository MerchantGameSettingRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public PerGameDetailLogAccountingIgnoreRepository PerGameDetailLogAccountingIgnoreRepository { get; set; }

        public PerGameDetailLogMerchantJoinRepository PerGameDetailLogMerchantJoinRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public PlaceBetRaiseRecordRepository PlaceBetRaiseRecordRepository { get; set; }

        public PlayerWalletRepository PlayerWalletRepository { get; set; }

        public PerGameLogRepository PerGameLogRepository { get; set; }

        public PerGameDetailLogTagMappingRepository PerGameDetailLogTagMappingRepository { get; set; }

        public PlayerGameSummaryRepository PlayerGameSummaryRepository { get; set; }

        public UserRepository UserRepository { get; set; }

        public SingleWalletMerchantApiRequestLogRepository SingleWalletMerchantApiRequestLogRepository { get; set; }

        public TransactionRepository TransactionRepository { get; set; }

        public VendorConfigMerchantMappingRepository VendorConfigMerchantMappingRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }

        public VendorConfigSettingRepository VendorConfigSettingRepository { get; set; }

        public WagerTransactionMappingVPlusRepository SMWagerTransactionMappingRepository { get; set; }


        public ISingleWalletCallbackNotifyProcessRefService SingleWalletMerchantBetProcessRefService { get; set; }

        public ISingleWalletPlatformApiRequestRefService SingleWalletPlatformApiRequestRefService { get; set; }

        public IPlayerWalletRefService PlayerWalletRefService { get; set; }

        public IGameVendorSingleWalletService GameVendorSingleWalletService { get; set; }

        public IBetRecordRecoverService BetRecordRecoverService { get; set; }

        public IGameVendorSingleWalletCommonLogicProcessService GameVendorSingleWalletCommonLogicProcessService { get; set; }



        /// <summary>
        /// 取得每一家投注資料 union起來的資料查詢
        /// </summary>
        public PagedList<UnionBetRecordViewModel> GetUnionBetRecordsByPagingFilter(int userID, QueryUnionBetRecordRequestParameterModel filter)
        {
            //union效能太差了 強制限制要篩選vendor
            filter.RuleFor(z => z.GameVendorID).Rule_IsNotNull();

            if (filter.GameVendorID == -1)
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberEmptyZero, new ErrorDetailModel("GameVendorID"));
            }

            if (filter.TransactionNoPGS.IsEmptyOrNull())
            {
                filter.RuleFor(z => z.CreateTimeBegin).Rule_NotNullDateTime();
                filter.RuleFor(z => z.CreateTimeEnd).Rule_NotNullDateTime();
            }

            bool isSearchTranNo = false;
            bool isSearchThirdNo = false;

            if (!string.IsNullOrWhiteSpace(filter.BetRecordNo) ||
                !string.IsNullOrWhiteSpace(filter.TransactionNoPGS))
            {
                int? vendorID = filter.GameVendorID;

                var originalFilter = filter.ReturnInjecteObject<QueryUnionBetRecordRequestParameterModel>();

                //如果查單號 直接清空其他條件 (被日期誤導好多次了...)
                filter = new QueryUnionBetRecordRequestParameterModel();
                filter.PageSize = originalFilter.PageSize;
                filter.BetRecordNo = originalFilter.BetRecordNo.TrimOrNull();
                filter.TransactionNoPGS = originalFilter.TransactionNoPGS.TrimOrNull();
                filter.GameVendorID = vendorID;  //沒篩選vendorType會很慢

                if (filter.BetRecordNo.IsNotEmptyTextNotNull())
                {
                    isSearchThirdNo = true;
                }

                //貼心功能 如果搜尋條件帶的的是 TransactionNoPGS 會自動篩選合適的vendorID
                if (!string.IsNullOrWhiteSpace(filter.TransactionNoPGS))
                {
                    isSearchTranNo = true;

                    var searchList = AppHelper.AnalyzeTextFromRawTextBySpaceChar(filter.TransactionNoPGS);

                    if (searchList.IsNotEmptyNull() && searchList.Count() >= 2)
                    {
                        List<DateTime> times = searchList.Select(x => RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(x))
                            .Where(x => x.Time.HasValue)
                            .Select(z => z.Time.Value)
                            .ToList();

                        filter.ListOfTransactionNoPGS = searchList;
                        filter.TransactionNoPGS = null;

                        int maxHoursRange = 72;

                        if(SettingHelper.IsBWG)
                        {
                            maxHoursRange = maxHoursRange * 2;
                        }

                        if (GameVendorDefinitionHelper.IsSupportPartitionTable(filter.GameVendorID.Value))
                        {
                            filter.PartitionTimeBegin = times.Min();
                            filter.PartitionTimeEnd = times.Max().AddHours(1);

                            if ((filter.PartitionTimeEnd.Value - filter.PartitionTimeBegin.Value).TotalHours > maxHoursRange)
                            {
                                throw new TerproException(ApiErrorCode.InvalidParameter, "PartitionTimeBegin ~ PartitionTimeEnd 時間拉太長了。效能會很差");
                            }
                        }
                        else
                        {
                            filter.CreateTimeBegin = times.Min();
                            filter.CreateTimeEnd = times.Max().AddHours(1);
                        }
                    }
                    else
                    {
                        var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(filter.TransactionNoPGS);

                        if (!recover.IsPgsOrderNo)
                            throw new TerproException(ApiErrorCode.InvalidParameter, "您輸入的單號非pgs合法單號。是否輸入到三方單號了?");

                        var queryByMapping = BetRecordTransactionNoMappingRepository.GetByTransactionNoPGS(filter.TransactionNoPGS);

                        if (queryByMapping != null)
                        {
                            filter.GameVendorID = queryByMapping.GameVendorID;
                        }

                        if (filter.GameVendorID.HasValue)
                        {
                            //如果是支援partition的資料表 會篩選partitionTime欄位
                            if (GameVendorDefinitionHelper.IsSupportPartitionTable(filter.GameVendorID.Value))
                            {
                                filter.PartitionTimeMatch = recover.Time.Value;
                            }
                        }
                    }
                }
            }

            filter.PlayerIDs = filter.PlayerID.ToIntegerListCollection();
            filter.MerchantIDs = filter.MerchantID.ToIntegerListCollection();
            filter.MerchantIDs = FilterAvailablePermissionMerchantIDs(userID, filter.MerchantIDs);

            string hintText = string.Empty;

            if (filter.GameID.HasValue)
            {
                var queryGame = GameListRepository.GetById(filter.GameID.Value);

                if (queryGame == null)
                    throw new TerproException(ApiErrorCode.NotFoundGame);

                if (queryGame.GameVendorType != filter.GameVendorID)
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_GameAndGameVendorNotMatch);
            }

            //如果搜尋playerName，會先直接從player查出playerID (效能考量)
            var queryPlayerName = GetPlayerIDsByPlayerName(userID, filter.PlayerName, filter.PlayerIDs);

            filter.PlayerIDs = queryPlayerName.PlayerIDs;

            if (queryPlayerName.IsPlayerNotFound)
                return new PagedList<UnionBetRecordViewModel>();

            var now = DateTimeHelper.GetCurrentTime();

            if (filter.GameVendorID.HasValue)
            {
                //如果AGG 默認不允許查詢太長時間
                if (filter.GameVendorID == GameVendorTypeEnum.AGG.ToInteger())
                {
                    if (filter.PlayerIDs.IsEmptyOrNull() && filter.MerchantIDs.IsEmptyOrNull() && SettingHelper.IsProduction)
                    {
                        if (filter.CreateTimeBegin.HasValue && filter.CreateTimeEnd.HasValue)
                        {
                            if ((filter.CreateTimeEnd.Value - filter.CreateTimeBegin.Value).TotalHours >= 23 && filter.CreateTimeEnd >= now)
                            {
                                filter.CreateTimeEnd = now.AddMinutes(1);
                                filter.CreateTimeBegin = filter.CreateTimeEnd.Value.AddHours(-1 * 6);
                                hintText = $"(⚠️*調整)已強制將查詢時間調整為 {filter.CreateTimeBegin.Value.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss")}~{filter.CreateTimeEnd.Value.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss")}";
                            }
                        }
                    }
                }

                //如果有支援parition createTime篩選會變成partitionTime篩選
                if (GameVendorDefinitionHelper.IsSupportPartitionTable(filter.GameVendorID.Value))
                {
                    if (filter.CreateTimeBegin.HasValue)
                    {
                        filter.PartitionTimeBegin = filter.CreateTimeBegin.Value.AddHours(-1);
                    }
                    if (filter.CreateTimeEnd.HasValue)
                    {
                        filter.PartitionTimeEnd = filter.CreateTimeEnd.Value;
                    }
                }
            }

            //真正的SQL查詢在此
            //真正的SQL查詢在此
            //真正的SQL查詢在此
            var query = BetRecordRepository.GetUnionBetRecordsByPagingFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<UnionBetRecordQueryModel, UnionBetRecordViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = ConvertToViewModels(x);
                    return convert;
                })
                .ToList();

            bool isBriefSearch = filter.PageSize >= 5000 ? true : false;

            //額外欄位設置
            SetExtraProperty(viewModels.Items, isBriefSearch);

            if (viewModels.Items.IsEmptyOrNull())
            {
                //如果輸入pgs單號。卻查無資料 部分情況下會跳一些提示通知
                CheckExtraTableAndThrowMessage(filter);

                if (isSearchThirdNo)
                {
                    var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(filter.BetRecordNo);

                    if (recover.IsPgsOrderNo)
                    {
                        throw new TerproException(ApiErrorCode.NotFoundData, new ErrorDetailModel("你選擇查詢三方單號。但格式卻是PGS單號。是否選錯了?"));
                    }
                }
            }

            UnionBetRecordViewModel myTotalSum = new UnionBetRecordViewModel();

            if (viewModels.Items.IsNotEmptyNull())
            {
                myTotalSum = viewModels.Items.CalculateTotalSumMergeToOneObject(); 
                
                List<int> merchantIDs = viewModels.Items.Select(z => z.MerchantID).Distinct().Take(5).ToList();

                foreach(var mID in merchantIDs)
                {
                    CheckViewMerchantPermission(userID, mID);
                }
            }

            viewModels.ExtraObject = new
            {
                TotalSummary = myTotalSum,
                HintText = hintText
            };

            return viewModels;
        }


        /// <summary>
        /// perGame頁面用的 (資料源是perGame)
        /// 差別在於 betRecord 三方原始注單。資料保存時間可能比較短
        /// </summary>
        public BetRecordGeneralInfoViewModel GetByTransactionNo_ForPerGamePage(int userID, BetRecordQueryRequestModel model)
        {
            model.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var queryMapping = BetRecordTransactionNoMappingRepository.GetByTransactionNoPGS(model.TransactionNo);

            if (queryMapping == null)
                throw new TerproException(ApiErrorCode.NotFoundRecord, new ErrorDetailModel($"{model.TransactionNo}"));

            var recoverResult = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(queryMapping.BetRecordNoPGS);

            var queryPerGame = PerGameDetailLogRepository.GetByTransactionNo_WithGameTimeRange(queryMapping.BetRecordNoPGS);

            bool isAccountingIgnore = false;

            if (queryPerGame == null)
            {
                queryPerGame = PerGameDetailLogAccountingIgnoreRepository.GetByTransactionNo_WithGameTimeRange(queryMapping.BetRecordNoPGS).ReturnInjecteObject<PerGameDetailLogEntity>();
                isAccountingIgnore = true;

                if (queryPerGame == null)
                {
                    throw new TerproException(ApiErrorCode.NotFoundBetRecord);
                }
            }

            CheckViewMerchantPermission(userID, queryPerGame.MerchantID);

            int myGameVendor = AppHelper.GetGameVendorByGameID(queryPerGame.GameID);

            BetRecordGeneralColumnViewModel viewModelConvert = null;

            if (myGameVendor > 9999)
            {
                var vendorService = ServiceInstanceHelper.InstanceVendorService((GameVendorTypeEnum)myGameVendor);

                var generalInfo = vendorService.GetByPGSTransactionNo(queryPerGame.TransactionNo);

                //如果資料已被清除 嘗試用perGame資料補
                if (generalInfo == null)
                {
                    viewModelConvert = new BetRecordGeneralColumnViewModel();
                    viewModelConvert.BetAmount = queryPerGame.BetAmount;
                    viewModelConvert.PayoutAmount = queryPerGame.PayoutAmount;
                    viewModelConvert.WinLossAmount = queryPerGame.UpdateAmount;
                    viewModelConvert.ValidAmount = queryPerGame.ValidAmount;
                    viewModelConvert.CreateTime = queryPerGame.GameTime;
                    viewModelConvert.SettlementTime = queryPerGame.GameTime;
                    viewModelConvert.GameID = queryPerGame.GameID;
                    viewModelConvert.MerchantID = queryPerGame.MerchantID;
                    viewModelConvert.PlayerID = queryPerGame.PlayerID;
                    viewModelConvert.VendorConfigID = queryPerGame.ServerID;
                    viewModelConvert.PlayerName = queryPerGame.PlayerName;
                    viewModelConvert.BetRecordNoPGS = queryPerGame.TransactionNo;
                    viewModelConvert.BetRecordNo = queryPerGame.ThirdPartyBetRecordNo;
                    viewModelConvert.ConvertRate = 1;
                }
                else
                {
                    viewModelConvert = generalInfo.ReturnInjecteObject<BetRecordGeneralColumnViewModel>();
                }

                viewModelConvert.ThirdPartyGameCode = GameListRepository.GetSingleByCache(queryPerGame.GameID)?.ThirdPartyGameCode;
            }
            else
            {
                viewModelConvert = queryPerGame.ReturnInjecteObject<BetRecordGeneralColumnViewModel>();
                viewModelConvert.CreateTime = queryPerGame.GameTime;
                viewModelConvert.SettlementTime = queryPerGame.GameTime;
                viewModelConvert.BetRecordNo = queryPerGame.ThirdPartyBetRecordNo;
                viewModelConvert.BetRecordNoPGS = queryPerGame.TransactionNo;
            }

            if (!currentUser.IsSuperAdminOrg && !currentUser.AvailableMerchantIDs.Contains(viewModelConvert.MerchantID))
                throw new TerproException(ApiErrorCode.NotFoundRecord);

            var viewModel = ConvertToBetRecordViewModel(viewModelConvert, queryPerGame);
            viewModel.IsAccountingIgnore = isAccountingIgnore;

            if (currentUser.IsSuperAdminOrg)
            {
                bool isCS_Role = HasCustomerServicePermission(userID);

                var assignVendorID = VendorConfigSettingRepository.GetAssignVendorIdByVendorConfigID(viewModel.VendorConfigID);

                var queryMerchant = MerchantRepository.GetById(viewModel.GeneralInfo.MerchantID);

                var queryGame = GameListRepository.GetById(viewModel.GeneralInfo.GameID);

                viewModel.CopyInfoForGameVendor = AppHelper.GenerateCopyInfoAndForwardToGameVendor(
                    (GameVendorTypeEnum)viewModel.GameVendorID,
                    isCS_Role,
                    assignVendorID?.Value,
                    queryMerchant.Currency,
                    viewModel.GeneralInfo.PlayerID,
                    new GameVendorCopyInfoExtraTextModel
                    {
                        BetRecordNoPGS = viewModel.GeneralInfo.BetRecordNoPGS,
                        ThirdPartyBetRecordNo = viewModel.GeneralInfo.BetRecordNo
                    });

                viewModel.CopyInfoForGameVendor += AppHelper.GenerateWagerCopyExtraInfo(viewModel.GeneralInfo);
            }

            return viewModel;
        }


        /// <summary>
        /// 遊戲商注單查詢頁面用的 (資料源是betRecord)
        /// </summary>
        public BetRecordGeneralInfoViewModel GetByTransactionNo(int userID, BetRecordQueryRequestModel model)
        {
            model.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();

            var queryMapping = BetRecordTransactionNoMappingRepository.GetByTransactionNoPGS(model.TransactionNo);

            if (queryMapping == null)
                throw new TerproException(ApiErrorCode.NotFoundRecord);

            var queryGenralInfoByVendor = GetByPgsTransactionNo(queryMapping.GameVendorID, queryMapping.BetRecordNoPGS);

            if (queryGenralInfoByVendor == null)
                throw new TerproException(ApiErrorCode.NotFoundRecord, new ErrorDetailModel(model.TransactionNo));

            var viewModelConvert = queryGenralInfoByVendor.ReturnInjecteObject<BetRecordGeneralColumnViewModel>();

            CheckViewMerchantPermission(userID, viewModelConvert.MerchantID);

            //queryPerGame 有可能是null
            var queryPerGame = PerGameDetailLogRepository.GetByTransactionNo_WithGameTimeRange(queryMapping.BetRecordNoPGS);

            var viewModel = ConvertToBetRecordViewModel(viewModelConvert, queryPerGame);

            var raiseRecords = PlaceBetRaiseRecordRepository.GetRaiseRecordsByPgsNo(queryMapping.BetRecordNoPGS);

            if (raiseRecords.IsNotEmptyNull())
            {
                viewModel.RaiseBetInfo = raiseRecords
                    .Select(x =>
                    {
                        var convert = x.ReturnInjecteObject<RaiseBetInfoModel>();
                        return convert;
                    })
                    .OrderBy(x => x.CreateTime)
                    .ToList();
            }

            if (queryMapping.GameVendorID == (int)GameVendorTypeEnum.SlotMania)
            {
                var queryTranNoMapping = SMWagerTransactionMappingRepository.GetByFilter(new QueryWagerTransactionMappingGeneralRequestViewModel
                {
                    BetRecordNoPGS = queryMapping.BetRecordNoPGS
                });

                viewModel.ExtraAmountDetail = queryTranNoMapping
                   .Select(x =>
                   {
                       return new ExtraAmountInfoModel
                       {
                           ThirdTransactionNo = x.TransactionID,
                           CreateTime = x.CreateTime,
                           Memo = x.IsComplete ? "Complete=1" : "",
                           Amount = x.Amount ?? -1,
                       };
                   })
                   .OrderBy(x => x.CreateTime)
                   .ToList();
            }

            if (viewModel.TagMapping.IsNotEmptyNull())
            {
                viewModel.IsVirtualItemCard = viewModel.ExtraReferPrizeType > 0;
            }

            bool isCS_role = HasCustomerServicePermission(userID);

            viewModel.CopyInfoForGameVendor = AppHelper.GenerateCopyInfoAndForwardToGameVendor(
                        (GameVendorTypeEnum)viewModel.GameVendorID,
                        isCS_role,
                        viewModel.VendorConfigAssignMerchantID,
                        viewModel.Currency,
                        viewModel.GeneralInfo.PlayerID,
                        new GameVendorCopyInfoExtraTextModel
                        {
                            BetRecordNoPGS = viewModel.GeneralInfo.BetRecordNoPGS,  
                            ThirdPartyBetRecordNo = viewModel.GeneralInfo.BetRecordNo
                        });

            viewModel.CopyInfoForGameVendor += AppHelper.GenerateWagerCopyExtraInfo(viewModel.GeneralInfo);

            return viewModel;
        }


        public CRUDResultModel GetBetRecordCopyInfo(int userID, LazyCommonRequestModel model)
        {
            model.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();

            var queryDetail = GetByTransactionNo(userID, new BetRecordQueryRequestModel
            {
                TransactionNo = model.BetRecordNoPGS
            });

            return new CRUDResultModel
            {
                Message = queryDetail.CopyInfoForGameVendor
            };
        }


        /// <summary>
        /// 遊戲商注單查詢頁面用的 (資料源是betRecord)
        /// </summary>
        public (string CopyText, int GameID) GetGameVendorInfoTextByTransactionNo(string transactionNo)
        {
            var queryRecord = GetByTransactionNo(SystemConstant.SkipValidateUserID, new BetRecordQueryRequestModel
            {
                TransactionNo = transactionNo
            });

            string myText = $@"注單號：{queryRecord.GeneralInfo.BetRecordNo}
服務器：{queryRecord.VendorConfigAssignMerchantID}
貨幣：{queryRecord.Currency}
玩家：{queryRecord.GeneralInfo.PlayerID} (可能对应贵司的玩家帐号或昵称)
遊戲：{queryRecord.GeneralInfo.GameName} ({queryRecord.GeneralInfo.ThirdPartyGameCode}／pgsGameID：{queryRecord.GeneralInfo.GameID})
時間：{queryRecord.GeneralInfo.CreateTime.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss")} (GMT+8)
投注：{queryRecord.GeneralInfo.BetAmount}
派獎：{queryRecord.GeneralInfo.PayoutAmount}";

            return (myText, queryRecord.GeneralInfo.GameID);
        }


        public BetRecordApiReqeustLogAndDebugSqlViewModel GetBetRecordDebugSQLAndApiReqeustLog(int userID, string transactionNo)
        {
            var querySingle = GetByTransactionNo(userID, new BetRecordQueryRequestModel
            {
                TransactionNo = transactionNo
            });

            var sqlGen = GenerateApiRequestLogSQL(querySingle.GeneralInfo, querySingle.GameVendorID, querySingle.GeneralInfo.BetRecordNo);

            var viewModel = new BetRecordApiReqeustLogAndDebugSqlViewModel
            {
                DebugSQL = sqlGen.DebugSQL,
                LogItems = new List<ApiRequestLogDebugModel>()
            };

            string extraDebugMsg = "";

            try
            {
                var query1 = ApiRequestLogRepository.GetByRawSQL(sqlGen.LogSQL);

                //2026/04 開始。errorLog全面切到ES了 不再使用標準DB
                var ESIndexName_normal = TableNameConvertHelper.GetElasticsearchIndexName(ElasticLogTableTypeEnum.ApiRequestLogExternalVendorApi);
                var ESIndexName_error = TableNameConvertHelper.GetElasticsearchIndexName(ElasticLogTableTypeEnum.ApiRequestLogExternalVendorAPIError);

                var esQuery1 = new ESKeywordSearchBasicParameter
                {
                    Begin = querySingle.GeneralInfo.CreateTime.AddHours(-1),
                    End = querySingle.GeneralInfo.CreateTime.AddHours(1),
                    IndexName = ESIndexName_normal,
                    PageSize = 10
                };

                var esQuery2 = new ESKeywordSearchBasicParameter
                {
                    Begin = querySingle.GeneralInfo.CreateTime.AddHours(-1),
                    End = querySingle.GeneralInfo.CreateTime.AddHours(1),
                    IndexName = ESIndexName_error,
                    PageSize = 10
                };

                var query3 = ElasticsearchHelper.KeywordSearchDataByAPI<FullInfoApiRequestLogESModel>(esQuery1, new ESKeywordSearchParameter
                {
                    ColumnName = "ReferOrderNo",
                    Keyword = querySingle.GeneralInfo.BetRecordNo
                });

                var query4 = ElasticsearchHelper.KeywordSearchDataByAPI<FullInfoApiRequestLogESModel>(esQuery2, new ESKeywordSearchParameter
                {
                    ColumnName = "ReferOrderNo",
                    Keyword = querySingle.GeneralInfo.BetRecordNo
                });

                extraDebugMsg = "q1 q2 已查詢成功";

                foreach (var item in query1)
                {
                    var inject = item.ReturnInjecteObject<ApiRequestLogDebugModel>();

                    if (item.UniqueID.IsNotEmptyTextNotNull())
                    {
                        if (TableNameConvertHelper.IsEnableElasticLogByTable(ElasticLogTableTypeEnum.ApiRequestLogExternalVendorApi))
                        {
                            var ESindexName = TableNameConvertHelper.GetElasticsearchIndexNameByTableName("ApiRequestLogExternalVendorApi");

                            var queryES = ApiRequestLogV2024Repository.GetESFullInformationByUniqueID(ESindexName, item.UniqueID);

                            inject.RequestBody = queryES?.RequestContent;
                            inject.ResponseText = queryES?.ResponseText;
                            inject.Headers = queryES?.RequestHeader;
                        }
                        else
                        {
                            var parse = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(item.UniqueID);

                            var queryExtraInfo = MappingTagValuesLargeRepository.GetByFilter(new QueryMappingTagValuesRequestViewModel
                            {
                                UniqueID = item.UniqueID,
                                PartitionTimeMatch = parse.Time.Value,
                                DbQueryExtraConfig = new DbQueryConfig
                                {
                                    ConnectTimeoutSeconds = 60
                                }
                            });

                            inject.RequestBody = queryExtraInfo.FirstOrDefault(x => x.SettingKey == (int)ApiReqeustLogTagValueMappingTypeEnum.RequestBody)?.SettingValue;
                            inject.ResponseText = queryExtraInfo.FirstOrDefault(x => x.SettingKey == (int)ApiReqeustLogTagValueMappingTypeEnum.ResponseText)?.SettingValue;
                            inject.Headers = queryExtraInfo.FirstOrDefault(x => x.SettingKey == (int)ApiReqeustLogTagValueMappingTypeEnum.Headers)?.SettingValue;
                        }
                    }

                    viewModel.LogItems.Add(inject);
                }

                foreach (var item in query3)
                {
                    var inject = item.ReturnInjecteObject<ApiRequestLogDebugModel>();
                    inject.ResponseText = item.ResponseText;
                    inject.RequestBody = item.RequestContent;
                    inject.Headers = item.RequestHeader;
                    inject.ExecutionTime = item.ResponseTime;
                    inject.RequestUrl = item.RequestUrlFull;

                    viewModel.LogItems.Add(inject);
                }

                foreach (var item in query4)
                {
                    var inject = item.ReturnInjecteObject<ApiRequestLogDebugModel>();
                    inject.IsErrorLog = true;
                    inject.ResponseText = item.ResponseText;
                    inject.RequestBody = item.RequestContent;
                    inject.Headers = item.RequestHeader;
                    inject.ExecutionTime = item.ResponseTime;
                    inject.RequestUrl = item.RequestUrlFull;

                    viewModel.LogItems.Add(inject);
                }


                if (viewModel.LogItems.IsNotEmptyNull())
                {
                    var uniqueIDs = viewModel.LogItems.Select(x => x.UniqueID).ToList();
                    var matchFirstID = uniqueIDs.FirstOrDefault();

                    string idsText = string.Join(",", uniqueIDs.Select(x => $"'{x}'"));

                    var parse = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(matchFirstID);

                    string extraSql = $@"
SELECT TOP 55 * FROM[dbo].[MappingTagValuesLarge] WITH(NOLOCK)
WHERE PartitionTime = '{parse.Time.Value.ToString("yyyy-MM-dd HH:mm:ss")}' AND UniqueID in ( {idsText} )";

                    viewModel.DebugSQL += extraSql;
                }
            }
            catch (Exception ex)
            {
                viewModel.DebugSQL = viewModel.DebugSQL + $"{Environment.NewLine}{extraDebugMsg}{Environment.NewLine}發生錯誤 {ex.GetMessageAndErrorLine()}";
            }

            return viewModel;
        }


        /// <summary>
        /// 批次取消多筆注單
        /// </summary>
        public void BatchManualRefundBetRecord(int userID, BatchManualRefundBetRecordRequestModel model)
        {
            if (model.ListOfTransactionNo.IsEmptyOrNull())
            {
                model.ListOfTransactionNo = AppHelper.AnalyzeTextFromRawTextBySpaceChar(model.AllTransactionNoText);
            }

            model.RuleFor(z => z.ListOfTransactionNo).Rule_NotEmptyNull();

            model.DelayMilliseconds = model.DelayMilliseconds <= 0 ? 100 : model.DelayMilliseconds;

            foreach (string transactionNo in model.ListOfTransactionNo)
            {
                Thread.Sleep(model.DelayMilliseconds);

                if (model.IsAsync)
                {
                    Task.Factory.StartNew(() =>
                    {
                        ManualRefundBetRecord(userID, new BetRecordManualRefundRequestModel
                        {
                            Memo = model.Memo,
                            TransactionNo = transactionNo
                        });
                    });
                }
                else
                {
                    ManualRefundBetRecord(userID, new BetRecordManualRefundRequestModel
                    {
                        Memo = model.Memo,
                        TransactionNo = transactionNo
                    });
                }
            }

            string myMsg = $"批次Refund 本次處理 {model.ListOfTransactionNo.Count()} 筆";
            SendMessageToMultipleChannel(myMsg);
        }


        /// <summary>
        /// 手動取消注單
        /// </summary>
        public void ManualRefundBetRecord(int userID, BetRecordManualRefundRequestModel model)
        {
            model.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var queryMapping = BetRecordTransactionNoMappingRepository.GetByTransactionNoPGS(model.TransactionNo);

            if (queryMapping == null)
                throw new TerproException(ApiErrorCode.NotFoundRecord);

            var queryGeneralInfoByVendor = GetByPgsTransactionNo(queryMapping.GameVendorID, queryMapping.BetRecordNoPGS);

            if (queryGeneralInfoByVendor == null)
                throw new TerproException(ApiErrorCode.NotFoundRecord);

            //只有pending中的狀態可以操作退款
            if (queryGeneralInfoByVendor.Status != (int)BetRecordStatusEnum.Pending)
            {
                throw new TerproException(ApiErrorCode.InvalidStatus);
            }

            var queryMerchant = MerchantRepository.GetById(queryGeneralInfoByVendor.MerchantID);

            string ip = IPHelper.GetIP();

            // Step 1. 修改狀態
            var vendorService = ServiceInstanceHelper.InstanceVendorService(queryMapping.GameVendorID);
            vendorService.CancelBet(queryMapping.BetRecordNoPGS, model.Memo);

            // Step 1-2. 紀錄額外資訊
            string debugInfo = $"Manual Cancel, {model.Memo} ,by {currentUser.UserName}";
            BetRecordExtraInfoRepository.CreateOrUpdateAsync(queryMapping.BetRecordNoPGS, debugInfo);

            // Step 2. 寫入一筆操作紀錄
            var log = InstanceOperateLog(userID, UserOperateTypeEnum.SingleWallet_CancelRefundWalletManual, queryGeneralInfoByVendor.Id);
            log.DebugInfo = $"{queryGeneralInfoByVendor.ConvertToJsonString()}";
            log.Memo = model.Memo;

            WriteOperateLog(log);

            // Step 3. 若 [單錢包模式] 
            if (CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode))
            {
                //呼叫對方API
                SingleWalletMerchantBetProcessRefService.CancelProcessCallbackMerchantApi(new SingleWalletModeCancelApiParameter
                {
                    PlayerID = queryGeneralInfoByVendor.PlayerID,
                    TransactionNo = queryGeneralInfoByVendor.BetRecordNoPGS,
                    GameID = queryGeneralInfoByVendor.GameID,
                    MerchantID = queryGeneralInfoByVendor.MerchantID,
                });
            }
            else
            {
                //更新PGS錢包
                PlayerWalletRepository.UpdateWalletAmountForbiddenMinus(
                    queryGeneralInfoByVendor.PlayerID,
                    queryGeneralInfoByVendor.GameID,
                    queryGeneralInfoByVendor.BetAmount,
                    TransactionTypeEnum.BetRecord_ManualCancelRecord_RefundWalletManual,
                    queryGeneralInfoByVendor.BetRecordNoPGS,
                    ip
                );
            }

            // Step 4. 標記Tag
            BetRecordTagMappingRepository.CreateIfNotExist(queryGeneralInfoByVendor.BetRecordNoPGS, SystemTagEnum.BetRecord_ManualCancel);

            if (model.ExtraTagID.HasValue)
            {
                BetRecordTagMappingRepository.CreateIfNotExist(queryGeneralInfoByVendor.BetRecordNoPGS, (SystemTagEnum)model.ExtraTagID.Value);
            }

            return;
        }


        public void ManualSettleBetRecord(int userID, BetRecordManualSettleRequestModel model)
        {
            model.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();
            model.RuleFor(z => z.PayoutAmount).Rule_NotMinus();
            model.RuleFor(z => z.ValidAmount).Rule_NotMinus();
            model.RuleFor(z => z.Memo).Rule_NotEmptyText();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            string transactionNoPGS = model.TransactionNo;
            var queryMapping = BetRecordTransactionNoMappingRepository.GetByTransactionNoPGS(transactionNoPGS);

            if (queryMapping == null)
                throw new TerproException(ApiErrorCode.NotFoundRecord);

            var queryGenralInfoByVendor = GetByPgsTransactionNo(queryMapping.GameVendorID, queryMapping.BetRecordNoPGS);

            if (queryGenralInfoByVendor == null)
                throw new TerproException(ApiErrorCode.NotFoundRecord);

            var queryMerchant = MerchantRepository.GetById(queryGenralInfoByVendor.MerchantID);

            string ip = IPHelper.GetIP();
            string processID = Guid.NewGuid().ToString();

            decimal payoutAmount = model.PayoutAmount;
            decimal validAmount = model.ValidAmount;

            var updateParam = new SettlementBetRecordRequiredInfoModel
            {
                BetRecordNoPGS = queryMapping.BetRecordNoPGS,
                PayoutAmount = payoutAmount,
                ValidAmount = validAmount,
                IsAdminSysManualOperate = true,
                DebugExtraMemo = processID,
                UserID = userID,
                ExtraColumn = new ExtraColumnByGameVendorParameter()
            };

            if (!string.IsNullOrWhiteSpace(model.ThirdSettleDate))
            {
                updateParam.ExtraColumn.ThirdPartySettleTime = DateTime.Parse(model.ThirdSettleDate);
            }

            // Step 1. 修改狀態
            var vendorService = ServiceInstanceHelper.InstanceVendorService(queryMapping.GameVendorID);
            var settleResult = vendorService.UpdateRawBetRecord(updateParam);

            // Step 1-2. 紀錄額外資訊
            string debugInfo = $"Manual Settle, {model.Memo} ,by {currentUser.UserName}";
            BetRecordExtraInfoRepository.CreateOrUpdateAsync(queryMapping.BetRecordNoPGS, debugInfo);

            // Step 2. 寫入一筆操作紀錄
            var log = InstanceOperateLog(userID, UserOperateTypeEnum.SingleWallet_CancelRefundWalletManual, queryGenralInfoByVendor.Id);
            log.DebugInfo = $"{queryGenralInfoByVendor.ConvertToJsonString()}";
            log.Memo = model.Memo;

            WriteOperateLog(log);

            // step 3. 轉成perGameLog格式
            var convertPerGameResult = vendorService.FormatParsing_ConvertBetRecordToPerGameDetailLog(queryGenralInfoByVendor.BetRecordNoPGS);
            var convertPerGame = convertPerGameResult.PerGameDetailLog;

            string UUID = $"MA_{convertPerGame.TransactionNo}";

            var queryEventPrize = BetRecordCustomizedSettingRepository.GetParseModelByUniqueKey(queryGenralInfoByVendor.BetRecordNoPGS, BetRecordCustomizedSettingEnum.ReferEventPrize);

            var referPrizeType = EventPrizeTypeEnum.Default;

            if (queryEventPrize != null)
            {
                referPrizeType = (EventPrizeTypeEnum)queryEventPrize.SettingValueOfInt;
            }

            var processConfig = new SettleBalanceUpdateProcessConfig
            {
                SessionToken = "",
                RequestUUID = UUID,
                ReferEventPrizeType = referPrizeType,
                IsSkipUpdateBalance = false,
                MerchantWalletMode = queryMerchant.WalletMode,
                Wager_RoundID = queryGenralInfoByVendor.ExtraInfo1,
                Wager_PayoutAmountWalletUpdate = payoutAmount
            };

            // step 4. 更新錢包金額
            var updateResult = GameVendorSingleWalletCommonLogicProcessService.UpdateWalletAmountSettleProcess((GameVendorTypeEnum)queryMapping.GameVendorID, processConfig, convertPerGameResult);

            // step 5. 寫入perGameLog
            convertPerGame.CurrentAmount = updateResult.BeforeAmount;
            WritePerGameLog(convertPerGame);

            // step 6. 更新玩家遊戲數據統計
            PlayerGameSummaryRepository.PlayerGameSummaryValuePlus(new PlayerGameSummaryUpdateParameter
            {
                PayoutAmount = convertPerGameResult.PerGameDetailLog.PayoutAmount,
                BetAmount = convertPerGameResult.PerGameDetailLog.BetAmount,
                ValidAmount = convertPerGameResult.PerGameDetailLog.ValidAmount,
                TaxAmount = convertPerGameResult.PerGameDetailLog.TaxAmount,
                WinLossAmount = convertPerGameResult.PerGameDetailLog.UpdateAmount,
                GameID = convertPerGameResult.PerGameDetailLog.GameID,
                PlayerID = convertPerGameResult.PerGameDetailLog.PlayerID
            });

            return;
        }

        public void ManualChangeSettleAmount(int userID, BetRecordManualChangeSettleAmountRequestModel model)
        {
            model.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();
            model.RuleFor(z => z.PayoutAmount).Rule_NotMinus();
            model.RuleFor(z => z.ValidAmount).Rule_NotMinus();
            model.RuleFor(z => z.Memo).Rule_NotEmptyText();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            string transactionNoPGS = model.TransactionNo;

            var queryMapping = BetRecordTransactionNoMappingRepository.GetByTransactionNoPGS(transactionNoPGS);

            if (queryMapping == null)
                throw new TerproException(ApiErrorCode.NotFoundRecord);

            var queryGenralInfoByVendor = GetByPgsTransactionNo(queryMapping.GameVendorID, queryMapping.BetRecordNoPGS);

            if (queryGenralInfoByVendor == null)
                throw new TerproException(ApiErrorCode.NotFoundRecord);

            //操作紀錄
            var log = InstanceOperateLog(userID, UserOperateTypeEnum.BetRecordManualChangeSettleAmount, 0);
            log.Memo = model.Memo;
            log.DebugInfo = model.ConvertToJsonString();

            WriteOperateLog(log);

            //執行修改已結算注單的邏輯
            var result = GameVendorSingleWalletService.ChangeSettleAmount((GameVendorTypeEnum)queryMapping.GameVendorID, new SingleWalletChangeBetPayoutRequestParameter
            {
                BetRecordNo = queryGenralInfoByVendor.BetRecordNo,
                UpdateBy = currentUser.UserName,
                IsSkipSeamlessCallback = !model.IsTriggerCallback,
                ExtraInfo = new CancelExtraParameterModel
                {
                    GameID = queryGenralInfoByVendor.GameID,
                },
                NewSettleInfo = new SingleWalletChangeSettleInfo
                {
                    PayoutAmount = model.PayoutAmount,
                    ValidAmount = model.ValidAmount,
                    TaxAmount = 0
                }
            });

            return;
        }


        /// <summary>
        /// (批次處理) 補救perGame資料用的情境
        /// </summary>
        public void ManualBatchWritePerGameLog(int userID, BetRecordManualBatchWritePerGameRequestModel model)
        {
            model.RuleFor(z => z.ListOfTransactionNoPGS);

            List<string> listOfTransactionNoPGS = model.ListOfTransactionNoPGS;

            if (listOfTransactionNoPGS.IsEmptyOrNull() && model.QueryDataByTagID.HasValue)
            {
                var queryTags = BetRecordTagMappingRepository.GetByFilter(new QueryBetRecordTagMappingRequestViewModel
                {
                    TagIDs = model.QueryDataByTagID.Value.ToListCollection(),
                    CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-3),
                    CreateTimeEnd = DateTimeHelper.GetCurrentTime(),
                });

                listOfTransactionNoPGS = queryTags.Select(x => x.BetRecordNoPGS).ToList();
            }
            else
            {
                //如果沒傳array參數 嘗試用逗號區隔的字串拆解
                if (listOfTransactionNoPGS.IsEmptyOrNull() && !string.IsNullOrWhiteSpace(model.CommaSeperateTransactionNoPGS))
                {
                    listOfTransactionNoPGS = AppHelper.AnalyzeTextFromRawTextBySpaceChar(model.CommaSeperateTransactionNoPGS);
                }
            }

            if (listOfTransactionNoPGS.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("ListOfTransactionNoPGS"));

            foreach (var tranNo in listOfTransactionNoPGS)
            {
                var param = model.ReturnInjecteObject<BetRecordManualWritePerGameRequestModel>();
                param.TransactionNoPGS = tranNo;

                try
                {
                    //批次迴圈單筆單筆處理
                    ManualWritePerGameLog(userID, param);
                }
                catch (Exception ex)
                {
                    string myMsg = $"單號{tranNo} 補救perGame失敗 {ex.GetMessageAndErrorLine()}";
                    SendMessageToMultipleChannel(myMsg, TelegramChannelEnum.pgs_adminSys_api);
                }
            }
        }


        public CRUDResultModel ManualDeletePerGameLog(int userID, BetRecordManualDeletePerGameRequestModel model)
        {
            model.RuleFor(z => z.ListOfTransactionNoPGS).Rule_NotEmptyNull();
            model.RuleFor(z => z.ReplaceToThisPlayerID).Rule_NotZeroNotMinus();

            List<string> listOfTransactionNoPGS = model.ListOfTransactionNoPGS;

            if (listOfTransactionNoPGS.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("ListOfTransactionNoPGS"));

            var queryPlayer = PlayerRepository.GetByIdOrThrowException(model.ReplaceToThisPlayerID);

            foreach (var tranNo in listOfTransactionNoPGS)
            {
                //批次迴圈單筆單筆處理
                ManualDeletePerGame(tranNo, queryPlayer.MerchantID, queryPlayer.PlayerID, queryPlayer.PlayerName);
            }

            return new CRUDResultModel
            {
                Message = $"共更新 {listOfTransactionNoPGS.Count()} 筆。執行完成後需要重跑統計帳務(StatDB -> MerchantGameSummary)",
            };
        }


        public void JobAutoRecoverPlaceBetExceptionRefundProcess(int userID, Guid referUUID)
        {
            string processName = "JobAutoRecoverPlaceBetException";

            var queryEazyTemp = EazyTempValueRepository.GetByFilter(new QueryEazyTempValueRequestViewModel
            {
                UUIDs = referUUID.ToListCollection(),
            })
            .FirstOrDefault();

            if (queryEazyTemp == null)
            {
                string msg = $"{processName} 查無對應Eazy資料 {referUUID}";
                SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_common_error);
            }

            string betRecordNoPGS = queryEazyTemp.BetRecordNoPGS;

            var queryMerchant = MerchantRepository.GetById(queryEazyTemp.MerchantID ?? 0);

            if (queryMerchant == null)
            {
                string msg = $"{processName} 單號:{betRecordNoPGS} 查無對應商戶 {queryEazyTemp.MerchantID}";
                SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_common_error);
            }

            int gameID = queryEazyTemp.GameID ?? 0;
            int gameVendorID = AppHelper.GetGameVendorByGameID(gameID);

            //會需要這種處理機制的廠商 先限定在有重複placeBet機制的廠商 (AGG真的很瞎)
            List<int> availableGameVendors = new List<int>
            {
                GameVendorTypeEnum.AGG.ToInteger(),
            };

            if (!availableGameVendors.Contains(gameVendorID))
            {
                string msg = $"{processName} 單號:{betRecordNoPGS} 不是合法的gameVendorID 不進行處理 ";
                SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_common_error);
            }

            //單錢包與轉帳錢包 處理方式不同
            if (AppHelper.IsSingleWalletMode(queryMerchant.WalletMode))
            {
                RecoverNotifyQueueData(userID, betRecordNoPGS);
            }
            else
            {
                var vendorService = ServiceInstanceHelper.InstanceVendorService((GameVendorTypeEnum)gameVendorID);

                var generalInfo = vendorService.GetByPGSTransactionNo(betRecordNoPGS);

                if (generalInfo != null)
                {
                    string msg = $"{processName} 單號:{betRecordNoPGS} 有資料存在、為何還會觸發這個排程?";
                    SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_common_error);
                    return;
                }

                decimal betAmount = queryEazyTemp.Amount1 ?? 0;
                decimal usdtAmount = CurrencyRepository.ConvertToUsdtAmount(betAmount, queryMerchant.Currency);

                if (usdtAmount > 10)
                {
                    string msg = $"{processName} 單號:{betRecordNoPGS} {betAmount} 金額過大或異常。不透過排程自動處理、請手動確認?";
                    SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_important_error);
                    return;
                }

                string selectColumns = " TransactionID , PlayerID , TransactionType , ReferenceOrderNo ";

                int playerID = queryEazyTemp.PlayerID.Value;

                var queryTransaction = TransactionRepository.GetByFilter(new QueryTransactionRecordRequestViewModel
                {
                    PlayerIDs = playerID.ToListCollection(),
                    CreateTimeBegin = queryEazyTemp.CreateTime.AddMinutes(-1 * 2),
                    CreateTimeEnd = queryEazyTemp.CreateTime.AddMinutes(10),

                }, selectColumns)
                .Where(x => x.ReferenceOrderNo == betRecordNoPGS)
                .ToList();

                if (queryTransaction.CountOrZero() == 0)
                {
                    string msg = $"{processName} 單號:{betRecordNoPGS} 查不到bet金幣異動紀錄? 怪怪的";
                    SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_important_error);
                    return;
                }

                if (queryTransaction.CountOrZero() >= 2)
                {
                    string msg = $"{processName} 單號:{betRecordNoPGS} 查到2筆以上的金幣異動紀錄? 怪怪的";
                    SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_important_error);
                    return;
                }

                //更新玩家遊戲錢包
                RefundWallet_NoWagerDataCase(betRecordNoPGS, playerID, gameID, betAmount);
            }

            return;
        }


        private void RefundWallet_NoWagerDataCase(string betRecordNoPGS, int playerID, int gameID, decimal amount)
        {
            //最後一道防線 避免重複退款
            var refundTag = SystemTagEnum.BetRecord_ManualCancel;
            var queryExistTag = BetRecordTagMappingRepository.GetByUniqueKey(betRecordNoPGS, refundTag.ToInteger());

            if (queryExistTag != null)
            {

                SendMessageToMultipleChannel($"單號: {betRecordNoPGS} 已有標記退款。不可重複操作");
                return;
            }

            //Step 1. 更新玩家遊戲錢包
            PlayerWalletRepository.UpdateWalletAmountForbiddenMinus(
                playerID,
                gameID,
                amount,
                TransactionTypeEnum.BetRecord_SystemCancelRefundWallet,
                betRecordNoPGS,
                SystemConstant.CompanyIPAddress
            );

            //Step 2. 標記Tag
            BetRecordTagMappingRepository.CreateIfNotExist(betRecordNoPGS, refundTag);

            SendMessageToMultipleChannel($"單號: {betRecordNoPGS} 觸發自動退款exception情境成功。金額 {amount}");
            return;

        }


        public CRUDResultModel TestBonusSettleSingleWalletMerchant(int userID, ManualTestBonusSettleCallbackRequest model)
        {
            model.RuleFor(z => z.PlayerID).Rule_NotZeroNotMinus();

            //先檢查是test Merchant
            var queryPlayer = PlayerRepository.GetByIdOrThrowException(model.PlayerID);

            var queyryMerchant = MerchantRepository.GetByIdOrThrowException(queryPlayer.MerchantID);

            if (queyryMerchant.Type == (int)MerchantTypeEnum.Plaform_ProductionEnvironment)
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel("Merchant Type"));

            if (!CommonLogicHelper.IsSingleWalletMode(queyryMerchant.WalletMode))
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel("WalletMode"));

            var queryEnableGames = MerchantGameSettingRepository.GetByFilter(new QueryMerchantGameSettingRequestViewModel
            {
                EnableGameOnly = true,
                MerchantIDs = queyryMerchant.MerchantID.ToListCollection(),
                Status = NormalActiveStatus.Enable.ToInteger(),
                GameVendorIDs = new List<int>
                {
                    GameVendorTypeEnum.target.ToInteger(),
                    GameVendorTypeEnum.Jili.ToInteger(),
                }
            });

            if (queryEnableGames.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.NotFoundGame);

            var matchFirstGame = queryEnableGames
                .Where(x => x.GameID <= 1999 || x.GameID >= 250000) //排除2000系列 棋牌
                .OrderBy(x => x.GameID)
                .FirstOrDefault();

            int gameVendor = AppHelper.GetGameVendorByGameID(matchFirstGame.GameID);

            string pgsOrderNo = "Fake_Data_" + DateTimeHelper.GetCurrentTimeTimestamp();

            DateTime betTime = DateTimeHelper.GetCurrentTime();

            //不產生資料。直接觸發一個假的request
            SingleWalletPlatformApiRequestRefService.BonusSettlement(new SingleWalletSettleCallbackParameter
            {
                BetRecordNoPGS = pgsOrderNo,
                SettlementTime = betTime,
                BetAmount = 0,
                PayoutAmount = 10,
                GameID = matchFirstGame.GameID,
                MerchantID = queyryMerchant.MerchantID,
                PlayerID = queryPlayer.PlayerID,
                PlayerName = queryPlayer.PlayerName,
                TaxAmount = 0,
                ValidAmount = 0,
            });

            return new CRUDResultModel
            {
                Message = $"OrderNo:{pgsOrderNo} MerchantID:{queyryMerchant.MerchantID}"
            };
        }

        public void BatchWriteTagsByBetRecordNoPGS(int userID, BatchWriteTagsRequestModel model)
        {
            model.RuleFor(z => z.ListOfTransactionNoPGS);
            model.RuleFor(z => z.TagID).Rule_NotZeroNotMinus();

            List<string> listOfTransactionNoPGS = model.ListOfTransactionNoPGS;

            List<BetRecordTagMappingEntity> inserts = new List<BetRecordTagMappingEntity>();

            List<string> queryAlreadyExist = new List<string>();

            //排除已經重複的
            if (listOfTransactionNoPGS.IsNotEmptyNull() && listOfTransactionNoPGS.Count() <= 500)
            {
                queryAlreadyExist = BetRecordTagMappingRepository.GetByFilter(new QueryBetRecordTagMappingRequestViewModel
                {
                    TagIDs = model.TagID.ToListCollection(),
                    BetRecordNoPGSList = listOfTransactionNoPGS
                })
                .Select(z => z.BetRecordNoPGS)
                .ToList();
            }

            var now = DateTimeHelper.GetCurrentTime();
            inserts = listOfTransactionNoPGS
                .Where(x => !queryAlreadyExist.Contains(x))
                .Select(x => new BetRecordTagMappingEntity
                {
                    BetRecordNoPGS = x,
                    CreateTime = now,
                    TagID = model.TagID,
                })
                .ToList();

            BetRecordTagMappingRepository.BulkInsert(inserts);
        }


        /// <summary>
        /// 基本上是補救資料用的情境
        /// </summary>
        public void ManualWritePerGameLog(int userID, BetRecordManualWritePerGameRequestModel model)
        {
            model.RuleFor(z => z.TransactionNoPGS).Rule_NotEmptyText();

            string transactionNoPGS = model.TransactionNoPGS;

            var queryMapping = BetRecordTransactionNoMappingRepository.GetByTransactionNoPGS(transactionNoPGS);

            if (queryMapping == null)
                throw new TerproException(ApiErrorCode.NotFoundRecord);

            var queryGenralInfoByVendor = GetByPgsTransactionNo(queryMapping.GameVendorID, queryMapping.BetRecordNoPGS);

            if (queryGenralInfoByVendor == null)
                throw new TerproException(ApiErrorCode.NotFoundRecord, new ErrorDetailModel(queryMapping.BetRecordNoPGS));

            //補救資料的情境是 已結算 但perGameLog寫入失敗
            if (queryGenralInfoByVendor.Status != (int)BetRecordStatusEnum.SettleComplete)
                throw new TerproException(ApiErrorCode.InvalidStatus);

            var queryPerGame = PerGameDetailLogRepository.GetByTransactionNo_WithGameTimeRange(queryMapping.BetRecordNoPGS);

            if (queryPerGame != null)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist, new ErrorDetailModel(queryMapping.BetRecordNoPGS));
            }

            string ip = IPHelper.GetIP();
            string processID = Guid.NewGuid().ToString();

            var queryMerchant = MerchantRepository.GetSingleByCache(queryGenralInfoByVendor.MerchantID);

            bool isSingleWallet = CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode);
            bool isTransferMode = CommonLogicHelper.IsTransferMode(queryMerchant.WalletMode);


            // Step 1. 寫入一筆操作紀錄
            var log = InstanceOperateLog(userID, UserOperateTypeEnum.BetRecordManualWritePerGameLog, queryGenralInfoByVendor.Id);
            log.DebugInfo = $"{queryGenralInfoByVendor.ConvertToJsonString()}";
            log.Memo = model.Memo;

            WriteOperateLog(log);

            // step 2. 轉成perGameLog格式
            var vendorService = ServiceInstanceHelper.InstanceVendorService(queryMapping.GameVendorID);
            var convertPerGameResult = vendorService.FormatParsing_ConvertBetRecordToPerGameDetailLog(queryGenralInfoByVendor.BetRecordNoPGS);
            var convertPerGame = convertPerGameResult.PerGameDetailLog;

            //如果參數有指定要將 gameTime(對帳的依據) 取代成現在
            if (model.IsReplaceGameTimeToNowTime)
            {
                convertPerGame.GameTime = DateTimeHelper.GetCurrentTime();
            }

            // step 3. 寫入perGameLog
            convertPerGame.CurrentAmount = -1; //如果-1 讓後台不顯示金額

            if (isTransferMode)
            {
                var queryWallet = PlayerWalletRefService.GetWalletAmountByMerchantWalletMode(convertPerGame.PlayerID, "");
                convertPerGame.CurrentAmount = queryWallet.Amount + convertPerGame.BetAmount;
            }

            //若沒指定，自動寫入tagID
            if (!model.TagID.HasValue)
            {
                model.TagID = SystemTagEnum.PerGameDetailLog_ManualWritePerGame.ToInteger();
            }

            WritePerGameLog(convertPerGame, model.TagID);

            var queryEventPrize = BetRecordCustomizedSettingRepository.GetParseModelByUniqueKey(queryGenralInfoByVendor.BetRecordNoPGS, BetRecordCustomizedSettingEnum.ReferEventPrize);

            var referPrizeType = EventPrizeTypeEnum.Default;

            if (queryEventPrize != null)
            {
                referPrizeType = (EventPrizeTypeEnum)queryEventPrize.SettingValueOfInt;
            }

            var processConfig = new SettleBalanceUpdateProcessConfig
            {
                SessionToken = "",
                RequestUUID = Guid.NewGuid().ToString(),
                ReferEventPrizeType = referPrizeType,
                IsSkipUpdateBalance = false,
                MerchantWalletMode = queryMerchant.WalletMode,
                Wager_PayoutAmountWalletUpdate = convertPerGame.PayoutAmount
            };

            if (isSingleWallet)
            {

            }
            else
            {
                //轉帳模式可以指定不更新錢包
                processConfig.IsSkipUpdateBalance = !model.IsUpdatePayoutAmount;
            }

            // step 4. 更新錢包金額
            if (isSingleWallet)
            {
                Task.Factory.StartNew(() =>
                {
                    GameVendorSingleWalletCommonLogicProcessService.UpdateWalletAmountSettleProcess((GameVendorTypeEnum)queryMapping.GameVendorID, processConfig, convertPerGameResult);
                });
            }
            else
            {
                GameVendorSingleWalletCommonLogicProcessService.UpdateWalletAmountSettleProcess((GameVendorTypeEnum)queryMapping.GameVendorID, processConfig, convertPerGameResult);
            }

            return;
        }


        public PagedList<BetRecordNotifyQueueViewModel> GetNotifyQueueList(int userID, QueryBetRecordMerchantNotifyQueueRequestParameter filter)
        {
            filter.Types = filter.Type.ToIntegerListCollection();

            var query = BetRecordMerchantNotifyQueueRepository.GetPagingByFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<BetRecordMerchantNotifyQueueEntity, BetRecordNotifyQueueViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = ConvertToNotifyQueueViewModels(x);
                    return convert;
                })
                .ToList();

            SetGetNotifyQueueExtraProperty(viewModels.Items);
            return viewModels;
        }


        public void ResetNotifyQueue(int userID, long id)
        {
            var queryExist = BetRecordMerchantNotifyQueueRepository.GetById(id);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            queryExist.RetryTimes = 0;
            queryExist.IsSuccess = false;
            queryExist.NextNotifyTime = DateTimeHelper.GetDefaultTime_2021();

            BetRecordMerchantNotifyQueueRepository.Update(queryExist);
            return;
        }


        public BetRecordNotifyQueueDetailInfoViewModel GetNotifyQueueInfo(int userID, long queueID)
        {
            BetRecordMerchantNotifyQueueEntity query = BetRecordMerchantNotifyQueueRepository.GetById(queueID);

            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            CheckViewMerchantPermission(userID, query.MerchantID);

            //會有多筆一樣的
            var queryLogs = SingleWalletMerchantApiRequestLogRepository.GetByFilter(new QuerySingleWalletMerchantApiRequestLogRequestViewModel
            {
                CreateTimeBegin = query.CreateTime.AddHours(-2),
                CreateTimeEnd = query.CreateTime.AddHours(36),
                ReferenceTransactionNo = query.BetRecordNoPGS,
            })
                //.Where(x => x.RequestID == query.LastRequestUUID)
                .ToList();

            var viewModel = query.ReturnInjecteObject<BetRecordNotifyQueueDetailInfoViewModel>();
            viewModel.TypeName = ((BetRecordMerchantNotifyQueueTypeEnum)query.Type).ToString();
            viewModel.Logs = queryLogs.OrderByDescending(z => z.CreateTime).ToList();

            return viewModel;
        }

        public void RecoverMultipleNotifyQueueData(int userID, List<string> betRecordNoPGS)
        {
            if (betRecordNoPGS.IsEmptyOrNull())
                return;

            foreach (var tranNo in betRecordNoPGS)
            {
                try
                {
                    RecoverNotifyQueueData(userID, tranNo);
                }
                catch (Exception ex)
                {
                    MyTelegramUtility.SendMessage(new TelegramMessageParameter
                    {
                        Message = $"單號:{tranNo} , {ex.GetMessageAndErrorLine()}"
                    });

                    EazyTempValueRepository.Create(new Models.Table.EazyTempValueEntity
                    {
                        BetRecordNoPGS = tranNo,
                        Type1 = 65511,
                        Memo1 = ex.GetMessageAndErrorLine().Truncate(200),
                    });
                }
            }
        }

        public void RecoverNotifyQueueData(int userID, string betRecordNoPGS)
        {
            var queryByMapping = BetRecordTransactionNoMappingRepository.GetByTransactionNoPGS(betRecordNoPGS);

            BetRecordGeneralColumnModel queryRecord = null;

            if (queryByMapping != null)
            {
                var service = ServiceInstanceHelper.InstanceVendorService(queryByMapping.GameVendorID);
                queryRecord = service.GetByPGSTransactionNo(queryByMapping.BetRecordNoPGS);
            }

            //如果沒有建資料 有可能在tempEazyValue找到暫存資料
            if (queryRecord == null)
            {
                int targetType_001 = EazyTempValueTypeEnum.SingleWallet_PlaceBet_CallbackProceeError.ToInteger();
                var queryTempValue = EazyTempValueRepository.GetByFilter(new QueryEazyTempValueRequestViewModel
                {
                    BetRecordNoPGS = betRecordNoPGS,
                    Types = new List<int>
                        {
                            targetType_001,
                            EazyTempValueTypeEnum.PlaceBetUnknowException.ToInteger()
                        }
                })
                    .OrderBy(x => x.Type1 == targetType_001 ? 0 : 999)
                    .FirstOrDefault();

                if (queryTempValue != null)
                {
                    queryRecord = new BetRecordGeneralColumnModel
                    {
                        BetRecordNoPGS = betRecordNoPGS,
                        GameID = queryTempValue.GameID ?? 0,
                        BetAmount = queryTempValue.Amount1 ?? 0,
                        PlayerID = queryTempValue.PlayerID ?? 0,
                        MerchantID = queryTempValue.MerchantID ?? 0,
                        Status = BetRecordStatusEnum.Cancel.ToInteger(),
                    };

                    if (queryRecord.MerchantID == 0 && queryRecord.PlayerID > 0)
                    {
                        var queryPlayer = PlayerRepository.GetById(queryRecord.PlayerID);

                        if (queryPlayer != null)
                        {
                            queryRecord.MerchantID = queryPlayer.MerchantID;
                        }
                    }
                }
            }

            if (queryRecord == null)
            {
                if (SettingHelper.IsLocalhostWebSiteName)
                {
                    string msg = $"查無單號 {betRecordNoPGS}, 無法進行 RecoverNotifyQueueData";
                    SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_adminSys_api);
                }
                else
                {
                    throw new TerproException(ApiErrorCode.NotFoundBetRecord, new ErrorDetailModel(betRecordNoPGS));
                }
            }

            //如果查無資料就無法繼續進行
            queryRecord.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();
            queryRecord.RuleFor(z => z.MerchantID).Rule_NotZeroNotMinus();

            var queryMerchant = MerchantRepository.GetById(queryRecord.MerchantID);

            if (!AppHelper.IsSingleWalletMode(queryMerchant.WalletMode))
            {
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel("WalletMode"));
            }

            var tartgetQueueType = BetRecordMerchantNotifyQueueTypeEnum.Cancel;

            if (queryRecord.Status == BetRecordStatusEnum.Cancel.ToInteger())
            {
                tartgetQueueType = BetRecordMerchantNotifyQueueTypeEnum.Cancel;
            }
            else if (queryRecord.Status == BetRecordStatusEnum.SettleComplete.ToInteger())
            {
                var gameVendor = AppHelper.GetGameVendorByGameID(queryRecord.GameID);

                if (GameVendorExtraLogicHelper.IsExecuteBonusSettleProcess(gameVendor, queryRecord.BetAmount))
                {
                    tartgetQueueType = BetRecordMerchantNotifyQueueTypeEnum.BonusSettle;
                }
                else
                {
                    tartgetQueueType = BetRecordMerchantNotifyQueueTypeEnum.Settle;
                }
            }
            else
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, new ErrorDetailModel($"{betRecordNoPGS} 狀態:{queryRecord?.Status} 非允許的注單狀態 無法繼續"));
            }

            var queryExist = BetRecordMerchantNotifyQueueRepository.GetByFilter(new QueryBetRecordMerchantNotifyQueueRequestViewModel
            {
                BetRecordNoPGS = queryRecord.BetRecordNoPGS,
                Types = tartgetQueueType.ToInteger().ToListCollection()
            })
                .FirstOrDefault();

            if (queryExist == null)
            {
                BetRecordMerchantNotifyQueueRepository.Create(new BetRecordMerchantNotifyQueueEntity
                {
                    BetRecordNoPGS = queryRecord.BetRecordNoPGS,
                    GameID = queryRecord.GameID,
                    MerchantID = queryRecord.MerchantID,
                    PlayerID = queryRecord.PlayerID,
                    RetryTimes = 1,
                    Type = tartgetQueueType.ToInteger(),
                    IsSuccess = false,
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    LastRequestUUID = Guid.NewGuid().ToString(),
                    NextNotifyTime = DateTimeHelper.GetCurrentTime().AddHours(-1),
                });
            }
            else
            {
                queryExist.IsSuccess = false;
                queryExist.RetryTimes = 0;
                queryExist.NextNotifyTime = DateTimeHelper.GetCurrentTime().AddHours(-1);

                BetRecordMerchantNotifyQueueRepository.Update(queryExist);
            }

            var log = InstanceOperateLog(userID, UserOperateTypeEnum.BetRecordRecoverNotifyQueue, 0, queryRecord.BetRecordNoPGS);
            log.Memo = queryRecord.BetRecordNoPGS;

            WriteOperateLog(log);
            return;
        }


        private BetRecordGeneralInfoViewModel ConvertToBetRecordViewModel(BetRecordGeneralColumnViewModel generalInfo, PerGameDetailLogEntity queryPerGame)
        {
            if (generalInfo == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecordOrDeleteAlready);

            var queryGame = GameListRepository.GetSingleByCache(generalInfo.GameID);

            generalInfo.GameName = DbMultiLanguageTextHelper.GetGameDisplayName(generalInfo.GameID);
            generalInfo.MerchantName = MerchantRepository.GetSingleByCache(generalInfo.MerchantID)?.Name;
            generalInfo.ThirdPartyGameCode = queryGame.ThirdPartyGameCode;

            var queryMerchant = MerchantRepository.GetSingleByCache(generalInfo.MerchantID);

            if (generalInfo.ManualSettleUserID.HasValue)
            {
                generalInfo.IsManualSettlement = true;
                generalInfo.ManualSettlementUserName = UserRepository.GetSingleByCache(generalInfo.ManualSettleUserID.Value)?.UserName;
            }


            if (generalInfo.ThirdSettleTime.HasValue)
            {
                //ThirdSettleDate
            }


            var gameVendorSupport = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(queryGame.GameVendorType);

            var viewModel = new BetRecordGeneralInfoViewModel
            {
                IsSingleWalletMode = CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode),
                GameVendorID = queryGame.GameVendorType,
                GameVendorName = ((GameVendorTypeEnum)queryGame.GameVendorType).ToString(),
                GeneralInfo = generalInfo,
                IsSupportExternalLink = gameVendorSupport.IsSupportExternalLinkScreenShot,
                IsSupportExperimentFeatureForInternalUsers = gameVendorSupport.IsSupportExperimentFeatureForInternalUsers,
                Currency = queryMerchant.Currency,
                VendorConfigID = generalInfo.VendorConfigID ?? 0,
            };

            bool isFinanceIgnore = false;

            if (viewModel.VendorConfigID > 0)
            {
                var asstignID = VendorConfigSettingRepository.GetAssignVendorIdByVendorConfigID(viewModel.VendorConfigID);
                viewModel.VendorConfigAssignMerchantID = asstignID?.Value;
            }

            if (queryPerGame != null)
            {
                var queryTags = PerGameDetailLogTagMappingRepository.GetByFilter(new QueryPerGameDetailLogTagMappingRequestViewModel
                {
                    PerGameDetailLogIDs = queryPerGame.Id.ToListCollection(),
                    GameTimeBegin = queryPerGame.GameTime.AddHours(-1),
                    GameTimeEnd = queryPerGame.GameTime.AddHours(1),
                });

                isFinanceIgnore = queryTags.Any(z => z.TagID == (int)SystemTagEnum.BetRecord_AccountingIgnore);

                var queryExtra = BetRecordCustomizedSettingRepository.GetParseModelByUniqueKey(queryPerGame.TransactionNo, BetRecordCustomizedSettingEnum.PgsGame_PerGameLogID);
                var queryReferPrize = BetRecordCustomizedSettingRepository.GetParseModelByUniqueKey(queryPerGame.TransactionNo, BetRecordCustomizedSettingEnum.ReferEventPrize);

                viewModel.PerGameLogInfo = new PerGameDetailInfo
                {
                    PerGameLogID = queryExtra?.SettingValueOfBigInt ?? 0,
                    PerGameDetailLogID = queryPerGame.Id,
                    ServerID = queryPerGame.ServerID,
                    Tags = queryTags
                        .Select(z => MultiLanguageTextHelper.GetEnumsCultureText((SystemTagEnum)z.TagID))
                        .ToList()
                };

                if (queryReferPrize != null)
                {
                    viewModel.ExtraReferPrizeType = queryReferPrize.SettingValueOfInt;
                    viewModel.ExtraInfoReferPrizeType = MultiLanguageTextHelper.GetEnums_i18n_FieldName((EventPrizeTypeEnum)queryReferPrize.SettingValueOfInt);
                }
            }

            var queryExtraInfo = BetRecordExtraInfoRepository.GetByBetRecordNoPGS(generalInfo.BetRecordNoPGS);
            viewModel.ExtraInfoText = queryExtraInfo?.ExtraInfo;

            var queryBetRecordTags = BetRecordTagMappingRepository.GetByFilter(new QueryBetRecordTagMappingRequestViewModel
            {
                BetRecordNoPGSList = new List<string> { generalInfo.BetRecordNoPGS }
            });

            if (queryBetRecordTags.IsNotEmptyNull())
            {
                viewModel.TagMapping = new List<EazyKeyAndTextModel>();

                foreach (var item in queryBetRecordTags)
                {
                    viewModel.TagMapping.Add(new EazyKeyAndTextModel
                    {
                        Key = item.TagID,
                        Text = MultiLanguageTextHelper.GetEnumsCultureText((SystemTagEnum)item.TagID)
                    });
                }

                if(!isFinanceIgnore)
                {
                    isFinanceIgnore = queryBetRecordTags.Any(z => z.TagID == (int)SystemTagEnum.BetRecord_AccountingIgnore);                    
                }

                // tags 是顯示在畫面下方的 額外資訊區塊
                // tags 是顯示在畫面下方的 額外資訊區塊
                if (viewModel.PerGameLogInfo != null)
                {
                    viewModel.PerGameLogInfo.Tags = viewModel.PerGameLogInfo.Tags ?? new List<string>();
                    viewModel.PerGameLogInfo.Tags.AddRange(viewModel.TagMapping.Select(x => x.Text).ToList());

                    if(isFinanceIgnore)
                    {
                        var queryPrize = EventPrizeRecordRepository.GetByFilter(new QueryEventPrizeRecordRequestViewModel
                        {
                            CreateTimeBegin = queryPerGame.GameTime.AddHours(-1),
                            CreateTimeEnd = queryPerGame.GameTime.AddHours(1),
                            ReferenceNo = queryPerGame.TransactionNo,
                        })
                        .FirstOrDefault();

                        if (queryPrize != null)
                        {
                            string prizeType = MultiLanguageTextHelper.GetEnumsCultureText((EventPrizeTypeEnum)queryPrize.EventPrizeType);
                            viewModel.PerGameLogInfo.Tags.Add($"(*Event){prizeType}");
                        }
                    }

                    viewModel.PerGameLogInfo.Tags = viewModel.PerGameLogInfo.Tags
                        .Distinct()
                        .ToList();
                }

            }

            return viewModel;
        }


        private void SetExtraProperty(List<UnionBetRecordViewModel> models, bool isBriefQuery = false)
        {
            if (models.IsEmptyOrNull())
                return;

            List<string> transactionNoList = models.Select(z => z.BetRecordNoPGS).ToList();

            var queryQueue = new List<BetRecordMerchantNotifyQueueEntity>();
            var queryTags = new List<BetRecordTagMappingEntity>();
            var queryRawTags = new List<BetRecordRawWagerTagMappingEntity>();
            var queryperGameTags = new List<PerGameDetailLogTagMappingEntity>();

            //如果匯出excel的情境。部分詳細資訊不會查詢
            if (!isBriefQuery)
            {
                DateTime minTime = DateTimeHelper.GetCurrentTime();

                var myDatetimeList = models.Where(x => x.SettlementTime.HasValue).Select(x => x.SettlementTime.Value);

                if (myDatetimeList.IsNotEmptyNull())
                {
                    minTime = myDatetimeList.Min();
                }

                queryQueue = BetRecordMerchantNotifyQueueRepository.GetByFilter(new QueryBetRecordMerchantNotifyQueueRequestViewModel
                {
                    ListOfBetRecordNoPGS = transactionNoList
                });

                queryTags = BetRecordTagMappingRepository.GetByFilter(new QueryBetRecordTagMappingRequestViewModel
                {
                    BetRecordNoPGSList = transactionNoList,
                });

                queryRawTags = BetRecordRawWagerTagMappingRepository.GetByFilter(new QueryBetRecordRawWagerTagMappingRequestViewModel
                {
                    BetRecordNoList = models.Select(z => z.BetRecordNo).ToList()
                });

                queryperGameTags = PerGameDetailLogTagMappingRepository.GetByFilter(new QueryPerGameDetailLogTagMappingRequestViewModel
                {
                    GameTimeBegin = minTime.AddDays(-1),
                    TagIDs = SystemTagEnum.PerGameDetailLog_ManualWritePerGame.ToInteger().ToListCollection(),
                    ListOfTransactionNo = transactionNoList
                });
            }

            var queryMerchants = DbValueCacheHelper.GetMerchants(models.Select(z => z.MerchantID));

            foreach (var item in models)
            {
                if (item.Tags == null)
                {
                    item.Tags = new List<string>();
                }

                var matchTags = queryTags.Where(x => x.BetRecordNoPGS == item.BetRecordNoPGS).ToList();

                if (matchTags.IsNotEmptyNull())
                {
                    item.TagDetails = new List<EazyKeyAndTextModel>();

                    foreach (var eachTag in matchTags)
                    {
                        item.TagDetails.Add(new EazyKeyAndTextModel
                        {
                            Key = eachTag.TagID,
                            Text = MultiLanguageTextHelper.GetEnumsCultureText((SystemTagEnum)eachTag.TagID)
                        });
                    }

                    item.Tags = item.TagDetails.Select(x => x.Text).ToList();
                }

                var matchQueue = queryQueue.FirstOrDefault(z => z.BetRecordNoPGS == item.BetRecordNoPGS);

                if (matchQueue != null)
                {
                    item.MerchantNotifyQueueID = matchQueue.Id;
                }

                var matchMerchant = queryMerchants.FirstOrDefault(x => x.MerchantID == item.MerchantID);

                if (matchMerchant != null)
                {
                    item.IsSingleWallet = CommonLogicHelper.IsSingleWalletMode(matchMerchant.WalletMode);
                    item.Currency = matchMerchant.Currency;
                }

                var matchPerGameTags = queryperGameTags
                    .Where(x => x.TransactionNo == item.BetRecordNoPGS && x.TagID == (int)SystemTagEnum.PerGameDetailLog_ManualWritePerGame)
                    .FirstOrDefault();

                if (matchPerGameTags != null)
                {
                    item.Tags.Add("手動補上遊戲歷程");
                }

                var matchCancelRawTags = queryRawTags
                    .Where(x => x.BetRecordNo == item.BetRecordNo && x.TagID == (int)SystemTagEnum.BetRecord_Cancel_ButNotFound)
                    .FirstOrDefault();

                if (matchCancelRawTags != null)
                {
                    item.Tags.Add("三方已Cancel。但查無注單");
                }

                if (!isBriefQuery)
                {
                    //item.DebugSQL = GenerateApiRequestLogSQL(item.CreateTime, item.GameVendor, item.BetRecordNo).DebugSQL;
                }
            }
        }

        private void SetGetNotifyQueueExtraProperty(List<BetRecordNotifyQueueViewModel> models)
        {
            if (models.IsEmptyOrNull())
                return;

            List<int> merchantIDs = models.Select(x => x.MerchantID).ToList();

            var queryMerchants = DbValueCacheHelper.GetMerchants(merchantIDs);

            foreach (var item in models)
            {
                item.MerchantName = queryMerchants.FirstOrDefault(z => z.MerchantID == item.MerchantID)?.Name;
            }

            return;
        }


        private (string DebugSQL, string LogSQL, string ErrorLogSQL, string ExtraErrorSQL) GenerateApiRequestLogSQL(BetRecordGeneralColumnViewModel queryWager, int gameVendor, string betRecordNo, int minutesRange = 9)
        {
            var createTime = queryWager.CreateTime;

            string dateText = createTime.ToString("yyyy-MM-dd HH:mm:ss");

            int extraMinutesRange = minutesRange;

            if (SettingHelper.PromoteBrandTypeEnum == PromoteBrandTypeEnum.BWG)
            {
                extraMinutesRange = minutesRange + 30;
            }

            string whereQuery = $" WHERE CreateTime BETWEEN DATEADD(MINUTE , -{minutesRange} , '{dateText}') AND DATEADD(MINUTE , {minutesRange} , '{dateText}') ";
            string whereQueryError = $" WHERE CreateTime BETWEEN DATEADD(MINUTE , -{minutesRange} , '{dateText}') AND DATEADD(MINUTE , {(extraMinutesRange)} , '{dateText}') ";

            string sql1 = string.Empty;
            string sql2 = string.Empty;
            string sql3 = string.Empty;

            //技術人員debug語法
            if (gameVendor == (int)GameVendorTypeEnum.target)
            {
                whereQuery += $" AND RequestBody like '%{betRecordNo}%' ";
                sql1 = GenerateSQLByTableName("ApiRequestLogGameAPI") + Environment.NewLine + whereQuery;
                //sql2 = GenerateSQLByTableName("ApiRequestLogGameAPIError") + Environment.NewLine + whereQuery;
            }
            else
            {
                sql1 = GenerateSQLByTableName("ApiRequestLogExternalVendorAPI") + Environment.NewLine + whereQuery + $"  AND ReferOrderNo = '{betRecordNo}' ";
                //sql2 = GenerateSQLByTableName("ApiRequestLogExternalVendorAPIError") + Environment.NewLine + whereQueryError + $" AND RequestBody like '%{betRecordNo}%' ";


                //if(queryWager.SettlementTime.HasValue)
                //{
                //    if((queryWager.SettlementTime.Value.Date == queryWager.CreateTime.Date))
                //    {
                //        //sql3
                //    }
                //    else
                //    {
                //        queryWager.SettlementTime.HasValue
                //        string whereQueryExtraError = $" WHERE CreateTime BETWEEN DATEADD(MINUTE , -{minutesRange} , '{dateText}') AND DATEADD(MINUTE , {(extraMinutesRange)} , '{dateText}') ";


                //        //
                //    }
                //}



            }

            string concateSQL = sql1 + Environment.NewLine + Environment.NewLine +
                                 sql2 + Environment.NewLine;

            return (concateSQL, sql1, sql2, sql3);
        }

        private string GenerateSQLByTableName(string tableName)
        {
            return string.Format("SELECT TOP 25 * FROM [PGS_SystemLog].[dbo].[{0}] WITH (NOLOCK)", tableName);
        }


        private BetRecordGeneralColumnModel GetByPgsTransactionNo(int gameVendorID, string pgsTransactionNo)
        {
            var service = ServiceInstanceHelper.InstanceVendorService(gameVendorID);

            var queryByVendor = service.GetByPGSTransactionNo(pgsTransactionNo);

            if (queryByVendor == null)
                throw new TerproException(ApiErrorCode.NotFoundOrAlreadyDelete);

            queryByVendor.StatusText = MultiLanguageTextHelper.GetEnumsCultureText((BetRecordStatusEnum)queryByVendor.Status);

            if (string.IsNullOrWhiteSpace(queryByVendor.ThirdPartyGameCode))
            {
                var queryGame = GameListRepository.GetSingleByCache(queryByVendor.GameID);
                queryByVendor.ThirdPartyGameCode = queryGame.ThirdPartyGameCode;
            }

            return queryByVendor;
        }

        private void ManualDeletePerGame(string transactionNo, int merchantID, int playerID, string playerName)
        {
            var queryPerGame = PerGameDetailLogRepository.GetByTransactionNo_WithGameTimeRange(transactionNo);

            if (queryPerGame == null)
            {
                throw new TerproException(ApiErrorCode.NotFoundBetRecord, new ErrorDetailModel(transactionNo));
            }

            var queryExistTags = PerGameDetailLogTagMappingRepository.GetByFilter(new QueryPerGameDetailLogTagMappingRequestViewModel
            {
                GameTimeBegin = queryPerGame.GameTime.AddMinutes(-1),
                GameTimeEnd = queryPerGame.GameTime.AddMinutes(1),
                PerGameDetailLogIDs = queryPerGame.Id.ToListCollection(),
                TagIDs = SystemTagEnum.PerGameDetailLog_ReplaceAndDeleteData.ToInteger().ToListCollection(),
            });

            //MSTest有可能跑到同一筆
            if (queryExistTags.IsNotEmptyNull())
                return;

            var param = new PerGameDeleteProcess_UpdateMerchantPlayerParameter
            {
                Id = queryPerGame.Id,
                GameTime = queryPerGame.GameTime,
                NewPlayerID = playerID,
                NewMerchantID = merchantID,
                NewPlayerName = playerName,
            };

            PerGameDetailLogRepository.UpdateAndReplaceMerchantIDPlayerID(param);

            PerGameDetailLogMerchantJoinRepository.UpdateAndReplaceMerchantID(queryPerGame.MerchantID, param);

            var insertTag = PerGameDetailLogTagMappingRepository.InstanceInsertEntity(queryPerGame, SystemTagEnum.PerGameDetailLog_ReplaceAndDeleteData);

            PerGameDetailLogTagMappingRepository.Create(insertTag);
        }


        private UnionBetRecordViewModel ConvertToViewModels(UnionBetRecordQueryModel query)
        {
            var convert = query.ReturnInjecteObject<UnionBetRecordViewModel>();
            convert.GameName = DbMultiLanguageTextHelper.GetGameDisplayName(query.GameID);
            convert.GameVendorName = MultiLanguageTextHelper.GetGameVendorDisplayName(query.GameVendor);
            convert.StatusText = MultiLanguageTextHelper.GetEnumsCultureText((BetRecordStatusEnum)query.Status);
            convert.MerchantName = MerchantRepository.GetSingleByCache(query.MerchantID)?.Name;
            convert.TypeName = MultiLanguageTextHelper.GetEnumsCultureText((BetRecordTypeEnum)query.Type);
            return convert;
        }

        private BetRecordNotifyQueueViewModel ConvertToNotifyQueueViewModels(BetRecordMerchantNotifyQueueEntity query)
        {
            var convert = query.ReturnInjecteObject<BetRecordNotifyQueueViewModel>();
            convert.TypeText = MultiLanguageTextHelper.GetEnumsCultureText((BetRecordMerchantNotifyQueueTypeEnum)convert.Type);



            //convert.GameName = DbMultiLanguageTextHelper.GetGameDisplayName(query.GameID);
            //convert.GameVendorName = MultiLanguageTextHelper.GetGameVendorDisplayName(query.GameVendor);
            //convert.StatusText = MultiLanguageTextHelper.GetEnumsCultureText((BetRecordStatusEnum)query.Status);
            //convert.MerchantName = MerchantRepository.GetSingleByCache(query.MerchantID)?.Name;
            //convert.TypeName = MultiLanguageTextHelper.GetEnumsCultureText((BetRecordTypeEnum)query.Type);


            return convert;
        }



        private void CheckExtraTableAndThrowMessage(QueryUnionBetRecordRequestParameterModel filter)
        {
            if (string.IsNullOrWhiteSpace(filter.TransactionNoPGS))
                return;

            var queryNotify = BetRecordMerchantNotifyQueueRepository.GetByFilter(new QueryBetRecordMerchantNotifyQueueRequestViewModel
            {
                BetRecordNoPGS = filter.TransactionNoPGS
            })
            .FirstOrDefault();

            string msg = "輸入pgs單號卻查無資料 的可能原因: 若是單錢包商戶。則可能因扣款失敗、整個流程跳出。沒有注單資料是正常現象。";

            if (queryNotify != null)
            {
                string extraMemo = ((BetRecordMerchantNotifyQueueTypeEnum)queryNotify.Type).ToString();

                msg = $"輸入pgs單號卻查無資料。但是有對應的 SW NotifyQueue，{extraMemo} {queryNotify.MerchantID}。可能已取消注單";
            }
            else
            {

                var queryTemp = EazyTempValueRepository.GetByTransactionNo(filter.TransactionNoPGS);

                //var queryTag = BetRecordTagMappingRepository.GetByUniqueKey(filter.TransactionNoPGS, (int)SystemTagEnum.BetRecord_PlaceBetFail_DueToMerchantApiFail);
                //if (queryTag != null)
                //{
                //    msg = $"輸入pgs單號卻查無資料。但是有對應的 3392 Tag (Placebet callback 失敗)。應已取消注單";
                //}

                if (queryTemp != null)
                {
                    var typeText = ((EazyTempValueTypeEnum)(queryTemp.Type1 ?? 0)).ToString();
                    msg = $"輸入pgs單號卻查無資料。但是有對應的 EazyTempValue 資料，{typeText} {queryTemp.MerchantID}。可能已取消注單";
                }
            }

            throw new TerproException(ApiErrorCode.BusinessLogicDeny, msg);
        }


        private long WritePerGameLog(PerGameDetailLogEntity convertPerGame, int? tagID = null)
        {
            // step 1. 寫入perGame
            long perGameID = PerGameLogRepository.Create(new PerGameLogEntity
            {
                BeginTime = convertPerGame.GameTime,
                EndTime = convertPerGame.GameTime,
                GameData = "",
                ServerID = convertPerGame.ServerID,
                GameID = convertPerGame.GameID,
                TaxAmount = 0,
                CreateTime = DateTimeHelper.GetCurrentTime()
            });

            // step 2. 寫入perGameDetail
            long resultPerGameDetailID = PerGameDetailLogRepository.CreateTransactionScope(convertPerGame);
            convertPerGame.Id = resultPerGameDetailID;

            // step 3. 如果有指定要標記tag / 或系統判斷需要標記tag
            List<SystemTagEnum> tags = CommonLogicHelper.CheckAnyTagToAdd(convertPerGame);

            if (tagID.HasValue)
            {
                tags.Add((SystemTagEnum)tagID.Value);
            }

            if (tags.IsNotEmptyNull())
            {
                tags = tags.Distinct().ToList();
                var insertItems = tags.Select(z =>
                {
                    var insertTag = PerGameDetailLogTagMappingRepository.InstanceInsertEntity(convertPerGame, z);
                    return insertTag;
                })
                .ToList();

                PerGameDetailLogTagMappingRepository.CreateMany(insertItems);
            }

            return resultPerGameDetailID;
        }

    }

}
