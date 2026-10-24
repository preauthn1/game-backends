using Dapper;
using game.Entity;
using target.Core.Repository;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;
using target.Utility.Extensions;
using System.Linq;

namespace target.DA.Repository
{

    public class ExternalApiOtherRequestLogRepository : FormatLongPrimaryKeyRepository<ExternalApiRequestLogEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.SystemLogDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[ExternalApiOtherRequestLog]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
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
            string sql = $@"SELECT * FROM {TableName} WITH (NOLOCK)
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
            
            if (filter.CreateTimeBegin.HasValue)
                sql += " AND CreateTime >= @CreateTimeBegin ";

            if (filter.CreateTimeEnd.HasValue)
                sql += " AND CreateTime < @CreateTimeEnd ";

            if(filter.ResponseStatusCode.HasValue)
                sql += " AND ResponseStatusCode = @ResponseStatusCode ";

            return sql;
        }

    }


}
