using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.DA.Repository
{
    public class PlayerCustomizedSettingRepository : FormatMultiplePrimaryKeyRepository<PlayerCustomizedSettingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[PlayerCustomizedSetting]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "PlayerID"; }
        }


        public void CreateOrUpdate(int playerID, PlayerCustomizedSettingKeyEnum settingKey, string settingValue)
        {
            var queryExistSetting = GetByFilter(new QueryPlayerCustomizedSettingRequestViewModel
            {
                PlayerIDs = playerID.ToListCollection(),
                SettingKeys = settingKey.ToInteger().ToListCollection()

            }).FirstOrDefault();

            if (queryExistSetting == null)
            {
                Create(new PlayerCustomizedSettingEntity
                {
                    PlayerID = playerID,
                    SettingValue = settingValue,
                    SettingKey = settingKey.ToInteger(),
                    UpdateTime = DateTimeHelper.GetCurrentTime(),
                    UpdateBy = "admin",
                });
            }
            else
            {
                queryExistSetting.SettingValue = settingValue;
                Update(queryExistSetting);
            }
        }


        public SystemConfigSettingParseModel GetByUniqueKey(int playerID , PlayerCustomizedSettingKeyEnum settingKey)
        {
            int mySettingKey = settingKey.ToInteger();

            var query = GetByFilter(new QueryPlayerCustomizedSettingRequestViewModel
                {
                    SettingKeys = mySettingKey.ToListCollection(),
                    PlayerIDs = playerID.ToListCollection()
                })
                .FirstOrDefault();

            if (query == null)
                return new SystemConfigSettingParseModel(mySettingKey, "");

            return new SystemConfigSettingParseModel(mySettingKey, query.SettingValue);
        }

        public List<PlayerCustomizedSettingEntity> GetByFilter(QueryPlayerCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<PlayerCustomizedSettingEntity>(sql, filter);
            return query;
        }


        public PagedList<PlayerCustomizedSettingEntity> GetPagingByFilter(QueryPlayerCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<PlayerCustomizedSettingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryPlayerCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += " AND PlayerID in @PlayerIDs ";

            if (filter.SettingKeys.IsNotEmptyNull())
                sql += " AND SettingKey in @SettingKeys ";

            return sql;
        }

    }


}
