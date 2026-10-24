using Dapper;
using game.Entity.Game;
using game.Entity.ViewModels.Query;
using System;
using System.Collections.Generic;
using game.Infrastructure.Extensions;
using System.Linq;
using FW.Core.Repository;
using game.Infrastructure;
using FW.Core.Helper;

namespace game.Service.DAO
{

    public class GameMerchantPlatformConfigRepository : IDapperRepository<GameMerchantPlatformConfigRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[ThirdParty].[GameMerchantPlatformConfig]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public List<GameMerchantPlatformConfigRawEntity> GetByFilter(QueryGameMerchantPlatformConfigRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<GameMerchantPlatformConfigRawEntity>(sql, filter);
            return query;
        }

        public PagedList<GameMerchantPlatformConfigRawEntity> GetPagingByFilter(QueryGameMerchantPlatformConfigRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<GameMerchantPlatformConfigRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryGameMerchantPlatformConfigRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.PlatformIDs.IsNotEmptyNull())
                sql += " AND PlatformID in @PlatformIDs ";

            if (filter.GameMerchantIDs.IsNotEmptyNull())
                sql += " AND GameMerchantID in @GameMerchantIDs ";

            return sql;
        }

    }


}
