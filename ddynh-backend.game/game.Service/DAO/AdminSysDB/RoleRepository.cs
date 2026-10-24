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
    public class RoleRepository : IDapperRepository<Role>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_AdminSys); }
        }

        public override string TableName
        {
            get { return "[Admin].[Role]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "RoleID"; }
        }
               
        public List<Role> GetByOrgId(int orgId)
        {
            string sql = GenerateColumnEqualWhereQuery("OrgID");
            var query = QueryByDapper<Role>(sql, new { OrgID = orgId });
            return query;
        }


        public List<Role> GetByFilter(QueryRoleRequestViewModel filter)
        {
            string sql = $@" SELECT * FROM {TableName} WHERE 1=1";

            if (filter.OrgId.HasValue)
                sql += " AND OrgId = @OrgId ";

            if (filter.IsFilterTemplateOnly)
                sql += " AND IsTemplateForCopy = 1 ";

            var query = QueryByDapper<Role>(sql, filter);
            return query;
        }


        public void CreateRoleAndRightByTemplateSetting(int orgId , string prefixText)
        {
            var para = new
            {
                targetOrgId = orgId,
                prefixText = prefixText
            };

            var result = ExecuteSPByDapper("PrWeb_CreateRoleAndRightByTemplateSetting", para);
            return;
        }


        public List<TargetRolesAssinRightCountModel> GetTargetRolesAssignRightCount(int orgId)
        {
            var para = new
            {
                OrgID = orgId
            };

            var result = ExecuteSPResultByDapper<TargetRolesAssinRightCountModel>("PrWeb_GetTargetRolesAssignRightCount", para);
            return result;
        }

    }


}
