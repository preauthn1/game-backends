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
    public class BetRecordCustomizedSettingAGGEventRepository : FormatMultiplePrimaryKeyRepository<BetRecordCustomizedSettingAGGEventEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[BetRecordCustomizedSettingAGGEvent]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "BetRecordNoPGS"; }
        }

        public void Create(string betRecordNoPGS, BetRecordCustomizedSettingEnum settingKey, string settingValue)
        {
            var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(betRecordNoPGS);

            this.Create(new BetRecordCustomizedSettingAGGEventEntity
            {
                BetRecordNoPGS = betRecordNoPGS,
                PartitionTime = recover.Time.Value,
                SettingKey = settingKey.ToInteger(),
                SettingValue = settingValue ?? "",
                CreateTime = DateTimeHelper.GetCurrentTime()
            });
        }


        public List<BetRecordCustomizedSettingAGGEventEntity> GetByFilter(QueryBetRecordCustomizedSettingAGGEventRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<BetRecordCustomizedSettingAGGEventEntity>(sql, filter);
            return query;
        }


        public PagedList<BetRecordCustomizedSettingAGGEventEntity> GetPagingByFilter(QueryBetRecordCustomizedSettingAGGEventRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<BetRecordCustomizedSettingAGGEventEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryBetRecordCustomizedSettingAGGEventRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            return sql;
        }

    }


}
