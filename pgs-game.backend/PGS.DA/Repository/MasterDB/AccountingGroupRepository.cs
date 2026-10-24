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
    public class AccountingGroupRepository : FormatIntegerPrimaryKeyRepository<AccountingGroupEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[AccountingGroup]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "AccountingGroupID"; }
        }

        public List<AccountingGroupEntity> GetByFilter(QueryAccountingGroupRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<AccountingGroupEntity>(sql, filter);
            return query;
        }


        public PagedList<AccountingGroupEntity> GetPagingByFilter(QueryAccountingGroupRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<AccountingGroupEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryAccountingGroupRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.AccountingGroupIDs.IsNotEmptyNull())
                sql += " AND AccountingGroupID in @AccountingGroupIDs ";

            if (filter.IsIncludeDelete == false)
                sql += " AND Status = 1 ";
           
            if (filter.InvoiceExportCategory.HasValue)
            {
                sql += " AND InvoiceExportCategory = @InvoiceExportCategory ";
            }

            if (filter.ExcludeInvoiceExportCategory.IsNotEmptyNull())
            {
                sql += " AND InvoiceExportCategory not in @ExcludeInvoiceExportCategory ";
            }

            return sql;
        }

    }


}
