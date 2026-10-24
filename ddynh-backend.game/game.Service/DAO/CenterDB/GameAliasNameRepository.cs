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

    public class GameAliasNameRepository : IDapperRepository<GameAliasNameRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Area].[GameAliasName]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "GameID"; }
        }

        public List<GameAliasNameRawEntity> GetByFilter(QueryGameAliasNameRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<GameAliasNameRawEntity>(sql, filter);
            return query;
        }

        public PagedList<GameAliasNameRawEntity> GetPagingByFilter(QueryGameAliasNameRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<GameAliasNameRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryGameAliasNameRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in @GameIDs ";

            return sql;
        }

    }


}
