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

namespace game.Service.DAO
{

    public class ThirdPartyDataMigratePendingQueueRepository : IDapperRepository<ThirdPartyDataMigratePendingQueueRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_LogDB); }
        }

        public override string TableName
        {
            get { return "[ThirdParty].[ThirdPartyDataMigratePendingQueue]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "BetRecordNo"; }
        }


        public void UpdateIsDone(int gameID , string jobIdentity, List<string> betRecords)
        {
            string sql = $@" UPDATE {TableName} 
                             SET JobIdentity = @JobIdentity , IsDone = 1 , UpdateTime = GETDATE()
                             WHERE BetRecordNo in @BetRecordNoList AND GameID = @GameID";

            var para = new
            {
                GameID = gameID,
                JobIdentity = jobIdentity,
                BetRecordNoList = betRecords
            };

            ExecuteByDapper(sql, para);
        }


        public void CreateNotExistData(int gameID , List<ThirdPartyDataMigratePendingQueueRawEntity> models)
        {
            List<string> betRecords = models.Select(x => x.BetRecordNo).ToList();

            var queryExist = GetExistBetRecordNo(gameID , betRecords);

            //只寫入不存在的資料
            models = models.Where(x => !queryExist.Contains(x.BetRecordNo)).ToList();

            CreateMany(models);
        }
        

        public List<string> GetExistBetRecordNo(int gameID , List<string> betRecordNos)
        {
            string queryColumns = " BetRecordNo ";

            var filter = new QueryThirdPartyDataMigratePendingQueueRequestViewModel
            {
                GameIDs = gameID.ToListCollection(),
                BetRecordNoList = betRecordNos
            };

            string sql = GenerateWhereSQL(filter, queryColumns);
            var query = QueryByDapper<string>(sql, filter);
            return query;
        }
        

        public List<ThirdPartyDataMigratePendingQueueRawEntity> GetByFilter(QueryThirdPartyDataMigratePendingQueueRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<ThirdPartyDataMigratePendingQueueRawEntity>(sql, filter);
            return query;
        }


        public PagedList<ThirdPartyDataMigratePendingQueueRawEntity> GetPagingByFilter(QueryThirdPartyDataMigratePendingQueueRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<ThirdPartyDataMigratePendingQueueRawEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryThirdPartyDataMigratePendingQueueRequestViewModel filter , string queryColumns = "*")
        {
            string sql = $@"SELECT {queryColumns} FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in @GameIDs ";

            if (filter.BetRecordNoList.IsNotEmptyNull())
                sql += " AND BetRecordNo in @BetRecordNoList ";

            return sql;
        }

    }


}
