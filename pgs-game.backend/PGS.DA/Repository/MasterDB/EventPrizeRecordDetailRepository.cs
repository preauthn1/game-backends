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
    public class EventPrizeRecordDetailRepository : FormatIntegerPrimaryKeyRepository<EventPrizeRecordDetailEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[EventPrizeRecordDetail]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public List<EventPrizeRecordDetailEntity> GetByFilter(QueryEventPrizeRecordDetailRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<EventPrizeRecordDetailEntity>(sql, filter);
            return query;
        }


        public PagedList<EventPrizeRecordDetailEntity> GetPagingByFilter(QueryEventPrizeRecordDetailRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<EventPrizeRecordDetailEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryEventPrizeRecordDetailRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.EventPrizeRecordIDs.IsNotEmptyNull())
                sql += " AND EventPrizeRecordID in @EventPrizeRecordIDs ";

            return sql;
        }

    }


}
