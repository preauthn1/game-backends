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
    public class SingleWalletMerchantBalanceRecordRepository : FormatMultiplePrimaryKeyRepository<SingleWalletMerchantBalanceRecordEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.TransactionDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[SingleWalletMerchantBalanceRecord]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "ReferenceNo"; }
        }

        public bool IsTargetRecordExist(string referenceNo , SingleWalletBalanceRecordTypeEnum type)
        {
            string selectColumns = $" {PrimaryKeyColumnName} , {nameof(SingleWalletMerchantBalanceRecordEntity.Type)} ";

            var filter = new QuerySingleWalletMerchantBalanceRecordRequestViewModel
            {
                ReferenceNo = referenceNo,
                Type = type.ToInteger()
            };

            var query = GetByFilter(filter, selectColumns);
            return query.IsNotEmptyNull();
        }


        public List<SingleWalletMerchantBalanceRecordEntity> GetByFilter(QuerySingleWalletMerchantBalanceRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<SingleWalletMerchantBalanceRecordEntity>(sql, filter);
            return query;
        }


        public PagedList<SingleWalletMerchantBalanceRecordEntity> GetPagingByFilter(QuerySingleWalletMerchantBalanceRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<SingleWalletMerchantBalanceRecordEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QuerySingleWalletMerchantBalanceRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.ReferenceNo.IsNotEmptyTextNotNull())
                sql += " AND ReferenceNo = @ReferenceNo ";

            if (filter.Type.HasValue)
                sql += " AND Type = @Type ";

            return sql;
        }

    }


}
