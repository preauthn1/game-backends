using Dapper;
using System;
using System.Collections.Generic;
using game.Infrastructure.Extensions;
using System.Linq;
using FW.Core.Repository;
using game.Infrastructure;
using FW.Core.Helper;
using game.Entity.Game;
using game.Entity.RawEntity;
using game.Entity.ViewModels.Query;
using game.Entity.ViewModels;

namespace game.Service.DAO
{

    public class APLFishBetRecordRepository : IDapperRepository<APLFishBetRecordRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_LogDB); }
        }

        public override string TableName
        {
            get { return "[ThirdParty].[APLFishBetRecord]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "ID"; }
        }

        public void TransformDataToGameLog()
        {
            ExecuteSPByDapper("PrJob_Transform_APLFish", null);
        }


        public List<QueryNotCalculateFlowAmountModel> GetNotCalculateFlowAmountRecords(int gameID)
        {
            string sql = $@"SELECT ID as BetRecordID , BetRecordNo as BetRecordNo, UserID , WinLossAmount as ValidBetAmount,WinLossAmount ,GameID as GameID , BetAmount as BetAmount
                            FROM {TableName} WITH (NOLOCK)
                            WHERE BetRecordNo in
                            (
	                            SELECT BetRecordNo 
	                            FROM [ThirdParty].[ThirdPartyDataMigratePendingQueue] WITH (NOLOCK)
	                            WHERE IsDone = 0  AND GameID = @gameID
                            ) ";

            var query = QueryByDapper<QueryNotCalculateFlowAmountModel>(sql, new { gameID  = gameID });
            return query;
        }


        public BetRecordDailySummaryModel GetDailyReportSummary(List<int> platformIDs, DateTime beginUTC, DateTime endUTC)
        {
            string sql = $@" SELECT 0 as totalPayout , sum(BetAmount) as totalBetAmount , sum(WinLossAmount) as totalWinLoss , count(*) as totalCount
                             FROM {TableName} WITH (NOLOCK)
                             WHERE  PlatformID in @platformIDs AND BetTimeUTC >= @beginUTC AND BetTimeUTC <= @endUTC ";

            var para = new
            {
                platformIDs = platformIDs,
                beginUTC = beginUTC,
                endUTC = endUTC
            };

            var query = QueryByDapper<BetRecordDailySummaryModel>(sql, para);
            return query.FirstOrDefault() ?? new BetRecordDailySummaryModel();
        }



        public void CreateOrUpdate(List<APLFishBetRecordRawEntity> models)
        {
            CreateOrUpdate(models, new List<string> { "BetRecordNo" });
            return;
        }



        public List<APLFishBetRecordRawEntity> GetByFilter(QueryAPLFishBetRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<APLFishBetRecordRawEntity>(sql, filter);
            return query;
        }


        public PagedList<APLFishBetRecordRawEntity> GetPagingByFilter(QueryAPLFishBetRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<APLFishBetRecordRawEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryAPLFishBetRecordRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.UserIDs.IsNotEmptyNull())
                sql += " AND UserID in @UserIDs ";

            if (filter.PlatformIDs.IsNotEmptyNull())
                sql += " AND PlatformID in @PlatformIDs ";

            return sql;
        }

    }


}
