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

    public abstract class BaseDataMigratePendingQueueRepository : IDapperRepository<DataMigratePendingQueueRawEntity>
    {

        public void UpdateIsDone(string jobIdentity, List<string> betRecords)
        {
            string sql = $@" UPDATE {TableName} 
                             SET JobIdentity = @JobIdentity , IsDone = 1 , UpdateTime = GETDATE()
                             WHERE BetRecordNo in @BetRecordNoList";

            var para = new
            {
                JobIdentity = jobIdentity,
                BetRecordNoList = betRecords
            };

            ExecuteByDapper(sql, para);
        }


        public void CreateNotExistData(List<DataMigratePendingQueueRawEntity> models)
        {
            List<string> betRecords = models.Select(x => x.BetRecordNo).ToList();

            var queryExist = GetExistBetRecordNo(betRecords);

            //只寫入不存在的資料
            models = models.Where(x => !queryExist.Contains(x.BetRecordNo)).ToList();

            CreateMany(models);
        }


        public List<string> GetExistBetRecordNo(List<string> betRecordNos)
        {
            string queryColumns = " BetRecordNo ";

            var filter = new QueryDataMigratePendingQueueRequestViewModel
            {
                BetRecordNos = betRecordNos
            };

            string sql = GenerateWhereSQL(filter, queryColumns);
            var query = QueryByDapper<string>(sql, filter);
            return query;
        }


        public List<DataMigratePendingQueueRawEntity> GetByFilter(QueryDataMigratePendingQueueRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<DataMigratePendingQueueRawEntity>(sql, filter);
            return query;
        }


        public PagedList<DataMigratePendingQueueRawEntity> GetPagingByFilter(QueryDataMigratePendingQueueRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<DataMigratePendingQueueRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryDataMigratePendingQueueRequestViewModel filter, string queryColumns = "*")
        {
            string noLock = filter.IsLockQuery ? "" : " WITH (NOLOCK) ";

            string sql = $@"SELECT {queryColumns} FROM {TableName} {noLock}
                           WHERE 1=1 ";

            if (filter.IsDone.HasValue)
                sql += " AND IsDone in @IsDone ";

            if (filter.BetRecordNos.IsNotEmptyNull())
                sql += " AND BetRecordNo in @BetRecordNos ";

            return sql;
        }

    }


}
