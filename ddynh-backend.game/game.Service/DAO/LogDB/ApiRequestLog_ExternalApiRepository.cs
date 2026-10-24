using Dapper;
using System;
using System.Collections.Generic;
using game.Infrastructure.Extensions;
using System.Linq;
using FW.Core.Repository;
using game.Infrastructure;
using FW.Core.Helper;
using game.Entity.Game;
using game.Entity.RawEntity;
using game.Entity.ViewModels.Query;

namespace game.Service.DAO
{

    public class ApiRequestLog_ExternalApiRepository : IDapperRepository<ApiRequestLog_ExternalApiRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_LogDB); }
        }

        public override string TableName
        {
            get { return "[dbo].[ApiRequestLog_ExternalApi]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public List<ApiRequestLog_ExternalApiRawEntity> GetByFilter(QueryApiRequestLog_ExternalApiRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<ApiRequestLog_ExternalApiRawEntity>(sql, filter);
            return query;
        }

        public PagedList<ApiRequestLog_ExternalApiRawEntity> GetPagingByFilter(QueryApiRequestLog_ExternalApiRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<ApiRequestLog_ExternalApiRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryApiRequestLog_ExternalApiRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND ID in @IDs ";

            return sql;
        }

    }


}
