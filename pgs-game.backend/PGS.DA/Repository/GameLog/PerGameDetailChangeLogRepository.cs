using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models.Enums;
using target.Models.Models;
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
    public class PerGameDetailChangeLogRepository : FormatIntegerPrimaryKeyRepository<PerGameDetailChangeLogEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[PerGameDetailChangeLog]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public SettleInfoChangedParsingViewModel ConvertToParseModel(PerGameDetailChangeLogEntity query)
        {
            var convert = query.ReturnInjecteObject<SettleInfoChangedParsingViewModel>();

            if (!string.IsNullOrWhiteSpace(query.BeforeJson))
            {
                convert.BeforeObject = query.BeforeJson.JsonConvertToModel<PerGameChangeSettleInfoParameter>();
            }

            if (!string.IsNullOrWhiteSpace(query.AfterJson))
            {
                convert.AfterObject = query.AfterJson.JsonConvertToModel<PerGameChangeSettleInfoParameter>();
            }

            return convert;
        }

        public PagedList<UFA5MinutesReportSqlQueryModel> GetUFA_5MinutesSummary(UFA5MinutesReportSqlQueryFilterParameter filter)
        {
            filter.OrderByColumn = "PlayerID, g.GameType ";

            string extraWhere = "";

            if (filter.PlayerIDs.IsNotEmptyNull())
            {
                extraWhere += " AND PlayerID in @PlayerIDs ";
            }

            string prefixSQL = " DECLARE @DEFAULT_TIME datetime = cast('2020-01-01 00:00:00' as datetime); ";
            string sql = $@" 
                 SELECT  
                     PlayerID , 
                     PlayerName ,
                     g.gameID , 
                     g.GameType ,
                     CAST(DATEDIFF(MINUTE, @DEFAULT_TIME, GameTime)  AS bigint) / 5 as TimeID , 
                     count(*) as TotalRound , 
                     sum(BetAmount) as TotalBetAmount,
                     sum(ValidAmount) as TotalValidAmount,
                     sum(PayoutAmount) as TotalPayoutAmount,
                     sum(TaxAmount) as TotalTaxAmount,
                     sum(UpdateAmount) as TotalWinLossAmount,
                     max(GameTime) as LastGameTime

                 FROM [dbo].[PerGameDetailLog] as per WITH (NOLOCK)
                 LEFT JOIN [PGS_Master].[dbo].[GameList] as g  WITH (NOLOCK) on per.GameID = g.GameID
                 WHERE GameTime >= @BeginTime AND GameTime < @EndTime  AND MerchantID = @MerchantID {extraWhere}
                 GROUP BY PlayerID , PlayerName , g.gameID , g.GameType , CAST(DATEDIFF(MINUTE, @DEFAULT_TIME, GameTime)  AS bigint) / 5";

            var query = GetPagingEntitiesBySQLScript<UFA5MinutesReportSqlQueryModel>(sql, filter, filter, prefixSQL);
            return query;
        }


        public ServerBetSummaryResultModel GetBetSummaryByServer(int serverID, DateTime begin, DateTime end)
        {
            string sql = @" SELECT sum(BetAmount) as TotalBetAmount , sum(UpdateAmount) as TotalWinLoss
                            FROM [dbo].[PerGameDetailLog] WITH (NOLOCK)
                            WHERE GameTime >= @begin AND GameTime < @end AND ServerID = @serverID ";

            var param = new
            {
                serverID = serverID,
                begin = begin,
                end = end
            };

            var query = QueryByDapper<ServerBetSummaryResultModel>(sql, param);
            return query.FirstOrDefault();
        }


        public List<PerGameDetailChangeLogEntity> GetByFilter(QueryPerGameDetailChangeLogRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<PerGameDetailChangeLogEntity>(sql, filter);
            return query;
        }


        public PagedList<PerGameDetailChangeLogEntity> GetPagingByFilter(QueryPerGameDetailChangeLogRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<PerGameDetailChangeLogEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryPerGameDetailChangeLogRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.TransactionNo.IsNotEmptyTextNotNull())
                sql += " AND TransactionNo = @TransactionNo ";

            if (filter.MerchantIDs.IsNotEmptyNull())
                sql += " AND MerchantID in @MerchantIDs ";

            if (filter.PerGameDetailLogIDs.IsNotEmptyNull())
                sql += " AND PerGameDetailLogID in @PerGameDetailLogIDs ";

            if (filter.CreateTimeBegin.HasValue)
                sql += " AND CreateTime >= @CreateTimeBegin ";

            if (filter.CreateTimeEnd.HasValue)
                sql += " AND CreateTime < @CreateTimeEnd ";

            return sql;
        }

    }


}
