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
    public class EventRuleBundleRepository : FormatIntegerPrimaryKeyRepository<EventRuleBundleEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[EventRuleBundle]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "EventRuleBundleID"; }
        }

        public List<EventRuleBundleEntity> GetByFilter(QueryEventPrizeRuleRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<EventRuleBundleEntity>(sql, filter);
            return query;
        }


        public PagedList<EventRuleBundleEntity> GetPagingByFilter(QueryEventPrizeRuleRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<EventRuleBundleEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryEventPrizeRuleRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.EventIDs.IsNotEmptyNull())
                sql += " AND EventID in @EventIDs ";

            return sql;
        }

    }


}
