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
    public class BOAnnouncementReadRepository : FormatMultiplePrimaryKeyRepository<BOAnnouncementReadEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.OperationDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[BOAnnouncementRead]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public List<BOAnnouncementReadEntity> GetByFilter(QueryBOAnnouncementReadRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<BOAnnouncementReadEntity>(sql, filter);
            return query;
        }


        public PagedList<BOAnnouncementReadEntity> GetPagingByFilter(QueryBOAnnouncementReadRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<BOAnnouncementReadEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryBOAnnouncementReadRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.BOAnnouncementIDs.IsNotEmptyNull())
                sql += " AND BOAnnouncementID in @BOAnnouncementIDs ";

            return sql;
        }

    }


}
