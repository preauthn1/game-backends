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
    public class MerchantCreditChangeLogRepository : FormatIntegerPrimaryKeyRepository<MerchantCreditChangeLogEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.TransactionDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[MerchantCreditChangeLog]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public List<MerchantCreditChangeLogEntity> GetByFilter(QueryMerchantCreditChangeLogRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<MerchantCreditChangeLogEntity>(sql, filter);
            return query;
        }


        public PagedList<MerchantCreditChangeLogEntity> GetPagingByFilter(QueryMerchantCreditChangeLogRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<MerchantCreditChangeLogEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryMerchantCreditChangeLogRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            //*這個的順序要在merchantIDs之前
            if (filter.HierarchyOfTargetMerchant.HasValue)
            {
                filter.MerchantIDs = AppHelper.SetListToEmptyWhenLargeFilter(filter.MerchantIDs);
                sql += AppHelper.GenerateMerchantHierarchyWhereSQL(filter.HierarchyOfTargetMerchant.Value);
            }

            if (filter.FilterMerchantIDsByMultipleHierarchyMerchant.IsNotEmptyNull())
            {
                sql += AppHelper.GenerateMultipleHierarchyMerchantFilterWhereSQL();
            }

            if (filter.MerchantIDs.IsNotEmptyNull())
                sql += " AND MerchantID in @MerchantIDs ";

            if (filter.CreateTimeBegin.HasValue)
                sql += " AND CreateTime >= @CreateTimeBegin ";

            if (filter.CreateTimeEnd.HasValue)
                sql += " AND CreateTime < @CreateTimeEnd ";

            if (filter.CreateTimeEnd.HasValue)
                sql += " AND CreateTime < @CreateTimeEnd ";

            if (filter.Type.HasValue)
                sql += " AND Type = @Type ";

            if (filter.ReferenceOrderNo.IsNotEmptyTextNotNull())
                sql += " AND ReferenceOrderNo = @ReferenceOrderNo ";

            return sql;
        }

    }


}
