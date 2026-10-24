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
    public class GameVendorAccountingSummaryFetchRepository : FormatIntegerPrimaryKeyRepository<GameVendorAccountingSummaryFetchEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[WagerFetch].[GameVendorAccountingSummaryFetch]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }



        public void DeleteDataByFilter(FetchAccountingJobRequestModel filter)
        {
            string sql = $@"DELETE FROM [WagerFetch].[GameVendorAccountingSummaryFetch]
                            WHERE StatDate >= @BeginDate AND StatDate < @EndData ";

            string extraWhere = "";

            if(filter.GameVendor > 0)
            {
                extraWhere += " AND GameVendorType = @GameVendor ";
            }
            if (filter.VendorConfigIDs.IsNotEmptyNull())
            {
                extraWhere += " AND VendorConfigID in @VendorConfigIDs ";
            }

            sql = sql + extraWhere;

            var para = new
            {
                BeginDate = filter.Begin.Value.AddHours(8).ToString("yyyy-MM-dd"),
                EndData = filter.End.Value.AddHours(8).ToString("yyyy-MM-dd"),
                GameVendor = filter.GameVendor,
                VendorConfigIDs = filter.VendorConfigIDs
            };

            ExecuteSqlByDapper(sql, para);
        }

        public List<ThirdPartyAccountingFetchGameSummryModel> GetEachGameSummary(int vendorConfigID , string targetDateTW)
        {
            string sql = $@"SELECT GameID , GameCode , StatDate , sum(TotalRound) as TotalRound, sum(TotalBetAmount) as TotalBetAmount, sum(TotalWinLoss) as TotalWinLoss
                            FROM [WagerFetch].[GameVendorAccountingSummaryFetch]
                            WHERE VendorConfigID = @vendorConfigID AND StatDate = @targetDateTW
                            GROUP BY GameID , GameCode , StatDate ";

            var para = new
            {
                vendorConfigID = vendorConfigID,
                targetDateTW = targetDateTW,
            };

            var queryByDate = QueryByDapper<ThirdPartyAccountingFetchGameSummryModel>(sql, para);
            return queryByDate.ToList();
        }


        public DateTime GetLastFetchDate(GameVendorTypeEnum gameVendorType)
        {
            string sql = $@"SELECT top 1 StatDate 
                           from {TableName} with (NOLOCK) 
                           WHERE GameVendorType = @GameVendorType
                           order by StatDate desc";

            var para = new
            {
                GameVendorType = gameVendorType.ToInteger()
            };

            var queryByDate = QueryByDapper<DateTime>(sql, para);
            return queryByDate.FirstOrDefault();
        }

        public List<GameVendorAccountingSummaryFetchEntity> GetByFilter(QueryGameVendorAccountingSummaryFetchRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<GameVendorAccountingSummaryFetchEntity>(sql, filter);
            return query;
        }


        public PagedList<GameVendorAccountingSummaryFetchEntity> GetPagingByFilter(QueryGameVendorAccountingSummaryFetchRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<GameVendorAccountingSummaryFetchEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryGameVendorAccountingSummaryFetchRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            return sql;
        }

    }


}
