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
    public class MerchantVendorConfigMigrateRecordRepository : FormatIntegerPrimaryKeyRepository<MerchantVendorConfigMigrateRecordEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.StatReportDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[MerchantVendorConfigMigrateRecord]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public List<MerchantGameSummaryCorrectionModel> GetNeedCorrectionData(int vendorConfigID , int gameVendor, DateTime begin , DateTime end , double timezone)
        {
            var query = GetByFilter(new QueryMerchantVendorConfigMigrateRecordRequestViewModel
            {
                MatchVendorConfigID = vendorConfigID,
                StatTimeBetweenLeft = begin,
                StatTimeBetweenRight = end
            });

            List<MerchantGameSummaryCorrectionModel> listResult = new List<MerchantGameSummaryCorrectionModel>();

            if (query.IsEmptyOrNull())
                return listResult;


            //因為每一個商戶的range時間可能不同 所以還是逐筆查詢比較好 雖然很蠢
            foreach(var item in query)
            {
                DateTime beginTime = new List<DateTime> { item.StatTimeBegin, begin }.Max();
                DateTime endTime = new List<DateTime> { item.StatTimeEnd, end }.Min();

                var param = new
                {
                    gameID_min = gameVendor,
                    gameID_max = gameVendor + 999,
                    merchantID = item.MerchantID,
                    begin = beginTime,
                    end = endTime,
                    timezone = timezone,
                };

                string mySql = @"
SELECT

MerchantID,
CAST(DATEADD(hour , @timezone , statTime) as date) as StatDate,
(GameID / 1000 * 1000) as GameVendorID,
sum(TotalRound) as TotalRound,
sum(BetAmount) as BetAmount,
sum(ValidAmount) as ValidAmount,
sum(PayoutAmount) as PayoutAmount,
sum(WinLossAmount) as WinLossAmount

FROM[PGS_StatReport].[dbo].[MerchantGameSummaryData] WITH(NOLOCK)
WHERE MerchantID = @merchantID AND GameID >= @gameID_min AND GameID <= @gameID_max AND StatTime >= @begin AND StatTime<@end
GROUP BY MerchantID, CAST(DATEADD(hour , @timezone , statTime) as date) , (GameID / 1000 * 1000)";

                var queryData = QueryByDapper<MerchantGameSummaryCorrectionModel>(mySql, param);

                bool isMinus = false;

                if(item.VendorConfigIDAfter == vendorConfigID)
                {
                    isMinus = true;
                }

                foreach(var eachQuery in queryData)
                {
                    eachQuery.IsMinus = isMinus;
                }

                listResult.AddRange(queryData);
            }

            return listResult;
        }

        public List<EventSummaryCorrectionModel> GetNeedCorrectionEventSummary(int vendorConfigID, DateTime begin, DateTime end, double timezone)
        {
            var query = GetByFilter(new QueryMerchantVendorConfigMigrateRecordRequestViewModel
            {
                MatchVendorConfigID = vendorConfigID,
                StatTimeBetweenLeft = begin,
                StatTimeBetweenRight = end
            });

            List<EventSummaryCorrectionModel> listResult = new List<EventSummaryCorrectionModel>();

            if (query.IsEmptyOrNull())
                return listResult;

            //因為每一個商戶的range時間可能不同 所以還是逐筆查詢比較好 雖然很蠢
            foreach (var item in query)
            {
                DateTime beginTime = new List<DateTime> { item.StatTimeBegin, begin }.Max();
                DateTime endTime = new List<DateTime> { item.StatTimeEnd, end }.Min();

                var param = new
                {
                    merchantID = item.MerchantID,
                    begin = beginTime,
                    end = endTime,
                    timezone = timezone,
                };

                string mySql = @"
SELECT

MerchantID,
CAST(DATEADD(hour , @timezone , statTime) as date) as StatDate,
EventID,
EventPrizeType,
sum(TotalCount) as TotalCount,
sum(TotalAmount) as TotalAmount

FROM[PGS_StatReport]. [dbo].[MerchantEventSummaryData] WITH(NOLOCK)
WHERE MerchantID = @merchantID AND StatTime >= @begin AND StatTime<@end
GROUP BY MerchantID, CAST(DATEADD(hour , @timezone , statTime) as date) , EventID , EventPrizeType

";

                var queryData = QueryByDapper<EventSummaryCorrectionModel>(mySql, param);

                bool isMinus = false;

                if (item.VendorConfigIDAfter == vendorConfigID)
                {
                    isMinus = true;
                }

                foreach (var eachQuery in queryData)
                {
                    eachQuery.IsMinus = isMinus;
                }

                listResult.AddRange(queryData);
            }

            return listResult;
        }



        public List<MerchantVendorConfigMigrateRecordEntity> GetByFilter(QueryMerchantVendorConfigMigrateRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<MerchantVendorConfigMigrateRecordEntity>(sql, filter);
            return query;
        }


        public PagedList<MerchantVendorConfigMigrateRecordEntity> GetPagingByFilter(QueryMerchantVendorConfigMigrateRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<MerchantVendorConfigMigrateRecordEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryMerchantVendorConfigMigrateRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            if (filter.MatchVendorConfigID.HasValue)
                sql += " AND ( VendorConfigIDBefore = @MatchVendorConfigID OR VendorConfigIDAfter = @MatchVendorConfigID )";

            if (filter.StatTimeBetweenLeft.HasValue && filter.StatTimeBetweenRight.HasValue)
                sql += " AND ( StatTimeBegin >= @StatTimeBetweenLeft OR StatTimeEnd <= @StatTimeBetweenRight )";

            return sql;
        }

    }


}
