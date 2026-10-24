using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.DA.Repository
{
    public class PlaceBetRaiseRecordRepository : FormatMultiplePrimaryKeyRepository<PlaceBetRaiseRecordEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[ThirdParty].[PlaceBetRaiseRecord]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "BetRecordNoPGS"; }
        }

        public bool IsRecordExist(string betRecordNoPGS , string betRecordNo)
        {
            var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(betRecordNoPGS);

            int queryCount = GetCountByFilter(new QueryPlaceBetRaiseRecordRequestViewModel
            {
                RaiseTransactionNo = betRecordNo,
                BetRecordNoPGS = betRecordNoPGS,
                PartitionTimeMatch = recover.Time

            });

            return queryCount > 0;
        }


        public void UpdateRaiseRecordStatus(string betRecordNoPGS , string raiseTransactionNo , BetRecordRaiseBetStatusEnum targetStatus)
        {
            var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(betRecordNoPGS);

            string sql = $@" UPDATE {TableName} SET Status = @TargetStatus 
                             WHERE BetRecordNoPGS = @BetRecordNoPGS AND PartitionTime = @PartitionTime AND RaiseTransactionNo = @RaiseTransactionNo";

            var param = new
            {
                BetRecordNoPGS = betRecordNoPGS,
                PartitionTime = recover.Time,
                RaiseTransactionNo = raiseTransactionNo,
                TargetStatus = targetStatus.ToInteger()
            };

            ExecuteSqlByDapper(sql, param);
        }


        public List<PlaceBetRaiseRecordEntity> GetRaiseRecordsByPgsNo(string betRecordNoPGS , bool isLockQuery = false)
        {
            var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(betRecordNoPGS);

            var query = GetByFilter(new QueryPlaceBetRaiseRecordRequestViewModel
            {
                BetRecordNoPGS = betRecordNoPGS,
                PartitionTimeMatch = recover.Time,
                IsLockQuery = isLockQuery
            });

            return query;
        }

        public int GetCountByFilter(QueryPlaceBetRaiseRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter, " count(*) ");
            var query = QueryByDapper<int>(sql, filter);
            return query.FirstOrDefault();
        }


        public List<PlaceBetRaiseRecordEntity> GetByFilter(QueryPlaceBetRaiseRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<PlaceBetRaiseRecordEntity>(sql, filter);
            return query;
        }


        public PagedList<PlaceBetRaiseRecordEntity> GetPagingByFilter(QueryPlaceBetRaiseRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<PlaceBetRaiseRecordEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryPlaceBetRaiseRecordRequestViewModel filter, string selectColumns = "*")
        {
            string noLock = filter.IsLockQuery ? "" : " WITH (NOLOCK) ";
            string sql = $@"SELECT {selectColumns} FROM {TableName} {noLock}
                           WHERE 1=1 ";

            //未來有可能加入parition。這樣查應該比較快
            if(string.IsNullOrWhiteSpace(filter.BetRecordNoPGS) && !filter.PartitionTimeMatch.HasValue)
            {
                filter.PartitionTimeMatch = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(filter.BetRecordNoPGS).Time;
            }

            if (filter.BetRecordNoPGS.IsNotEmptyTextNotNull())
                sql += " AND BetRecordNoPGS = @BetRecordNoPGS ";

            if (filter.RaiseTransactionNo.IsNotEmptyTextNotNull())
                sql += " AND RaiseTransactionNo = @RaiseTransactionNo ";

            if (filter.PartitionTimeMatch.HasValue)
                sql += " AND PartitionTime = @PartitionTimeMatch ";

            if (filter.Status.HasValue)
                sql += " AND Status = @Status ";

            return sql;
        }

    }


}
