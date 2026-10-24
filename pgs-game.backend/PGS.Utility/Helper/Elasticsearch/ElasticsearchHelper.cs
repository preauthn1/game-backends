using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2013.PowerPoint.Roaming;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver.Core.Misc;
using target.Models.CultureText;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Utility.Extensions;
using target.Utility.Helper.Elasticsearch.Model;
using target.Utility.Utility;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace target.Utility.Helper
{

    /// <summary>
    /// Elasticsearch 搜尋
    /// 常用型別 FullInfoApiRequestLogThirdPartyEntity
    /// </summary>
    public static class ElasticsearchHelper
    {
        // elastic的Index等同於 Table的概念
        // mapping (定義欄位型別) 可以加快搜尋


        private static string ConvertIndexName(string indexName)
        {
            return indexName.ToLower();
        }


        public static string CreateNewIndex(string indexName, List<IndexColumnMappingDefine> columnDefine)
        {
            string apiUrl = $"/{ConvertIndexName(indexName)}";

            var myParam = InstanceRequestPrameter(apiUrl, HttpMethodType.HttpPut);

            Dictionary<string, object> dic = new Dictionary<string, object>();

            foreach (var item in columnDefine)
            {
                object myValue = null;

                if (!string.IsNullOrWhiteSpace(item.Format))
                {
                    myValue = new
                    {
                        type = item.DataType,
                        format = item.Format
                    };
                }
                else
                {
                    myValue = new
                    {
                        type = item.DataType,
                    };
                }

                dic.Add(item.ColumnName, myValue);
            }

            //object body = null;

            var body = new
            {
                mappings = new
                {
                    properties = dic
                }
            };

            var apiResult = APIRequestHelper.RequestApiResult<string>(myParam, body);
            return apiResult.ResponseText;
        }

        public static string BulkInsert<T>(string indexName, List<T> items) where T : class, IElasticSearchLog
        {
            if (SettingHelper.IsMSTest)
                return "OK";

            if (items.IsEmptyOrNull())
                return "Empty";

            indexName = ConvertIndexName(indexName);

            var myParam = InstanceRequestPrameter("_bulk", HttpMethodType.HttpPut);

            if (SettingHelper.IsDev && string.IsNullOrWhiteSpace(myParam.BaseUrl))
                return "BaseUrl not setup";

            StringBuilder insertPayload = new StringBuilder();

            foreach (var item in items)
            {
                insertPayload.Append(("{'create':{'_index':'" + indexName + "','_id':'" + Guid.NewGuid().ToString() + "'}}" + Environment.NewLine).Replace("'", @""""));
                insertPayload.Append(item.ConvertToJsonString() + Environment.NewLine);
            }

            var apiResult = APIRequestHelper.RequestApiResult<string>(myParam, insertPayload.ToString());

            if (apiResult.DebugInfo != null)
            {
                if (apiResult.DebugInfo.ExecutedTime > 1000 * 15)
                {
                    string myMsg = $"ES BulkInsert 花費時間過長 {apiResult.DebugInfo.ExecutedTime}。是否機器異常";

                    MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                    {
                        Message = myMsg,
                        Channel = SlackChannelEnum.n_error_pgs_common
                    });
                }
            }

            return apiResult.ResponseText;
        }


        public static string GetIndexSingleData(string indexName)
        {
            indexName = ConvertIndexName(indexName);
            var myParam = InstanceRequestPrameter($"{indexName}/_doc/1", HttpMethodType.HttpGet);
            var apiResult = APIRequestHelper.RequestApiResult<string>(myParam);
            return apiResult.ResponseText;
        }

        public static ESGetEachApiErrorCountResponse GetEachApiErrorCount(ElasticLogTableTypeEnum indexType, DateTime begin, DateTime end)
        {
            string indexName = TableNameConvertHelper.GetElasticsearchIndexName(indexType);
            var myParam = InstanceRequestPrameter($"/{indexName}/_search", HttpMethodType.HttpPost);

            var body = new
            {
                size = 0,
                query = new
                {
                    @bool = new
                    {
                        must = new List<object>
                        {
                            new
                            {
                                range = new
                                {
                                    CreateTime = new
                                    {
                                        gte = begin.ToString("yyyy-MM-ddTHH:mm:ss"),
                                        lte = end.ToString("yyyy-MM-ddTHH:mm:ss")
                                    }
                                }
                            }
                        }
                    }
                },
                aggs = new
                {
                    group_by_category = new
                    {
                        terms = new
                        {
                            field = "RelativeUrl.keyword",
                            size = 15
                        }
                    }
                }
            };

            var apiResult = APIRequestHelper.RequestApiResult<ESGetEachApiErrorCountResponse>(myParam, body);
            return apiResult.ResultObject;
        }


        public static ESGetEachHourCountIn48HoursResponse GetEachHourCount_in48Hours(ElasticLogTableTypeEnum indexType)
        {
            string indexName = TableNameConvertHelper.GetElasticsearchIndexName(indexType);
            var myParam = InstanceRequestPrameter($"/{indexName}/_search", HttpMethodType.HttpPost);

            var body = new
            {
                size = 0,
                query = new
                {
                    @bool = new
                    {
                        filter = new List<object>
                        {
                            new
                            {
                                range = new
                                {
                                    CreateTime = new
                                    {
                                        gte = "now-48h/h",
                                        lte = "now"
                                    }
                                }
                            }
                        }
                    }
                },
                aggs = new
                {
                    hourly_counts = new
                    {
                        date_histogram = new
                        {
                            field = "CreateTime",
                            calendar_interval = "1h",
                            min_doc_count = 0,
                            extended_bounds = new
                            {
                                min = "now-48h/h",
                                max = "now"
                            }
                        }
                    }
                }
            };

            var apiResult = APIRequestHelper.RequestApiResult<ESGetEachHourCountIn48HoursResponse>(myParam, body);
            return apiResult.ResultObject;
        }


        public static (bool IsOK, string RequestUrl, List<string> Items) GetAllIndex()
        {
            var myParam = InstanceRequestPrameter($"/_cat/indices/*log*", HttpMethodType.HttpGet);

            var apiResult = APIRequestHelper.RequestApiResult<string>(myParam);

            bool isOK = apiResult.ResponseText.Contains("yellow") || apiResult.ResponseText.Contains("green");

            string myUrl = $"{myParam.BaseUrl}{myParam.ApiUrl}";

            return (isOK, myUrl, AppHelper.AnalyzeTextFromRawTextBySpaceChar(apiResult.ResponseText, new List<string> { "\n", "\r\n" }));
        }



        /// <summary>
        /// 搜尋單筆資料 (by UniqueID)
        /// 常用型別 FullInfoApiRequestLogESModel
        /// </summary>
        public static T GetByUniqueID<T>(string indexName, string uniqueID) where T : class
        {
            indexName = ConvertIndexName(indexName);
            var myParam = InstanceRequestPrameter($"{indexName}/_search", HttpMethodType.HttpPost);

            var body = new
            {
                query = new
                {
                    term = new
                    {
                        UniqueID = uniqueID
                    }
                }
            };

            var apiResult = APIRequestHelper.RequestApiResult<ESSearchAPIResponse<T>>(myParam, body);
            return apiResult.ResultObject?.hits?.hits.Select(x => x._source).FirstOrDefault();
        }


        /// <summary>
        /// 搜尋多筆資料 (by UniqueIDs)
        /// 常用型別 FullInfoApiRequestLogThirdPartyEntity
        /// </summary>
        public static List<T> GetByUniqueIDs<T>(string indexName, List<string> uniqueIDs) where T : class
        {
            indexName = ConvertIndexName(indexName);
            var myParam = InstanceRequestPrameter($"{indexName}/_search", HttpMethodType.HttpPost);

            var body = new
            {
                query = new
                {
                    terms = new
                    {
                        UniqueID = uniqueIDs
                    }
                }
            };

            var apiResult = APIRequestHelper.RequestApiResult<ESSearchAPIResponse<T>>(myParam, body);
            return apiResult.ResultObject?.hits?.hits.Select(x => x._source).ToList();
        }


        public static void DeleteDataByTime<T>(string indexName, DateTime lessThanTime) where T : class
        {
            indexName = ConvertIndexName(indexName);

            string apiUrl = $"{indexName}/_delete_by_query?conflicts=proceed&scroll_size=10000";

            var myParam = InstanceRequestPrameter(apiUrl, HttpMethodType.HttpPost);

            var body = new
            {
                query = new
                {
                    range = new
                    {
                        CreateTime = new
                        {
                            lt = lessThanTime.ToString("yyyy-MM-dd")
                        }
                    }
                },
            };

            var apiResult = APIRequestHelper.RequestApiResult<ESSearchAPIResponse<T>>(myParam, body);
            return;
        }




        public static string GenerateTestDataByBulkInsert()
        {
            var myParam = InstanceRequestPrameter("_bulk", HttpMethodType.HttpPut);

            StringBuilder insertPayload = new StringBuilder();

            List<string> randTime = new List<string>
            {
                "2023-07-25 01:00:00",
                "2023-07-25 02:00:00",
                "2023-07-25 03:00:00",
                "2023-07-25 04:00:00",
                "2023-07-25 05:00:00",
                "2023-07-25 06:00:00",
                "2023-07-25 07:00:00",
                "2023-07-25 08:00:00",
                "2023-07-25 09:00:00",
            };

            string indexName = "my_index_002";

            for (int i = 0; i < 1000; i++)
            {
                var ts = DateTimeHelper.GetCurrentTimeTimestamp() + i;

                insertPayload.Append(("{'create':{'_index':'" + indexName + "','_id':'" + ts + "'}}" + Environment.NewLine).Replace("'", @""""));
                var myObject = new
                {
                    name = ts,
                    age = RandomHelper.GetRandIntegerNumbers(20, 40),
                    create_time = randTime.OrderByRandom().FirstOrDefault(),
                };

                Thread.Sleep(RandomHelper.GetRandIntegerNumbers(1, 5));
                insertPayload.Append(myObject.ConvertToJsonString() + Environment.NewLine);
            }

            var qqqq = APIRequestHelper.RequestApiResult<string>(myParam, insertPayload.ToString());
            return "";
        }

        public static ESKeywordSearchParameter InstanceKeywordSearchModelOrNull(string columnName, string searchKeywordValue)
        {
            if (string.IsNullOrWhiteSpace(searchKeywordValue))
                return null;

            return new ESKeywordSearchParameter
            {
                ColumnName = columnName,
                Keyword = searchKeywordValue
            };
        }

        public static ESKeywordSearchParameter InstanceKeywordSearchModelOrNull(string columnName, int? searchIntMatchValue)
        {
            if (searchIntMatchValue == null)
                return null;

            return new ESKeywordSearchParameter
            {
                ColumnName = columnName,
                gte = searchIntMatchValue.Value,
                lte = searchIntMatchValue.Value
            };
        }


        /// <summary>
        /// 根據第一個不為 空值的欄位 來做keyword搜尋
        /// 欄位可以參考 FullInfoThirdPartyApiRequestLogESModel
        /// </summary>
        public static ESKeywordSearchParameter InstanceKeywordSearchModelOrNull(List<(string column, string keyword)> ColumnAndKeywordValues)
        {
            if (ColumnAndKeywordValues.IsEmptyOrNull())
                return null;

            foreach (var item in ColumnAndKeywordValues)
            {
                if (!string.IsNullOrWhiteSpace(item.keyword))
                {
                    return new ESKeywordSearchParameter
                    {
                        ColumnName = item.column,
                        Keyword = item.keyword
                    };
                }
            }

            return null;
        }

        /// <summary>
        /// 填入多個 ESKeywordSearchParameter (有可能是NULL)
        /// 會取第一個不是NULL的Filter來作為查詢條件查詢
        /// </summary>
        public static List<T> KeywordSearchDataByFirstFilter<T>(ESKeywordSearchBasicParameter basicConfig, List<ESKeywordSearchParameter> filter) where T : class
        {
            basicConfig.RuleFor(x => x.IndexName).Rule_NotEmptyText();
            basicConfig.RuleFor(x => x.Begin).Rule_NotEmptyDateTime();
            basicConfig.RuleFor(x => x.End).Rule_NotEmptyDateTime();

            filter = filter ?? new List<ESKeywordSearchParameter>();

            var myfilterList = filter.Where(x => x != null).ToList();

            var matchFirst = myfilterList.FirstOrDefault();

            if (matchFirst == null)
            {
                matchFirst = new ESKeywordSearchParameter();
            }

            if (matchFirst.ColumnName == "UniqueID")
            {
                var querySingle = GetByUniqueID<T>(basicConfig.IndexName, matchFirst.Keyword);

                if (querySingle == null)
                    return new List<T>();

                return new List<T> { querySingle };
            }

            var query = KeywordSearchDataByAPI<T>(basicConfig, matchFirst);
            return query;
        }

        public static List<T> KeywordSearchDataByAPI<T>(ESKeywordSearchBasicParameter basicConfig, ESKeywordSearchParameter filter) where T : class
        {
            basicConfig.AdvanceParameter = basicConfig.AdvanceParameter ?? new ESKeywordAdvanceFilterParameter();

            var indexName = ConvertIndexName(basicConfig.IndexName);
            var myParam = InstanceRequestPrameter($"{indexName}/_search", HttpMethodType.HttpPost);

            List<object> multipleCondiction = new List<object>();

            basicConfig.RuleFor(x => x.Begin).Rule_NotEmptyDateTime();
            basicConfig.RuleFor(x => x.End).Rule_NotEmptyDateTime();

            multipleCondiction.Add(new
            {
                range = new
                {
                    CreateTime = new
                    {
                        gte = basicConfig.Begin.ToString("yyyy-MM-ddTHH:mm:ss"),
                        lte = basicConfig.End.ToString("yyyy-MM-ddTHH:mm:ss"),
                    }
                }
            });

            if (basicConfig.AdvanceParameter.AlwaysFilter.IsNotEmptyNull())
            {
                foreach (var item in basicConfig.AdvanceParameter.AlwaysFilter)
                {
                    var eachfilterObject = InstanceESFilterObject(item);
                    if (eachfilterObject != null)
                    {
                        multipleCondiction.Add(eachfilterObject);
                    }
                }
            }

            object filterObject = InstanceESFilterObject(filter);

            if (filterObject != null)
            {
                multipleCondiction.Add(filterObject);
            }

            object boolCondition = new
            {
                must = multipleCondiction
            };

            if (basicConfig.AdvanceParameter.ShouldQuery != null)
            {
                boolCondition = new
                {
                    must = multipleCondiction,
                    should = basicConfig.AdvanceParameter.ShouldQuery,
                    minimum_should_match = 1
                };
            }

            var body = new
            {
                _source = new
                {
                    excludes = basicConfig.ExcludeColumns ?? new List<string>()
                },
                from = 0,
                size = basicConfig.PageSize <= 0 ? 500 : basicConfig.PageSize,
                sort = new List<object>
                {
                    new
                    {
                        CreateTime = new
                        {
                            order = "desc"
                        }
                    }
                },
                query = new
                {
                    @bool = boolCondition
                }
            };

            if (SettingHelper.IsDev)
            {
                var debug_body = body.ConvertToJsonString();
                string test = "";
            }

            if (SettingHelper.IsLocalhostWebSiteName)
            {
                if (SettingHelper.IsProductionDbConnection && myParam.BaseUrl.ContainsWihtoutCase("192.168.20"))
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataIncorrect, new ErrorDetailModel("Elastic is dev conn"));
                }
            }



            var apiResult = APIRequestHelper.RequestApiResult<ESSearchAPIResponse<T>>(myParam, body);
            var items = apiResult.ResultObject?.hits?.hits.Select(x => x._source).ToList() ?? new List<T>();
            return items;
        }


        private static object InstanceESFilterObject(ESKeywordSearchParameter filter)
        {
            if (filter == null)
                return null;

            if (filter.ForceObject != null)
            {
                return filter.ForceObject;
            }

            if (string.IsNullOrWhiteSpace(filter.ColumnName))
                return null;

            if (filter.ColumnName.IsNotEmptyTextNotNull() && filter.Keyword.IsNotEmptyTextNotNull())
            {
                if (filter.ColumnName == "*")
                {
                    return new
                    {
                        simple_query_string = new
                        {
                            query = filter.Keyword,
                            default_operator = "and"
                        }
                    };
                }
                else if (filter.ColumnName == nameof(FullInfoApiRequestLogESModel.MerchantID))
                {
                    return new
                    {
                        term = new
                        {
                            MerchantID = filter.Keyword,
                        }
                    };
                }
                else if (filter.ColumnName == "XXXXXXXXXXXXXXXXXXXXXXX")
                {
                    //如果沒有定義mapping的欄位 貌似無法查詢
                    throw new TerproException(ApiErrorCode.NotImplement);
                }
                else if(filter.ColumnName == nameof(FullInfoApiRequestLogESModel.RequestContent) ||
                        filter.ColumnName == nameof(FullInfoApiRequestLogESModel.RequestUrlFull) ||
                        filter.ColumnName == nameof(FullInfoApiRequestLogESModel.RequestHeader) ||
                        filter.ColumnName == nameof(FullInfoApiRequestLogESModel.ResponseText))
                {
                    //長度會超過256的欄位 一律用這種方式查
                    Dictionary<string, object> keyValue = new Dictionary<string, object>();
                    keyValue.Add($"{filter.ColumnName}", filter.Keyword);

                    return new
                    {
                        match_phrase = keyValue
                    };
                }
                else if (
                    //filter.ColumnName == nameof(FullInfoApiRequestLogESModel.RequestContent) ||
                     //   filter.ColumnName == nameof(FullInfoApiRequestLogESModel.ResponseText) ||
                     //   filter.ColumnName == nameof(FullInfoApiRequestLogESModel.RequestHeader) ||
                     //   filter.ColumnName == nameof(FullInfoApiRequestLogESModel.RequestUrlFull) ||
                        filter.ColumnName == nameof(FullInfoApiRequestLogESModel.RelativeUrl) ||
                        filter.ColumnName == nameof(FullInfoApiRequestLogESModel.Category) ||
                        filter.ColumnName == nameof(FullInfoApiRequestLogESModel.SubCategory) ||
                        filter.ColumnName == nameof(FullInfoApiRequestLogESModel.WebSiteName))
                {
                    //設定 text 型別的欄位 需要這樣查詢 (keyword型別的不需要這樣查)
                    Dictionary<string, object> keyValue = new Dictionary<string, object>();
                    keyValue.Add($"{filter.ColumnName}.keyword", new
                    {
                        value = $"*{filter.Keyword}*"
                    });

                    return new
                    {
                        wildcard = keyValue
                    };
                }
                else
                {
                    Dictionary<string, object> keyValue = new Dictionary<string, object>();
                    keyValue.Add($"{filter.ColumnName}", new
                    {
                        value = $"*{filter.Keyword}*"
                    });

                    return new
                    {
                        wildcard = keyValue
                    };
                }
            }
            else if (filter.gte.HasValue || filter.lte.HasValue)
            {
                Dictionary<string, object> keyValue = new Dictionary<string, object>();
                keyValue.Add($"{filter.ColumnName}", new
                {
                    gte = filter.gte.HasValue ? filter.gte.Value: 0,
                    lte = filter.lte.HasValue ?filter.lte.Value : 99999999 
                });

                return new
                {
                    range = keyValue
                };
            }
            else
            {
                return null;
            }

        }


        public static List<T> EqualSearchDataByAPI<T>(ESKeywordSearchBasicParameter basicConfig, ESKeywordSearchParameter filter) where T : class
        {
            if (filter == null)
                return new List<T>();

            var indexName = ConvertIndexName(basicConfig.IndexName);
            var myParam = InstanceRequestPrameter($"{indexName}/_search", HttpMethodType.HttpPost);

            List<object> multipleCondiction = new List<object>();

            multipleCondiction.Add(new
            {
                range = new
                {
                    CreateTime = new
                    {
                        gte = basicConfig.Begin.ToString("yyyy-MM-ddTHH:mm:ss"),
                        lte = basicConfig.End.ToString("yyyy-MM-ddTHH:mm:ss"),
                    }
                }
            });

            if (filter.ColumnName.IsNotEmptyTextNotNull() && filter.Keyword.IsNotEmptyTextNotNull())
            {
                Dictionary<string, object> keyValue = new Dictionary<string, object>();

                //如果型別是text的欄位 會需要使用.keyword 查詢
                keyValue.Add($"{filter.ColumnName}", new
                {
                    value = filter.Keyword,
                    case_insensitive = true
                });

                multipleCondiction.Add(new
                {
                    term = keyValue
                });
            }

            var body = new
            {
                query = new
                {
                    @bool = new
                    {
                        must = multipleCondiction
                    }
                }
            };

            var debug333 = body.ConvertToJsonString();

            var apiResult = APIRequestHelper.RequestApiResult<ESSearchAPIResponse<T>>(myParam, body);
            return apiResult.ResultObject?.hits?.hits.Select(x => x._source).ToList();
        }


        public static object DebugRequest<T>(string indexName, List<string> uniqueIDs) where T : class
        {
            indexName = ConvertIndexName(indexName);
            var myParam = InstanceRequestPrameter($"{indexName}/_search", HttpMethodType.HttpPost);

            var body = new
            {
                query = new
                {
                    terms = new
                    {
                        UniqueID = uniqueIDs
                    }
                }
            };

            var apiResult = APIRequestHelper.RequestApiResult<ESSearchAPIResponse<T>>(myParam, body);
            return apiResult;
        }



        private static RequestParameters InstanceRequestPrameter(string apiUrl, HttpMethodType method)
        {

            //本地常常設錯。防呆下
            if (SettingHelper.IsLocalhostWebSiteName)
            {
                if (SettingHelper.IsProductionPGSConnection)
                {
                    if (!SettingHelper.ThirdParty.ElastisearchAPIUrl.Contains(".110"))
                    {
                        throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataIncorrect, "ElastisearchAPIUrl not target's config.");
                    }
                }

                if (SettingHelper.IsProductionBWGConnection)
                {
                    if (!SettingHelper.ThirdParty.ElastisearchAPIUrl.Contains(".111"))
                    {
                        throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataIncorrect, "ElastisearchAPIUrl not BWG's config.");
                    }
                }
            }

            var myParam = new RequestParameters
            {
                BaseUrl = SettingHelper.ThirdParty.ElastisearchAPIUrl,
                ApiUrl = apiUrl,
                Method = method,
                AdvancedSetting = new AdvancedSetting
                {
                    IsIgnoreSSLCertificateError = true,
                    IsIgnoreJsonParsingColumnError = true,
                }
            };

            myParam.AddHeaderData("Authorization", $"Basic {SettingHelper.ThirdParty.ElastisearchAPIAuthorizationToekn}"); //base64 decode後是一組帳密
            return myParam;
        }

    }

}
