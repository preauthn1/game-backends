using target.Core.Helper;
using target.Core.Repository;
using target.Models.Table;
using target.Utility.Extensions;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using target.Models.ViewModels;
using target.Models.Paging;
using target.Utility.Helper;

namespace target.DA.Repository
{
    public class ApiRequestLogRepository : FormatLongPrimaryKeyRepository<ApiRequestLogEntity>
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
            get { return "Id"; }
        }

        protected override string DefaultSortColumn
        {
            get
            {
                return $"{nameof(ApiRequestLogEntity.CreateTime)}";
            }
        }

        public void CreateToTargetTable(string tableName , ApiRequestLogEntity model)
        {
            if(string.IsNullOrWhiteSpace(model.RequestBody))
            {
                model.RequestBody = "";
            }

            model.Memo = model.Memo.Truncate(500);
            model.UserIdentity = model.UserIdentity.Truncate(100);
            model.IdentityKey = model.IdentityKey.Truncate(200);

            string insertSQL = SQLHelper.GetInsertCommand<ApiRequestLogEntity>(tableName);
            ExecuteSqlByDapper(insertSQL , model);
        }


        public void CreateToTargetTable(string tableName, IEnumerable<ApiRequestLogEntity> models)
        {
            if (models.IsEmptyOrNull())
                return;

            string insertSQL = SQLHelper.GetInsertCommand<ApiRequestLogEntity>(tableName);

            using (var conn = InstanceSQLConn())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    var resultId = conn.Execute(insertSQL, models, trans);
                    trans.Commit();
                    return;
                }
            }
        }

        public List<ApiRequestLogExtraEntity> GetByRawSQL(string sql)
        {
            var query = QueryByDapper<ApiRequestLogExtraEntity>(sql, null , timeoutSeconds: 180);
            return query;
        }


        public List<ApiRequestLogEntity> DebugTest_GetList()
        {
            string sql = "SELECT * FROM  [dbo].[ApiRequestLogGameAPI] WHERE Id <= 1500";
            var query = QueryByDapper<ApiRequestLogEntity>(sql , null);
            return query;
        }

        public List<ApiRequestLogInjectViewModel> GetBriefTableTopSizeByFilter(QueryApiRequestRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateBriefTableWhereSQL(filter, selectColumns);
            var query = QueryByDapper<ApiRequestLogInjectViewModel>(sql, filter, filter.TimeoutSeconds);
            return query;
        }

        public List<ApiRequestLogInjectViewModel> GetTopSizeViewModelByFilter(QueryApiRequestRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<ApiRequestLogInjectViewModel>(sql, filter, filter.TimeoutSeconds);
            return query;
        }


        public List<ApiRequestLogEntity> GetByFilter(QueryApiRequestRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter , selectColumns);
            var query = QueryByDapper<ApiRequestLogEntity>(sql, filter , filter.TimeoutSeconds);
            return query;
        }


        public PagedList<ApiRequestLogEntity> GetPagingByFilter(QueryApiRequestRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter , selectColumns);
            var query = GetPagingEntitiesBySQLScript<ApiRequestLogEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryApiRequestRequestViewModel filter , string selectColumns = "*")
        {
            filter.RuleFor(x => x.TargetTableName).Rule_NotEmptyText();
            filter.RuleFor(x => x.CreateTimeBegin).Rule_NotNullDateTime();
            filter.RuleFor(x => x.CreateTimeEnd).Rule_NotNullDateTime();

            string topSQL = filter.TopSize.HasValue ? $" top {filter.TopSize.Value} " : "";

            string sql = $@"SELECT {topSQL} {selectColumns} FROM {filter.TargetTableName} WITH (NOLOCK)
                           WHERE 1=1";

            if (filter.UniqueID.IsNotEmptyTextNotNull())
                sql += " AND UniqueID = @UniqueID ";

            if (filter.UniqueIDs.IsNotEmptyNull())
                sql += " AND UniqueID in @UniqueIDs ";

            if (filter.DebugInfo.IsNotEmptyTextNotNull())
                sql += " AND DebugInfo = @DebugInfo ";

            if (filter.DebugInfo2.IsNotEmptyTextNotNull())
                sql += " AND DebugInfo2 = @DebugInfo2 ";

            if (filter.DebugInfo3.IsNotEmptyTextNotNull())
                sql += " AND DebugInfo3 = @DebugInfo3 ";

            if(filter.RelativeUrl.IsNotEmptyTextNotNull())
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

            if (filter.VendorAPITrySearchMerchantID.IsNotEmptyTextNotNull())
            {
                sql += $" AND ((IdentityKey = @VendorAPITrySearchMerchantID) OR (HttpMethod = 'GET' AND {GenerateLikeQuery("RequestUrl" , "@VendorAPITrySearchMerchantID")} ) OR  (HttpMethod = 'POST' AND  {GenerateLikeQuery("RequestBody", "@VendorAPITrySearchMerchantID")} )) ";
            }

            if (filter.Id.HasValue)
                sql += " AND Id = @Id ";

            if(filter.ResponseStatusCode.HasValue)
                sql += " AND ResponseStatusCode = @ResponseStatusCode ";

            if (filter.RequestBodyKeyword.IsNotEmptyTextNotNull())
                sql += $" AND {GenerateLikeQuery("RequestBody", "@RequestBodyKeyword")} ";

            if (filter.ResponseTextKeyword.IsNotEmptyTextNotNull())
                sql += $" AND {GenerateLikeQuery("ResponseText", "@ResponseTextKeyword")} ";

            if (filter.RequestUrlKeyword.IsNotEmptyTextNotNull())
                sql += $" AND {GenerateLikeQuery("RequestUrl", "@RequestUrlKeyword")} ";

            if (filter.ReferOrderNo.IsNotEmptyTextNotNull())
                sql += $" AND ReferOrderNo = @ReferOrderNo ";

            if (filter.CreateTimeBegin.HasValue)
                sql += $" AND CreateTime >= @CreateTimeBegin ";

            if (filter.CreateTimeEnd.HasValue)
                sql += $" AND CreateTime < @CreateTimeEnd ";

            if (filter.MappingTagValuesSearchFilter_SettingKey.HasValue && filter.MappingTagValuesSearchFilter_SettingValue.IsNotEmptyTextNotNull())
            {
                sql += @$" AND UniqueID in
                (
                    SELECT UniqueID FROM [dbo].[MappingTagValuesLarge] WITH (NOLOCK)
                    WHERE [PartitionTime] >= DATEADD(HOUR , -1 , @CreateTimeBegin) AND [PartitionTime] <  DATEADD(HOUR , -1 , @CreateTimeEnd) 
                       AND [SettingKey] = @MappingTagValuesSearchFilter_SettingKey AND  {GenerateLikeQuery("SettingValue", "@MappingTagValuesSearchFilter_SettingValue")} 
                )";
            }              

            sql += $" {filter.ExtraSpecificOrderBy} ";            
            return sql;
        }

        private string GenerateBriefTableWhereSQL(QueryApiRequestRequestViewModel filter, string selectColumns = "*")
        {
            filter.RuleFor(x => x.TargetTableName).Rule_NotEmptyText();
            filter.RuleFor(x => x.CreateTimeBegin).Rule_NotNullDateTime();
            filter.RuleFor(x => x.CreateTimeEnd).Rule_NotNullDateTime();

            string topSQL = filter.TopSize.HasValue ? $" top {filter.TopSize.Value} " : "";

            string sql = $@"SELECT {topSQL} {selectColumns} FROM {filter.TargetTableName} WITH (NOLOCK)
                           WHERE 1=1";

            if (filter.UniqueID.IsNotEmptyTextNotNull())
                sql += " AND UniqueID = @UniqueID ";

            if (filter.UniqueIDs.IsNotEmptyNull())
                sql += " AND UniqueID in @UniqueIDs ";

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
            {
                if(RegexRuleHelper.IsMerchantID(filter.IdentityKey))
                {
                    sql += $" AND MerchantID = {filter.IdentityKey} ";
                }
            }

            if (filter.MerchantID.HasValue)
            {
                sql += $" AND MerchantID = {filter.MerchantID} ";
            }
           
            if (filter.ReferOrderNo.IsNotEmptyTextNotNull())
                sql += $" AND ReferOrderNo = @ReferOrderNo ";

            if (filter.CreateTimeBegin.HasValue)
                sql += $" AND CreateTime >= @CreateTimeBegin ";

            if (filter.CreateTimeEnd.HasValue)
                sql += $" AND CreateTime < @CreateTimeEnd ";
          
            sql += $" {filter.ExtraSpecificOrderBy} ";
            return sql;
        }


    }


}
