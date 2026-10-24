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

    public class SlotGameHistoryLogRepository : IDapperRepository<SlotGameHistoryLogRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_LogDB); }
        }

        public override string TableName
        {
            get { return "[Game].[SlotGameHistoryLog]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "LogID"; }
        }

        public List<SlotGameHistoryLogRawEntity> GetByFilter(QuerySlotGameHistoryLogRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<SlotGameHistoryLogRawEntity>(sql, filter);
            return query;
        }

        public PagedList<SlotGameHistoryLogRawEntity> GetPagingByFilter(QuerySlotGameHistoryLogRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<SlotGameHistoryLogRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QuerySlotGameHistoryLogRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.UserIDs.IsNotEmptyNull())
                sql += " AND UserID in @UserIDs ";

            if (filter.PlatformIDs.IsNotEmptyNull())
                sql += " AND PlatformID in @PlatformIDs ";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in @GameIDs ";

            if (filter.GameTypes.IsNotEmptyNull())
                sql += " AND GameType in @GameTypes ";

            if (filter.CreateBeginTime.HasValue)
                sql += " AND CreateTime >= @CreateBeginTime ";

            if (filter.CreateEndTime.HasValue)
                sql += " AND CreateTime < @CreateEndTime ";

            return sql;
        }

    }


}
