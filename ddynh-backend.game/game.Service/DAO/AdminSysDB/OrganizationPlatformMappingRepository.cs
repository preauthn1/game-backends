using FW.Core.Repository;
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
    public class OrganizationPlatformMappingRepository : IDapperRepository<OrganizationPlatformMapping>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_AdminSys); }
        }

        public override string TableName
        {
            get { return "[Admin].[OrganizationPlatformMapping]"; }
        }




        public List<OrganizationPlatformMapping> GetByOrgId(int orgId)
        {
            var query = GetByOrgIds(new List<int> { orgId });
            return query;
        }


        public List<OrganizationPlatformMapping> GetByOrgIds(IEnumerable<int> orgIds)
        {
            string sql = GenerateColumnInWhereQuery("OrgID");
            var query = this.QueryByDapper<OrganizationPlatformMapping>(sql, new { OrgID = orgIds });
            return query;
        }

        public void UpdateOrganizationPlatform(int orgId, List<int> platformIds)
        {
            //SPϥΨ۩wqTable Value type
            var dt = new DataTable("dbo.ListInt");
            dt.Columns.Add("[Value]", typeof(int));

            foreach (var id in platformIds)
            {
                dt.Rows.Add(id);
            }

            var para = new
            {
                OrgID = orgId,
                SelectedPlatforms = dt
            };

            var result = ExecuteSPByDapper("PrWeb_UpdateOrganizationPlatform", para);
            return;
        }


    }


}
