using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using MongoDB.Driver;
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
    public class EventRepository : FormatIntegerPrimaryKeyRepository<EventEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[Event]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "EventID"; }
        }


        public EventEntity GetSingleByCache(int eventID)
        {
            string cacheKey = CacheKey.DbEntity_Event(eventID);

            var queryCachae = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var querySingle = GetById(eventID);
                return querySingle;

            }, 60);

            return queryCachae;
        }

        public List<QueryEventMerchantSupportGameModel> GetEventMerchantSupportGames_V1(int eventType)
        {
            string sql = @" SELECT e.EventID , em.MerchantID , eg.GameID , e.BeginTime , e.EndTime
                            FROM [dbo].[Event] as e
                            JOIN [dbo].[EventMerchantMapping] as em on e.EventID = em.EventID
                            JOIN [dbo].[EventGameMapping] as eg on e.EventID = eg.EventID
                            WHERE e.EventType = @EventType AND e.Status != @Status";

            var para = new
            {
                Status = NormalActiveStatus.PermanentDelete.ToInteger(),
                EventType = eventType
            };

            var query = QueryByDapper<QueryEventMerchantSupportGameModel>(sql, para);
            return query;
        }

        public List<QueryEventMerchantSupportGameModel> GetEachEventSupportMerchantAndGames(int eventType)
        {
            string sql = @" SELECT e.EventID , em.MerchantID , eg.GameID , e.BeginTime , e.EndTime
                                FROM [dbo].[Event] as e WITH (NOLOCK)
                                inner join [dbo].[MerchantSelectMappingItem] as em WITH (NOLOCK) on e.MerchantSelectTemplateID = em.TemplateID and em.Type = @TemplateMappingItemType
                                inner join [dbo].[EventGameMapping] as eg WITH (NOLOCK) on e.EventID = eg.EventID
                            WHERE e.[MerchantSelectTemplateID] is not null AND  e.EventType = @EventType AND e.Status != @Status ";

            var para = new
            {
                TemplateMappingItemType = MerchantSelectMappingItemTypeEnum.IncludeMerchantID.ToInteger(),
                Status = NormalActiveStatus.PermanentDelete.ToInteger(),
                EventType = eventType
            };

            var query = QueryByDapper<QueryEventMerchantSupportGameModel>(sql, para);
            return query;
        }



        public int GetCountByFilter(QueryEventRequestViewModel filter)
        {
            var sqlGen = GenerateWhereSQL(filter, " count(*) ");
            var query = QueryByDapper<int>(sqlGen.WhereSQL, filter);
            return query.FirstOrDefault();
        }



        public List<EventEntity> GetByFilter(QueryEventRequestViewModel filter, string selectColumns = "*")
        {
            var sqlGen = GenerateWhereSQL(filter , selectColumns);

            var config = filter.DbQueryExtraConfig ?? new DbQueryConfig();
            config.TempIDsAppendSQL = sqlGen.TempIDsAppendSQL;

            var query = QueryByDapper<EventEntity>(sqlGen.WhereSQL, filter , config);
            return query;
        }


        public PagedList<EventEntity> GetPagingByFilter(QueryEventRequestViewModel filter, string selectColumns = "*")
        {
            var sqlGen = GenerateWhereSQL(filter , selectColumns);
            var query = GetPagingEntitiesBySQLScript<EventEntity>(sqlGen.WhereSQL, filter, filter);
            return query;
        }


        private GenerateWhereSQLResult GenerateWhereSQL(QueryEventRequestViewModel filter, string selectColumns = "*")
        {
            var result = new GenerateWhereSQLResult();

            string sql = $@"SELECT {selectColumns} FROM {TableName} 
                           WHERE 1=1 ";

            if (!filter.IsIncludeDeleteData)
                sql += $" AND Status != {NormalActiveStatus.PermanentDelete.ToInteger()} ";

            if (filter.EventIDs.IsNotEmptyNull())
                sql += " AND EventID in @EventIDs ";

            if (filter.EventTypes.IsNotEmptyNull())
                sql += " AND EventType in @EventTypes ";

            if (filter.EventSubTypes.IsNotEmptyNull())
                sql += " AND EventSubType in @EventSubTypes ";

            if (filter.EventTemplateType.HasValue)
                sql += " AND EventTemplateType = @EventTemplateType ";

            if (filter.Status.HasValue)
                sql += " AND ( Status = @Status )";

            if (filter.StatusNotEqual.HasValue)
                sql += " AND ( Status != @StatusNotEqual )";

            if (!string.IsNullOrWhiteSpace(filter.DisplayName))
            {
                filter.DisplayName = filter.DisplayName.Trim();
                sql += $" AND {GenerateLikeQuery("DisplayName", "@DisplayName")} ";
            }

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                filter.Keyword = filter.Keyword.Trim();
                sql += $" AND (" +
                  $"{GenerateLikeQuery("DisplayName", "@Keyword")} OR " +
                  $"{GenerateLikeQuery("DisplayNameExternal", "@Keyword")} OR " +                  
                  $"{GenerateLikeQuery("cast(EventID as nvarchar(10))", "@Keyword")} "  +
                ")";
            }

            if (filter.IsNotExpired)
                sql += " AND ( BeginTime < getUTCDate() AND getUTCDate() < EndTime )";

            if (filter.IsNotExpiredBufferRange)
                sql += " AND ( DATEADD(hour , -2 , BeginTime) < getUTCDate() AND getUTCDate() < DATEADD(hour , 2 , EndTime))";

            if (filter.IsEndTimeNotExpired)
                sql += " AND ( getUTCDate() < EndTime )";

            if (filter.FilterMerchantIDsByMultipleHierarchyMerchant.IsNotEmptyNull())
            {
                filter.MerchantIDs = AppHelper.SetListToEmptyWhenLargeFilter(filter.MerchantIDs);
                string merchantIDsSql = AppHelper.GenerateMultipleHierarchyMerchantFilterWhereSQL();

                int type = MerchantSelectMappingItemTypeEnum.IncludeMerchantID.ToInteger();
                sql += $@" AND MerchantSelectTemplateID in 
                        ( 
                           SELECT TemplateID FROM [dbo].[MerchantSelectMappingItem] WITH (NOLOCK) 
                           WHERE Type = {type} {merchantIDsSql}
                        ) ";
            }

            if (filter.MerchantIDs.IsNotEmptyNull())
            {
                int type = MerchantSelectMappingItemTypeEnum.IncludeMerchantID.ToInteger();

                var queryColumns = GenerateInQuery_Or_LargeIDsQuery(" @MerchantIDs ", filter.MerchantIDs);

                sql += $@" AND MerchantSelectTemplateID in 
                        ( 
                           SELECT TemplateID FROM [dbo].[MerchantSelectMappingItem] WITH (NOLOCK) 
                           WHERE Type = {type} AND MerchantID in {queryColumns.WhereSQL} 
                        ) ";

                result.TempIDsAppendSQL = queryColumns.TempIDsAppendSQL;
            }

            if (filter.HierarchyOfTargerMerchant.HasValue)
            {
                int type = MerchantSelectMappingItemTypeEnum.IncludeMerchantID.ToInteger();

                var filterMerchantID_SQL = AppHelper.GenerateMerchantHierarchyWhereSQL(filter.HierarchyOfTargerMerchant.Value);

                sql += $@" AND MerchantSelectTemplateID in 
                        ( 
                           SELECT TemplateID FROM [dbo].[MerchantSelectMappingItem] WITH (NOLOCK) 
                           WHERE Type = {type} {filterMerchantID_SQL}
                        ) ";
            }

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in (SELECT GameID FROM [dbo].[EventGameMapping] WHERE GameID in @GameIDs)   ";


            result.WhereSQL = sql;
            return result;
        }

    }


}
