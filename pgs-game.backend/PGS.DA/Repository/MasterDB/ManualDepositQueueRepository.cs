using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.DA.Repository
{
    public class ManualDepositQueueRepository : FormatIntegerPrimaryKeyRepository<ManualDepositQueueEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[ManualDepositQueue]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "QueueID"; }
        }

        public void UpdateQueueID(long beforeQueueID , long newQueueID)
        {
            string sql = $@" UPDATE {TableName} SET QueueID = @newQueueID where QueueID = @beforeQueueID";

            var param = new
            {
                beforeQueueID = beforeQueueID,
                newQueueID = newQueueID,
            };

            ExecuteSqlByDapper(sql, param);
        }


        public List<ManualDepositQueueEntity> GetByFilter(QueryManualDepositQueueRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<ManualDepositQueueEntity>(sql, filter);
            return query;
        }


        public PagedList<ManualDepositQueueEntity> GetPagingByFilter(QueryManualDepositQueueRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<ManualDepositQueueEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryManualDepositQueueRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.QueueID.HasValue)
                sql += " AND QueueID = @QueueID ";

            return sql;
        }

    }


}
