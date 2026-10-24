using DocumentFormat.OpenXml.Presentation;
using MongoDB.Driver.Core.Misc;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Helper.Elasticsearch.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace target.BL.Services
{
    public class SingleWalletMerchantApiRequestLogService : BaseAdminSysService, ISingleWalletMerchantApiRequestLogService
    {

        public SingleWalletMerchantApiRequestLogRepository SingleWalletMerchantApiRequestLogRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }


        public PagedList<ExternalApiRequestLogViewModel> GetByPagingFilter(int userID, QuerySingleWalletMerchantApiRequestLogApiRequestParameterModel filter)
        {
            filter.RuleFor(z => z.CreateTimeBegin).Rule_NotNullDateTime();

            if (filter.PlayerID.NotNullNotZeroNotMinus())
            {
                var queryPlayer = PlayerRepository.GetSingleByCache(filter.PlayerID.Value);

                if (queryPlayer == null)
                    throw new TerproException(ApiErrorCode.NotFoundPlayer);

                filter.MerchantID = queryPlayer.MerchantID;
            }

            if (SettingHelper.IsProduction)
            {
                if ((filter.CreateTimeEnd.Value - filter.CreateTimeBegin.Value).TotalMinutes > 90)
                {
                    if(string.IsNullOrWhiteSpace(filter.UniqueID))
                    {
                        filter.RuleFor(z => z.MerchantID).Rule_IsNotNullNotZeroNotMinus();
                    }
                }
            }

            if(filter.ReferenceTransactionNo.IsNotEmptyTextNotNull())
            {
                filter.ReferenceTransactionNo = filter.ReferenceTransactionNo.Trim();

                if(!filter.MerchantID.HasValue && RegexRuleHelper.IsPgs32TransactionNo(filter.ReferenceTransactionNo))
                {
                    var queryPerGame = PerGameDetailLogRepository.GetByTransactionNo_WithGameTimeRange(filter.ReferenceTransactionNo);

                    if(queryPerGame != null)
                    {
                        filter.MerchantID = queryPerGame.MerchantID;
                    }
                }
            }

            if(filter.MerchantID.NotNullNotZeroNotMinus())
            {
                var queryMerchant = MerchantRepository.GetById(filter.MerchantID.Value);

                if(!AppHelper.IsSingleWalletMode(queryMerchant.WalletMode))
                {
                    throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel($"{filter.MerchantID} not sw mode"));
                }
            }

            var ESBasicConfig = new ESKeywordSearchBasicParameter
            {
                Begin = filter.CreateTimeBegin.Value,
                End = filter.CreateTimeEnd.Value,
                IndexName = TableNameConvertHelper.GetElasticsearchIndexName(ElasticLogTableTypeEnum.SinglewalletMerchantApiRequestLog),
                PageSize = filter.PageSize.ReturnDefaultValueIfZero(filter.TopSize ?? 200),
                ExcludeColumns = new List<string> { "RequestContent", "RequestHeader", "ResponseText" },
                AdvanceParameter = new ESKeywordAdvanceFilterParameter
                {
                    AlwaysFilter = new List<ESKeywordSearchParameter>()
                }
            };

            //特殊篩選
            if (filter.SpecialFilterType.HasValue)
            {
                var targetEnum = (SpecialFilterTypeOfSwApiRequestLog)filter.SpecialFilterType.Value;

                switch (targetEnum)
                {
                    case SpecialFilterTypeOfSwApiRequestLog.RequestTime_LargeThan_3_Seconds:
                        filter.ResponseTimeLargeThan = 1000 * 3;

                        ESBasicConfig.AdvanceParameter.AlwaysFilter.Add(new ESKeywordSearchParameter
                        {
                            ColumnName = "ResponseTime",
                            gte = filter.ResponseTimeLargeThan,
                            lte = filter.ResponseTimeLargeThan * 10,
                        });

                        break;

                    case SpecialFilterTypeOfSwApiRequestLog.RequestTime_LargeThan_10_Seconds:
                        filter.ResponseTimeLargeThan = 1000 * 10;

                        ESBasicConfig.AdvanceParameter.AlwaysFilter.Add(new ESKeywordSearchParameter
                        {
                            ColumnName = "ResponseTime",
                            gte = filter.ResponseTimeLargeThan,
                            lte = filter.ResponseTimeLargeThan * 10,
                        });

                        break;

                    case SpecialFilterTypeOfSwApiRequestLog.ErrorCode_Or_Not200Response:
                        filter.IsErrorCodeOrNot200Response = true;

                        var multipleCondition = new List<object>();
                        multipleCondition.Add(new
                        {
                            range = new
                            {
                                ErrorCode = new
                                {
                                    gte = 400000,
                                    lte = 599999,
                                }
                            }
                        });
                        multipleCondition.Add(new
                        {
                            range = new
                            {
                                ResponseStatusCode = new
                                {
                                    gte = 300,
                                    lte = 599,
                                }
                            }
                        });

                        ESBasicConfig.AdvanceParameter.ShouldQuery = multipleCondition;
                        break;
                }
            }

            if (!filter.CreateTimeEnd.HasValue)
            {
                filter.CreateTimeEnd = filter.CreateTimeBegin.Value.AddDays(1);
            }

            if (filter.PlayerName.IsNotEmptyTextNotNull())
            {
                if(filter.MerchantID.HasValue)
                {
                    var queryPlayer = PlayerRepository.GetByPlayerNameAndMerchantID(filter.PlayerName, filter.MerchantID.Value);

                    if (queryPlayer == null)
                        throw new TerproException(ApiErrorCode.NotFoundPlayer);

                    filter.PlayerID = queryPlayer.PlayerID;
                }
                else
                {
                    var recent_14 = DateTimeHelper.GetCurrentTime().AddDays(-1 * 14);

                    var queryPlayer = PlayerRepository.GetByPlayerName(filter.PlayerName)
                        .Where(x => x.LastLoginTime >= recent_14)
                        .ToList();

                    if (queryPlayer.IsEmptyOrNull())
                        throw new TerproException(ApiErrorCode.NotFoundPlayer);

                    if (queryPlayer.CountOrZero() >= 2)
                        throw new TerproException(ApiErrorCode.BusinessLogicDeny_MultiplePlayerNameMatch_PleaseUsePlayerID);

                    var matchPlayer = queryPlayer.FirstOrDefault();
                    filter.PlayerID = matchPlayer.PlayerID;
                    filter.MerchantID = matchPlayer.MerchantID;
                }                    
            }

            var cureentUser = GetCurrentOwnPermissionByCache(userID);

            PagedList<SingleWalletMerchantApiRequestLogQueryResult> query =  new PagedList<SingleWalletMerchantApiRequestLogQueryResult>
            {
                PageSize = filter.TopSize ?? 0,
            };

            var now = DateTimeHelper.GetCurrentTime();

            bool isReleaseES = now > new DateTime(2026, 5, 26);

            if (AppHelper.IsEnableSwLogMigrateToES())
            {
                if (!filter.MerchantID.IsNullOrZeroOrMinus())
                {
                    ESBasicConfig.AdvanceParameter.AlwaysFilter.Add(new ESKeywordSearchParameter
                    {
                        ColumnName = "MerchantID",
                        gte = filter.MerchantID,
                        lte = filter.MerchantID,
                    });
                }

                if (filter.ResponseStatusCode.HasValue)
                {
                    ESBasicConfig.AdvanceParameter.AlwaysFilter.Add(new ESKeywordSearchParameter
                    {
                        ColumnName = "ResponseStatusCode",
                        gte = filter.ResponseStatusCode.Value,
                        lte = filter.ResponseStatusCode.Value,
                    });
                }

                if (!string.IsNullOrWhiteSpace(filter.Category))
                {
                    string columnName = nameof(FullInfoSingleWalletCallbackRequestLogESModel.Category);
                    ESBasicConfig.AdvanceParameter.AlwaysFilter.Add(new ESKeywordSearchParameter(columnName, filter.Category));
                }

                if (!string.IsNullOrWhiteSpace(filter.SubCategory))
                {
                    string columnName = nameof(FullInfoSingleWalletCallbackRequestLogESModel.SubCategory);
                    ESBasicConfig.AdvanceParameter.AlwaysFilter.Add(new ESKeywordSearchParameter(columnName, filter.SubCategory));
                }

                List<ESKeywordSearchParameter> filterList = new List<ESKeywordSearchParameter>
                {
                    ElasticsearchHelper.InstanceKeywordSearchModelOrNull(nameof(FullInfoSingleWalletCallbackRequestLogESModel.UniqueID), filter.UniqueID),
                    ElasticsearchHelper.InstanceKeywordSearchModelOrNull(nameof(FullInfoSingleWalletCallbackRequestLogESModel.ReferOrderNo), filter.ReferenceTransactionNo),
                    ElasticsearchHelper.InstanceKeywordSearchModelOrNull(nameof(FullInfoApiRequestLogESModel.PlayerID), filter.PlayerID),
                    ElasticsearchHelper.InstanceKeywordSearchModelOrNull(nameof(FullInfoSingleWalletCallbackRequestLogESModel.RequestContent), filter.RequestContentKeyword),
                    ElasticsearchHelper.InstanceKeywordSearchModelOrNull(nameof(FullInfoSingleWalletCallbackRequestLogESModel.ResponseText), filter.ResponseTextKeyword),
                    ElasticsearchHelper.InstanceKeywordSearchModelOrNull(nameof(FullInfoSingleWalletCallbackRequestLogESModel.RequestUrlFull), filter.RequestUrlKeyword),
                };

                var ESItems = ElasticsearchHelper.KeywordSearchDataByFirstFilter<FullInfoSingleWalletCallbackRequestLogESModel>(ESBasicConfig, filterList);

                var convertItems = ESItems
                    .Select(x => DataModelConvertHelper.ConvertToSingleWalletLogQueryResult(x))
                    .ToList();

                query.Items = convertItems;
                query.TotalCount = convertItems.Count();
                query.TotalPage = 1;
            }
            else if (filter.TopSize.HasValue && filter.TopSize > 0)
            {
               

                filter.TimeoutSeconds = 120;
                filter.ExtraSpecificOrderBy = " ORDER BY brief.CreateTime desc ";

                if (SettingHelper.IsLocalhostWebSiteName)
                {
                    query.Items = SingleWalletMerchantApiRequestLogRepository.GetTopSizeByFilter_V2(filter);
                    query.TotalCount = query.Items.Count();
                    query.TotalPage = 1;
                }
                else
                {
                    query.Items = SingleWalletMerchantApiRequestLogRepository.GetTopSizeByFilter_V2(filter);
                    query.TotalCount = query.Items.Count();
                    query.TotalPage = 1;
                }
            }
            else
            {
                query = SingleWalletMerchantApiRequestLogRepository.GetPagingByFilter(filter);
            }

            var viewModels = ConvertToNewTypePagingObject<SingleWalletMerchantApiRequestLogQueryResult, ExternalApiRequestLogViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = ConvertToViewModel(x);
                    return convert;
                })
                .ToList();
            

            if(viewModels.Items.IsEmptyOrNull() && filter.PlayerID > 0)
            {
                var queryPlayer = PlayerRepository.GetById(filter.PlayerID.Value);
                throw new TerproException(ApiErrorCode.NotFoundData, new ErrorDetailModel($"玩家最後登入時間: {queryPlayer.LastLoginTime}"));
            }

            SetExtraProperty(viewModels.Items, filter);
            return viewModels;
        }



        private void SetExtraProperty(List<ExternalApiRequestLogViewModel> items, QuerySingleWalletMerchantApiRequestLogApiRequestParameterModel filter)
        {
            if (items.IsEmptyOrNull())
                return;

            foreach (var item in items)
            {
                if (item.ErrorCode > 300)
                {
                    item.IsNotSuccessCode = true;
                }

                if (string.IsNullOrWhiteSpace(item.RequestUrl))
                {
                    if (filter.MerchantID.HasValue)
                    {
                        item.RequestUrl = ("{Callback_URL}/" + item.SubCategory).ReplaceDuplicateUrlChar();
                    }
                    else
                    {
                        item.RequestUrl = ("{Callback_URL}/" + item.SubCategory).ReplaceDuplicateUrlChar();
                    }
                }
            }
        }


        public ExternalApiRequestLogViewModel GetSingleByUniqueID(int userID, QuerySingleWalletMerchantApiRequestLogApiRequestParameterModel filter)
        {
            filter.RuleFor(z => z.UniqueID).Rule_NotEmptyText();
            filter.RuleFor(z => z.CreateTimeBegin).Rule_NotNullDateTime();

            var compareTime = filter.CreateTimeBegin.Value;

            filter.CreateTimeBegin = compareTime.AddSeconds(-1);
            filter.CreateTimeEnd = compareTime.AddSeconds(1);

            SingleWalletMerchantApiRequestLogEntity query = null;

            if (TableNameConvertHelper.IsEnableElasticLogByTable(ElasticLogTableTypeEnum.SinglewalletMerchantApiRequestLog))
            {
                var ESquery = SingleWalletMerchantApiRequestLogRepository.GetFullInformationByUniqueID(filter.UniqueID);
                query = DataModelConvertHelper.ConvertToSingleWalletLogEntity(ESquery);
            }
            else
            {
                query = SingleWalletMerchantApiRequestLogRepository.GetByFilter(filter).FirstOrDefault();
            }

            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            var convert = ConvertToViewModel(query);
            convert.ResponseText = convert.ResponseText.Truncate(3000);

            return convert;
        }


        private ExternalApiRequestLogViewModel ConvertToViewModel(SingleWalletMerchantApiRequestLogEntity query)
        {
            //刻意轉成跟externalAPI一樣的物件
            var convert = query.ReturnInjecteObject<ExternalApiRequestLogViewModel>();
            convert.ResponseTime = query.ResponseTime;
            convert.DebugInfo3 = query.ReferenceTransactionNo;
            convert.RequestStartTime = query.CreateTime;
            convert.UniqueID = query.UniqueID;

            //如果請求時間太長。會扣除掉執行時間 (避免造成誤會)
            if (convert.ResponseTime > 500)
            {
                convert.RequestStartTime = convert.CreateTime.AddMilliseconds(-1 * convert.ResponseTime);
            }

            return convert;
        }



    }

}
