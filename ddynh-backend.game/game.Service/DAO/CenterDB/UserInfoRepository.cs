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
using Dapper;

namespace game.Service.DAO
{
    public class UserInfoRepository : IDapperRepository<UserInfoRawEntity>
    {
        public override string ConnectionString
        {
            get {return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "UserID"; }
        }

        public override string TableName
        {
            get { return "[Users].[UserInfo]"; }
        }


        public void UpdateSumFlowAmount(int userID , long flowAmount)
        {
            string sql = $@"UPDATE Users.UserInfo 
                            SET SumFlowAmount = SumFlowAmount + @flowAmount
                            WHERE userID = @userID ";

            var para = new { flowAmount  , userID };
            ExecuteByDapper(sql, para);
        }


        /// <summary>
        /// 取得使用率很高的 幾項User資訊欄位
        /// </summary>
        public List<UserInfoBriefModel> GetUniversalUserInfoByUserIDs(List<int> userIDs)
        {
            string sql = $@"SELECT u.UserID , u.LoginName , u.GradeID , u.UserType  ,  u.CreateTime , u.LastLogonTime , u.LastLogonIPAddress  ,p.PlatformID , p.Name as PlatformName , u.AreaID , a.AreaName 
                            FROM [Users].[UserInfo] as u WITH (NOLOCK)
                            LEFT JOIN Area.Area as a WITH (NOLOCK) on u.AreaID = a.AreaID
                            LEFT JOIN Area.Platform as p WITH (NOLOCK) on a.PlatformID = p.PlatformID 
                            WHERE u.UserID in ( SELECT Id FROM #tempIDs ) ";

            var query = ExecuteQueryWithLargeIDs<UserInfoBriefModel>(sql , userIDs);
            return query;
        }
       
    }

}
