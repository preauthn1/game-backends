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

    public class AreaGameListRepository : IDapperRepository<AreaGameListRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Area].[GameList]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public List<AreaGameListRawEntity> GetByFilter(QueryAreaGameListRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<AreaGameListRawEntity>(sql, filter);
            return query;
        }

        public PagedList<AreaGameListRawEntity> GetPagingByFilter(QueryAreaGameListRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<AreaGameListRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryAreaGameListRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.AreaIDs.IsNotEmptyNull())
                sql += " AND AreaID in @AreaIDs ";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in @GameIDs ";

            if (filter.Status.HasValue)
                sql += " AND Status = @Status ";

            return sql;
        }

    }


}
