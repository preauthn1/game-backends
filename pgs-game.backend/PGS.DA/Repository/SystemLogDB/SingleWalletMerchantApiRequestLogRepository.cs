using Dapper;
using game.Entity;
using game.Entity.Game;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
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
using System.Linq;
using System.Text;

namespace target.DA.Repository
{
    public class SingleWalletMerchantApiRequestLogRepository : FormatLongPrimaryKeyRepository<SingleWalletMerchantApiRequestLogEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.SystemLogDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[SingleWalletMerchantApiRequestLog]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "UniqueID"; }
        }

        protected override string DefaultSortColumn
        {
            get { return "CreateTime"; }
        }

     

        private int TryGetErrorCode(string responseText)
        {
            int defaultErrorCode = ApiErrorCode.InternalServerError.ToInteger();

            if (string.IsNullOrWhiteSpace(responseText))
                return defaultErrorCode;

            if (responseText.Count() <= 5)
                return defaultErrorCode;

            try
            {
                var parseRes = responseText.JsonConvertToModel<BaseSingleWalletModeResponseModel<object>>();

                if (parseRes == null)
                {
                    return defaultErrorCode;
                }

                return parseRes.Code;
            }
            catch
            {

            }

            return defaultErrorCode;
        }


        public FullInfoSingleWalletCallbackRequestLogESModel GetFullInformationByUniqueID(string uniqueID)
        {
            var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(uniqueID);

            string indeName = TableNameConvertHelper.GetElasticsearchIndexName(ElasticLogTableTypeEnum.SinglewalletMerchantApiRequestLog);

            var query = ElasticsearchHelper.GetByUniqueID<FullInfoSingleWalletCallbackRequestLogESModel>(indeName, uniqueID);
            return query;
        }


        public void BulkInsertToMultipleStorage(List<SingleWalletMerchantApiRequestLogEntity> listOfEntity, SingleWalletMerchantApiRequestLogBriefRepository briefRep)
        {
            if (listOfEntity.IsEmptyOrNull())
                return;

            var insertBriefList = new List<SingleWalletMerchantApiRequestLogBriefEntity>();
            var insertESModels = new List<FullInfoSingleWalletCallbackRequestLogESModel>();

            foreach (var item in listOfEntity)
            {
                var briefEntity = item.ReturnInjecteObject<SingleWalletMerchantApiRequestLogBriefEntity>();
                briefEntity.ReferTransactionNo = item.ReferenceTransactionNo;
                briefEntity.MerchantID = item.MerchantID.ParseNullToZero();
                briefEntity.ErrorCode = TryGetErrorCode(item.ResponseText);

                if (item.DebugInfo2.IsNumbericOfPositiveInt32())
                {
                    briefEntity.PlayerID = Int32.Parse(item.DebugInfo2);
                }

                insertBriefList.Add(briefEntity);

                var ESModelItems = DataModelConvertHelper.ConvertToSingleWalletFullInfoLogESModel(item , briefEntity.ErrorCode);
                ESModelItems.MerchantID = briefEntity.MerchantID;
                ESModelItems.PlayerID = briefEntity.PlayerID;
                ESModelItems.ReferOrderNo = briefEntity.ReferTransactionNo;

                insertESModels.Add(ESModelItems);
            }

            if (TableNameConvertHelper.IsEnableElasticLogByTable(ElasticLogTableTypeEnum.SinglewalletMerchantApiRequestLog))
            {
                //2026/05 開始 不再寫入brief
                //briefRep.BulkInsert(insertBriefList);

                string ESindeName = TableNameConvertHelper.GetElasticsearchIndexName(ElasticLogTableTypeEnum.SinglewalletMerchantApiRequestLog);
                ElasticsearchHelper.BulkInsert(ESindeName, insertESModels);
            }
            else
            {
                BulkInsert(listOfEntity);
                briefRep.BulkInsert(insertBriefList);
            }

            return;
        }


        public void SetExtraProperty(SingleWalletMerchantApiRequestLogEntity model)
        {
            if (!string.IsNullOrWhiteSpace(model.Memo))
            {
                model.Memo = model.Memo.Truncate(50);
            }
        }


        public override long Create(SingleWalletMerchantApiRequestLogEntity model, bool isTransactionScope = false)
        {
            SetExtraProperty(model);
            return base.Create(model, isTransactionScope);
        }

        public List<FullInfoSingleWalletCallbackRequestLogESModel> GetTopSizeByES(DateTime begin , DateTime end , int? merchantID = null)
        {
            var config = new ESKeywordSearchBasicParameter
            {
                Begin = begin,
                End = end,
                PageSize = 100,
                IndexName = TableNameConvertHelper.GetElasticsearchIndexName(ElasticLogTableTypeEnum.SinglewalletMerchantApiRequestLog),
                AdvanceParameter = new ESKeywordAdvanceFilterParameter
                {
                  AlwaysFilter = new List<ESKeywordSearchParameter>(),
                }
            };

            if(merchantID.NotNullNotZeroNotMinus())
            {
                config.AdvanceParameter.AlwaysFilter.Add(new ESKeywordSearchParameter
                {
                    ColumnName = "MerchantID",
                    gte = merchantID.Value,
                    lte = merchantID.Value,
                });
            }

            var ESItems = ElasticsearchHelper.KeywordSearchDataByFirstFilter<FullInfoSingleWalletCallbackRequestLogESModel>(config, null);
            return ESItems;
        }


        public List<SingleWalletMerchantApiRequestLogQueryResult> GetTopSizeByFilter_V2(QuerySingleWalletMerchantApiRequestLogRequestViewModel filter)
        {
            string sql = GenerateWhereSQL_V2(filter);
            var query = QueryByDapper<SingleWalletMerchantApiRequestLogQueryResult>(sql, filter, filter.TimeoutSeconds);
            return query;
        }


        public List<SingleWalletMerchantApiRequestLogEntity> GetByFilter(QuerySingleWalletMerchantApiRequestLogRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<SingleWalletMerchantApiRequestLogEntity>(sql, filter, filter.TimeoutSeconds);
            return query;
        }


        public PagedList<SingleWalletMerchantApiRequestLogQueryResult> GetPagingByFilter(QuerySingleWalletMerchantApiRequestLogRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = GetPagingEntitiesBySQLScript<SingleWalletMerchantApiRequestLogQueryResult>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QuerySingleWalletMerchantApiRequestLogRequestViewModel filter)
        {
            string topSize = filter.TopSize.HasValue ? $" TOP {filter.TopSize.Value}" : "";

            string sql = $@"SELECT {topSize} * FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.UniqueID.IsNotEmptyTextNotNull())
                sql += " AND UniqueID = @UniqueID ";

            if (filter.MerchantID.HasValue)
                sql += " AND MerchantID = @MerchantID ";

            //if (filter.RequestID.IsNotEmptyTextNotNull())
            //    sql += " AND RequestID = @RequestID ";

            if (filter.Category.IsNotEmptyTextNotNull())
                sql += " AND Category = @Category ";

            if (filter.SubCategory.IsNotEmptyTextNotNull())
                sql += " AND SubCategory = @SubCategory ";

            if (filter.CreateTimeBegin.HasValue)
                sql += " AND CreateTime >= @CreateTimeBegin ";

            if (filter.CreateTimeEnd.HasValue)
                sql += " AND CreateTime < @CreateTimeEnd ";

            if (filter.ResponseTimeLargeThan.HasValue)
                sql += " AND ResponseTime >= @ResponseTimeLargeThan ";

            if (filter.ResponseStatusCode.HasValue)
            {
                if (filter.ResponseStatusCode < 0)
                {
                    filter.ResponseStatusCode = filter.ResponseStatusCode.Value.ToAbs();
                    sql += " AND ResponseStatusCode != @ResponseStatusCode ";
                }
                else
                {
                    sql += " AND ResponseStatusCode = @ResponseStatusCode ";
                }
            }

            if (filter.ReferenceTransactionNo.IsNotEmptyTextNotNull())
                sql += " AND ReferenceTransactionNo = @ReferenceTransactionNo ";

            if (filter.RequestContentKeyword.IsNotEmptyTextNotNull())
                sql += $" AND {GenerateLikeQuery("RequestContent", "@RequestContentKeyword")} ";

            if (filter.ResponseTextKeyword.IsNotEmptyTextNotNull())
                sql += $" AND {GenerateLikeQuery("ResponseText", "@ResponseTextKeyword")} ";

            if (filter.DebugInfo2.IsNotEmptyTextNotNull())
                sql += $" AND DebugInfo2 = @DebugInfo2 ";

            sql += $" {filter.ExtraSpecificOrderBy} ";
            return sql;
        }


        private string GenerateWhereSQL_V2(QuerySingleWalletMerchantApiRequestLogRequestViewModel filter)
        {
            string topSize = filter.TopSize.HasValue ? $" TOP {filter.TopSize.Value}" : "";

            string sql = $@"SELECT {topSize} * , brief.ReferTransactionNo as ReferenceTransactionNo  
                            FROM [dbo].[SingleWalletMerchantApiRequestLogBrief] as brief WITH (NOLOCK)";

            bool mustJoinMasterTable = false;

            string whereSQL = "";

            if (filter.UniqueID.IsNotEmptyTextNotNull())
                whereSQL += " AND UniqueID = @UniqueID ";

            if (filter.MerchantID.HasValue)
                whereSQL += " AND brief.MerchantID = @MerchantID ";

            if (filter.PlayerID.NotNullNotZeroNotMinus())
                whereSQL += " AND brief.PlayerID = @PlayerID ";

            if (filter.SubCategory.IsNotEmptyTextNotNull())
                whereSQL += " AND brief.SubCategory = @SubCategory ";

            if (filter.CreateTimeBegin.HasValue)
                whereSQL += " AND brief.CreateTime >= @CreateTimeBegin ";

            if (filter.CreateTimeEnd.HasValue)
                whereSQL += " AND brief.CreateTime < @CreateTimeEnd ";

            if (filter.ResponseTimeLargeThan.HasValue)
                whereSQL += " AND brief.ResponseTime >= @ResponseTimeLargeThan ";

            if (filter.IsErrorCodeOrNot200Response)
            {
                whereSQL += " AND ( brief.ResponseStatusCode != 200 OR brief.ErrorCode >= 400000 )";
            }

            if (filter.ResponseStatusCode.HasValue)
            {
                if (filter.ResponseStatusCode < 0)
                {
                    filter.ResponseStatusCode = filter.ResponseStatusCode.Value.ToAbs();
                    whereSQL += " AND brief.ResponseStatusCode != @ResponseStatusCode ";
                }
                else
                {
                    whereSQL += " AND brief.ResponseStatusCode = @ResponseStatusCode ";
                }
            }

            if (filter.ReferenceTransactionNo.IsNotEmptyTextNotNull())
                whereSQL += " AND brief.ReferTransactionNo = @ReferenceTransactionNo ";

            if (filter.RequestContentKeyword.IsNotEmptyTextNotNull())
            {
                mustJoinMasterTable = true;
                whereSQL += $" AND {GenerateLikeQuery("main.RequestContent", "@RequestContentKeyword")} ";
            }

            if (filter.ResponseTextKeyword.IsNotEmptyTextNotNull())
            {
                mustJoinMasterTable = true;
                whereSQL += $" AND {GenerateLikeQuery("main.ResponseText", "@ResponseTextKeyword")} ";
            }

            if (filter.DebugInfo2.IsNotEmptyTextNotNull())
            {
                mustJoinMasterTable = true;
                whereSQL += $" AND main.DebugInfo2 = @DebugInfo2 ";
            }

            if (mustJoinMasterTable)
            {
                sql += @"INNER JOIN 
                        (
	                        SELECT * FROM [dbo].[SingleWalletMerchantApiRequestLog] WITH (NOLOCK)
	                        WHERE CreateTime >= @CreateTimeBegin AND  CreateTime < @CreateTimeEnd
                        ) as main on brief.UniqueID = main.UniqueID AND brief.CreateTime = main.CreateTime";
            }

            sql += " WHERE 1=1 ";
            sql += whereSQL + $" {filter.ExtraSpecificOrderBy} ";
            return sql;
        }


    }


}
