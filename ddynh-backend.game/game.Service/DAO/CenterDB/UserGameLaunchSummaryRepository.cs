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
using game.Infrastructure.Helper;

namespace game.Service.DAO
{

    public class UserGameLaunchSummaryRepository : IDapperRepository<UserGameLaunchSummaryRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[ThirdParty].[UserGameLaunchSummary]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "ID"; }
        }



        public void CreateOrUpdate(int userID , int gameID)
        {
            var para = new
            {
                userID = userID,
                gameID = gameID
            };

            ExecuteSPByDapper("PrWeb_ThirdPartyGame_CreateOrUpdateUserGameLaunchTime", para);
        }

        public UserGameLaunchSummaryRawEntity GetByUserAndGameID(int userID , int gameID)
        {
            var query = GetByFilter(new QueryUserGameLaunchSummaryRequestViewModel
            {
                UserIDs = userID.ToListCollection(),
                GameIDs = gameID.ToListCollection()
            });

            return query.FirstOrDefault();
        }



        public List<UserGameLaunchSummaryRawEntity> GetByFilter(QueryUserGameLaunchSummaryRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<UserGameLaunchSummaryRawEntity>(sql, filter);
            return query;
        }

        public PagedList<UserGameLaunchSummaryRawEntity> GetPagingByFilter(QueryUserGameLaunchSummaryRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<UserGameLaunchSummaryRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryUserGameLaunchSummaryRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.UserIDs.IsNotEmptyNull())
                sql += " AND UserID in @UserIDs ";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in @GameIDs ";

            return sql;
        }

    }


}
