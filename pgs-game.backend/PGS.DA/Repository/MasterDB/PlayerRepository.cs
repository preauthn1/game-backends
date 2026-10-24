using Dapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using game.Entity.Game;
using Lck.Utility.Extensions.Helper;
using MongoDB.Driver;
using target.Core.Helper;
using target.Core.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace target.DA.Repository
{
    public class PlayerRepository : FormatIntegerPrimaryKeyRepository<PlayerEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[Player]"; }
        }

        protected override string PrimaryKeyColumnName 
        {
            get { return "PlayerID"; }
        }


        public void TestDebug_TransactionIn()
        {
            using (var connection = InstanceSQLConn())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    string sql1 = @" update [dbo].[Player] set UpdateTime = getUTCDate() where PlayerID = 1000000 ";
                    string sql2 = @" update [dbo].[PlayerWallet] set Amount = Amount + 1 where PlayerID = 1000000 ";

                    connection.Execute(sql1, null, transaction);
                    connection.Execute(sql2, null, transaction);
                    transaction.Commit();
                }
            }           
        }


        public void UpdatePlayerWalletAfterGameSettle(WriteGameResulSPParameterModel model)
        {
            string spName = "PrWeb_GamePlay_UpdatePlayerWalletAfterGameSettle";
            ExecuteStoreProcedureByDapper(spName , model);
        }


        /// <summary>
        /// 新增 Player 除了此處
        /// 還有另一個SP PrSys_CreateRobot (機器人)
        /// </summary>
        public CRUDResultModel CreatePlayerAndWallet(PlayerEntity model , decimal defaultWalletAmount)
        {
            string insertPlayerSQL = SQLHelper.GetInsertCommand<PlayerEntity>(TableName);

            //step 1. 建立player
            //step 2. 建立playerWallet
            //step 3. 建立PlayerTotalSummary

            string sql = $@" DECLARE @newID int;
                            {insertPlayerSQL}
                            SET @newID = (SELECT @@IDENTITY)     
                            INSERT INTO [dbo].[PlayerWallet] (PlayerID , WalletType , Amount) 
                            Values (@newID , {WalletTypeEnum.GameWallet.ToInteger()} , {defaultWalletAmount}) 
                            INSERT INTO [dbo].[PlayerTotalSummary] (PlayerID ,SumTax ,SumLoginTimes ,SumFlowAmount) 
                            VALUES (@newID , 0 , 0 , 0) 
                            SELECT @newID ";

            //包交易transaction
            using (var connection = InstanceSQLConn())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var resultId = connection.Query<int>(sql, model , transaction).FirstOrDefault();
                    transaction.Commit();
                    return new CRUDResultModel
                    {
                        IsSuccess = true,
                        ResultId = resultId
                    };
                }
            }
        }

        public PlayerEntity GetSingleByCache(int playerID , bool isLockQuery = false)
        {
            string cacheKey = CacheKey.DbEntity_Player(playerID);

            var queryCachae = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var querySingle = GetById(playerID , isLockQuery);
                return querySingle;

            } , expiredMinute: 5);

            return queryCachae;
        }


        public void CheckStatusAndThrowException<T>(T queryPlayer) where T: IPlayerEntity
        {
            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            if(queryPlayer.Status != NormalActiveStatus.Enable.ToInteger())
                throw new TerproException(ApiErrorCode.InvalidStatus_AccountStatus);
        }


        public PlayerBriefModel GetBriefByCache(int playerID)
        {
            if (playerID <= 0)
                return null;

            string cacheKey = CacheKey.AdminSys_PlayerBriefInfo(playerID);

            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   var q = GetBriefByID(playerID);
                   return q;

               }, expiredMinute: 15);

            return queryCache;
        }


        /// <summary>
        /// 查詢精簡版欄位
        /// </summary>
        public PlayerBriefModel GetBriefByID(int playerID)
        {
            string sql = $@" SELECT PlayerID , MerchantID , PlayerName , Status , Currency
                             FROM {TableName} WITH (NOLOCK) 
                             WHERE playerID = @playerID ";

            var query = QueryByDapper<PlayerBriefModel>(sql, new { playerID = playerID });
            return query.FirstOrDefault();
        }


        public PlayerEntity GetByPlayerNameAndMerchantIdFromCache(string playerName, int merchantID , string selectColumns = " PlayerID , PlayerName , MerchantID , Status , Currency " , bool isLockQuery = true)
        {
            string cacheKey = CacheKey.DbEntity_Player_ByPlayerNameMerchantID(playerName , merchantID , isLockQuery);

            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   PlayerEntity query = GetByPlayerNameAndMerchantID(playerName, merchantID , selectColumns , isLockQuery);
                   return query;

               } , 60);

            return queryCache;
        }


        public void UpdateStatus(int playerID, int targetStatus)
        {
            string sql = $@"UPDATE {TableName} 
                            SET Status = @targetStatus 
                            WHERE PlayerID = @PlayerID";

            var para = new
            {
                targetStatus = targetStatus,
                PlayerID = playerID
            };

            ExecuteSqlByDapper(sql, para);
        }



        public void UpdateLastLoginTime(int playerID)
        {
            string sql = $@"UPDATE {TableName} SET 
                                LastLoginTime = GETUTCDATE() 
                            WHERE PlayerID = @PlayerID";
            
            var para = new
            {
                PlayerID = playerID,
            };

            ExecuteSqlByDapper(sql, para);
        }

        public List<PlayerBriefModel> GetByLargeAccounts(int merchantID , List<string> playerNameList)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"CREATE TABLE #tempIDs (Id nvarchar(50) not null primary key) ");

            string mySQL = "INSERT INTO #tempIDs VALUES " + string.Join(",", playerNameList.Select(x => $"('{x}')")) + " ;";
            sb.Append(mySQL);

            string tempTableSQL = sb.ToString();

            string sql = $@"SELECT PlayerID FROM {TableName} WITH (NOLOCK)
                            WHERE MerchantID = {merchantID} AND PlayerName in ( SELECT Id FROM #tempIDs ) ";

            using (var db = InstanceSQLConn())
            {
                db.Open();
                db.Execute(tempTableSQL);

                var query = db.Query<PlayerBriefModel>(sql).ToList();
                return query;
            }
        }


        public List<int> GetPlayerIDsByPlayerName(string playerName, List<int> availableMerchantIDs = null)
        {
            var query = GetByPlayerName(playerName, availableMerchantIDs , isLockQuery: false);
            return query.Select(x => x.PlayerID).ToList();
        }


        public PlayerEntity GetByPlayerNameAndMerchantID(string playerName, int merchantID, string selectColumns = "*" , bool isLockQuery = true)
        {
            var filter = new QueryPlayerRequestViewModel
            {
                PlayerName = playerName,
                MerchantID = merchantID,
                IsLockQuery = isLockQuery,
            };

            string sql = GenerateWhereSQL(filter , selectColumns);

            var param = new
            {
                PlayerName = ToVarchar(playerName, 50),
                MerchantID = merchantID,
            };

            var query = QueryByDapper<PlayerEntity>(sql, param);
            return query.FirstOrDefault();
        }


        /// <summary>
        /// 多商戶架構下，可能不是唯一值
        /// </summary>
        public List<PlayerEntity> GetByPlayerName(string playerName , List<int> availableMerchantIDs = null , bool isLockQuery = true)
        {
            var filter = new QueryPlayerRequestViewModel
            {
                PlayerNames = new List<string> { playerName.Trim() },
                MerchantIDs = availableMerchantIDs.IsEmptyOrNull() ? new List<int>() : availableMerchantIDs,
                IsLockQuery = isLockQuery,
            };

            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<PlayerEntity>(sql, filter);
            return query;
        }

        public List<PlayerJoinInfoModel> GetPlayerJoinInfo(List<int> playerIds)
        {
            if (playerIds.IsEmptyOrNull())
                return new List<PlayerJoinInfoModel>();

            string sql = @" SELECT top 2500 
                            p.PlayerID , p.PlayerName , m.MerchantID , m.Name as MerchantName , m.Currency , m.WalletMode
                            FROM [dbo].[Player] as p with (NOLOCK)
                            LEFT JOIN [dbo].[Merchant] as m on p.MerchantID = m.MerchantID
                            WHERE p.PlayerID in @playerIDs";

            var param = new
            {
                playerIDs = playerIds
            };

            var query = QueryByDapper<PlayerJoinInfoModel>(sql, param);
            return query;
        }


        public List<PlayerEntity> GetByFilter(QueryPlayerRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter , selectColumns);
            var query = QueryByDapper<PlayerEntity>(sql, filter);
            return query;
        }


        public PagedList<PlayerEntity> GetPagingByFilter(QueryPlayerRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter , selectColumns);
            var query = GetPagingEntitiesBySQLScript<PlayerEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryPlayerRequestViewModel filter , string selectColumns = "*")
        {
            string noLock = GetLockQuerySQL(filter.IsLockQuery);

            string logSource = GenerateDebugSourceInfo(filter.DebugMethodSource);
            string sql = $@" {logSource} SELECT {selectColumns} FROM {TableName} {noLock}
                           WHERE 1=1 ";

            //*這個的順序要在merchantIDs之前
            if (filter.FilterMerchantIDByHierarchyMerchant.HasValue)
            {
                filter.MerchantIDs = AppHelper.SetListToEmptyWhenLargeFilter(filter.MerchantIDs);
                sql += AppHelper.GenerateMerchantHierarchyWhereSQL(filter.FilterMerchantIDByHierarchyMerchant.Value);
            }
            else if(filter.FilterMerchantIDsByMultipleHierarchyMerchant.IsNotEmptyNull())
            {
                filter.MerchantIDs = AppHelper.SetListToEmptyWhenLargeFilter(filter.MerchantIDs);

                string sql1 = $" {AppHelper.GenerateMultipleHierarchyMerchantFilterWhereSQL("MerchantID", "")} ";
                string sql2 = " MerchantID in @FilterMerchantIDsByMultipleHierarchyMerchant ";
                sql += $" AND ( {sql1} OR {sql2} )";
            }

            if (filter.MerchantIDs.IsNotEmptyNull())
                sql += " AND MerchantID in @MerchantIDs ";

            if (filter.MerchantID.HasValue)
                sql += " AND MerchantID = @MerchantID ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += " AND PlayerID in @PlayerIDs ";

            if (filter.PlayerNames.IsNotEmptyNull())
                sql += " AND PlayerName in @PlayerNames ";

            if (filter.StatusValues.IsNotEmptyNull())
                sql += " AND Status in @StatusValues ";

            if (!string.IsNullOrWhiteSpace(filter.PlayerName))
            {
                filter.PlayerName = filter.PlayerName.Trim();
                sql += " AND PlayerName = @PlayerName ";
            }

            if (!string.IsNullOrWhiteSpace(filter.PlayerNameFuzzyQuery))
            {
                filter.PlayerNameFuzzyQuery = filter.PlayerNameFuzzyQuery.Trim();
                sql += $" AND {GenerateLikeQuery("PlayerName", "@PlayerNameFuzzyQuery")} ";
            }

            if (filter.CreateTimeBegin.HasValue)
                sql += " AND CreateTime >= @CreateTimeBegin ";

            if (filter.CreateTimeEnd.HasValue)
                sql += " AND CreateTime < @CreateTimeEnd ";

            if (filter.LastLoginTimeBegin.HasValue)
                sql += " AND LastLoginTime >= @LastLoginTimeBegin ";

            if (filter.LastLoginTimeEnd.HasValue)
                sql += " AND LastLoginTime < @LastLoginTimeEnd ";

            if (filter.TagID.HasValue)
                sql += " AND PlayerID in ( SELECT PlayerID FROM [dbo].[PlayerTagMapping] WHERE TagID = @TagID ) ";

            if(filter.WalletAmountLargeThan.HasValue)
                sql += " AND PlayerID in ( SELECT PlayerID FROM [dbo].[PlayerWallet] WITH (NOLOCK) WHERE [Amount] > @WalletAmountLargeThan ) ";
            
            return sql;
        }

    }


}
