using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
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
    public class AccountingAdjustmentRuleGroupAssignRepository : FormatIntegerPrimaryKeyRepository<AccountingAdjustmentRuleGroupAssignEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[AccountingAdjustmentRuleGroupAssign]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }


        public List<AccountingAdjustmentRuleAndAssinGroupInfoModel> GetAdjustmentRuleAndAssinGroups()
        {
            string sql = @"SELECT mapping.* , g.Name as GroupName 
                           FROM [dbo].[AccountingAdjustmentRuleGroupAssign] as mapping
                           LEFT JOIN [dbo].[AccountingGroup] as g on mapping.AccountingGroupID = g.AccountingGroupID";

            var query = QueryByDapper<AccountingAdjustmentRuleAndAssinGroupInfoModel>(sql, null);
            return query;
        }



        public List<AccountingAdjustmentRuleGroupAssignEntity> GetByFilter(QueryAccountingAdjustmentRuleGroupAssignRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<AccountingAdjustmentRuleGroupAssignEntity>(sql, filter);
            return query;
        }


        public PagedList<AccountingAdjustmentRuleGroupAssignEntity> GetPagingByFilter(QueryAccountingAdjustmentRuleGroupAssignRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<AccountingAdjustmentRuleGroupAssignEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryAccountingAdjustmentRuleGroupAssignRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.AccountingAdjustmentRuleIDs.IsNotEmptyNull())
                sql += " AND AccountingAdjustmentRuleID in @AccountingAdjustmentRuleIDs ";

            if (filter.AccountingGroupIDs.IsNotEmptyNull())
                sql += " AND AccountingGroupID in @AccountingGroupIDs ";            

            return sql;
        }

    }


}
