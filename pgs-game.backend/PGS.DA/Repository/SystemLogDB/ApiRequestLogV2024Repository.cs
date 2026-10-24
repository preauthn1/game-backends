using Dapper;
using target.Core.Helper;
using target.Core.Repository;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System.Collections.Generic;
using System.Linq;

namespace target.DA.Repository
{
    public class ApiRequestLogV2024Repository : FormatMultiplePrimaryKeyRepository<ApiRequestLogV2025Entity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.SystemLogDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[ApiRequestLog]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "UniqueID"; }
        }

        protected override string DefaultSortColumn
        {
            get
            {
                return $"{nameof(ApiRequestLogV2025Entity.PartitionTime)}";
            }
        }



        public void BulkInsertToMultipleStorage(ElasticLogTableTypeEnum tableType, List<ApiRequestLogQueueTempModel> myItems)
        {
            if (myItems.IsEmptyOrNull())
                return;

            var insertESModels = new List<FullInfoApiRequestLogESModel>();

            foreach (var item in myItems)
            {
                var ESModel = DataModelConvertHelper.ConvertToElasticFullRequestModel(item);
                insertESModels.Add(ESModel);
            }

            string ESIndexName = TableNameConvertHelper.GetElasticsearchIndexName(tableType);

            //2026/04 開始 errorLog (有定義ES的) 不再寫入DB，改為只寫入ES
            //2026/04 開始 errorLog (有定義ES的) 不再寫入DB，改為只寫入ES
            //2026/04 開始 errorLog (有定義ES的) 不再寫入DB，改為只寫入ES
            //BulkInsert(SQLTableName, insertBriefList);

            ElasticsearchHelper.BulkInsert(ESIndexName, insertESModels);
        }


        public void TruncateColumnMaxLength(ApiRequestLogV2025Entity model)
        {
            model.RequestUrl = model.RequestUrl.Truncate(200);
            model.RelativeUrl = model.RelativeUrl.Truncate(100);
            model.DebugInfo = model.DebugInfo.Truncate(200);
            model.DebugInfo2 = model.DebugInfo2.Truncate(200);
            model.ReferOrderNo = model.ReferOrderNo.Truncate(50);
        }


        /// <summary>
        /// ExternalVendor 資料量最大。因此特別區別開
        /// </summary>
        /// <returns></returns>
        public void BulkInsertApiReqeustLogToElasticSearch(string tableName, List<FullInfoApiRequestLogESModel> items)
        {
            if (items.IsEmptyOrNull())
                return;

            string indexName = TableNameConvertHelper.GetElasticsearchIndexNameByTableName(tableName);
            ElasticsearchHelper.BulkInsert(indexName, items);
        }


        public FullInfoApiRequestLogESModel GetESFullInformationByUniqueID(string ESIndexName, string uniqueID)
        {
            var query = ElasticsearchHelper.GetByUniqueID<FullInfoApiRequestLogESModel>(ESIndexName, uniqueID);
            return query;
        }


        public List<ApiRequestLogEntity> GetByRawSQL(string sql)
        {
            var query = QueryByDapper<ApiRequestLogEntity>(sql, null, timeoutSeconds: 180);
            return query;
        }


        public List<ApiRequestLogEntity> GetByFilter(QueryApiRequestRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<ApiRequestLogEntity>(sql, filter, filter.TimeoutSeconds);
            return query;
        }


        public PagedList<ApiRequestLogEntity> GetPagingByFilter(QueryApiRequestRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<ApiRequestLogEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryApiRequestRequestViewModel filter, string selectColumns = "*")
        {
            filter.RuleFor(x => x.TargetTableName).Rule_NotEmptyText();
            filter.RuleFor(x => x.CreateTimeBegin).Rule_NotNullDateTime();
            filter.RuleFor(x => x.CreateTimeEnd).Rule_NotNullDateTime();

            string topSQL = filter.TopSize.HasValue ? $" top {filter.TopSize.Value} " : "";

            string sql = $@"SELECT {topSQL} {selectColumns} FROM {filter.TargetTableName} WITH (NOLOCK)
                           WHERE 1=1";

            if (filter.DebugInfo.IsNotEmptyTextNotNull())
                sql += " AND DebugInfo = @DebugInfo ";

            if (filter.DebugInfo2.IsNotEmptyTextNotNull())
                sql += " AND DebugInfo2 = @DebugInfo2 ";

            if (filter.DebugInfo3.IsNotEmptyTextNotNull())
                sql += " AND DebugInfo3 = @DebugInfo3 ";

            if (filter.RelativeUrl.IsNotEmptyTextNotNull())
            {
                filter.RelativeUrl = filter.RelativeUrl.Trim();
                sql += " AND RelativeUrl = @RelativeUrl ";
            }

            if (filter.RelativeUrlKeyword.IsNotEmptyTextNotNull())
            {
                filter.RelativeUrlKeyword = filter.RelativeUrlKeyword.Trim();
                sql += $" AND {GenerateLikeQuery("RelativeUrl", "@RelativeUrlKeyword")} ";
            }

            if (filter.UserIdentity.IsNotEmptyTextNotNull())
                sql += " AND UserIdentity = @UserIdentity ";

            if (filter.IdentityKey.IsNotEmptyTextNotNull())
                sql += " AND IdentityKey = @IdentityKey ";

            if (filter.Id.HasValue)
                sql += " AND Id = @Id ";

            if (filter.ResponseStatusCode.HasValue)
                sql += " AND ResponseStatusCode = @ResponseStatusCode ";

            if (filter.RequestBodyKeyword.IsNotEmptyTextNotNull())
                sql += $" AND {GenerateLikeQuery("RequestBody", "@RequestBodyKeyword")} ";

            if (filter.ResponseTextKeyword.IsNotEmptyTextNotNull())
                sql += $" AND {GenerateLikeQuery("ResponseText", "@ResponseTextKeyword")} ";

            if (filter.RequestUrlKeyword.IsNotEmptyTextNotNull())
                sql += $" AND {GenerateLikeQuery("RequestUrl", "@RequestUrlKeyword")} ";

            if (filter.CreateTimeBegin.HasValue)
                sql += $" AND CreateTime >= @CreateTimeBegin ";

            if (filter.CreateTimeEnd.HasValue)
                sql += $" AND CreateTime < @CreateTimeEnd ";

            sql += $" {filter.ExtraSpecificOrderBy} ";
            return sql;
        }






    }


}
