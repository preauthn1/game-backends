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
    public class PlayerGameTokenLogRepository : FormatIntegerPrimaryKeyRepository<PlayerGameTokenLogEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.TransactionDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[PlayerGameTokenLog]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public PlayerGameTokenLogEntity GetCacheByTokenHash(string tokenHash)
        {
            //ps. 這個token的cache不好清除 會有延遲
            string cacheKey = CacheKey.DbEntity_PlayerGameToken_ByTokenHash(tokenHash);

            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var query = GetByFilter(new QueryPlayerGameTokenLogRequestViewModel
                {
                    TokenHash = tokenHash,

                    //篩選 ExpiredTime 是因為partition欄位所以速度有差
                    ExpiredTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1)
                });

                return query.FirstOrDefault();

            } , expiredMinute: 2);

            return queryCache;
        }


        /// <summary>
        /// 重製token (PGS自製遊戲)
        /// </summary>
        public void ResetPlayerToken(int playerID)
        {
            var queryNotExpiredToken = [REDACTED] QueryPlayerGameTokenLogRequestViewModel
            {
                PlayerIDs = playerID.ToListCollection(),
                IsNotExpiredOnly = true
            });

            string sql = $@"Update {TableName} SET [ExpiredTime] = @EmptyTime
                            WHERE [ExpiredTime] >= @NowTime AND PlayerID = @PlayerID";

            var param = new
            {
                EmptyTime = DateTimeHelper.GetDefaultTime_2021(),
                NowTime = DateTimeHelper.GetCurrentTime(),
                PlayerID = playerID,
            };

            ExecuteSqlByDapper(sql, param);

            if (queryNotExpiredToken.IsNotEmptyNull())
            {
                foreach (var item in queryNotExpiredToken)
                {
                    CacheRemoveHelper.RemoveCache_WhenResetPgsPlayerToken(item.TokenHash);
                }
            }
        }

        public void ResetTokenAndCreateNew(PlayerGameTokenLogEntity newInsert)
        {
            ResetPlayerToken(newInsert.PlayerID);

            Create(newInsert);
        }


        public void ResetAllToken(ResetTokenParameter filter)
        {
            string extraWhere = "";

            if(filter.IsFilterPlayerOnly)
            {
                ResetPlayerToken(filter.PlayerID.Value);
                return;
            }

            if(filter.MerchantID.HasValue)
            {
                extraWhere +=  $" AND MerchantID = @MerchantID ";
            }

            if (filter.PlayerID.HasValue)
            {
                extraWhere += $" AND PlayerID = @PlayerID ";
            }

            string sql = $@"Update {TableName} SET [ExpiredTime] = @EmptyTime
                            WHERE [ExpiredTime] >= @NowTime {extraWhere} ";

            var param = new
            {
                EmptyTime = DateTimeHelper.GetDefaultTime_2021(),
                NowTime = DateTimeHelper.GetCurrentTime(),
                PlayerID = filter.PlayerID,
                MerchantID = filter.MerchantID
            };

            ExecuteSqlByDapper(sql, param , 120);
        }


        public List<PlayerGameTokenLogEntity> GetByFilter(QueryPlayerGameTokenLogRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<PlayerGameTokenLogEntity>(sql, filter);
            return query;
        }


        public PagedList<PlayerGameTokenLogEntity> GetPagingByFilter(QueryPlayerGameTokenLogRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<PlayerGameTokenLogEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryPlayerGameTokenLogRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += " AND PlayerID in @PlayerIDs ";

            if (filter.TokenHash.IsNotEmptyTextNotNull())
                sql += " AND TokenHash = @TokenHash ";

            if (filter.IsNotExpiredOnly)
                sql += " AND ExpiredTime >= GETUTCDATE() ";

            if (filter.ExpiredTimeBegin.HasValue)
                sql += " AND ExpiredTime >= @ExpiredTimeBegin ";

            if (filter.ExpiredTimeEnd.HasValue)
                sql += " AND ExpiredTime < @ExpiredTimeEnd ";

            return sql;
        }

    }


}
