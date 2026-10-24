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
using game.Entity.ViewModels;

namespace game.Service.DAO
{

    public class GameUserListRepository : IDapperRepository<GameUserListRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Game].[GameUserList]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "UserID"; }
        }

        public List<OnlineGameUserSummarySpResultModel> GetEachGameOnlineUserCount(int adminUserID)
        {
            var para = new
            {
                intAdminID = adminUserID
            };

            var query = ExecuteSPResultByDapper<OnlineGameUserSummarySpResultModel>("[dbo].[PrWeb_GetOnlineSummary_EachGame]", para);
            return query;
        }
        

        public List<GameUserListRawEntity> GetByFilter(QueryGameUserListRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<GameUserListRawEntity>(sql, filter);
            return query;
        }

        public PagedList<GameUserListRawEntity> GetPagingByFilter(QueryGameUserListRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<GameUserListRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryGameUserListRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.UserIDs.IsNotEmptyNull())
                sql += " AND UserID in @UserIDs ";

            if (filter.ServerIDs.IsNotEmptyNull())
                sql += " AND ServerID in @ServerIDs ";

            return sql;
        }

    }


}
