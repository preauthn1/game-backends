using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models.Enums;
using target.Models.Models;
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
    public class BetRecordCustomizedSettingRepository : FormatMultiplePrimaryKeyRepository<BetRecordCustomizedSettingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[BetRecordCustomizedSetting]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "BetRecordNoPGS"; }
        }

        protected override bool IsConvertToDbValueType
        {
            get
            {
                return true;
            }
        }

        public SystemConfigSettingParseModel GetParseModelByUniqueKey(string betRecordNoPGS, BetRecordCustomizedSettingEnum settingKey)
        {
            var query = GetByUniqueKey(betRecordNoPGS, settingKey);

            if(query == null)
            {
                return new SystemConfigSettingParseModel(settingKey.ToInteger(), "");
            }

            return new SystemConfigSettingParseModel(settingKey.ToInteger(), query.SettingValue);
        }

        public BetRecordCustomizedSettingEntity GetByUniqueKey(string betRecordNoPGS , BetRecordCustomizedSettingEnum settingKey)
        {
            var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(betRecordNoPGS);

            if (!recover.IsPgsOrderNo)
                return null;

            var query = GetByFilter(new QueryBetRecordCustomizedSettingRequestViewModel
            {
                BetRecordNoPGS = betRecordNoPGS,
                PartitionTimeMatch = recover.Time,
                SettingKey = settingKey.ToInteger(),
            });

            return query.FirstOrDefault();
        }

        public void Create(string betRecordNoPGS, BetRecordCustomizedSettingEnum settingKey , string settingValue)
        {
            var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(betRecordNoPGS);

            this.Create(new BetRecordCustomizedSettingEntity
            {
                BetRecordNoPGS = betRecordNoPGS,
                PartitionTime = recover.Time.Value,
                SettingKey = settingKey.ToInteger(),
                SettingValue = settingValue ?? "",
                CreateTime = DateTimeHelper.GetCurrentTime()
            });
        }


        public List<BetRecordCustomizedSettingEntity> GetByFilter(QueryBetRecordCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<BetRecordCustomizedSettingEntity>(sql, filter);
            return query;
        }


        public PagedList<BetRecordCustomizedSettingEntity> GetPagingByFilter(QueryBetRecordCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<BetRecordCustomizedSettingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryBetRecordCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.BetRecordNoPGS.IsNotEmptyTextNotNull())
                sql += " AND BetRecordNoPGS = @BetRecordNoPGS ";

            if (filter.ListOfBetRecordNoPGS.IsNotEmptyNull())
                sql += " AND BetRecordNoPGS in @ListOfBetRecordNoPGS ";

            if (filter.PartitionTimeMatch.HasValue)
                sql += " AND PartitionTime = @PartitionTimeMatch ";

            if (filter.PartitionTimeBegin.HasValue)
                sql += " AND PartitionTime >= @PartitionTimeBegin ";

            if (filter.PartitionTimeEnd.HasValue)
                sql += " AND PartitionTime < @PartitionTimeEnd ";

            if (filter.SettingKey.HasValue)
                sql += " AND SettingKey = @SettingKey ";

            if (filter.SettingKeys.IsNotEmptyNull())
                sql += " AND SettingKey in @SettingKeys ";

            if (filter.SettingValueMatch.IsNotEmptyTextNotNull())
                sql += " AND SettingValue = @SettingValueMatch ";

            return sql;
        }

    }


}
