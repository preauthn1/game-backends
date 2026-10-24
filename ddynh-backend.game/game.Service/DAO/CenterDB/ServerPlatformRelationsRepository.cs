using Dapper;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Area;
using game.Entity.Game;
using game.Entity.Payment;
using game.Entity.SystemLog;
using game.Infrastructure;
using game.Infrastructure.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using game.Infrastructure.Extensions;
using game.Entity.ViewModels;

namespace game.Service.DAO
{
    public class ServerPlatformRelationsRepository : IDapperRepository<ServerPlatformRelationsEntity>
    {
        public override string ConnectionString
        {
            get {return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[ServerInfo].[ServerPlatformRelations]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }


        public void CopyServerRelationByTemplatePlatform(int copy_to_platformID , int template_platformID)
        {
            var para = new
            {
                copy_to_platformID = copy_to_platformID,
                copy_template_platformID = template_platformID
            };

            ExecuteSPByDapper("PrWeb_CopyServerPlatformRelation", para);
        }


        public List<ServerPlatformRelationsEntity> GetAvailableServersByAdminUserID(int adminID)
        {
            string sql = @" SELECT * FROM [ServerInfo].[ServerPlatformRelations]
                            WHERE PlatformID in
                            (
	                            SELECT PlatformID 
                                FROM [Area].[PlatformAdminUser]
	                            WHERE AdminID = @AdminID
                            )";

            //ps. 此查詢不考慮status (大部分頁面是status關閉也要顯示出來)
            var query = QueryByDapper<ServerPlatformRelationsEntity>(sql , new { AdminID  = adminID });
            return query.ToList();
        }

        public ServerPlatformRelationsEntity GetByUniqueCondition(int platformID , int serverID)
        {
            string sql = @" SELECT * FROM [ServerInfo].[ServerPlatformRelations]
                            WHERE PlatformID = @platformID AND serverID = @serverID";

            var query = QueryByDapper<ServerPlatformRelationsEntity>(sql, new { platformID = platformID , serverID = serverID });
            return query.FirstOrDefault();
        }


        public List<ServerPlatformRelationsEntity> GetByPlatformID(int platformID)
        {
            string sql = @" SELECT * FROM [ServerInfo].[ServerPlatformRelations]
                            WHERE PlatformID = @platformID";

            var query = QueryByDapper<ServerPlatformRelationsEntity>(sql, new { platformID = platformID });
            return query;
        }
        


        public List<int> GetAvailableServerIDsByAdminUserID(int adminID)
        {
            var query = GetAvailableServersByAdminUserID(adminID);
            return query.Select(x => x.ServerID).ToList();
        }


    }


}
