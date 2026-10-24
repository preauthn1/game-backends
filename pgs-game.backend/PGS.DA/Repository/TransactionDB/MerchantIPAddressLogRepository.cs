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
    public class MerchantIPAddressLogRepository : FormatIntegerPrimaryKeyRepository<MerchantIPAddressLogEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.TransactionDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[MerchantIPAddressLog]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public List<MerchantIPAddressLogEntity> GetByFilter(QueryMerchantIPAddressLogRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<MerchantIPAddressLogEntity>(sql, filter);
            return query;
        }


        public PagedList<MerchantIPAddressLogEntity> GetPagingByFilter(QueryMerchantIPAddressLogRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<MerchantIPAddressLogEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryMerchantIPAddressLogRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            return sql;
        }

    }


}
