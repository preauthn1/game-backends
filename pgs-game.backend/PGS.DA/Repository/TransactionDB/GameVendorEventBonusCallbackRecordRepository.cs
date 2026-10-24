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
    public class GameVendorEventBonusCallbackRecordRepository : FormatStringPrimaryKeyRepository<GameVendorEventBonusCallbackRecordEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.TransactionDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[GameVendorEventBonusCallbackRecord]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "TransactionNo"; }
        }

        public List<GameVendorEventBonusCallbackRecordEntity> GetByFilter(QueryGameVendorEventBonusCallbackRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<GameVendorEventBonusCallbackRecordEntity>(sql, filter);
            return query;
        }


        public PagedList<GameVendorEventBonusCallbackRecordEntity> GetPagingByFilter(QueryGameVendorEventBonusCallbackRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<GameVendorEventBonusCallbackRecordEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryGameVendorEventBonusCallbackRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.GameVendorID.HasValue)
                sql += " AND GameVendorID = @GameVendorID ";

            if (filter.ThirdOrderNo.IsNotEmptyTextNotNull())
                sql += " AND ThirdOrderNo = @ThirdOrderNo ";

            return sql;
        }

    }


}
