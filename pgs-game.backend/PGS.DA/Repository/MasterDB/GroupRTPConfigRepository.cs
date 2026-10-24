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
    public class GroupRTPConfigRepository : FormatIntegerPrimaryKeyRepository<GroupRTPConfigEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[RTP].[GroupRTPConfig]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "SettingGroupID"; }
        }

        public List<GroupRTPConfigEntity> GetByFilter(QueryGroupRTPConfigRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<GroupRTPConfigEntity>(sql, filter);
            return query;
        }


        public PagedList<GroupRTPConfigEntity> GetPagingByFilter(QueryGroupRTPConfigRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<GroupRTPConfigEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryGroupRTPConfigRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.SettingGroupIDs.IsNotEmptyNull())
                sql += " AND SettingGroupID in @SettingGroupIDs ";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in @GameIDs ";

            return sql;
        }

    }


}
