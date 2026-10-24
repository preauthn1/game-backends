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

    public class DetailLogRepository : IDapperRepository<DetailLogRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_PerGameLog); }
        }

        public override string TableName
        {
            get { return "[PerLog].[DetailLog]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "GameTime"; }
        }

        public List<DetailLogRawEntity> GetByFilter(QueryDetailLogRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<DetailLogRawEntity>(sql, filter);
            return query;
        }

        public PagedList<DetailLogRawEntity> GetPagingByFilter(QueryDetailLogRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<DetailLogRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryDetailLogRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.UserIDs.IsNotEmptyNull())
                sql += " AND UserID in @UserIDs ";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in @GameIDs ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND ID in @IDs ";

            if (filter.LogIDs.IsNotEmptyNull())
                sql += " AND LogID in @LogIDs ";

            if (filter.GameTimeBegin.HasValue)
                sql += " AND GameTime >= @GameTimeBegin ";

            if (filter.GameTimeEnd.HasValue)
                sql += " AND GameTime < @GameTimeEnd ";

            return sql;
        }

    }


}
