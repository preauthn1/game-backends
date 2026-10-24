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

    public class APLBetRecordRepository : IDapperRepository<APLBetRecordRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_LogDB); }
        }

        public override string TableName
        {
            get { return "[ThirdParty].[APLBetRecord]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "ID"; }
        }


        public void TransformDataToGameLog()
        {
            ExecuteSPByDapper("PrJob_Transform_APL", null);
        }


        public List<QueryNotSettleBetRecordBriefModel> GetNotSettlementRecords(DateTime maxTimeUTC)
        {
            int status = APLBetRecordStatusEnum.Pending.ToInteger();
            string sql = $@"SELECT BetRecordNo , BetTimeUTC 
                            FROM {TableName} WITH (NOLOCK)
                            WHERE Status = {status} AND BetTimeUTC <= @maxTimeUTC";

            var query = QueryByDapper<QueryNotSettleBetRecordBriefModel>(sql, new { maxTimeUTC  = maxTimeUTC });
            return query;
        }


        public List<QueryNotCalculateFlowAmountModel> GetNotCalculateFlowAmountRecords()
        {
            string sql = $@"SELECT ID as BetRecordID , BetRecordNo as BetRecordNo, UserID , WinLossAmount as ValidBetAmount,WinLossAmount ,GameID as GameID , BetAmount as BetAmount
                            FROM {TableName} WITH (NOLOCK)
                            WHERE BetRecordNo in
                            (
	                            SELECT BetRecordNo 
	                            FROM [ThirdParty].[APLDataMigratePendingQueue] WITH (NOLOCK)
	                            WHERE IsDone = 0
                            ) ";

            var query = QueryByDapper<QueryNotCalculateFlowAmountModel>(sql, null);
            return query;
        }


        public BetRecordDailySummaryModel GetDailyReportSummary(List<int> platformIDs, DateTime beginUTC, DateTime endUTC)
        {
            string sql = $@" SELECT 0 as totalPayout , sum(BetAmount) as totalBetAmount , sum(WinLossAmount) as totalWinLoss , count(*) as totalCount
                             FROM {TableName} WITH (NOLOCK)
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


        public void CreateOrUpdate(List<APLBetRecordRawEntity> models)
        {
            CreateOrUpdate(models, new List<string> { "BetRecordNo" });
            return;
        }


        public List<APLBetRecordRawEntity> GetByFilter(QueryAPLBetRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<APLBetRecordRawEntity>(sql, filter);
            return query;
        }

        public PagedList<APLBetRecordRawEntity> GetPagingByFilter(QueryAPLBetRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<APLBetRecordRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryAPLBetRecordRequestViewModel filter)
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
