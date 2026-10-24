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
using target.Models.Models.StoreProcedures;
using System.Data.SqlClient;
using System.Data;
using Lck.Utility.Extensions.Helper;
using target.Utility;
using target.Models.Models;
using target.Utility.Helper;

namespace target.DA.Repository
{

    public class GameListRepository : FormatIntegerPrimaryKeyRepository<GameListEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[GameList]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "GameID"; }
        }

        public int GetMaxIdByVendor(int vendorID)
        {
            //999系列通常是大廳
            List<int> excludeIDs = new List<int>
            {
                vendorID + 999,
                vendorID + 998,
                vendorID + 997,
            };

            string sql = "SELECT max(GameID) FROM [dbo].[GameList] where GameVendorType = @vendorID AND GameID not in @excludeIDs ";

            var param = new
            {
                vendorID = vendorID,
                excludeIDs = excludeIDs,
            };

            using (var conn = InstanceSQLConn())
            {
                var query = conn.Query<int>(sql, param);
                return query.FirstOrDefault();
            }
        }


        public void SetDefaultValueForNewInsertEntity(GameListEntity entity)
        {
            entity.IsSupportSingleWallet = true;
            entity.IsSupportTrialMerchantLaunch = true;
            entity.IsSupportForAccountingIgnorePlayer = true;
            entity.IsSkipInsertZeroBet = false;
            entity.IsHideAfterAmountInfo = false;
            entity.CreateTime = DateTimeHelper.GetCurrentTime();
        }

        /// <summary>
        /// slot88的遊戲 會歸類在pgs中
        /// </summary>
        public List<GameVendorGamesModel> GetAllGameVendorMapping()
        {
            List<GameVendorGamesModel> listResult = new List<GameVendorGamesModel>();

            var queryAll = GetAll();

            var groupBy = queryAll
                .GroupBy(z => z.GameVendorType == GameVendorTypeEnum.Slot88.ToInteger() ? GameVendorTypeEnum.target.ToInteger() : z.GameVendorType)
                .Select(z => new GameVendorGamesModel
                {
                    GameVendorID = z.Key,
                    GameIDs = z.Select(z => z.GameID).ToList()
                })
                .ToList();

            return groupBy;
        }



        public void CheckNullOrStatusThenException(GameListEntity queryGame)
        {
            if (queryGame == null)
                throw new TerproException(ApiErrorCode.NotFoundGame);

            if(queryGame.Status != NormalActiveStatus.Enable.ToInteger())
                throw new TerproException(ApiErrorCode.InvalidStatus_GameStatus);
        }

        public GameListEntity GetSingleByCache(int gameID)
        {
            if (gameID <= 0)
                return null;

            string cacheKey = CacheKey.DbEntity_GameList(gameID);

            var queryCachae = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var querySingle = GetById(gameID);
                return querySingle;

            }, expiredMinute: 30);

            return queryCachae;
        }

        public bool IsAnyGamesNotSupportSingleWallet(List<int> gameIDs)
        {
            string sql = @"SELECT count(*) from [dbo].[GameList] WITH (NOLOCK)
                           Where [IsSupportSingleWallet] = 0 AND GameID in @gameIDs ";

            var param = new
            {
                gameIDs = gameIDs
            };

            var queryCount = QueryByDapper<int>(sql, param).FirstOrDefault();
            return queryCount > 0;
        }


        public GameListEntity GetSingleByCacheAndCheckValid(int gameID)
        {
            var queryGame = GetSingleByCache(gameID);

            if (queryGame == null)
                throw new TerproException(ApiErrorCode.NotFoundGame);

            if(queryGame.Status != NormalActiveStatus.Enable.ToInteger())
                throw new TerproException(ApiErrorCode.InvalidStatus_GameStatus);

            return queryGame;
        }

        /// <summary>
        /// GPI遊戲商專用的查詢
        /// </summary>
        public GameListEntity GetGameCacheByGPIGameCode(string gpiGameCode)
        {
            string cacheKey = CacheKey.DbEntity_GameList_GPIGameCode(gpiGameCode);

            //gpi有兩種gameCode ...只好這樣寫了 (launch 跟 bet 給的gameCode不同...暈)
            var queryCachae = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var queryGames = GetByFilter(new QueryGameListRequestViewModel
                {
                    GameVendorTypes = GameVendorTypeEnum.GPI.ToInteger().ToListCollection(),
                    GameCodeOrSecondGameCode = gpiGameCode
                });

                var matchGame = queryGames
                    .OrderBy(x => x.ThirdPartySecondCode == gpiGameCode ? 0 : 999)
                    .FirstOrDefault();

                return matchGame;

            }, expiredMinute: 10);

            return queryCachae;
        }


        public GameListEntity GetGameCacheByVendorGameCode(int vendorID, string thirdPartyGameCode)
        {
            string cacheKey = CacheKey.DbEntity_GameList_GameVendorCode(vendorID, thirdPartyGameCode);

            var queryCachae = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var querySingle = GetGamebyVendorGameCode(vendorID , thirdPartyGameCode);
                return querySingle;

            }, expiredMinute: 10);

            return queryCachae;
        }


        public GameListEntity GetGamebyVendorGameCode(int vendorID , string thirdPartyGameCode)
        {
            var query = GetByFilter(new QueryGameListRequestViewModel
            {
                ThirdPartyGameCode = thirdPartyGameCode,
                GameVendorTypes = vendorID.ToListCollection()
            });

            return query.FirstOrDefault();
        }


        public List<Stat_GameHealthSummarySPResultModel> GetGameHealthResult(DateTime begin , DateTime end , List<int> merchantIDs)
        {
            var para = new
            {
                dateStart = begin,
                dateEnd = end,
                merchantList = string.Join("," , merchantIDs) ?? ""
            };

            string myConn = GetConnectionStringByDbName(DBConnType.StatReportDB);
            using (var conn = new SqlConnection(myConn))
            {
                var user = conn.Query<Stat_GameHealthSummarySPResultModel>("PrWeb_Stat_GameServiceStatus", para, commandType: CommandType.StoredProcedure);
                return user.ToList();
            }
        }



        public List<GameListEntity> GetAllStatusEnableGames()
        {
            var filter = new QueryGameListRequestViewModel
            {
                Status = NormalActiveStatus.Enable.ToInteger()
            };

            var query = GetByFilter(filter);
            return query;
        }


        public bool IsGameExistByCache(int gameID)
        {
            var queryCachae = GetSingleByCache(gameID);
            return queryCachae != null;
        }

        public GameListEntity GetByIdWithSelectColumns(int gameID, string selectColumns)
        {
            var query = GetByFilter(new QueryGameListRequestViewModel
            {
                GameIDs = gameID.ToListCollection()
            }, selectColumns);

            return query.FirstOrDefault();
        }


        public List<GameListEntity> GetByFilter(QueryGameListRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter , selectColumns);
            var query = QueryByDapper<GameListEntity>(sql, filter);
            return query;
        }


        public PagedList<GameListEntity> GetPagingByFilter(QueryGameListRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<GameListEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryGameListRequestViewModel filter , string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in @GameIDs ";

            if (filter.GameID_MinValue.HasValue)
                sql += " AND GameID >= @GameID_MinValue ";

            if (filter.GameID_MaxValue.HasValue)
                sql += " AND GameID < @GameID_MaxValue ";

            if (filter.GameVendorTypes.IsNotEmptyNull())
                sql += " AND GameVendorType in @GameVendorTypes ";

            if (filter.GameCode.IsNotEmptyTextNotNull())
                sql += " AND GameCode = @GameCode ";

            if (filter.GameCodeOrSecondGameCode.IsNotEmptyTextNotNull())
                sql += " AND ( ThirdPartyGameCode = @GameCodeOrSecondGameCode OR ThirdPartySecondCode =@GameCodeOrSecondGameCode ) ";

            if (filter.ThirdPartyGameCode.IsNotEmptyTextNotNull())
                sql += " AND ThirdPartyGameCode = @ThirdPartyGameCode ";

            if (filter.GameType.HasValue)
                sql += " AND GameType = @GameType ";

            if (filter.Status.HasValue)
                sql += " AND Status = @Status ";

            if (filter.IsNotDelete == true)
                sql += $" AND Status != {NormalActiveStatus.PermanentDelete.ToInteger()} ";

            if (filter.AssignMerchantIDs.IsNotEmptyNull())
                sql += $" AND GameID in ( SELECT GameID FROM [dbo].[MerchantGameSetting] WHERE MerchantID in @AssignMerchantIDs AND Status = 1 )";

            if(filter.IsSupportSingleWallet.HasValue)
                sql += " AND IsSupportSingleWallet = @IsSupportSingleWallet ";

            if(filter.IsSupportRaiseProcess.HasValue)
            {
                int settingKey = GameListCustomizedSettingKeyEnum.IsSupportRaiseProcessWhenPlaceBet.ToInteger();
                string extraWhere = filter.IsSupportRaiseProcess == true ? "": " not ";
                sql += $@" AND GameID {extraWhere} in 
                          ( 
                             SELECT GameID FROM [dbo].[GameListCustomizedSetting] WITH (NOLOCK) 
                             WHERE SettingKey = {settingKey} AND SettingValue = '1'
                          )";
            }

            if (filter.IsForbiddenDirectLaunchGame.HasValue)
            {
                int settingKey = GameListCustomizedSettingKeyEnum.IsForbiddenDirectLaunchGame.ToInteger();
                string extraWhere = filter.IsForbiddenDirectLaunchGame == true ? "" : " not ";
                sql += $@" AND GameID {extraWhere} in 
                          ( 
                            SELECT GameID FROM [dbo].[GameListCustomizedSetting] WITH (NOLOCK) 
                            WHERE SettingKey = {settingKey} AND SettingValue = '1'
                          )";
            }

            if(filter.FilterGameCutomizedSettingKey.HasValue && filter.FilterGameCutomizedSettingValue.IsNotEmptyTextNotNull())
            {
                int settingKey = filter.FilterGameCutomizedSettingKey.Value;
                bool isNotEqual = filter.FilterGameCutomizedSettingValue.Contains("!=");

                filter.FilterGameCutomizedSettingValue = filter.FilterGameCutomizedSettingValue.Replace("!=", "");

                string extraWhere = isNotEqual ? " not " : "";
                sql += $@" AND GameID {extraWhere} in 
                          ( 
                            SELECT GameID FROM [dbo].[GameListCustomizedSetting] WITH (NOLOCK) 
                            WHERE SettingKey = {settingKey} AND SettingValue = @FilterGameCutomizedSettingValue
                          )";
            }

            return sql;
        }

    }


}
