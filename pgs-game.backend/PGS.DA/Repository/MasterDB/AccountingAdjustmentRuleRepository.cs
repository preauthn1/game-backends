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
    public class AccountingAdjustmentRuleRepository : FormatIntegerPrimaryKeyRepository<AccountingAdjustmentRuleEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[AccountingAdjustmentRule]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public List<AccountingAdjustmentRuleEntity> GetByFilter(QueryAccountingAdjustmentRuleRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<AccountingAdjustmentRuleEntity>(sql, filter);
            return query;
        }


        public PagedList<AccountingAdjustmentRuleEntity> GetPagingByFilter(QueryAccountingAdjustmentRuleRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<AccountingAdjustmentRuleEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryAccountingAdjustmentRuleRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.AccountingGroupIDs.IsNotEmptyNull())
                sql += " AND Id in (  SELECT AccountingAdjustmentRuleID from [dbo].[AccountingAdjustmentRuleGroupAssign] WHERE AccountingGroupID in @AccountingGroupIDs ) ";

            if (filter.Types.IsNotEmptyNull())
                sql += " AND Type in @Types ";

            if (filter.IsUpCommingOrNotExpired)
                sql += " AND (BeginTime >= getutcdate() OR getutcdate() < EndTime )";

            if (filter.IsRecentOrNotExpired)
                sql += " AND (EndTime is NULL OR (EndTime >= DATEADD(MONTH , -2 , getUtcDate())))";

            if (filter.FilterBeginTime_Begin.HasValue)
                sql += " AND BeginTime >= @FilterBeginTime_Begin ";

            if (filter.FilterBeginTime_End.HasValue)
                sql += " AND BeginTime < @FilterBeginTime_End ";

            if (filter.IsAssignAnyMerchantIDs)
                sql += " AND LEN(MerchantIDs) >= 5 ";

            return sql;
        }

    }


}
