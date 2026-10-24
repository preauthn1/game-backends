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

    public class GameMerchantRepository : IDapperRepository<GameMerchantRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[ThirdParty].[GameMerchant]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "GameMerchantID"; }
        }

        public List<GameMerchantRawEntity> GetByFilter(QueryGameMerchantRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<GameMerchantRawEntity>(sql, filter);
            return query;
        }

        public PagedList<GameMerchantRawEntity> GetPagingByFilter(QueryGameMerchantRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<GameMerchantRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryGameMerchantRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.GameMerchantIDs.IsNotEmptyNull())
                sql += " AND GameMerchantID in @GameMerchantIDs ";

            if(!string.IsNullOrWhiteSpace(filter.GameMerchantCode))
                sql += " AND GameMerchantCode = @GameMerchantCode ";

            return sql;
        }

    }


}
