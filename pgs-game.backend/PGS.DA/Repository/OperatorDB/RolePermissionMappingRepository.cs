using Dapper;
using game.Entity;
using target.Core.Repository;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;
using target.Utility.Extensions;
using System.Linq;
using target.Models;

namespace target.DA.Repository
{

    public class RolePermissionMappingRepository : FormatIntegerPrimaryKeyRepository<RolePermissionMappingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.OperationDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[RolePermissionMapping]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }


        public void CopyPermissionFromRole(int roleID, int templateRoleID)
        {
            var filter = new QueryRolePermissionMappingRequestViewModel
            {
                RoleIDs = templateRoleID.ToListCollection()
            };

            var insertCopySettings = GetByFilter(filter);
            DeleteAndInsertNew(roleID, insertCopySettings.Select(x=> x.PermissionID).ToList());
        }


        public void DeleteAndInsertNew(int roleID, List<int> roleIDs)
        {
            roleIDs = roleIDs.Distinct().ToList();
            var models = roleIDs.Select(x => new RolePermissionMappingEntity
                {
                    PermissionID = x,
                    RoleID = roleID
                })
                .ToList();

            DeleteAndInsertNewData("RoleID", roleID, models);
        }

        public bool IsDataExistByFilter(QueryRolePermissionMappingRequestViewModel filter)
        {
            var queryCount = GetCountByFilter(filter);
            return queryCount > 0;
        }


        public int GetCountByFilter(QueryRolePermissionMappingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter, " count(*) ");
            var query = QueryByDapper<int>(sql, filter);
            return query.FirstOrDefault();
        }


        public List<RolePermissionMappingEntity> GetByFilter(QueryRolePermissionMappingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<RolePermissionMappingEntity>(sql, filter);
            return query;
        }


        public PagedList<RolePermissionMappingEntity> GetPagingByFilter(QueryRolePermissionMappingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = GetPagingEntitiesBySQLScript<RolePermissionMappingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryRolePermissionMappingRequestViewModel filter , string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.RoleIDs.IsNotEmptyNull())
                sql += " AND RoleID in @RoleIDs ";

            if (filter.PermissionIDs.IsNotEmptyNull())
                sql += " AND PermissionID in @PermissionIDs ";

            if (filter.PermissionToken.IsNotEmptyTextNotNull())
            {
                sql += @" AND PermissionID in 
                        (  
                            SELECT PermissionID 
                            FROM [dbo].[Permission] WITH (NOLOCK)
                            WHERE PermissionToken = [REDACTED]  
                        ) ";
            }

            if (filter.IsMatchHighRiskPermission)
            {
                filter.MatchAnyPermissionTokens = SystemConstant.AdminSys_HighRiskPermissions;
            }

            if (filter.MatchAnyPermissionTokens.IsNotEmptyNull())
            {
                sql += @" AND PermissionID in 
                        (  
                            SELECT PermissionID 
                            FROM [dbo].[Permission] WITH (NOLOCK)
                            WHERE PermissionToken in @MatchAnyPermissionTokens  
                        ) ";
            }

            return sql;
        }

    }


}
