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
    public class GameVendorRTPChangeRecordRepository : FormatIntegerPrimaryKeyRepository<GameVendorRTPChangeRecordEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.TransactionDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[GameVendorRTPChangeRecord]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public List<GameVendorRTPChangeRecordEntity> GetByFilter(QueryGameVendorRTPChangeRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<GameVendorRTPChangeRecordEntity>(sql, filter);
            return query;
        }


        public PagedList<GameVendorRTPChangeRecordEntity> GetPagingByFilter(QueryGameVendorRTPChangeRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<GameVendorRTPChangeRecordEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryGameVendorRTPChangeRecordRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += " AND PlayerID in @PlayerIDs ";

            return sql;
        }

    }


}
