using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models.Attributes;
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
    public class GameListCustomizedSettingRepository : FormatIntegerPrimaryKeyRepository<GameListCustomizedSettingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[GameListCustomizedSetting]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }


        /// <summary>
        /// 若查無資料會返回 預設空model
        /// </summary>
        public SystemConfigSettingParseModel GetParseModelByCache(int gameID, GameListCustomizedSettingKeyEnum key)
        {
            string cacheKey = CacheKey.DbEntity_GameListCustomizedSetting(gameID, key.ToInteger());

            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   var query = GetGameSettingParseModel(gameID, key);
                   return query;

               }, expiredMinute: 15);

            return queryCache;
        }

        public SystemConfigSettingParseModel GetGameSettingParseModel(int gameID , GameListCustomizedSettingKeyEnum key)
        {
            string selectColumns = "  SettingKey , SettingValue ";
            var query = GetByFilter(new QueryGameListCustomizedSettingRequestViewModel
                {
                    GameID = gameID,
                    SettingKey = key.ToInteger()
                } , selectColumns)
                .FirstOrDefault();

            if (query == null)
            {
                return new SystemConfigSettingParseModel
                {
                    SettingKey = key.ToInteger(),
                    SettingValue = key.GetAttributeFromEnum<SettingDefaultValue>()?.Value
                };
            }

            return new SystemConfigSettingParseModel(query.SettingKey, query.SettingValue);
        }

        public List<GameListCustomizedSettingEntity> GetByFilter(QueryGameListCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<GameListCustomizedSettingEntity>(sql, filter);
            return query;
        }


        public PagedList<GameListCustomizedSettingEntity> GetPagingByFilter(QueryGameListCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<GameListCustomizedSettingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryGameListCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.GameID.HasValue)
                sql += " AND GameID = @GameID ";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in @GameIDs ";

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
