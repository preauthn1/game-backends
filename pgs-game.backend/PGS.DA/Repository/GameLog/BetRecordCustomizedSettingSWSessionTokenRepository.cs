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
    public class BetRecordCustomizedSettingSWSessionTokenRepository : FormatMultiplePrimaryKeyRepository<BetRecordCustomizedSettingSWSessionTokenEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[BetRecordCustomizedSettingSWSessionToken]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "BetRecordNoPGS"; }
        }

        public void Create(string betRecordNoPGS , string tokenValue)
        {
            if (string.IsNullOrWhiteSpace(tokenValue))
                return;

            var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(betRecordNoPGS);

            Create(new BetRecordCustomizedSettingSWSessionTokenEntity
            {
                BetRecordNoPGS = betRecordNoPGS,
                PartitionTime = recover.Time.Value,
                SettingValue = tokenValue
            });
        }

        public BetRecordCustomizedSettingSWSessionTokenEntity GetByRecordNoPGS(string betRecordNoPGS)
        {
            var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(betRecordNoPGS);

            var query = GetByFilter(new QueryBetRecordCustomizedSettingSWSessionTokenRequestViewModel
            {
                BetRecordNoPGS = betRecordNoPGS,
                PartitionTime = recover.Time
            });

            return query.FirstOrDefault();
        }


        public List<BetRecordCustomizedSettingSWSessionTokenEntity> GetByFilter(QueryBetRecordCustomizedSettingSWSessionTokenRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<BetRecordCustomizedSettingSWSessionTokenEntity>(sql, filter);
            return query;
        }


        public PagedList<BetRecordCustomizedSettingSWSessionTokenEntity> GetPagingByFilter(QueryBetRecordCustomizedSettingSWSessionTokenRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<BetRecordCustomizedSettingSWSessionTokenEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryBetRecordCustomizedSettingSWSessionTokenRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.BetRecordNoPGS.IsNotEmptyTextNotNull())
                sql += " AND BetRecordNoPGS = @BetRecordNoPGS ";

            if (filter.PartitionTime.HasValue)
                sql += " AND PartitionTime = @PartitionTime ";

            return sql;
        }

    }


}
