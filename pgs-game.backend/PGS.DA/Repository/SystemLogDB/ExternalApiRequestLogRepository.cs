using Dapper;
using game.Entity;
using target.Core.Repository;
using target.Models.Enums;
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

namespace target.DA.Repository
{

    public class ExternalApiRequestLogRepository : FormatLongPrimaryKeyRepository<ExternalApiRequestLogEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.SystemLogDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[ExternalApiRequestLog]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }
             


        public FullInfoThirdPartyApiRequestLogESModel GetElasticDataByUniqueID(string uniqueID)
        {
            var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(uniqueID);

            string indexName = TableNameConvertHelper.GetElasticsearchIndexName(ElasticLogTableTypeEnum.ExternalApiRequestlog);
            var query = ElasticsearchHelper.GetByUniqueID<FullInfoThirdPartyApiRequestLogESModel>(indexName , uniqueID);
            return query;
        }

        public List<FullInfoThirdPartyApiRequestLogESModel> SearchElasticDataByKeyword(ESKeywordSearchBasicParameter basicConfig, List<ESKeywordSearchParameter> filterList)
        {          
            if(string.IsNullOrWhiteSpace(basicConfig.IndexName))
            {
                string indexName = TableNameConvertHelper.GetElasticsearchIndexName(ElasticLogTableTypeEnum.ExternalApiRequestlog);
                basicConfig.IndexName = indexName;
            }

            var query = ElasticsearchHelper.KeywordSearchDataByFirstFilter<FullInfoThirdPartyApiRequestLogESModel>(basicConfig, filterList);
            return query;
        }

        public List<ExternalApiRequestLogEntity> GetByFilter(QueryExternalApiRequestLogRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<ExternalApiRequestLogEntity>(sql, filter);
            return query;
        }


        public PagedList<ExternalApiRequestLogEntity> GetPagingByFilter(QueryExternalApiRequestLogRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = GetPagingEntitiesBySQLScript<ExternalApiRequestLogEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryExternalApiRequestLogRequestViewModel filter)
        {
            string topSize = filter.PageSize > 0 ? $" TOP {filter.PageSize} " : " TOP 50 "; //太吃效能 默認最多查50
            string sql = $@"SELECT {topSize} * FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND ID in @IDs ";

            if (filter.Id.HasValue)
                sql += " AND ID = @ID ";

            if (filter.RequestID.IsNotEmptyTextNotNull())
                sql += " AND RequestID = @RequestID ";

            if (filter.Category.IsNotEmptyTextNotNull())
                sql += " AND Category = @Category ";

            if (filter.SubCategory.IsNotEmptyTextNotNull())
                sql += " AND SubCategory = @SubCategory ";

            if (filter.RequestContentKeyword.IsNotEmptyTextNotNull())
                sql += $" AND {GenerateLikeQuery("RequestContent", "@RequestContentKeyword")} ";

            if (filter.ResponseTextKeyword.IsNotEmptyTextNotNull())
                sql += $" AND {GenerateLikeQuery("ResponseText", "@ResponseTextKeyword")} ";

            if (filter.RequestUrlKeyword.IsNotEmptyTextNotNull())
                sql += $" AND {GenerateLikeQuery("RequestUrl", "@RequestUrlKeyword")} ";

            if (filter.CreateTimeBegin.HasValue)
                sql += " AND CreateTime >= @CreateTimeBegin ";

            if (filter.CreateTimeEnd.HasValue)
                sql += " AND CreateTime < @CreateTimeEnd ";

            if(filter.ResponseStatusCode.HasValue)
                sql += " AND ResponseStatusCode = @ResponseStatusCode ";

            if (!string.IsNullOrWhiteSpace(filter.DebugInfo2))
                sql += " AND DebugInfo2 = @DebugInfo2 ";

            if (!string.IsNullOrWhiteSpace(filter.OrderByColumn))
            {
                RegexRuleHelper.ThrowSqlInjectionException(filter.OrderByColumn);

                string ASC = filter.IsASC ? "" : " desc ";
                sql += $" ORDER BY {filter.OrderByColumn} {ASC} ";
            }

            return sql;
        }

    }


}
