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

    public class GameListRepository : IDapperRepository<GameListRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[ServerInfo].[GameList]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "GameID"; }
        }

        public List<GameListRawEntity> GetByFilter(QueryGameListRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<GameListRawEntity>(sql, filter);
            return query;
        }

        public PagedList<GameListRawEntity> GetPagingByFilter(QueryGameListRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<GameListRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryGameListRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in @GameIDs ";

            if (filter.GameMerchantIDs.IsNotEmptyNull())
                sql += " AND GameMerchantID in @GameMerchantIDs ";

            return sql;
        }

    }


}
