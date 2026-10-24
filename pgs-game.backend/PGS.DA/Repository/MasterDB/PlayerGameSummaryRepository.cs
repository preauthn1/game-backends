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

    public class PlayerGameSummaryRepository : FormatIntegerPrimaryKeyRepository<PlayerGameSummaryEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[PlayerGameSummary]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "PlayerID"; }
        }


        /// <summary>
        /// 如果summary資料不存在 新增一筆 (判斷多個gameID)
        /// </summary>
        public void CreateDefaultGameSummaryData(int playerID, List<int> gameIDs)
        {
            try
            {
                var queryExistGames = GetByFilter(new QueryPlayerGameSummaryRequestViewModel
                {
                    GameIDs = gameIDs,
                    PlayerIDs = playerID.ToListCollection()
                });

                List<int> existGameIDs = queryExistGames.Select(z => z.GameID).ToList();

                List<int> notExistGameIDs = gameIDs.Where(z => !existGameIDs.Contains(z)).ToList();

                if(notExistGameIDs.IsEmptyOrNull())
                {
                    return;
                }

                var now = DateTimeHelper.GetCurrentTime();

                CreateMany(notExistGameIDs
                    .Select(gameID => new PlayerGameSummaryEntity
                    {
                        GameID = gameID,
                        PlayerID = playerID,
                        UpdateTime = now,
                        CreateTime = now,
                    }).ToList()
                );
            }
            catch (Exception ex)
            {
                string msg = "";
            }
        }


        /// <summary>
        /// 如果資料不存在 新增一筆
        /// </summary>
        public void CreateDefaultGameSummaryData(int playerID, int gameID)
        {
            this.CreateDefaultGameSummaryData(playerID, gameID.ToListCollection());
        }
               

        public void PlayerGameSummaryValuePlus(PlayerGameSummaryUpdateParameter model)
        {
            model.RuleFor(z => z.PlayerID);
            model.RuleFor(z => z.GameID);

            string winOrloss = model.WinLossAmount >= 0 ? " WinCount = WinCount + 1 , " : " LoseCount = LoseCount + 1 , ";
            string sql = $@" UPDATE {TableName} SET

                                {winOrloss}
                                Score = Score + @WinLossAmount , 
                                BetAmount = BetAmount + @BetAmount ,
                                ValidAmount = ValidAmount + @ValidAmount ,
                                PayoutAmount = PayoutAmount + @PayoutAmount ,
                                TaxAmount = TaxAmount +  @TaxAmount ,
                                UpdateTime = GETUTCDATE()

                            WHERE PlayerID = @PlayerID AND GameID = @GameID";

            var result = UpdateAndSelectRowCount(sql, model);

            if(result.UpdateRowCount <= 0)
            {
                var now = DateTimeHelper.GetCurrentTime();
                Create(new PlayerGameSummaryEntity
                {
                    GameID = model.GameID,
                    PlayerID = model.PlayerID,
                    BetAmount = model.BetAmount,
                    PayoutAmount = model.PayoutAmount,
                    ValidAmount = model.ValidAmount,
                    Score = model.WinLossAmount,
                    TaxAmount = model.TaxAmount,
                    UpdateTime = now,
                    CreateTime = now,
                });
            }
        }


        public PlayerGameSummaryEntity GetByPlayerAndGameID(int playerID , int gameID)
        {
            var filter = new QueryPlayerGameSummaryRequestViewModel
            {
                GameIDs = gameID.ToListCollection(),
                PlayerIDs = playerID.ToListCollection()
            };

            var query = GetByFilter(filter);
            return query.FirstOrDefault();
        }


        public List<PlayerGameSummaryEntity> GetByFilter(QueryPlayerGameSummaryRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter , selectColumns);
            var query = QueryByDapper<PlayerGameSummaryEntity>(sql, filter);
            return query;
        }


        public PagedList<PlayerGameSummaryEntity> GetPagingByFilter(QueryPlayerGameSummaryRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter , selectColumns);
            var query = GetPagingEntitiesBySQLScript<PlayerGameSummaryEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryPlayerGameSummaryRequestViewModel filter , string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += " AND PlayerID in @PlayerIDs ";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in @GameIDs ";

            return sql;
        }

    }


}
