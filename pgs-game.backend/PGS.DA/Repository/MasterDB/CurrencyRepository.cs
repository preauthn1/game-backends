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
    public class CurrencyRepository : FormatStringPrimaryKeyRepository<CurrencyEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[Currency]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Currency"; }
        }

        public decimal ConvertToUsdtAmount(decimal amount , string currency)
        {
            var query = GetById(currency);

            if (query == null)
                return 0;

            return query.ExchangeRateToUSDT * amount;
        }

        public List<CurrencyEntity> GetByFilter(QueryCurrencyRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<CurrencyEntity>(sql, filter);
            return query;
        }

        public PagedList<CurrencyEntity> GetPagingByFilter(QueryCurrencyRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = GetPagingEntitiesBySQLScript<CurrencyEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryCurrencyRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            return sql;
        }

    }


}
