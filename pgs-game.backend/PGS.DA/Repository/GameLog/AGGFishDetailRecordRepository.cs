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
    public class AGGFishDetailRecordRepository : FormatIntegerPrimaryKeyRepository<AGGFishDetailRecordEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[ThirdParty].[AGGFishDetailRecord]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public List<AGGFishDetailRecordEntity> GetByFilter(QueryAGGFishDetailRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<AGGFishDetailRecordEntity>(sql, filter);
            return query;
        }


        public PagedList<AGGFishDetailRecordEntity> GetPagingByFilter(QueryAGGFishDetailRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<AGGFishDetailRecordEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryAGGFishDetailRecordRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.MerchantIDs.IsNotEmptyNull())
                sql += " AND MerchantID in @MerchantIDs ";

            if(filter.RoundSettleTimeBegin.HasValue)
                sql += " AND RoundSettleTime >= @RoundSettleTimeBegin ";

            if (filter.RoundSettleTimeEnd.HasValue)
                sql += " AND RoundSettleTime < @RoundSettleTimeEnd ";

            return sql;
        }

    }


}
