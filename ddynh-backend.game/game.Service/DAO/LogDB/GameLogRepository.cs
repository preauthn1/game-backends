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

    public class GameLogRepository : IDapperRepository<GameLogRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_LogDB); }
        }

        public override string TableName
        {
            get { return "[Game].[GameLog]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "LogID"; }
        }


        public List<GameLogRawEntity> GetByFilter(QueryGameLogRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<GameLogRawEntity>(sql, filter);
            return query;
        }

        public PagedList<GameLogRawEntity> GetPagingByFilter(QueryGameLogRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<GameLogRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryGameLogRequestViewModel filter)
        {
            string isNoLock = filter.IsNoLockQuery ? " WITH (NOLOCK) " : "";
            string sql = $@"SELECT * FROM {TableName} {isNoLock}
                            WHERE 1=1 ";

            if (filter.UserIDs.IsNotEmptyNull())
                sql += " AND UserID in @UserIDs ";

            if (filter.PlatformIDs.IsNotEmptyNull())
                sql += " AND PlatformID in @PlatformIDs ";

            if (filter.AreaIDs.IsNotEmptyNull())
                sql += " AND AreaID in @AreaIDs ";

            if (filter.LoginTimeBeginTime.HasValue)
                sql += " AND LoginTime >= @LoginTimeBeginTime ";

            if (filter.LoginTimeEndTime.HasValue)
                sql += " AND LoginTime <= @LoginTimeEndTime ";

            return sql;
        }

    }


}
