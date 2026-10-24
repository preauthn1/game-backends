using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.AdminSys;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace game.Service.DAO
{
    public class AdminUserRepository : IDapperRepository<AdminUser>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_AdminSys); }
        }

        public override string TableName
        {
            get { return "[Admin].[Users]"; }
        }
        
        public override string PrimaryKeyColumnName
        {
            get { return "UserID"; }
        }
        

        public List<AdminUser> GetByOrgIds(int orgId)
        {
            string sql = GenerateColumnEqualWhereQuery("OrgID");
            var query = this.QueryByDapper<AdminUser>(sql, new { OrgID = orgId });
            return query;
        }


        public PagedList<AdminUser> GetPagingByOrgIds(QueryAdminUserRequestViewModel filter)
        {
            string sql = $@" SELECT * FROM {TableName} WHERE 1=1";

            if (filter.OrgId.HasValue)
                sql += " AND OrgId = @OrgId ";

            if (!string.IsNullOrWhiteSpace(filter.UserNameKeyword))
                sql += " AND (LoginName LIKE CONCAT('%',@UserNameKeyword,'%') OR Displayname LIKE CONCAT('%',@UserNameKeyword,'%') )";
                

            var query = GetPageingEntitiesBySQLScript<AdminUser>(sql, filter, filter);
            return query;
        }


    }


}
