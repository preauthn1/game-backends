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
    public class UserTokenRecordRepository : FormatIntegerPrimaryKeyRepository<UserTokenRecordEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.OperationDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[UserTokenRecord]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        /// <summary>
        /// 強制登出所有後台帳號(不包含超級管理組織)
        /// </summary>
        public void ForceLogoutAdminSysAccounts(int? targetUserID = null)
        {
            string whereSQL = " AND UserID = @targetUserID ";

            if(targetUserID.IsNullOrZeroOrMinus())
            {
                whereSQL = @"AND UserID in
                            (
                                SELECT UserID from [dbo].[User] with(NOLOCK) where OrganizationID in
                                (
                                    SELECT OrganizationID from[dbo].[Organization] with(NOLOCK)
                                    WHERE OrganizationType != @SuperOrganizationType
                                )
                            )";
            }

            string sql = $@" UPDATE [dbo].[UserTokenRecord] set tokenMD5 = @ResetValue
                             WHERE CreateTime >= @Begin {whereSQL} ";

            var param = new
            {
                targetUserID = (targetUserID ?? 0),
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-7),
                SuperOrganizationType = OrganizationTypeEnum.SuperAdmin.ToInteger(),
                ResetValue = Guid.NewGuid().ToString()
            };

            ExecuteSqlByDapper(sql, param);

            string keyword = CacheKey.AdminSys_UserToken_ByHash("");
            CacheHelper.RemoveCacheByPrefixKeyword(keyword);
        }


        public UserTokenRecordEntity GetCacheByTokenHash(string tokenHash)
        {
            string cacheKey = CacheKey.AdminSys_UserToken_ByHash(tokenHash);

            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var q = GetByFilter(new QueryUserTokenRecordRequestViewModel
                {
                    TokenMD5 = tokenHash
                });

                return q.FirstOrDefault();

            }, expiredMinute: 15);

            return queryCache;
        }

        public List<UserTokenRecordEntity> GetByFilter(QueryUserTokenRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<UserTokenRecordEntity>(sql, filter);
            return query;
        }


        public PagedList<UserTokenRecordEntity> GetPagingByFilter(QueryUserTokenRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<UserTokenRecordEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryUserTokenRecordRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            if (filter.TokenMD5.IsNotEmptyTextNotNull())
                sql += " AND TokenMD5 = @TokenMD5 ";

            return sql;
        }

    }


}
