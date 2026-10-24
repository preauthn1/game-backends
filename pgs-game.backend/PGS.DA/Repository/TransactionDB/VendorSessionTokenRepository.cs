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
using Lck.Utility.Extensions.Helper;
using target.Models.Models;
using target.Models.Enums;

namespace target.DA.Repository
{

    /// <summary>
    /// 記住我方傳給三方遊戲的合法sessionnToken
    /// </summary>
    public class VendorSessionTokenRepository : FormatIntegerPrimaryKeyRepository<VendorSessionTokenEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.TransactionDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[VendorSessionToken]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }


        /// <summary>
        /// 讓整個系統全部token過期
        /// </summary>
        public void ResetAllSessionToken(ResetTokenParameter filter)
        {
            DateTime beginTime = DateTimeHelper.GetCurrentTime().AddDays(-7);
            DateTime emptyDate = DateTimeHelper.GetDefaultTime();

            string extraWhere = string.Empty;

            //當一用戶的部分
            if(filter.IsFilterPlayerOnly)
            {
                ResetPlayerAllSessionToken(filter.PlayerID.Value , filter.IsRemoveVendorSessionTokenCache);
                return;
            }

            if (filter.MerchantID.HasValue)
                extraWhere += " AND PlayerID in (SELECT PlayerID FROM [PGS_Master].[dbo].[Player] WHERE MerchantID = @MerchantID ) ";

            if (filter.PlayerID.HasValue)
                extraWhere += " AND PlayerID = @PlayerID ";

            string sql = @$"UPDATE {TableName} 
                            SET ExpiredTime = @emptyDate
                            WHERE ExpiredTime > @beginTime {extraWhere}";

            var para = new
            {
                emptyDate = emptyDate,
                beginTime = beginTime,
                MerchantID = filter.MerchantID,
                PlayerID = filter.PlayerID,
            };

            ExecuteSqlByDapper(sql, para , 120);
        }


        public void ResetPlayerAllSessionToken(int playerID , bool isAlwaysRemoveExistCache = false)
        {
            //查出還存活的cache
            var queryExistToken = [REDACTED] QueryVendorSessionTokenRequestViewModel
            {
                PlayerIDs = playerID.ToListCollection(),
                IsNotExpired = true
            } , selectColumns: " Id , SessionToken , GameID , CreateTime ");

            if (queryExistToken.IsEmptyOrNull())
                return;

            List<long> Ids = queryExistToken.Select(x => x.Id).ToList();

            string sql = @$"UPDATE {TableName} 
                            SET ExpiredTime = @emptyDate
                            WHERE Id in @Ids";

            DateTime emptyDate = DateTimeHelper.GetDefaultTime();

            var para = new
            {
                emptyDate = emptyDate,
                playerID = playerID,
                Ids = Ids
            };

            ExecuteSqlByDapper(sql, para);

            List<string> deleteTokens = new List<string>();

            if(isAlwaysRemoveExistCache)
            {
                //一律清除全部cache  但是執行flush 有時候會出現timeout、導致API回應錯誤... 默認不要 (2024/03)
                foreach (var item in queryExistToken)
                {
                    deleteTokens.Add(item.SessionToken);
                }
            }
            else
            {
                var now = DateTimeHelper.GetCurrentTime();

                int gameVendorAGG = (int)GameVendorTypeEnum.AGG;

                bool isAllAGGGames = queryExistToken.All(z => AppHelper.GetGameVendorByGameID(z.GameID) == gameVendorAGG);

                foreach (var item in queryExistToken)
                {
                    //如果都是玩AGG遊戲、不需要處理 (他們服務器會擋、節省一些cache資源浪費)
                    if(isAllAGGGames)
                    {
                        continue;
                    }

                    //如果是幾分鐘內(短時間內) 的連續登入 才會清除cache
                    if ((now - item.CreateTime).TotalMinutes <= 5)
                    {
                        deleteTokens.Add(item.SessionToken);
                    }
                }
            }

            if(deleteTokens.IsNotEmptyNull())
            {
                foreach (var sessionToken in deleteTokens)
                {
                    string cacheKey = CacheKey.DbEntity_VendorSessionToken(sessionToken);
                    CacheHelper.RemoveCacheByKey(cacheKey, isAsync: true);
                }
            }
        }

        public VendorSessionTokenEntity GetTokenByCache(string sessionToken)
        {
            string cacheKey = CacheKey.DbEntity_VendorSessionToken(sessionToken);

            int expiredMinute = 3; //這個cache 不要設太久，為了避免token該失效的情況沒有反應 (2024某個版本後 先移除清cache邏輯了)

            var queryCacche = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var query = GetByFilter(new QueryVendorSessionTokenRequestViewModel
                {
                    SessionToken = [REDACTED]
                });
                return query.FirstOrDefault();
            }, expiredMinute);

            return queryCacche;
        }


        public VendorSessionBriefModel GetTokenBriefByCache(string sessionToken)
        {
            string cacheKey = CacheKey.DbEntity_VendorSessionToken(sessionToken);

            int expiredMinute = 3; //這個cache 不要設太久，為了避免token該失效的情況沒有反應

            var queryCacche = CacheHelper.GetCacheOrQuery(cacheKey, () =>
              {
                  var query = GetByFilter(new QueryVendorSessionTokenRequestViewModel
                  {
                      SessionToken = [REDACTED]
                  })
                  .FirstOrDefault();

                  if (query == null)
                      return null;

                  return query.ReturnInjecteObject<VendorSessionBriefModel>();

              }, expiredMinute);

            return queryCacche;
        }

        public VendorSessionTokenEntity GetByToken(string sessionToken)
        {
            var query = GetByFilter(new QueryVendorSessionTokenRequestViewModel
            {
                SessionToken = [REDACTED]
            });

            return query.FirstOrDefault();
        }

        public VendorSessionTokenEntity GetPlayerLatestToken(int playerID)
        {
            string sql = @" SELECT TOP 1 * FROM [dbo].[VendorSessionToken] WITH (NOLOCK)
                            WHERE PlayerID = @playerID
                            ORDER BY Id desc ";

            var param = new
            {
                playerID = playerID
            };

            var query = QueryByDapper<VendorSessionTokenEntity>(sql, param);
            return query.FirstOrDefault();
        }



        public List<VendorSessionTokenEntity> GetByFilter(QueryVendorSessionTokenRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter , selectColumns);
            var query = QueryByDapper<VendorSessionTokenEntity>(sql, filter);
            return query;
        }


        public PagedList<VendorSessionTokenEntity> GetPagingByFilter(QueryVendorSessionTokenRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter , selectColumns);
            var query = GetPagingEntitiesBySQLScript<VendorSessionTokenEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryVendorSessionTokenRequestViewModel filter , string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += " AND PlayerID in @PlayerIDs ";

            if (filter.Status.IsNotEmptyNull())
                sql += " AND Status in @Status ";

            if (filter.SessionToken.IsNotEmptyTextNotNull())
                sql += " AND SessionToken = [REDACTED] ";

            if (filter.IsNotExpired)
                sql += " AND  ExpiredTime > GETUTCDATE() ";

            return sql;
        }

    }


}
