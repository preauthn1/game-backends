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
    public class W88PlayerDailyWalletRemainingRepository : FormatIntegerPrimaryKeyRepository<W88PlayerDailyWalletRemainingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.StatReportDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[W88PlayerDailyWalletRemaining]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "PlayerID"; }
        }


        public List<W88PlayerDailyWalletRemainingEntity> GetDailySummaryEachMerchant(List<int> merchantIDs , DateTime begin , DateTime end)
        {
            string sql = @" SELECT MerchantID , StatTime , sum(Amount) as Amount  
                            FROM [dbo].[W88PlayerDailyWalletRemaining] WITH (NOLOCK)
                            WHERE MerchantID in @merchantIDs AND StatTime >= @begin AND StatTime < @end
                            GROUP BY MerchantID , StatTime ";

            var param = new
            {
                begin = begin,
                end = end,
                merchantIDs = merchantIDs
            };

            var query = QueryByDapper<W88PlayerDailyWalletRemainingEntity>(sql, param);
            return query;
        }



        public List<W88PlayerDailyWalletRemainingEntity> GetByFilter(QueryW88PlayerDailyWalletRemainingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<W88PlayerDailyWalletRemainingEntity>(sql, filter);
            return query;
        }


        public PagedList<W88PlayerDailyWalletRemainingEntity> GetPagingByFilter(QueryW88PlayerDailyWalletRemainingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<W88PlayerDailyWalletRemainingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryW88PlayerDailyWalletRemainingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.MerchantIDs.IsNotEmptyNull())
                sql += " AND MerchantID in @MerchantIDs ";

            if (filter.TargetDate.HasValue)
                sql += " AND StatTime = @TargetDate ";

            if (filter.Begin.HasValue)
                sql += " AND StatTime >= @Begin ";

            if (filter.End.HasValue)
                sql += " AND StatTime < @End ";

            return sql;
        }

    }


}
