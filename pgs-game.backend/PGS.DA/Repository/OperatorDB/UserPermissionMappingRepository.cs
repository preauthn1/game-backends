using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.DA.Repository
{
    public class UserPermissionMappingRepository : FormatIntegerPrimaryKeyRepository<UserPermissionMappingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.OperationDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[UserPermissionMapping]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public List<UserPermissionMappingEntity> GetByFilter(QueryUserPermissionMappingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<UserPermissionMappingEntity>(sql, filter);
            return query;
        }


        public PagedList<UserPermissionMappingEntity> GetPagingByFilter(QueryUserPermissionMappingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<UserPermissionMappingEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryUserPermissionMappingRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.UserIDs.IsNotEmptyNull())
                sql += " AND UserID in @UserIDs ";

            if (filter.PermissionToken.IsNotEmptyTextNotNull())
            {
                sql += @" AND PermissionID in 
                        (  
                            SELECT PermissionID 
                            FROM [dbo].[Permission] WITH (NOLOCK)
                            WHERE PermissionToken = [REDACTED]  
                        ) ";
            }
            if (filter.PermissionTokenList.IsNotEmptyNull())
            {
                sql += @" AND PermissionID in 
                        (  
                            SELECT PermissionID 
                            FROM [dbo].[Permission] WITH (NOLOCK)
                            WHERE PermissionToken in @PermissionTokenList  
                        ) ";
            }

            return sql;
        }

    }


}
