using Dapper;
using game.Entity;
using target.Core.Repository;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;
using target.Utility.Extensions;
using System.Linq;
using target.Utility.Helper;

namespace target.DA.Repository
{

    public class UserOperateRecordRepository : FormatIntegerPrimaryKeyRepository<UserOperateRecordEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.OperationDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[UserOperateRecord]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public List<UserOperateRecordEntity> GetByFilter(QueryUserOperateRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<UserOperateRecordEntity>(sql, filter);
            return query;
        }


        public PagedList<UserOperateRecordEntity> GetPagingByFilter(QueryUserOperateRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            if(filter.OrderByColumn == "CreateTime")
            {
                filter.OrderByColumn = "main.CreateTime";
            }

            var query = GetPagingEntitiesBySQLScript<UserOperateRecordEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryUserOperateRecordRequestViewModel filter)
        {
            string sql = $@"SELECT main.* FROM {TableName} as main
                           LEFT JOIN [dbo].[User] as u on main.UserID = u.UserID
                           WHERE 1=1 ";

            if (filter.OperateTypes.IsNotEmptyNull())
                sql += " AND main.OperateType in @OperateTypes ";

            if (filter.MerchantIDs.IsNotEmptyNull())
                sql += " AND main.MerchantID in @MerchantIDs ";

            if (filter.TargetKey.IsNotEmptyTextNotNull())
                sql += " AND main.TargetKey = @TargetKey ";

            if (filter.TargetIDs.IsNotEmptyNull())
                sql += " AND main.TargetID in @TargetIDs ";

            if (filter.TargetID.HasValue)
                sql += " AND main.TargetID = @TargetID ";

            if (filter.UserID.HasValue)
                sql += " AND main.UserID = @UserID ";

            if (filter.OrganizationID.HasValue)
                sql += @" AND u.OrganizationID = @OrganizationID ";

            if (filter.CreateTimeBegin.HasValue)
                sql += " AND main.CreateTime >= @CreateTimeBegin ";

            if (filter.CreateTimeEnd.HasValue)
                sql += " AND main.CreateTime <= @CreateTimeEnd ";
            
            if (!string.IsNullOrWhiteSpace(filter.DeviceID))
                sql += $" AND  main.DeviceID = @DeviceID ";

            if (!string.IsNullOrWhiteSpace(filter.CreateIPAddress))
                sql += $" AND  main.CreateIPAddress = @CreateIPAddress ";

            if (!string.IsNullOrWhiteSpace(filter.CreateIPAddressLikeQuery))
                sql += $" AND ( {GenerateLikeQuery("main.CreateIPAddress", "@CreateIPAddressLikeQuery")} )";

            if (!string.IsNullOrWhiteSpace(filter.Memo))
                sql += $" AND ( {GenerateLikeQuery("main.Memo", "@Memo")} )";

            if (!string.IsNullOrWhiteSpace(filter.UserName))
            {
                sql += @$" AND ( {GenerateLikeQuery("u.UserName", "@UserName")} )";
            }

            //*這個的順序要在 ReferenceCreditMerchantIDs 之前
            if (filter.FilterMerchantIDByHierarchyMerchant.HasValue)
            {
                filter.ReferenceCreditMerchantIDs = AppHelper.SetListToEmptyWhenLargeFilter(filter.MerchantIDs);
                sql += AppHelper.GenerateMerchantHierarchyWhereSQL(filter.FilterMerchantIDByHierarchyMerchant.Value, " u.[ReferenceCreditMerchant] ");
            }

            if (filter.ReferenceCreditMerchantIDs.IsNotEmptyNull())
            {
                sql += @$" AND u.[ReferenceCreditMerchant] in @ReferenceCreditMerchantIDs ";
            }

            return sql;
        }

    }


}
