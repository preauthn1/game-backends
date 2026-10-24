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
    public class EventDrawPrizeRepository : FormatIntegerPrimaryKeyRepository<EventDrawPrizeEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[EventDrawPrize]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "EventDrawPrizeID"; }
        }

        public void ResetEventID(int eventID)
        {
            string sql = @$" Update {TableName} set EventID = NULL 
                             WHERE EventID = @eventID";

            var para = new
            {
                eventID = eventID
            };

            ExecuteSqlByDapper(sql, para);
        }


        public List<EventDrawPrizeEntity> GetByFilter(QueryEventDrawPrizeRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<EventDrawPrizeEntity>(sql, filter);
            return query;
        }


        public PagedList<EventDrawPrizeEntity> GetPagingByFilter(QueryEventDrawPrizeRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<EventDrawPrizeEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryEventDrawPrizeRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (!filter.IsIncludeDeleteData)
                sql += $" AND Status != {NormalActiveStatus.PermanentDelete.ToInteger()} ";

            if (filter.EventIDs.IsNotEmptyNull())
                sql += " AND EventID in @EventIDs ";

            if(filter.EventIdEqualOrEventNull.HasValue)
                sql += " AND (EventID = @EventIdEqualOrEventNull OR EventID is NULL)";

            if (filter.Status.HasValue)
                sql += " AND Status = @Status ";

            if (filter.Type.HasValue)
                sql += " AND Type = @Type ";

            if (!string.IsNullOrWhiteSpace(filter.DisplayNameLikeQuery))
            {
                filter.DisplayNameLikeQuery = filter.DisplayNameLikeQuery.Trim();
                string eventNameKeyword = $" EventID in ( SELECT EventID from [dbo].[Event] WHERE {GenerateLikeQuery("DisplayName", "@DisplayNameLikeQuery")} )";
                sql += $" AND ( {GenerateLikeQuery("DisplayName", "@DisplayNameLikeQuery")} OR {eventNameKeyword}) ";
            }

            return sql;
        }

    }


}
