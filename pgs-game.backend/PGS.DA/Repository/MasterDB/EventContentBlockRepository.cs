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
    public class EventContentBlockRepository : FormatIntegerPrimaryKeyRepository<EventContentBlockEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[EventContentBlock]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public List<EventContentBlockEntity> GetByFilter(QueryEventContentBlockRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<EventContentBlockEntity>(sql, filter);
            return query;
        }


        public PagedList<EventContentBlockEntity> GetPagingByFilter(QueryEventContentBlockRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<EventContentBlockEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryEventContentBlockRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.EventIDs.IsNotEmptyNull())
                sql += " AND EventID in @EventIDs ";

            if (filter.StatusList.IsNotEmptyNull())
                sql += " AND Status in @StatusList ";

            if (filter.Status.HasValue)
                sql += " AND Status = @Status ";

            if (filter.LayoutTypes.IsNotEmptyNull())
                sql += " AND LayoutType in @LayoutTypes ";

            return sql;
        }

    }


}
