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
    public class EventPrizeBundleRepository : FormatIntegerPrimaryKeyRepository<EventPrizeBundleEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[EventPrizeBundle]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "EventPrizeBundleID"; }
        }

        public List<EventPrizeBundleEntity> GetByFilter(QueryEventPrizeBundleRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<EventPrizeBundleEntity>(sql, filter);
            return query;
        }


        public PagedList<EventPrizeBundleEntity> GetPagingByFilter(QueryEventPrizeBundleRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<EventPrizeBundleEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryEventPrizeBundleRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.EventIDs.IsNotEmptyNull())
                sql += " AND EventID in @EventIDs ";

            return sql;

        }

    }


}
