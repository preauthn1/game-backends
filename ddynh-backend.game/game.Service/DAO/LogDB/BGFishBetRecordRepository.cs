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

    public class BGFishBetRecordRepository : IDapperRepository<BGFishBetRecordRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_LogDB); }
        }

        public override string TableName
        {
            get { return "[ThirdParty].[BGFishBetRecord]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public BetRecordDailySummaryModel GetDailyReportSummary(List<int> platformIDs, DateTime beginUTC, DateTime endUTC)
        {
            string sql = $@" SELECT sum(PayoutAmount) as totalPayout , sum(BetAmount) as totalBetAmount , sum(WinLossAmount) as totalWinLoss , count(*) as totalCount
                             FROM  {TableName} WITH (NOLOCK)
                             WHERE  PlatformID in @platformIDs AND SettleTimeUTC >= @beginUTC AND SettleTimeUTC <= @endUTC ";

            var para = new
            {
                platformIDs = platformIDs,
                beginUTC = beginUTC,
                endUTC = endUTC
            };

            var query = QueryByDapper<BetRecordDailySummaryModel>(sql, para);
            return query.FirstOrDefault() ?? new BetRecordDailySummaryModel();
        }
        public void TransformDataToGameLog()
        {
            ExecuteSPByDapper("PrJob_Transform_BG", null);
        }

        public void CreateOrUpdate(List<BGFishBetRecordRawEntity> models)
        {
            CreateOrUpdate(models, new List<string> { "BetRecordNo" });
            return;
        }


        public List<QueryNotCalculateFlowAmountModel> GetNotCalculateFlowAmountRecords()
        {
            int gameID = GameListEnum.BG_Fish.ToInteger();
            string sql = $@"SELECT ID as BetRecordID , BetRecordNo as BetRecordNo, UserID , ValidBetAmount as ValidBetAmount,WinLossAmount ,GameID as GameID , BetAmount as BetAmount
                            FROM {TableName} WITH (NOLOCK)
                            WHERE BetRecordNo in
                            (
	                            SELECT BetRecordNo 
	                            FROM [ThirdParty].[ThirdPartyDataMigratePendingQueue] WITH (NOLOCK)
	                            WHERE IsDone = 0 AND GameID = @GameID
                            ) ";

            var query = QueryByDapper<QueryNotCalculateFlowAmountModel>(sql, new { GameID  = gameID });
            return query;
        }

        public List<BGFishBetRecordRawEntity> GetByFilter(QueryBGFishBetRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<BGFishBetRecordRawEntity>(sql, filter);
            return query;
        }

        public PagedList<BGFishBetRecordRawEntity> GetPagingByFilter(QueryBGFishBetRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<BGFishBetRecordRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryBGFishBetRecordRequestViewModel filter)
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
