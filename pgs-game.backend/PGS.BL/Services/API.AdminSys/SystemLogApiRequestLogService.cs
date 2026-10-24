using target.BL.Interface;
using target.DA.Repository;
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
using System.Linq;

namespace target.BL.Services
{
    public class SystemLogApiRequestLogService : BaseAdminSysService, ISystemLogApiRequestLogService
    {

        public ApiRequestLogRepository ApiRequestLogRepository { get; set; }

        public ApiRequestLogV2024Repository ApiRequestLogV2024Repository { get; set; }

        public MappingTagValuesLargeRepository MappingTagValuesLargeRepository { get; set; }



        public PagedList<ExternalApiRequestLogViewModel> GetTopSizeList(int userID, QueryApiRequestLogRequestParameterModel filter)
        {
            filter.RuleFor(x => x.TargetTableName).Rule_NotEmptyText();

            //過渡期處理方案 (預計2026/05 可以移除?)
            filter.TargetTableName = TableNameReplace(filter.TargetTableName);

            bool isSearchES_thirdOrderNo = false;

            var ESIndexName = TableNameConvertHelper.GetElasticsearchIndexNameByTableName(filter.TargetTableName);

            if (filter.SpecialFilterType == (int)SpecialFilterTypeOfApiReqeustLog.VendorAPI_SearchAllColumnByMerchantID)
            {
                filter.RuleFor(x => x.IdentityKey).Rule_NotEmptyText();

                if (filter.TargetTableName != "[ApiRequestLogVendorAPIError]")
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, new ErrorDetailModel("特殊篩選僅支援 ApiRequestLogVendorAPIError"));
                }

                filter.VendorAPITrySearchMerchantID = filter.IdentityKey;
                filter.IdentityKey = "";
            }

            if (RandomHelper.CheckIsPgsUniqueSequenceOrderNoAndRecentDays(filter.SearchID).IsPgsOrderNo)
            {
                filter.UniqueID = filter.SearchID;
            }
            else if (filter.SearchID.IsNumeric())
            {
                filter.Id = Int64.Parse(filter.SearchID);
            }

            if (!filter.CreateTimeBegin.HasValue)
                filter.CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1);

            if (!filter.CreateTimeEnd.HasValue)
                filter.CreateTimeEnd = DateTimeHelper.GetCurrentTime();

            bool isESSearchTable_v2026 = false;

            if (TableNameConvertHelper.IsEnableElasticLogByTableName(filter.TargetTableName))
            {
                isESSearchTable_v2026 = true;
            }

            if (!isESSearchTable_v2026)
            {
                //舊版DB查詢才需要這樣
                if (filter.RequestBodyKeyword.IsNotEmptyTextNotNull() ||
                filter.ResponseTextKeyword.IsNotEmptyTextNotNull())
                {
                    if (filter.RelativeUrl.IsEmptyOrNull())
                    {
                        throw new TerproException(ApiErrorCode.InvalidParameter, "若要做關鍵字搜尋。請篩選 RelativeUrl 或 輸入*號");
                    }
                    else if (filter.RelativeUrl == "*")
                    {
                        //某些情況下還是很煩。允許skip此防呆
                        filter.RelativeUrl = null;
                    }
                }
            }
           
            var cureentUser = GetCurrentOwnPermissionByCache(userID);

            string dynamicColumns = " , Id as UniqueID ";

            string selectColumns = " RequestUrl, RelativeUrl, HttpMethod, IPAddress, WebSiteName, UserIdentity, RequestBody , ResponseText" +
            $" , ResponseStatusCode, IdentityKey, CreateTime, DebugInfo, DebugInfo2 , ExecutionTime {dynamicColumns} ";

            if (AppHelper.IsExternalVendorApiLogTableName(filter.TargetTableName))
            {
                selectColumns = " * , ReferOrderNo as DebugInfo3 ";
            }

            //因資料量大 效能會'極差' 不使用標準分頁查詢語法
            filter.PageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;
            filter.TopSize = filter.PageSize;
            filter.TimeoutSeconds = 120;
            filter.ExtraSpecificOrderBy = " ORDER BY CreateTime desc ";

            List<ApiRequestLogInjectViewModel> query = new List<ApiRequestLogInjectViewModel>();
        
            if (isESSearchTable_v2026)
            {
                List<ESKeywordSearchParameter> filterList = new List<ESKeywordSearchParameter>
                {
                    ElasticsearchHelper.InstanceKeywordSearchModelOrNull(nameof(FullInfoApiRequestLogESModel.UniqueID), filter.UniqueID),
                    ElasticsearchHelper.InstanceKeywordSearchModelOrNull(nameof(FullInfoApiRequestLogESModel.ReferOrderNo), filter.ReferOrderNo),
                    ElasticsearchHelper.InstanceKeywordSearchModelOrNull("*", filter.SearchAllColumnsKeyword),
                    ElasticsearchHelper.InstanceKeywordSearchModelOrNull(nameof(FullInfoApiRequestLogESModel.RequestUrlFull), filter.RequestUrlKeyword),
                    ElasticsearchHelper.InstanceKeywordSearchModelOrNull(nameof(FullInfoApiRequestLogESModel.MerchantID), filter.MerchantID.HasValue ? filter.MerchantID.Value.ToString() : ""),
                    ElasticsearchHelper.InstanceKeywordSearchModelOrNull(nameof(FullInfoApiRequestLogESModel.PlayerID), filter.PlayerID),
                    ElasticsearchHelper.InstanceKeywordSearchModelOrNull(nameof(FullInfoApiRequestLogESModel.RequestContent), filter.RequestBodyKeyword),
                    ElasticsearchHelper.InstanceKeywordSearchModelOrNull(nameof(FullInfoApiRequestLogESModel.ResponseText), filter.ResponseTextKeyword),
                    ElasticsearchHelper.InstanceKeywordSearchModelOrNull(nameof(FullInfoApiRequestLogESModel.IPAddress), filter.IPAddress),
                    ElasticsearchHelper.InstanceKeywordSearchModelOrNull(nameof(FullInfoApiRequestLogESModel.RelativeUrl), filter.RelativeUrlKeyword),
                };

                var basicConfig = new ESKeywordSearchBasicParameter
                {
                    PageSize = filter.PageSize,
                    Begin = filter.CreateTimeBegin.Value,
                    End = filter.CreateTimeEnd.Value,
                    IndexName = ESIndexName,
                    AdvanceParameter = new ESKeywordAdvanceFilterParameter
                    {
                        AlwaysFilter = new List<ESKeywordSearchParameter>(),                        
                    },
                    ExcludeColumns = new List<string>
                    {
                        nameof(FullInfoApiRequestLogESModel.RequestContent) ,
                        nameof(FullInfoApiRequestLogESModel.ResponseText) ,
                        nameof(FullInfoApiRequestLogESModel.RequestHeader) ,
                    }
                };

                if(filter.ResponseStatusCode.NotNullNotZeroNotMinus())
                {
                    basicConfig.AdvanceParameter.AlwaysFilter.Add(new ESKeywordSearchParameter
                    {
                        ColumnName = "ResponseStatusCode",
                        lte = filter.ResponseStatusCode.Value,
                        gte = filter.ResponseStatusCode.Value
                    });
                }

                if (filter.RelativeUrl.IsNotEmptyNull())
                {
                    basicConfig.AdvanceParameter.AlwaysFilter.Add(new ESKeywordSearchParameter
                    {
                        ColumnName = "RelativeUrl",
                        Keyword = filter.RelativeUrl.Trim(),
                    });
                }


                if (filter.SpecialFilterType.HasValue)
                {
                    var specialFilterType = (SpecialFilterTypeOfApiReqeustLog)filter.SpecialFilterType.Value;

                    switch (specialFilterType)
                    {
                        case SpecialFilterTypeOfApiReqeustLog.ResponseTime_Large_Than_3:
                            basicConfig.AdvanceParameter.AlwaysFilter.Add(new ESKeywordSearchParameter
                            {
                                ColumnName = "ResponseTime",
                                gte = 1000 * 3
                            });
                            break;

                        case SpecialFilterTypeOfApiReqeustLog.ResponseTime_Large_Than_10:
                            basicConfig.AdvanceParameter.AlwaysFilter.Add(new ESKeywordSearchParameter
                            {
                                ColumnName = "ResponseTime",
                                gte = 1000 * 10
                            });
                            break;

                        default:
                            break;
                    }
                   
                }

                var ESsearchResult = ElasticsearchHelper.KeywordSearchDataByFirstFilter<FullInfoApiRequestLogESModel>(basicConfig, filterList);

                query = ESsearchResult.Select(x =>
                {
                    var convert = x.ReturnInjecteObject<ApiRequestLogInjectViewModel>();
                    convert.ExecutionTime = x.ResponseTime;

                    if(x.MerchantID > 0)
                    {
                        string merchantIDText = x.MerchantID.ToString();

                        if (string.IsNullOrWhiteSpace(x.DebugInfo))
                        {
                            convert.DebugInfo = merchantIDText;
                        }
                        else if (string.IsNullOrWhiteSpace(x.DebugInfo2))
                        {
                            convert.DebugInfo2 = merchantIDText;
                        }
                    }

                    convert.DebugInfo2 = (convert.DebugInfo2 ?? "").Truncate(30);

                    if (string.IsNullOrWhiteSpace(convert.DebugInfo3) && x.PlayerID > 0)
                    {
                        convert.DebugInfo3 = x.PlayerID.ToString();
                    }

                    return convert;
                })
                .ToList();
            }
            else
            {
                if (AppHelper.IsExternalVendorApiLogTableName(filter.TargetTableName))
                {
                    if (filter.ResponseTextKeyword.IsNotEmptyTextNotNull())
                    {
                        filter.MappingTagValuesSearchFilter_SettingKey = ApiReqeustLogTagValueMappingTypeEnum.ResponseText.ToInteger();
                        filter.MappingTagValuesSearchFilter_SettingValue = filter.ResponseTextKeyword;
                        filter.ResponseTextKeyword = "";
                    }
                    else if (filter.RequestBodyKeyword.IsNotEmptyTextNotNull())
                    {
                        filter.MappingTagValuesSearchFilter_SettingKey = ApiReqeustLogTagValueMappingTypeEnum.RequestBody.ToInteger();
                        filter.MappingTagValuesSearchFilter_SettingValue = filter.RequestBodyKeyword;
                        filter.RequestBodyKeyword = "";
                    }
                }

                filter.RequestUrlKeyword = filter.RequestUrlKeyword.TrimOrNothing();
                filter.RelativeUrl = filter.RelativeUrl.TrimOrNothing();
                filter.RequestBodyKeyword = filter.RequestBodyKeyword.TrimOrNothing();
                filter.ResponseTextKeyword = filter.ResponseTextKeyword.TrimOrNothing();

                query = ApiRequestLogRepository.GetTopSizeViewModelByFilter(filter, selectColumns);
            }

            var viewModels = new PagedList<ExternalApiRequestLogViewModel>
            {
                PageSize = filter.PageSize,
                PageIndex = 1,
                TotalCount = query.Count(),
                TotalPage = 1,
            };

            viewModels.Items = query.Select(x =>
                {
                    var convert = ConvertToViewModel(x);
                    convert.Id = x.UniqueID;
                    convert.ElasticIndexName = ESIndexName;

                    return convert;
                })
                .ToList();

            SetExtraProperty(filter, viewModels);

            return viewModels;
        }


        private void SetExtraProperty(QueryApiRequestLogRequestParameterModel filter, PagedList<ExternalApiRequestLogViewModel> query)
        {
            if (query.Items.IsEmptyOrNull())
                return;

            foreach (var item in query.Items)
            {
                if (item.UniqueID.IsNotEmptyTextNotNull())
                {
                    if (RegexRuleHelper.IsPgs32TransactionNo(item.UniqueID))
                    {
                        item.IsSearchDetailByAPI = true;
                    }
                }
            }


            //bool isExternalAPI = filter.TargetTableName.ContainsWihtoutCase("LogExternalVendorAPI") &&
            //                    !filter.TargetTableName.ContainsWihtoutCase("APIError");

            //if (isExternalAPI)
            //{

            //}
        }

        public ExternalApiRequestLogViewModel GetByID(long id)
        {
            var query = ApiRequestLogRepository.GetById(id);
            return ConvertToViewModel(query);
        }

        public ExternalApiRequestLogViewModel GetByUniqueID(LazyCommonRequestModel filter)
        {
            filter.RuleFor(x => x.UniqueID).Rule_NotEmptyText();
            filter.RuleFor(x => x.TableName).Rule_NotEmptyText();

            filter.TableName = TableNameReplace(filter.TableName);

            var parse = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(filter.UniqueID);

            ExternalApiRequestLogViewModel viewModel = new ExternalApiRequestLogViewModel();

            if (TableNameConvertHelper.IsEnableElasticLogByTableName(filter.TableName))
            {
                var ESindexName = TableNameConvertHelper.GetElasticsearchIndexNameByTableName(filter.TableName);

                var queryES = ApiRequestLogV2024Repository.GetESFullInformationByUniqueID(ESindexName, filter.UniqueID);

                if (queryES != null)
                {
                    viewModel = queryES.ReturnInjecteObject<ExternalApiRequestLogViewModel>();
                    viewModel.RequestContent = queryES.RequestContent;
                    viewModel.ResponseText = queryES.ResponseText;
                    viewModel.RequestHeader = queryES.RequestHeader;
                    viewModel.RequestUrl = queryES.RequestUrlFull;
                }
            }
            else
            {
                var query = ApiRequestLogRepository.GetByFilter(new QueryApiRequestRequestViewModel
                {
                    UniqueID = filter.UniqueID,
                    TargetTableName = filter.TableName,
                    CreateTimeBegin = parse.Time.Value.AddHours(-2),
                    CreateTimeEnd = parse.Time.Value.AddHours(1),
                })
                .FirstOrDefault();

                if (query == null)
                {
                    throw new TerproException(ApiErrorCode.NotFoundData);
                }

                viewModel = ConvertToViewModel(query);

                SetDetailInfoByMappingValueDB(viewModel, filter.UniqueID, parse.Time.Value);
            }

            return viewModel;
        }


        private void SetDetailInfoByMappingValueDB(ExternalApiRequestLogViewModel viewModel, string uniqueID, DateTime partitionTime)
        {
            var queryExtraInfo = MappingTagValuesLargeRepository.GetByFilter(new QueryMappingTagValuesRequestViewModel
            {
                UniqueID = uniqueID,
                PartitionTimeMatch = partitionTime
            });

            viewModel.RequestContent = queryExtraInfo.FirstOrDefault(x => x.SettingKey == (int)ApiReqeustLogTagValueMappingTypeEnum.RequestBody)?.SettingValue;
            viewModel.ResponseText = queryExtraInfo.FirstOrDefault(x => x.SettingKey == (int)ApiReqeustLogTagValueMappingTypeEnum.ResponseText)?.SettingValue;
            viewModel.RequestHeader = queryExtraInfo.FirstOrDefault(x => x.SettingKey == (int)ApiReqeustLogTagValueMappingTypeEnum.Headers)?.SettingValue;
        }


        private ExternalApiRequestLogViewModel ConvertToViewModel<T>(T query) where T : ApiRequestLogEntity
        {
            //將model轉成viewModels
            var convert = query.ReturnInjecteObject<ExternalApiRequestLogViewModel>();
            convert.Category = query.RelativeUrl;
            convert.SubCategory = query.HttpMethod;
            convert.RequestContent = query.RequestBody;
            convert.ResponseStatusCode = query.ResponseStatusCode;
            convert.ResponseText = query.ResponseText;

            if(query.UniqueID.IsNotEmptyTextNotNull())
            {
                convert.Id = query.UniqueID;
            }

            if (SettingHelper.IsProduction)
            {
                convert.ResponseText = convert.ResponseText.Truncate(500, "....");
            }


            SetDebugInfo(convert, query);

            return convert;
        }


        private void SetDebugInfo(ExternalApiRequestLogViewModel model, ApiRequestLogEntity query)
        {
            List<string> textList = new List<string>();

            if (query.IdentityKey.IsNotEmptyTextNotNull())
            {
                textList.Add(query.IdentityKey);
            }

            if (query.UserIdentity.IsNotEmptyTextNotNull() && query.UserIdentity != "0")
            {
                textList.Add(query.UserIdentity);
            }

            if (query.DebugInfo.IsNotEmptyTextNotNull() && query.DebugInfo != "0")
            {
                textList.Add(query.DebugInfo);
            }

            if (query.DebugInfo2.IsNotEmptyTextNotNull())
            {
                textList.Add(query.DebugInfo2);
            }

            model.IdentityText1 = textList.FirstOrDefault();

            if (textList.IsNotEmptyNull() && textList.Count() >= 2)
            {
                model.IdentityText2 = textList[1];
            }
        }

        private string TableNameReplace(string tableName)
        {
            if (tableName.ContainsWihtoutCase("ApiRequestLogExternalVendorAPIError") && !tableName.ContainsWihtoutCase("Brief"))
            {
                return tableName.Replace("ApiRequestLogExternalVendorAPIError", "ApiRequestLogExternalVendorAPIErrorBrief");
            }

            return tableName;
        }


    }

}
