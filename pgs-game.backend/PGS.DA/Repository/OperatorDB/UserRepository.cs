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
using target.Models.Enums;
using Lck.Utility.Extensions.Helper;
using target.Utility.Helper;
using target.Models.Models;

namespace target.DA.Repository
{

    public class UserRepository : FormatIntegerPrimaryKeyRepository<UserEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.OperationDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[User]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "UserID"; }
        }


        public UserEntity GetSingleByCache(int userID)
        {
            string cacheKey = CacheKey.AdminSys_UserInfo(userID);

            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   var q = GetById(userID);
                   return q;

               }, expiredMinute: 15);

            return queryCache;
        }
        

        public void UpdateLastLoginInfo(int userID , string ipAddress)
        {
            string sql = $@"UPDATE {TableName}
                            SET [LastLogonTime] = GETUTCDATE() , 
                                [LastLogonIPAddress] = @LastLogonIPAddress
                            WHERE UserID = @UserID";

            var para = new
            {
                LastLogonIPAddress = ipAddress,
                UserID = userID
            };

            ExecuteSqlByDapper(sql , para);
        }


        public UserEntity GetByAccount(string account)
        {
            var filter = new QueryUserRequestViewModel
            {
                IsIncludeDelete = true,  //已刪除的(999) 也包含進去好了
                Account = account
            };

            var query = GetByFilter(filter);
            return query.FirstOrDefault();
        }
        

        public UserEntity GetByAccountPassword(string account , string password)
        {
            var filter = new QueryUserRequestViewModel
            {
                Account = account,
                Password = [REDACTED]
            };

            var query = GetByFilter(filter);
            return query.FirstOrDefault();
        }


        public List<EachOrganizationUserCountModel> GetEachOrganizationUserCount()
        {
            string sql = @"SELECT OrganizationID , count(*) as TotalCount
                           FROM [dbo].[User] WITH (NOLOCK)
                           WHERE [status] != 999
                           group by OrganizationID";

            var query = QueryByDapper<EachOrganizationUserCountModel>(sql , null);
            return query;
        }


        public bool IsViewUserPermissionByCreditMerchant(int targetUserID , int referenceCreditMerchant)
        {
            var query = GetByFilter(new QueryUserRequestViewModel
            {
                UserIDs = targetUserID.ToListCollection(),
                FilterReferenceCreditMerchantIDByHierarchyMerchantID = referenceCreditMerchant
            });

            return query.IsNotEmptyNull();
        }



        public List<UserEntity> GetByFilter(QueryUserRequestViewModel filter, string selectColumns = "*")
        {
            var sqlGen = GenerateWhereSQL(filter , selectColumns);

            var config = filter.DbQueryExtraConfig ?? new DbQueryConfig();
            config.TempIDsAppendSQL = sqlGen.TempIDsAppendSQL;

            var query = QueryByDapper<UserEntity>(sqlGen.WhereSQL, filter , config);
            return query;
        }


        public PagedList<UserEntity> GetPagingByFilter(QueryUserRequestViewModel filter, string selectColumns = "*")
        {
            var sqlGen = GenerateWhereSQL(filter , selectColumns);
            var query = GetPagingEntitiesBySQLScript<UserEntity>(sqlGen.WhereSQL, filter, filter);
            return query;
        }


        private GenerateWhereSQLResult GenerateWhereSQL(QueryUserRequestViewModel filter, string selectColumns = "*")
        {
            var sqlGen = new GenerateWhereSQLResult();

            string sql = $@"SELECT {selectColumns} FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.UserIDs.IsNotEmptyNull())
            {
                var whereColumns = GenerateInQuery_Or_LargeIDsQuery(" @UserIDs ", filter.UserIDs);
                sqlGen.TempIDsAppendSQL = whereColumns.TempIDsAppendSQL;

                sql += $" AND UserID in {whereColumns.WhereSQL} ";
            }

            if (filter.Status.HasValue)
                sql += " AND Status = @Status ";

            if (filter.RoleIDs.IsNotEmptyNull())
                sql += " AND UserID in ( SELECT UserID FROM [dbo].[UserRoleMapping] WHERE RoleID in @RoleIDs) ";

            if (filter.ReferenceCreditMerchantIDs.IsNotEmptyNull())
                sql += " AND ReferenceCreditMerchant in @ReferenceCreditMerchantIDs ";

            if (filter.AssignPermissionID.HasValue)
            {
                sql += @$" AND UserID in 
                        (
                            SELECT UserID FROM [dbo].[UserPermissionMapping] WITH (NOLOCK) 
                            WHERE PermissionID = @AssignPermissionID
                        )";
            }

            if (filter.FilterReferenceCreditMerchantIDByHierarchyMerchantID.HasValue)
            {
                int merchanID = filter.FilterReferenceCreditMerchantIDByHierarchyMerchantID.Value;
                sql += AppHelper.GenerateMerchantHierarchyWhereSQL(merchanID, "ReferenceCreditMerchant");
            }

            if (filter.FilterAvailableUsersByCreditMerchantID_NeedSpecialPermission.HasValue)
            {
                int merchantID = filter.FilterAvailableUsersByCreditMerchantID_NeedSpecialPermission.Value;

                string keyword = AppHelper.GenerateHierarchyLikeKeyword(merchantID);

                string sql1 = $@" ReferenceCreditMerchant in (
                                        SELECT MerchantID from [PGS_Master].[dbo].[MerchantHierarchy] WITH (NOLOCK)
                                        WHERE Hierarchy LIKE CONCAT('%', '{keyword}' ,'%') 
                                   )";

                string sql2 = $@" UserID  in ( 
                                        SELECT UserID FROM [dbo].[UserMerchantMapping] WITH (NOLOCK) 
                                        WHERE MerchantID in 
                                        (
                                          SELECT MerchantID from [PGS_Master].[dbo].[MerchantHierarchy] WITH (NOLOCK)
                                          WHERE Hierarchy LIKE CONCAT('%', '{keyword}' ,'%') 
                                        )
                                    )";

                sql += $@" AND ( {sql1}  OR {sql2} )";
            }

            if (filter.IsAssignDisableMerchant)
            {
                string sql1 = $@" ReferenceCreditMerchant in (
                                        SELECT MerchantID from [PGS_Master].[dbo].[Merchant] WITH (NOLOCK)
                                        WHERE Status != 1 
                                   )";

                sql += $@" AND ( {sql1} )";
            }

            if (filter.AssignMerchantID.HasValue)
            {
                sql += @" AND UserID in (
                            SELECT UserID FROM [dbo].[UserMerchantMapping] 
                            WHERE MerchantID = @AssignMerchantID
                          ) ";
            }

            if (filter.MerchantIDsMatchInAssignMerchantOrCreditMerchant.IsNotEmptyNull())
            {
                sql += @" AND 
                    (
                        UserID in (
                                    SELECT UserID FROM [dbo].[UserMerchantMapping] WITH (NOLOCK)
                                    WHERE MerchantID in @MerchantIDsMatchInAssignMerchantOrCreditMerchant
                                   ) 
                        OR ReferenceCreditMerchant in @MerchantIDsMatchInAssignMerchantOrCreditMerchant
                    )";
            }

            if (filter.OrganizationIDs.IsNotEmptyNull())
                sql += " AND OrganizationID in @OrganizationIDs ";

            if (filter.OrganizationType.HasValue)
            {
                sql += @" AND OrganizationID in 
                    (
                        SELECT [OrganizationID] 
                        FROM [dbo].[Organization] with (NOLOCK) 
                        WHERE [OrganizationType] = @OrganizationType
                    ) ";
            }                

            if (!string.IsNullOrWhiteSpace(filter.Account))
                sql += " AND UserName = @Account ";

            if (!string.IsNullOrWhiteSpace(filter.AccountFuzzyQuery))
                sql += $" AND ( {GenerateLikeQuery("UserName", "@AccountFuzzyQuery")}  ) ";

            if (!string.IsNullOrWhiteSpace(filter.Password))
                sql += " AND Password = [REDACTED] ";

            if (filter.UserCustomizedSettingExist.HasValue)
            {
                sql += @" AND UserID in (
                            SELECT UserID
                            FROM [dbo].[UserCustomizedSetting] WITH (NOLOCK)
                            WHERE SettingKey = @UserCustomizedSettingExist
                          ) ";
            }

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                filter.Keyword = filter.Keyword.Trim();
                sql += @$" AND ( {GenerateLikeQuery("UserName", "@Keyword")} OR 
                                 {GenerateLikeQuery("DisplayName", "@Keyword")} OR 
                                 {GenerateLikeQuery("UserID", "@Keyword")} OR 
                                 {GenerateLikeQuery("Memo", "@Keyword")} 
                               ) ";
            }

            if (!string.IsNullOrWhiteSpace(filter.MyAccountsOrCreatedByMySelf))
                sql += " AND ( UserName = @MyAccountsOrCreatedByMySelf  OR  CreateBy = @MyAccountsOrCreatedByMySelf)";

            if (!filter.IsIncludeDelete)
                sql += $" AND Status != {NormalActiveStatus.PermanentDelete.ToInteger()}";

            if (filter.LastLoginTimeBegin.HasValue)
                sql += $" AND LastLogonTime >= @LastLoginTimeBegin ";

            if (filter.LastLoginTimeEnd.HasValue)
                sql += $" AND LastLogonTime < @LastLoginTimeEnd ";

            if (filter.CreateTimeBegin.HasValue)
                sql += $" AND CreateTime >= @CreateTimeBegin ";

            if (filter.CreateTimeEnd.HasValue)
                sql += $" AND CreateTime < @CreateTimeEnd ";

            if (filter.CreateByMatch.IsNotEmptyTextNotNull())
                sql += $" AND CreateBy = @CreateByMatch ";

            sqlGen.WhereSQL = sql;
            return sqlGen;
        }

    }


}
