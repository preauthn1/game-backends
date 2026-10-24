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
    public class EventPrizeItemRepository : FormatIntegerPrimaryKeyRepository<EventPrizeItemEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[EventPrizeItem]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "EventPrizeItemID"; }
        }

        public List<EventPrizeItemEntity> GetByFilter(QueryEventPrizeItemRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<EventPrizeItemEntity>(sql, filter);
            return query;
        }


        public PagedList<EventPrizeItemEntity> GetPagingByFilter(QueryEventPrizeItemRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<EventPrizeItemEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryEventPrizeItemRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.EventPrizeBundleIDs.IsNotEmptyNull())
                sql += " AND EventPrizeBundleID in @EventPrizeBundleIDs ";

            return sql;
        }

    }


}
