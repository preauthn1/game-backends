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

    public class EGamingBetRecordRepository : IDapperRepository<EGamingBetRecordRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_LogDB); }
        }

        public override string TableName
        {
            get { return "[ThirdParty].[EGamingBetRecord]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "ID"; }
        }


        public void TransformDataToGameLog()
        {
            ExecuteSPByDapper("PrJob_Transform_EG" , null);
        }
        

        public List<QueryNotCalculateFlowAmountModel> GetNotCalculateFlowAmountRecords()
        {
            string sql =$@" SELECT ID as BetRecordID , BetRecordNo as BetRecordNo, UserID , ValidBetAmount ,WinLossAmount , [BetAmount] as BetAmount, [PayoutAmount] as PayoutAmount
                            FROM {TableName} WITH (NOLOCK)
                            WHERE BetRecordNo in
                            (
	                            SELECT BetRecordNo 
	                            FROM [ThirdParty].[EGamingDataMigratePendingQueue] WITH (NOLOCK)
	                            WHERE IsDone = 0
                            ) ";

            var query = QueryByDapper<QueryNotCalculateFlowAmountModel>(sql, null);
            return query;
        }



        public BetRecordDailySummaryModel GetDailyReportSummary(List<int> platformIDs , DateTime beginUTC , DateTime endUTC)
        {
            string sql = @" SELECT sum(PayoutAmount) as totalPayout , sum(BetAmount) as totalBetAmount , sum(WinLossAmount) as totalWinLoss , count(*) as totalCount
                            FROM [ThirdParty].[EGamingBetRecord] WITH (NOLOCK)
                            WHERE  PlatformID in @platformIDs AND PayoutTimeUTC >= @beginUTC AND PayoutTimeUTC <= @endUTC ";

            var para = new
            {
                platformIDs = platformIDs,
                beginUTC = beginUTC,
                endUTC = endUTC
            };

            var query = QueryByDapper<BetRecordDailySummaryModel>(sql , para);
            return query.FirstOrDefault() ?? new BetRecordDailySummaryModel();
        }


        public void CreateOrUpdate(List<EGamingBetRecordRawEntity> models)
        {
            CreateOrUpdate(models, new List<string> { "BetRecordNo" });
            return;
        }



        public List<EGamingBetRecordRawEntity> GetByFilter(QueryEGamingBetRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<EGamingBetRecordRawEntity>(sql, filter);
            return query;
        }

        public PagedList<EGamingBetRecordRawEntity> GetPagingByFilter(QueryEGamingBetRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<EGamingBetRecordRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryEGamingBetRecordRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.UserIDs.IsNotEmptyNull())
                sql += " AND UserID in @UserIDs ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND ID in @IDs ";

            if (filter.BetRecordNo.IsNotEmptyNull())
                sql += " AND BetRecordNo = @BetRecordNo ";
            return sql;
        }

    }


}
