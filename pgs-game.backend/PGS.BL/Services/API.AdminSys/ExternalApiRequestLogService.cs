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
    public class ExternalApiRequestLogService : BaseAdminSysService, IExternalApiRequestLogService
    {

        public ExternalApiRequestLogRepository ExternalApiRequestLogRepository { get; set; }

        public BriefApiRequestLogThirdPartyRepository BriefApiRequestLogThirdPartyRepository { get; set; }

        public MappingValueThirdPartyMongoRepository MappingValueThirdPartyMongoRepository { get; set; }




        public PagedList<ExternalApiRequestLogViewModel> GetByPagingFilter(int userID, QueryExternalApiRequestLogApiRequestParameterModel filter)
        {
            filter.IDs = filter.Id.ToIntegerListCollection();

            var cureentUser = GetCurrentOwnPermissionByCache(userID);

            filter.TopSize = 200;
            filter.OrderByColumn = " CreateTime ";

            if (filter.SpecialFilterType.HasValue)
            {
                if (filter.SpecialFilterType == (int)AdminSysExtraFilterEnum.ThirdPartyApiLog_ResponseTime_5s)
                {
                    filter.ResponseTimeLargeThan = 1000 * 5;
                }
                else if (filter.SpecialFilterType == (int)AdminSysExtraFilterEnum.ThirdPartyApiLog_ResponseTime_10s)
                {
                    filter.ResponseTimeLargeThan = 1000 * 10;
                }
            }

            if (TableNameConvertHelper.IsEnableElasticLogByTable(ElasticLogTableTypeEnum.ExternalApiRequestlog))
            {               
                var viewModels = new PagedList<ExternalApiRequestLogViewModel>
                {
                    PageSize = filter.TopSize.Value,
                    Items = new List<ExternalApiRequestLogViewModel>(),
                    TotalPage = 1,
                };

                var basicConfig = new ESKeywordSearchBasicParameter
                {
                    PageSize = filter.TopSize.Value,
                    Begin = filter.CreateTimeBegin.Value,
                    End = filter.CreateTimeEnd.Value,
                    AdvanceParameter = new ESKeywordAdvanceFilterParameter
                    {
                        AlwaysFilter = new List<ESKeywordSearchParameter>()
                    }
                };

                if(!string.IsNullOrWhiteSpace(filter.Category))
                {
                    basicConfig.AdvanceParameter.AlwaysFilter.Add(new ESKeywordSearchParameter("Category", filter.Category));
                }

                if (!string.IsNullOrWhiteSpace(filter.SubCategory))
                {
                    basicConfig.AdvanceParameter.AlwaysFilter.Add(new ESKeywordSearchParameter("SubCategory", filter.SubCategory));
                }

                if (!filter.MerchantID.IsNullOrZeroOrMinus())
                {
                    basicConfig.AdvanceParameter.AlwaysFilter.Add(new ESKeywordSearchParameter
                    {
                        ColumnName = "MerchantID",
                        gte = filter.MerchantID,
                        lte = filter.MerchantID,
                    });
                }

                if (filter.ResponseStatusCode.HasValue)
                {
                    basicConfig.AdvanceParameter.AlwaysFilter.Add(new ESKeywordSearchParameter
                    {
                        ColumnName = "ResponseStatusCode",
                        gte = filter.ResponseStatusCode.Value,
                        lte = filter.ResponseStatusCode.Value,
                    });
                }

                List<ESKeywordSearchParameter> filterList = new List<ESKeywordSearchParameter>
                {
                    ElasticsearchHelper.InstanceKeywordSearchModelOrNull("UniqueID" ,  filter.UniqueID),
                    ElasticsearchHelper.InstanceKeywordSearchModelOrNull("ReferOrderNo" , filter.ReferOrderNo),
                    ElasticsearchHelper.InstanceKeywordSearchModelOrNull("RequestContent" ,  filter.RequestContentKeyword),
                    ElasticsearchHelper.InstanceKeywordSearchModelOrNull("ResponseText" ,  filter.ResponseTextKeyword),
                    ElasticsearchHelper.InstanceKeywordSearchModelOrNull("RequestUrlFull" , filter.RequestUrlKeyword),
                    ElasticsearchHelper.InstanceKeywordSearchModelOrNull("PlayerID" , filter.PlayerID),
                };

                if(filter.ResponseTimeLargeThan > 0)
                {
                    basicConfig.AdvanceParameter.AlwaysFilter.Add(
                        new ESKeywordSearchParameter
                        {
                            ColumnName = "ResponseTime",
                            gte = filter.ResponseTimeLargeThan,
                            lte = filter.ResponseTimeLargeThan * 10,
                        });
                }

                var searchResult = ExternalApiRequestLogRepository.SearchElasticDataByKeyword(basicConfig, filterList);

                if (searchResult.IsEmptyOrNull())
                {
                    return viewModels;
                }

                viewModels.Items = searchResult.Select(x =>
                    {
                        var convert = x.ReturnInjecteObject<ExternalApiRequestLogViewModel>();
                        convert.RequestUrl = x.RequestUrlFull;
                        convert.DebugInfo = x.MerchantID.ToStringOrZeroEmpty();
                        convert.DebugInfo2 = x.PlayerID.ToStringOrZeroEmpty();
                        convert.Memo = x.ReferOrderNo;
                        convert.ResponseTime = x.ResponseTime;

                        return convert;
                    })
                    .ToList();

                viewModels.TotalCount = viewModels.Items.Count();
                return viewModels;
            }
            else
            {
                var query = ExternalApiRequestLogRepository.GetPagingByFilter(filter);

                var viewModels = ConvertToNewTypePagingObject<ExternalApiRequestLogEntity, ExternalApiRequestLogViewModel>(query);
                    viewModels.Items = query.Items.Select(x =>
                    {
                        var convert = ConvertToViewModel(x);
                        return convert;
                    })
                    .ToList();

                return viewModels;
            }
        }


        public ExternalApiRequestLogViewModel GetByUniqueID(string uniqueID)
        {
            var query = ExternalApiRequestLogRepository.GetElasticDataByUniqueID(uniqueID);
            return ConvertToViewModel(query);
        }


        public ExternalApiRequestLogViewModel GetByID(long id)
        {
            var query = ExternalApiRequestLogRepository.GetById(id);
            return ConvertToViewModel(query);
        }


        public ExternalApiRequestLogViewModel GetByRequestID(string requestID)
        {
            var query = ExternalApiRequestLogRepository.GetByFilter(new QueryExternalApiRequestLogRequestViewModel
            {
                RequestID = requestID
            })
                .FirstOrDefault();

            return ConvertToViewModel(query);
        }




        private ExternalApiRequestLogViewModel ConvertToViewModel(ExternalApiRequestLogEntity query)
        {
            //將model轉成viewModels
            var convert = query.ReturnInjecteObject<ExternalApiRequestLogViewModel>();
            return convert;
        }

        private ExternalApiRequestLogViewModel ConvertToViewModel(FullInfoThirdPartyApiRequestLogESModel query)
        {
            if (query == null)
                return new ExternalApiRequestLogViewModel();

            //將model轉成viewModels
            var convert = query.ReturnInjecteObject<ExternalApiRequestLogViewModel>();
            convert.ResponseText = query.ResponseText;
            convert.RequestContent = query.RequestContent;
            convert.RequestHeader = query.RequestHeader;
            convert.RequestUrl = query.RequestUrlFull;
            return convert;
        }


        private ExternalApiRequestLogViewModel ConvertToViewModelV2(BriefApiRequestLogThirdPartyEntity query)
        {
            //將model轉成viewModels
            var convert = query.ReturnInjecteObject<ExternalApiRequestLogViewModel>();

            if(query.MerchantID > 0)
            {
                convert.DebugInfo = query.MerchantID.ToString();
            }

            if (query.PlayerID > 0)
            {
                convert.DebugInfo2 = query.PlayerID.ToString();
            }

            convert.RequestUrl = "{BASE_URL}" + query.RelativeUrl;
            convert.MerchantID = query.MerchantID;
            convert.PlayerID = query.PlayerID;
            convert.ResponseTime = query.ResponseTime;
            convert.ExecutionTime = query.ResponseTime;

            return convert;
        }


    }

}
