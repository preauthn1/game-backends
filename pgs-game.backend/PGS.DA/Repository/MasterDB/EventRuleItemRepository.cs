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
    public class EventRuleItemRepository : FormatIntegerPrimaryKeyRepository<EventRuleItemEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[EventRuleItem]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "EventRuleItemID"; }
        }


        public List<EventRuleItemInfoModel> GetEventAlwaysRules(int eventID )
        {
            var query = GetEventRuleInfosByFilter(new QueryEventRuleItemInfoParameterModel
            {
                IsAlwaysRule = true,
                EventID = eventID
            });

            return query;
        }

        public List<EventRuleItemInfoModel> GetEventRuleInfosByFilter(QueryEventRuleItemInfoParameterModel filter)
        {
            filter.RuleFor(z => z.EventID).Rule_NotZeroNotMinus();

            string sql = @" SELECT bundle.EventID ,  ri.EventRuleItemID , bundle.EventPrizeBundleID , bundle.EventRuleBundleID , bundle.isAlwaysRule, bundle.Sequence ,
                                   ri.ruleType , ri.Value , ri.[BeginTime] ,ri.[EndTime] 
                            FROM[dbo].[EventRuleItem] as ri
                            JOIN[dbo].[EventRuleBundle] as bundle on ri.EventRuleBundleID = bundle.EventRuleBundleID";

            string whereQuery = "  WHERE bundle.eventID = @eventID AND bundle.isAlwaysRule = @IsAlwaysRule ";

            if (filter.RuleTypes.IsNotEmptyNull())
                whereQuery += " AND ri.RuleType in @RuleTypes";

            if (filter.EventPrizeBundleIDs.IsNotEmptyNull())
                whereQuery += " AND bundle.EventPrizeBundleID in @EventPrizeBundleIDs";

            sql += whereQuery;

            var query = QueryByDapper<EventRuleItemInfoModel>(sql, filter);
            return query.OrderBy(z=> z.Sequence).ToList();
        }


        public List<EventRuleItemEntity> GetByFilter(QueryEventRuleItemRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<EventRuleItemEntity>(sql, filter);
            return query;
        }


        public PagedList<EventRuleItemEntity> GetPagingByFilter(QueryEventRuleItemRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<EventRuleItemEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryEventRuleItemRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.EventRuleBundleIDs.IsNotEmptyNull())
                sql += " AND EventRuleBundleID in @EventRuleBundleIDs ";

            if (filter.EventID.HasValue)
            {
                sql += @" AND EventRuleBundleID in 
                        ( 
                            SELECT EventRuleBundleID 
                            FROM [dbo].[EventRuleBundle] WITH (NOLOCK)
                            WHERE EventID = @EventID
                        )";
            }               

            return sql;
        }

    }


}
