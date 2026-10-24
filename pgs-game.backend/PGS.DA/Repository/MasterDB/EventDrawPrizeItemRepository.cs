using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
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
    public class EventDrawPrizeItemRepository : FormatIntegerPrimaryKeyRepository<EventDrawPrizeItemEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[EventDrawPrizeItem]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "EventDrawPrizeItemID"; }
        }




        public List<QueryDrawPrizeCountModel> GetPrizeItemCount(List<int> eventDrawPrizeIDs)
        {
            //為了避免超賣(超額) where 條件更嚴格
            string sql = @$"SELECT [EventDrawPrizeID] , count(*) as prizeCount, sum(LimitQuantity) as totalQuantity, sum(UsageQuantity) as TotalUsageQuantity
                            FROM [dbo].[EventDrawPrizeItem]
                            WHERE EventDrawPrizeID in @eventDrawPrizeIDs
                            GROUP BY [EventDrawPrizeID]";

            var para = new
            {
                eventDrawPrizeIDs = eventDrawPrizeIDs
            };

            var query = QueryByDapper<QueryDrawPrizeCountModel>(sql , para);
            return query;
        }


        public SQLUpdateRowCountResult UsageQuantityPlus(int eventDrawPrizeItemID, bool isThrowException = true)
        {
            //為了避免超賣(超額) where 條件更嚴格
            string sql = @$" Update {TableName} set UsageQuantity = UsageQuantity + 1 
                             WHERE EventDrawPrizeItemID = @eventDrawPrizeItemID AND (LimitQuantity is NULL OR LimitQuantity > UsageQuantity)";

            var para = new
            {
                eventDrawPrizeItemID = eventDrawPrizeItemID
            };

            var updateResult = UpdateAndSelectRowCount(sql, para);

            if (updateResult.UpdateRowCount <= 0 && isThrowException)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DatabaseBusyAndUpdateFail);

            return updateResult;
        }

        /// <summary>
        /// 將所有獎品更新成 已全數派發完畢
        /// </summary>
        public void UpdateAllPrizeToMaxUsage(int eventDrawPrizeID )
        {
            string sql = @$" Update {TableName} set UsageQuantity = LimitQuantity
                             WHERE EventDrawPrizeID = @eventDrawPrizeID AND LimitQuantity > 0";

            var para = new
            {
                eventDrawPrizeID = eventDrawPrizeID
            };

            ExecuteSqlByDapper(sql , para);
        }


        public void ResetUsageByEventParentDrawID(int drawPrizeID)
        {
            string sql = @$" Update {TableName} set UsageQuantity = 0
                             WHERE EventDrawPrizeID = @EventDrawPrizeID ";

            var para = new
            {
                EventDrawPrizeID = drawPrizeID
            };

            ExecuteSqlByDapper(sql , para);
        }


        public void ResetUsageByEventID(int eventID , int newUsageQuantity = 0)
        {
            string sql = @$" Update {TableName} set UsageQuantity = @newUsageQuantity
                             WHERE EventDrawPrizeID in
                            (
                               SELECT EventDrawPrizeID from [dbo].[EventDrawPrize] 
                               WHERE eventID = @eventID
                            ) ";

            var para = new
            {
                eventID = eventID,
                newUsageQuantity = newUsageQuantity
            };

            ExecuteSqlByDapper(sql, para);
        }



        public List<EventDrawPrizeItemEntity> GetByEventID(int eventID)
        {
            var query = GetByFilter(new QueryEventDrawPrizeItemRequestViewModel
            {
                EventIDs = eventID.ToListCollection()
            });

            return query;
        }


        public List<EventDrawPrizeItemEntity> GetByFilter(QueryEventDrawPrizeItemRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<EventDrawPrizeItemEntity>(sql, filter);
            return query;
        }


        public PagedList<EventDrawPrizeItemEntity> GetPagingByFilter(QueryEventDrawPrizeItemRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<EventDrawPrizeItemEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryEventDrawPrizeItemRequestViewModel filter)
        {
            string noLock = filter.IsLockQuery ? "" : " WITH (NOLOCK) ";

            string sql = $@"SELECT * FROM {TableName} {noLock}
                           WHERE 1=1 ";

            if (filter.EventDrawPrizeIDs.IsNotEmptyNull())
                sql += " AND EventDrawPrizeID in @EventDrawPrizeIDs ";

            if (filter.EventIDs.IsNotEmptyNull())
                sql += " AND EventDrawPrizeID in ( SELECT EventDrawPrizeID FROM EventDrawPrize WHERE EventID in @EventIDs ) ";

            if (filter.IsQuantityStillEnough)
                sql += " AND (LimitQuantity is NULL OR LimitQuantity > UsageQuantity )";

            return sql;
        }

    }


}
