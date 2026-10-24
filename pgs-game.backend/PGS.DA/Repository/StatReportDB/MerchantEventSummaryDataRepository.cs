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
    public class MerchantEventSummaryDataRepository : FormatIntegerPrimaryKeyRepository<MerchantEventSummaryDataEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.StatReportDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[MerchantEventSummaryData]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "MerchantID"; }
        }
              
        public List<PromotionCostDailySummaryModel> GetMerchantEventCostDailySummary(int merchantID, DateTime beginUTC, DateTime endUTC, double timezone)
        {
            string sql = $@" SELECT CAST(dateadd(HOUR , @Timezone , StatTime) as date) as date, sum(TotalAmount) as Amount
                            FROM {TableName} with (NOLOCK)
                            WHERE MerchantID = @MerchantID AND StatTime >= @Begin AND StatTime < @End
                            GROUP BY CAST(dateadd(HOUR , @Timezone , StatTime) as date)";

            var param = new
            {
                MerchantID = merchantID,
                Begin = beginUTC,
                End = endUTC,
                Timezone = timezone
            };

            var queryDaily = QueryByDapper<PromotionCostDailySummaryModel>(sql, param);
            return queryDaily;
        }

        public List<MerchantEventSummaryDataEntity> GetEachEventSummaryByHierarchyMerchantID(int merchantID, DateTime begin, DateTime end)
        {
            string sql = $@" SELECT 
                                EventID , 
                                EventPrizeType,
                                SUM(TotalPlayers) as TotalPlayers, 
                                SUM([TotalCount]) as [TotalCount] , 
                                SUM(TotalAmount) as TotalAmount 
                            FROM {TableName} WITH (NOLOCK)
                            WHERE StatTime >= @begin AND StatTime < @end AND MerchantID in 
                            (
                                SELECT ChildMerchantID FROM [PGS_Master].[dbo].[MerchantHierarchyChildMapping] WITH (NOLOCK)
                                WHERE MerchantID = @agentMerchantID
                            )
                            GROUP BY EventID, EventPrizeType";

            var param = new
            {
                begin = begin,
                end = end,
                agentMerchantID = merchantID
            };

            var queryDaily = QueryByDapper<MerchantEventSummaryDataEntity>(sql, param);
            return queryDaily;
        }

        public List<EachDateEventSummary> GetEachDateEventSummaryByVendorConfigID(int vendorConfigID, DateTime begin, DateTime end)
        {
            string sql = $@" SELECT 
                                Cast(DATEADD(hour , 8 , StatTime) as date) as [Date],
                                EventID , 
                                EventPrizeType,
                                SUM(TotalPlayers) as TotalPlayers, 
                                SUM([TotalCount]) as [TotalCount] , 
                                SUM(TotalAmount) as TotalAmount 
                            FROM {TableName} WITH (NOLOCK)
                            WHERE StatTime >= @begin AND StatTime < @end AND MerchantID in 
                            (
                                SELECT MerchantID FROM [PGS_Master].[ThirdParty].[VendorConfigMerchantMapping] WITH (NOLOCK)
                                WHERE VendorConfigID = @vendorConfigID
                            )
                            GROUP BY Cast(DATEADD(hour , 8 , StatTime) as date), EventID, EventPrizeType";

            var param = new
            {
                begin = begin,
                end = end,
                vendorConfigID = vendorConfigID
            };

            var queryDaily = QueryByDapper<EachDateEventSummary>(sql, param);
            return queryDaily;
        }

        public MerchantEventSummaryDataEntity GetMerchantTotalSumOnDuration(int merchantID, DateTime begin, DateTime end)
        {
            var queryMerchant = GetEachMerchantTotalSumOnDuration(merchantID.ToListCollection(), begin, end);
            return queryMerchant.FirstOrDefault();
        }


        public List<MerchantEventSummaryDataEntity> GetEachMerchantTotalSumOnDuration(List<int> merchantIDs, DateTime begin, DateTime end)
        {
            string sql = $@" SELECT 
                                MerchantID , 
                                SUM(TotalPlayers) as TotalPlayers, 
                                SUM([TotalCount]) as [TotalCount] , 
                                SUM(TotalAmount) as TotalAmount 
                            FROM {TableName} WITH (NOLOCK)
                            WHERE StatTime >= @begin AND StatTime < @end AND MerchantID in @merchantIDs
                            GROUP BY MerchantID";

            var filter = new
            {
                merchantIDs = merchantIDs,
                begin = begin,
                end = end
            };

            var query = QueryByDapper<MerchantEventSummaryDataEntity>(sql, filter);
            return query;
        }



        public List<MerchantEventSummaryDataEntity> GetByFilter(QueryMerchantEventSummaryDataRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<MerchantEventSummaryDataEntity>(sql, filter);
            return query;
        }


        public PagedList<MerchantEventSummaryDataEntity> GetPagingByFilter(QueryMerchantEventSummaryDataRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<MerchantEventSummaryDataEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryMerchantEventSummaryDataRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.MerchantIDs.IsNotEmptyNull())
                sql += " AND MerchantID in @MerchantIDs ";

            if (filter.StatTimeBegin.HasValue)
                sql += " AND StatTime >= @StatTimeBegin ";

            if (filter.StatTimeEnd.HasValue)
                sql += " AND StatTime < @StatTimeEnd ";

            return sql;
        }

    }


}
