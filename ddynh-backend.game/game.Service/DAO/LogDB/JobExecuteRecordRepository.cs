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

    public class JobExecuteRecordRepository : IDapperRepository<JobExecuteRecordRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_LogDB); }
        }

        public override string TableName
        {
            get { return "[ThirdParty].[JobExecuteRecord]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "ID"; }
        }

        public List<JobExecuteRecordRawEntity> GetByFilter(QueryJobExecuteRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<JobExecuteRecordRawEntity>(sql, filter);
            return query;
        }

        public PagedList<JobExecuteRecordRawEntity> GetPagingByFilter(QueryJobExecuteRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<JobExecuteRecordRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryJobExecuteRecordRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND ID in @IDs ";

            if (filter.JobTypes.IsNotEmptyNull())
                sql += " AND JobType in @JobTypes ";

            return sql;
        }

    }


}
