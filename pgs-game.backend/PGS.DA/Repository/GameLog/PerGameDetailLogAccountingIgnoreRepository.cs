using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.DA.Repository
{
    public class PerGameDetailLogAccountingIgnoreRepository : FormatLongPrimaryKeyRepository<PerGameDetailLogAccountingIgnoreEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[PerGameDetailLogAccountingIgnore]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        protected override string DefaultSortColumn
        {
            get
            {
                return "GameTime";
            }
        }


        public PerGameDetailLogAccountingIgnoreEntity GetByTransactionNo_WithGameTimeRange(string transactionNo)
        {
            var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(transactionNo);

            var myFilter = new QueryPerGameDetailLogAccountingIgnoreRequestViewModel
            {
                TransactionNo = transactionNo,
                GameStartTimeBegin = recover.Time,
                GameStartTimeEnd = DateTimeHelper.GetCurrentTime(),
            };

            var query = GetByFilter(myFilter);
            return query.FirstOrDefault();
        }


        public List<PerGameDetailLogAccountingIgnoreEntity> GetByFilter(QueryPerGameDetailLogAccountingIgnoreRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter , selectColumns);
            var query = QueryByDapper<PerGameDetailLogAccountingIgnoreEntity>(sql, filter);
            return query;
        }


        public PagedList<PerGameDetailLogAccountingIgnoreEntity> GetPagingByFilter(QueryPerGameDetailLogAccountingIgnoreRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter , selectColumns);
            var query = GetPagingEntitiesBySQLScript<PerGameDetailLogAccountingIgnoreEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryPerGameDetailLogAccountingIgnoreRequestViewModel filter , string selectColumns = "*")
        {
            string noLock = filter.IsLockQuery ? "" : " WITH (NOLOCK) ";

            string sql = $@"SELECT {selectColumns} FROM {TableName} {noLock}
                           WHERE 1=1 ";

            //*這個的順序要在merchantIDs之前
            if (filter.FilterMerchantIDByHierarchyMerchant.HasValue)
            {
                filter.MerchantIDs = AppHelper.SetListToEmptyWhenLargeFilter(filter.MerchantIDs);
                sql += AppHelper.GenerateMerchantHierarchyWhereSQL(filter.FilterMerchantIDByHierarchyMerchant.Value);
            }

            if (filter.PerGameDetailLogID.HasValue)
                sql += " AND Id = @PerGameDetailLogID ";

            if (filter.MerchantIDs.IsNotEmptyNull())
                sql += " AND MerchantID in @MerchantIDs ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += " AND PlayerID in @PlayerIDs ";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in @GameIDs ";

            if (filter.PerGameLogIDs.IsNotEmptyNull())
                sql += " AND PerGameLogID in @PerGameLogIDs ";

            if (filter.GameStartTimeBegin.HasValue)
                sql += " AND GameTime >= @GameStartTimeBegin ";

            if (filter.GameStartTimeEnd.HasValue)
                sql += " AND GameTime <= @GameStartTimeEnd ";

            if (filter.TransactionNo.IsNotEmptyTextNotNull())
                sql += " AND TransactionNo = @TransactionNo ";

            if (filter.ThirdPartyBetRecordNo.IsNotEmptyTextNotNull())
                sql += " AND ThirdPartyBetRecordNo = @ThirdPartyBetRecordNo ";

            if (filter.VendorConfigID.HasValue)
                sql += " AND VendorConfigID = @VendorConfigID ";

            return sql;
        }

    }


}
