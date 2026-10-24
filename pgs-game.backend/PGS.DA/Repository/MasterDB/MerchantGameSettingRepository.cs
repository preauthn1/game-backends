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
using target.Utility.Helper;
using Lck.Utility.Extensions.Helper;

namespace target.DA.Repository
{

    public class MerchantGameSettingRepository : FormatIntegerPrimaryKeyRepository<MerchantGameSettingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[MerchantGameSetting]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        

        /// <summary>
        /// 檢查 1. 商戶是否開啟此遊戲
        /// 檢查 2. 系統是否開啟此遊戲
        /// </summary>
        public bool IsMerchantGameEnableInSystem(int merchantID , int gameID)
        {
            string sql = @"SELECT Id FROM [dbo].[MerchantGameSetting]
                           WHERE MerchantID = @merchantID AND[Status] = 1 AND GameID in
                            (
                                SELECT GameID FROM GameList
                                WHERE Status = 1 AND GameID = @gameID
                            ) ";

            using (var conn = InstanceSQLConn())
            {
                var query = conn.Query<int>(sql, new { merchantID, gameID }).FirstOrDefault();
                return query > 0;
            }
        }

        public List<QueryMerchantCountResultModel> GetEachMerchantEnableGameCount(List<int> merchantIDs)
        {
            if (merchantIDs.IsEmptyOrNull())
                return new List<QueryMerchantCountResultModel>();

            bool isLargeAmount = merchantIDs.Count() >= 1000;

            string extraWhere = isLargeAmount ? "" : " AND MerchantID in @merchantIDs ";

            string sql = $@"SELECT MerchantID , count(*) as TotalCount 
                           FROM [dbo].[MerchantGameSetting] WITH (NOLOCK)
                           WHERE Status = 1 {extraWhere}
                           GROUP BY MerchantID";

            var para = new
            {
                merchantIDs = merchantIDs
            };

            var query = QueryByDapper<QueryMerchantCountResultModel>(sql , para);

            if(isLargeAmount)
            {
                query = query
                    .Where(x => merchantIDs.Contains(x.MerchantID))
                    .ToList();
            }

            return query;
        }

        /// <summary>
        /// 查出有開通特定遊戲商遊戲的所有商戶
        /// </summary>
        public List<int> GetEnableSpecificGameMerchantIDs(GameVendorTypeEnum gameVendorType)
        {
            int minGameID = gameVendorType.ToInteger();
            int maxGameID = minGameID + 9999;

            string sql = @"SELECT distinct MerchantID 
                           FROM [dbo].[MerchantGameSetting] with (NOLOCK)
                           WHERE GameID >= @minGameID AND GameID < @maxGameID AND Status = 1";

            var para = new
            {
                minGameID = minGameID,
                maxGameID = maxGameID,
            };

            var query = QueryByDapper<int>(sql, para);
            return query;
        }


        public List<QueryGameEnableMerchantCountResultModel> GetEachGameEnableMerchantCount()
        {
            string sql = @"SELECT GameID , count(*) as TotalCount 
                           FROM [dbo].[MerchantGameSetting] with (NOLOCK)
                           WHERE Status = 1 AND MerchantID in ( SELECT MerchantID FROM [dbo].[Merchant] WITH (NOLOCK) where status != 999 )
                           GROUP BY GameID";

            var query = QueryByDapper<QueryGameEnableMerchantCountResultModel>(sql, null);
            return query.ToList();
        }


        public void DeleteByIDs(List<int> merchantIDs, List<int> gameIDs)
        {
            //保險起見 不允許傳入空陣列
            if (merchantIDs.IsEmptyOrNull() || gameIDs.IsEmptyOrNull())
                return;

            var splitArrayToSmallSize = merchantIDs.SplitListByArraySize(2000);

            foreach(var myIDs in splitArrayToSmallSize)
            {
                if (myIDs.IsEmptyOrNull())
                    continue;

                var para = new
                {
                    MerchantIDs = myIDs,
                    GameIDs = gameIDs
                };

                string sql = $" DELETE FROM {TableName} WHERE MerchantID in @MerchantIDs AND GameID in @GameIDs";
                ExecuteSqlByDapper(sql, para);
            }

        }

        public List<int> GetMerchantAvailableGameIDs(int merchantID)
        {
            var filter = new QueryMerchantGameSettingRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection(),
                Status = NormalActiveStatus.Enable.ToInteger(),
                EnableGameOnly = true
            };

            var query = GetByFilter(filter);
            return query.Select(x => x.GameID).ToList();
        }


        public List<int> GetMerchantEnableGameIDsByCache(int merchantID)
        {
            string cacheKey = CacheKey.Entity_GetMerchantEnableGameIDs(merchantID);

            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   var query = GetByMerchantID(merchantID, NormalActiveStatus.Enable);
                   return query.Select(x => x.GameID).ToList();

               } , expiredMinute: 15);

            return queryCache ?? new List<int>();
        }


        public List<MerchantGameSettingEntity> GetByMerchantID(int merchantID , NormalActiveStatus? status = null)
        {
            var filter = new QueryMerchantGameSettingRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection()
            };

            if (status.HasValue)
                filter.Status = status.Value.ToInteger();

            var query = GetByFilter(filter);
            return query;
        }


        public MerchantGameSettingEntity GetByMerchantIDAndGameID(int merchantID , int gameID)
        {
            var filter = new QueryMerchantGameSettingRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection(),
                GameIDs = gameID.ToListCollection()
            };

            var query = GetByFilter(filter);
            return query.FirstOrDefault();
        }

        public int GetCountByFilter(QueryMerchantGameSettingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter , " count(*) ");
            var query = QueryByDapper<int>(sql, filter);
            return query.FirstOrDefault();
        }


        public List<MerchantGameSettingEntity> GetByFilter(QueryMerchantGameSettingRequestViewModel filter , string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter , selectColumns);
            var query = QueryByDapper<MerchantGameSettingEntity>(sql, filter);
            return query;
        }


        public PagedList<MerchantGameSettingEntity> GetPagingByFilter(QueryMerchantGameSettingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter , "*");

            var query = GetPagingEntitiesBySQLScript<MerchantGameSettingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryMerchantGameSettingRequestViewModel filter , string selectColumns)
        {
            string debugInfo = GenerateDebugSourceInfo("");

            string sql = $@" {debugInfo} 
                             SELECT {selectColumns} FROM {TableName} WITH (NOLOCK) 
                             WHERE 1=1 ";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in @GameIDs ";

            if (filter.MerchantIDs.IsNotEmptyNull())
                sql += " AND MerchantID in @MerchantIDs ";

            if (filter.FilterMerchantIDsByMultipleHierarchyMerchant.IsNotEmptyNull())
            {
                sql += AppHelper.GenerateMultipleHierarchyMerchantFilterWhereSQL();
            }

            if (filter.IsExcludeDeleteMerchant)
                sql += $" AND MerchantID in ( SELECT MerchantID FROM [dbo].[Merchant] WITH (NOLOCK) WHERE status != {NormalActiveStatus.PermanentDelete.ToInteger()} ) ";

            if (filter.IsExcludeDeleteGames)
                sql += $" AND GameID not in ( SELECT GameID FROM [dbo].[GameList] WITH (NOLOCK) WHERE status = {NormalActiveStatus.PermanentDelete.ToInteger()} ) ";

            if (filter.Status.HasValue)
                sql += " AND Status = @Status ";

            if (filter.EnableGameOnly)
                sql += " AND GameID in (SELECT GameID FROM [dbo].[GameList] WITH (NOLOCK) WHERE Status = 1) ";

            if (filter.IsSupportSingleWalletOnly)
                sql += " AND GameID in (SELECT GameID FROM [dbo].[GameList] WITH (NOLOCK) WHERE [IsSupportSingleWallet] = 1) ";

            if (filter.GameVendorIDs.IsNotEmptyNull())
                sql += " AND GameID in (SELECT GameID FROM [dbo].[GameList] WITH (NOLOCK) WHERE [GameVendorType] in @GameVendorIDs ) ";

            return sql;
        }

    }


}
