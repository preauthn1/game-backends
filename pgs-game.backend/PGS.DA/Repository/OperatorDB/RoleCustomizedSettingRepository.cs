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
    public class RoleCustomizedSettingRepository : FormatMultiplePrimaryKeyRepository<RoleCustomizedSettingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.OperationDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[RoleCustomizedSetting]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "RoleID"; }
        }

        public List<RoleCustomizedSettingEntity> GetByFilter(QueryRoleCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<RoleCustomizedSettingEntity>(sql, filter);
            return query;
        }


        public PagedList<RoleCustomizedSettingEntity> GetPagingByFilter(QueryRoleCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<RoleCustomizedSettingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryRoleCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.RoleIDs.IsNotEmptyNull())
                sql += " AND RoleID in @RoleIDs ";

            if (filter.SettingKeys.IsNotEmptyNull())
                sql += " AND SettingKey in @SettingKeys ";

            return sql;
        }

    }


}
