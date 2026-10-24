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
    public class AccountingInvoiceCategoryRepository : FormatIntegerPrimaryKeyRepository<AccountingInvoiceCategoryEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[AccountingInvoiceCategory]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "AccountingInvoiceCategoryID"; }
        }


        public List<AccountingInvoiceCategoryEntity> GetByFilter(QueryAccountingInvoiceCategoryRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<AccountingInvoiceCategoryEntity>(sql, filter);
            return query;
        }


        public PagedList<AccountingInvoiceCategoryEntity> GetPagingByFilter(QueryAccountingInvoiceCategoryRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<AccountingInvoiceCategoryEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryAccountingInvoiceCategoryRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.AccountingInvoiceCategoryIDs.IsNotEmptyNull())
                sql += " AND AccountingInvoiceCategoryID in @AccountingInvoiceCategoryIDs ";

            return sql;
        }

    }


}
