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
    /// <summary>
    /// 這張表最一開始是 針對捕魚遊戲內部轉帳
    /// 後來有了pgs大廳轉帳機制。演變成 大廳轉帳也是用此table紀錄 (並且gameVendorID=0)
    /// </summary>
    public class GameVendorTransferRecordRepository : FormatStringPrimaryKeyRepository<GameVendorTransferRecordEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.TransactionDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[GameVendorTransferRecord]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "TransactionNo"; }
        }

        public List<GameVendorTransferRecordEntity> GetByFilter(QueryGameVendorTransferRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<GameVendorTransferRecordEntity>(sql, filter);
            return query;
        }


        public PagedList<GameVendorTransferRecordEntity> GetPagingByFilter(QueryGameVendorTransferRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<GameVendorTransferRecordEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryGameVendorTransferRecordRequestViewModel filter, string selectColumns = "*")
        {
            string isNoLock = filter.IsLockQuery ? "" : " WITH (NOLOCK) ";
            string sql = $@"SELECT {selectColumns} FROM {TableName} {isNoLock}
                           WHERE 1=1 ";

            if(filter.TransactionNo.IsNotEmptyTextNotNull())
                sql += " AND TransactionNo = @TransactionNo ";

            if (filter.ThirdOrderNo.IsNotEmptyTextNotNull())
                sql += " AND ThirdOrderNo = @ThirdOrderNo ";

            if (filter.PlatformOrderNo.IsNotEmptyTextNotNull())
                sql += " AND PlatformOrderNo = @PlatformOrderNo ";

            if (filter.GameVendorID.HasValue)
                sql += " AND GameVendorID = @GameVendorID ";

            //*這個的順序要在merchantIDs之前
            if (filter.FilterMerchantIDByHierarchyMerchant.HasValue)
            {
                filter.MerchantIDs = AppHelper.SetListToEmptyWhenLargeFilter(filter.MerchantIDs);
                sql += AppHelper.GenerateMerchantHierarchyWhereSQL(filter.FilterMerchantIDByHierarchyMerchant.Value);
            }

            if (filter.MerchantIDs.IsNotEmptyNull())
                sql += " AND MerchantID in @MerchantIDs ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += " AND PlayerID in @PlayerIDs ";

            if (filter.CreateTimeBegin.HasValue)
                sql += " AND CreateTime >= @CreateTimeBegin ";

            if (filter.CreateTimeEnd.HasValue)
                sql += " AND CreateTime < @CreateTimeEnd ";

            if (filter.Type.HasValue)
                sql += " AND Type = @Type ";

            return sql;
        }

    }


}
