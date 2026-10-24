using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Game;
using game.Entity.Payment;
using game.Entity.SystemLog;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using game.Infrastructure.Extensions;

namespace game.Service.DAO
{
    public class UserGameInfoRepository : IDapperRepository<UserGameInfoEntity>
    {
        public override string ConnectionString
        {
            get {return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Game].[UserGameInfo]"; }
        }


        public void CreateOrPlusAmount(UserGameInfoEntity model)
        {
            var queryExist = GetByFilter(new QueryUserGameInfoRequestModel
            {
                UserIDs = model.UserID.ToListCollection(),
                GameIDs = model.GameID.ToListCollection()
            });

            bool isExist = queryExist.IsNotEmptyNull();

            if(isExist)
            {
                UpdatePlusAmount(model);
            }
            else
            {
                this.Create(model);
            }
        }

        public void UpdatePlusAmount(UserGameInfoEntity model)
        {
            string updateSQL = @"
                    UPDATE [Game].[UserGameInfo] SET 

	                    WinCount = WinCount + @WinCount,
	                    LoseCount = LoseCount + @LoseCount,
	                    DrawCount = DrawCount + @DrawCount,
                        BetAmount = BetAmount + @BetAmount,
                        PayAmount = PayAmount + @PayAmount,
	                    Score = Score + @Score,
	                    GameTime = GameTime + @GameTime

                    WHERE UserID = @UserID AND GameID = @GameID";

            ExecuteByDapper(updateSQL, model);
        }


        public List<UserGameInfoEntity> GetByFilter(QueryUserGameInfoRequestModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<UserGameInfoEntity>(sql, filter);
            return query;
        }


        public PagedList<UserGameInfoEntity> GetEntityPagingByFilter(QueryUserGameInfoRequestModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = GetPageingEntitiesBySQLScript<UserGameInfoEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryUserGameInfoRequestModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            filter.UserIDs = filter.UserIDsText.SeperateToListIntOrDefaultValue(filter.UserIDs);
            filter.GameIDs = filter.GameIDsText.SeperateToListIntOrDefaultValue(filter.GameIDs);

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in @GameIDs ";

            if (filter.UserIDs.IsNotEmptyNull())
                sql += " AND UserID in @UserIDs ";

            return sql;
        }


        public PagedList<UserGameInfoViewModel> GetPagingByFilter(QueryUserGameInfoRequestModel filter)
        {
            string sql = $@" SELECT A.* , G.GameName FROM {TableName} as A WITH(NOLOCK) 
                             LEFT JOIN [Users].[UserInfo] as u WITH(NOLOCK) on A.UserID = u.UserID                             
                             LEFT JOIN [Area].[Area] as area WITH(NOLOCK) on u.AreaID = area.AreaID
                             LEFT JOIN [ServerInfo].[GameList] as G WITH(NOLOCK) on A.GameID = G.GameID
                             WHERE 1=1 ";
            
            filter.UserIDs = filter.UserIDsText.SeperateToListIntOrDefaultValue(filter.UserIDs);
            filter.GameIDs = filter.GameIDsText.SeperateToListIntOrDefaultValue(filter.GameIDs);
            filter.PlatformIDs = filter.PlatformIDsText.SeperateToListIntOrDefaultValue(filter.PlatformIDs);

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND A.GameID in @GameIDs ";

            if (filter.UserIDs.IsNotEmptyNull())
                sql += " AND A.UserID in @UserIDs ";

            if (!string.IsNullOrWhiteSpace(filter.LoginName))
                sql += " AND u.LoginName = @LoginName ";

            if (filter.PlatformIDs.IsNotEmptyNull())
                sql += " AND area.PlatformID in @PlatformIDs ";


            var query = GetPageingEntitiesBySQLScript<UserGameInfoViewModel>(sql, filter, filter);
            return query;
        }

    }


}
