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
    public class BOAnnouncementRepository : FormatIntegerPrimaryKeyRepository<BOAnnouncementEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.OperationDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[BOAnnouncement]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "BOAnnouncementID"; }
        }

        public List<BOAnnouncementEntity> GetByFilter(QueryBOAnnouncementRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<BOAnnouncementEntity>(sql, filter);
            return query;
        }


        public PagedList<BOAnnouncementEntity> GetPagingByFilter(QueryBOAnnouncementRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<BOAnnouncementEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryBOAnnouncementRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.BOAnnouncementIDs.IsNotEmptyNull())
                sql += " AND BOAnnouncementID in @BOAnnouncementIDs ";

            if (!filter.IsIncludeDeleteData)
                sql += $" AND Status != {NormalActiveStatus.PermanentDelete.ToInteger()} ";

            if (filter.OwnerUserID.HasValue)
                sql += $" AND OwnerUserID = @OwnerUserID ";

            if (filter.Status.HasValue)
                sql += $" AND Status = @Status ";

            if (filter.IsOnProgressing)
                sql += $" AND getutcDate() > BeginTime AND getutcDate() < EndTime ";

            if (filter.Keyword.IsNotEmptyTextNotNull())
                sql += $" AND ( { GenerateLikeQuery("Title", "@Keyword")} OR { GenerateLikeQuery("Content", "@Keyword")} ) ";

            return sql;
        }

    }


}
