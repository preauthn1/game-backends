using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models;
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
    public class PlayerGameDataRepository : FormatIntegerPrimaryKeyRepository<PlayerGameDataEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.StatReportDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[PlayerGameData]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }


        public List<QueryDirectAgentOnlinePlayerSummaryReportByHierarchyChildDataModel> GetEachMerchantPlayersCountByHierarchyChildMapping(QueryDirectAgentSummaryReportByHierarchyChildRequestModel filter)
        {
            string extraWhere = "  mapping.MerchantID = @TargetMerchantID ";
            string extraFilterGame = " ";

            //請參考該欄位的註解說明
            if (filter.IsQueryDirectAgentDetailOnly)
            {
                extraWhere = @" mapping.MerchantID in 
		                            (	
			                            SELECT MerchantID from [PGS_Master].[dbo].[MerchantHierarchy] WITH (NOLOCK)
			                            where ParentMerchantID = @targetMerchantID
		                            )";
            }

            //請參考該欄位的註解說明
            if (filter.GameIDs.IsNotEmptyNull())
            {
                extraFilterGame = @" AND GameID in @GameIDs ";
            }

            string sql = $@"SELECT 
                            join_summary.OwnerMerchantID as MerchantID,
                            sum(join_summary.TotalPlayers) as TotalPlayers

                            FROM 
                            (
	                            SELECT 

		                            mapping.MerchantID as OwnerMerchantID , 
		                            mapping.ChildMerchantID as ChildMerchantID , 
		                            ISNULL(join_data.TotalPlayers , 0) as TotalPlayers

	                            FROM [PGS_Master].[dbo].[MerchantHierarchyChildMapping] as mapping WITH (NOLOCK)
	                            left join
	                            (
		                            SELECT 
	                                    summary.MerchantID as MerchantID , 		
	                                    count(distinct PlayerID) as TotalPlayers
                                    FROM [PGS_StatReport].[dbo].[PlayerGameData] as summary with (NOLOCK)
                                    where StatTime >= @begin AND StatTime < @end {extraFilterGame}
                                    group by summary.MerchantID

	                            ) as join_data on mapping.ChildMerchantID = join_data.MerchantID                                

	                            where {extraWhere} 	
                            ) as join_summary
                            group by join_summary.OwnerMerchantID
                            order by sum(join_summary.TotalPlayers) desc";

            var query = QueryByDapper<QueryDirectAgentOnlinePlayerSummaryReportByHierarchyChildDataModel>(sql, filter);
            return query;
        }


        public List<QueryPlayerLanguageReportModel> GetMerchantLanguageDistributeReport(int merchantID , DateTime begin, DateTime end)
        {
            //2025/10/03 已棄用此欄位 (若需要請重新寫入其他table來記錄)
            return new List<QueryPlayerLanguageReportModel>();


            string sql = @" SELECT LanguageCode , count(*) as TotalCount 
                            FROM [PGS_Master].[dbo].[Player] with (NOLOCK)
                            WHERE PlayerID in
                            (	
	                            SELECT distinct playerID 
                                FROM [PGS_StatReport].[dbo].[PlayerGameData]  with (NOLOCK)
	                            WHERE StatTime >= @beginTime AND StatTime < @endTime AND MerchantID = @merchantID
                            )
                            group by LanguageCode";

            var para = new
            {
                merchantID = merchantID,
                beginTime = begin,
                endTime = end
            };

            var query = QueryByDapper<QueryPlayerLanguageReportModel>(sql, para);
            return query.ToList();
        }


        public List<QueryEachPlayerTotalBetAmountModel> GetEachPlayerTotalBetAmount(int merchantID, DateTime begin, DateTime end)
        {
            //非捕魚遊戲
            var noneFishingGame = GetEachPlayerTotalBetAmountExcludeFishing(merchantID, begin, end);

            //捕魚遊戲
            var fishingGame = GetEachPlayerTotalValidAmountFishingGame(merchantID, begin, end);

            List<QueryEachPlayerTotalBetAmountModel> results = noneFishingGame ?? new List<QueryEachPlayerTotalBetAmountModel>();

            foreach (var fishGame in fishingGame)
            {
                var existData = noneFishingGame.FirstOrDefault(z => z.PlayerID == fishGame.PlayerID);

                if(existData != null)
                {
                    existData.BetAmount += fishGame.BetAmount; //把捕魚的有效投注額跟其他遊戲的投注額合併計算
                    existData.IsAnyFishingGame = true;
                }
                else
                {
                    noneFishingGame.Add(fishGame);
                }
            }

            return noneFishingGame;
        }

        public List<PlayerGameSummaryGroupByGameModel> GetEachGameSummary(List<int> playerIDs, DateTime begin, DateTime end)
        {
            string sql = $@"SELECT PlayerID , GameID , sum(BetAmount) as BetAmount, sum(PayoutAmount) as PayoutAmount , sum(PayoutAmount - BetAmount) as PlayerWinLoss , sum(WinCount + LoseCount + DrawCount) as TotalRound
                            FROM [dbo].[PlayerGameData] WITH (NOLOCK)
                            WHERE PlayerID in @playerIDs AND StatTime >= @begin AND StatTime < @end
                            GROUP BY PlayerID , GameID";

            var param = new
            {
                playerIDs = playerIDs,
                begin = begin,
                end = end,
            };

            var query = QueryByDapper<PlayerGameSummaryGroupByGameModel>(sql, param);
            var result = query.ToList();

            foreach (var item in result)
            {
                item.RTP = CommonLogicHelper.CalculateRTP_Percentage(item.BetAmount, item.PayoutAmount);
            }

            return result;
        }

        public List<QueryEachPlayerTotalBetAmountModel> GetEachPlayerTotalBetAmountExcludeFishing(int merchantID, DateTime begin, DateTime end)
        {
            string sql = @" SELECT PlayerID , sum(BetAmount) as BetAmount
                            FROM [dbo].[PlayerGameData] with (NOLOCK)
                            WHERE MerchantID = @merchantID AND StatTime >= @beginTime AND StatTime < @endTime AND GameID not in @fishGameIDs
                            GROUP BY PlayerID";

            var para = new
            {
                fishGameIDs = SystemConstant.FishGameIDs,
                merchantID = merchantID,
                beginTime = begin,
                endTime = end
            };

            var query = QueryByDapper<QueryEachPlayerTotalBetAmountModel>(sql, para).ToList();
            return query;
        }


        public List<QueryEachPlayerTotalBetAmountModel> GetEachPlayerTotalValidAmountFishingGame(int merchantID, DateTime begin, DateTime end)
        {
            string sql = @" SELECT PlayerID , sum(ValidAmount) as BetAmount
                            FROM [dbo].[PlayerGameData] with (NOLOCK)
                            WHERE MerchantID = @merchantID AND StatTime >= @beginTime AND StatTime < @endTime AND GameID in @fishGameIDs
                            GROUP BY PlayerID";

            var para = new
            {
                fishGameIDs = SystemConstant.FishGameIDs,
                merchantID = merchantID,
                beginTime = begin,
                endTime = end
            };

            var query = QueryByDapper<QueryEachPlayerTotalBetAmountModel>(sql, para).ToList();
            query.ForEach(z => z.IsAnyFishingGame = true);
            return query;
        }




        /// <summary>
        /// 取得不重複玩家人數 (正式商戶)
        /// </summary>
        public QueryBetSummaryBriefInfoModel GetProductionMerchantPlayerCount(DateTime begin, DateTime end)
        {
            string sql = @"SELECT count(distinct PlayerID) as TotalPlayers , sum([WinCount] + LoseCount + DrawCount) as TotalRound
                           FROM [dbo].[PlayerGameData] with (NOLOCK)
                           WHERE StatTime >= @begin AND StatTime < @end 
                            AND MerchantID in
                            (
	                            SELECT MerchantID FROM [PGS_Master].[dbo].[Merchant]  with (NOLOCK)
	                            WHERE Type = 20
                            )";

            var para = new
            {
                begin = begin,
                end = end,
            };

            var query = QueryByDapper<QueryBetSummaryBriefInfoModel>(sql, para);
            return query.FirstOrDefault();
        }


        public List<QueryEachDateBetSummaryModel> GetEachDatePlayerGameSummary(DateTime beginUTC, DateTime endUTC, int merchantID , int playerID, double timeZone)
        {
            if (beginUTC > endUTC)
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_StartTimeMustSmallThanEndTime);
            }

            string sql = $@"SELECT 
                            CAST(DATEADD(HOUR , 8 , StatTime) as DATE) AS [DATE], 
                            sum(WinCount + LoseCount + DrawCount) AS TotalRound,  
                            sum(BetAmount) AS BetAmount , 
                            sum(PayoutAmount) AS PayoutAmount, 
                            sum(ValidAmount) AS ValidAmount, 
                            sum(WinAmount - LoseAmount) AS WinLossAmount

                          FROM [dbo].[PlayerGameData] WITH (NOLOCK)
                          WHERE MerchantID = @merchantID AND PlayerID = @playerID AND StatTime >= @begin AND StatTime < @end
                          GROUP BY CAST(DATEADD(HOUR , 8 , StatTime) as DATE) ";

            var para = new
            {
                merchantID = merchantID, //因為partition是採用merchantID分割  這樣會查比較快
                playerID = playerID,  
                begin = beginUTC,
                end = endUTC,
                timeZone = timeZone
            };

            var result = QueryByDapper<QueryEachDateBetSummaryModel>(sql, para);
            return result.ToList();
        }


        public Dictionary<DateTime , List<int>> GetEachDatePlayerIDsByMerchant(DateTime beginUTC, DateTime endUTC, int merchantID , double timeZone)
        {
            if(beginUTC > endUTC)
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_StartTimeMustSmallThanEndTime);
            }

            string sql = $@"SELECT cast(DATEADD(hour , @timeZone , StatTime) as date) as Date , PlayerID 
                            FROM [dbo].[PlayerGameData] with (NOLOCK)
                            WHERE MerchantID = @merchantID AND StatTime >= @begin AND StatTime < @end
                            group by cast(DATEADD(hour , @timeZone , StatTime) as date) , PlayerID ";

            var para = new
            {
                merchantID = merchantID,  //因為partition是採用merchantID分割  這樣會查比較快
                begin = beginUTC,
                end = endUTC,
                timeZone = timeZone
            };

            var result = QueryByDapper<GroupByDateAndPlayerIDResultModel>(sql, para);

            var resultDic = new Dictionary<DateTime, List<int>>();

            int dayOffset = 0;

            while(true)
            {
                DateTime targetDate = beginUTC.AddHours(timeZone).AddDays(dayOffset).Date;

                if (targetDate > endUTC.AddHours(timeZone))
                {
                    break;
                }

                List<int> playerIDsOfTargetDate = result.Where(z => z.Date == targetDate).Select(z => z.PlayerID).ToList();
                resultDic.Add(targetDate, playerIDsOfTargetDate);

                dayOffset++;
            }

            return resultDic;
        }


        /// <summary>
        /// 取得不重複玩家人數
        /// </summary>
        public int GetPlayerCountByMerchantAndGameIDs(DateTime begin, DateTime end , int merchantID , List<int> gameIDs)
        {
            if (gameIDs.IsEmptyOrNull())
                return 0;

            string sql = @"SELECT count(distinct PlayerID) 
                           FROM [dbo].[PlayerGameData] with (NOLOCK)
                           WHERE StatTime >= @begin AND StatTime < @end AND MerchantID = @merchantID AND gameID in @gameIDs";

            var para = new
            {
                begin = begin,
                end = end,
                merchantID = merchantID,
                gameIDs = gameIDs,
            };

            var query = QueryByDapper<int>(sql , para);
            return query.FirstOrDefault();
        }


        public List<QueryEachMerchantPlayerCountAndTotalRoundModel> GetEachMerchantPlayerCountAndTotalRound(DateTime begin, DateTime end , bool isProductionMerchantOnly = true)
        {
            string extraWhere = isProductionMerchantOnly ? $" AND m.Type = {MerchantTypeEnum.Plaform_ProductionEnvironment.ToInteger()} " : "";
            string sql = $@"SELECT sub.* , m.Name FROM
                            (
	                            SELECT MerchantID , count(distinct PlayerID) as PlayerCount , sum(WinCount + LoseCount + DrawCount) TotalRound
	                            FROM [dbo].[PlayerGameData] WITH (NOLOCK)
	                            WHERE StatTime >= @begin AND StatTime < @end
	                            GROUP BY MerchantID
                            ) as sub
                            LEFT JOIN [PGS_Master].[dbo].[Merchant] as m WITH (NOLOCK) on sub.MerchantID = m.MerchantID 
                            WHERE 1=1 {extraWhere} ";

            var para = new
            {
                begin = begin,
                end = end,
            };

            var query = QueryByDapper<QueryEachMerchantPlayerCountAndTotalRoundModel>(sql, para);
            return query.ToList();
        }


        public List<QueryEachHierarchyMerchantSummaryModel> GetEachHierarchyMerchantSummaryData(DateTime begin, DateTime end, int parentMerchantID , string extraWhere = "")
        {
            string sql = $@"SELECT sub.* , m.Name FROM
                            (
	                            SELECT MerchantID , 
                                       count(distinct PlayerID) as PlayerCount , 
                                       sum(WinCount + LoseCount + DrawCount) TotalRound ,
                                       sum([BetAmount]) BetAmount ,
                                       sum([ValidAmount]) ValidAmount ,
                                       sum([PayoutAmount]) PayoutAmount ,
                                       sum([WinAmount] + [LoseAmount]) WinLossAmount 

	                            FROM [dbo].[PlayerGameData] WITH (NOLOCK)
	                            WHERE StatTime >= @begin AND StatTime < @end {extraWhere} AND MerchantID in
                                (
                                    SELECT ChildMerchantID FROM [PGS_Master].[dbo].[MerchantHierarchyChildMapping] WITH (NOLOCK)
                                    WHERE MerchantID = @parentMerchantID
                                )
	                            GROUP BY MerchantID
                            ) as sub
                            LEFT JOIN [PGS_Master].[dbo].[Merchant] as m WITH (NOLOCK) on sub.MerchantID = m.MerchantID ";

            var para = new
            {
                begin = begin,
                end = end,
                parentMerchantID = parentMerchantID
            };

            var query = QueryByDapper<QueryEachHierarchyMerchantSummaryModel>(sql, para);
            return query.ToList();
        }

        public List<QueryEachHierarchyMerchantEachGameSummaryModel> GetEachHierarchyMerchantEachGameSummaryData(DateTime begin, DateTime end, int parentMerchantID, List<string> groupByColumns , string extraWhere = "")
        {
            if (groupByColumns.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("groupByColumns"));

            string groupByColumnSql = " " + string.Join("," , groupByColumns) + " ";

            string sql = $@"SELECT sub.* FROM
                            (
	                            SELECT {groupByColumnSql} ,
                                       count(distinct PlayerID) as PlayerCount , 
                                       sum(WinCount + LoseCount + DrawCount) TotalRound ,
                                       sum([BetAmount]) BetAmount ,
                                       sum([ValidAmount]) ValidAmount ,
                                       sum([PayoutAmount]) PayoutAmount ,
                                       sum([WinAmount] + [LoseAmount]) WinLossAmount 

	                            FROM [dbo].[PlayerGameData] WITH (NOLOCK)
	                            WHERE StatTime >= @begin AND StatTime < @end {extraWhere} AND MerchantID in
                                (
                                    SELECT ChildMerchantID FROM [PGS_Master].[dbo].[MerchantHierarchyChildMapping] WITH (NOLOCK)
                                    WHERE MerchantID = @parentMerchantID
                                )
	                            GROUP BY {groupByColumnSql}
                            ) as sub ";

            var para = new
            {
                begin = begin,
                end = end,
                parentMerchantID = parentMerchantID
            };

            var query = QueryByDapper<QueryEachHierarchyMerchantEachGameSummaryModel>(sql, para);
            return query.ToList();
        }


        public List<QueryPlayerGameDataStatisticModel> GetGameSummaryGroupByPlayerAndGame(QueryPlayerGameStatisticParameter filter)
        {
            string extraWhere1 = filter.PlayerID.HasValue ? " AND PlayerID = @PlayerID " : "";
            string extraWhere2 = filter.GameID.HasValue ? " AND GameID = @GameID " : "";

            //建議一定要帶merchantID (因為partition是使用此欄位切割) 沒帶的查詢速度很慢
            string extraWhere3 = filter.MerchantID.HasValue ? " AND MerchantID = @MerchantID " : "";

            string sql = $@"SELECT PlayerID , GameID ,  
                            sum([WinCount] + [DrawCount]+ [LoseCount]) as totalRound, 
                            sum(BetAmount) as BetAmount, 
                            sum(PayoutAmount) as PayoutAmount, 
                            sum(ValidAmount) as ValidAmount,
                            sum([WinAmount] - [LoseAmount]) as WinLossAmount,
                            sum(TaxAmount) as TaxAmount
                        FROM [dbo].[PlayerGameData] with (NOLOCK)
                        WHERE StatTime >= @begin AND StatTime < @end {extraWhere1} {extraWhere2} {extraWhere3}
                        GROUP BY playerID , gameID";

            if(filter.IsJoinPlayerName)
            {
                sql = $@"SELECT sub.* , p.PlayerName FROM
                        (
	                        {sql}
                        ) as sub
                        LEFT JOIN [PGS_Master].[dbo].[Player] as p WITH (NOLOCK) on sub.PlayerID = p.PlayerID";
            }

            var query = QueryByDapper<QueryPlayerGameDataStatisticModel>(sql, filter);
            return query;
        }

        public PagedList<PlayerWinLossWinRateReportModel> GetPlayerWinLossRankingReport(QueryPlayerGameSummaryFilterByVendorConfigApiRequestParameterModel filter)
        {
            filter.RuleFor(x => x.Begin).Rule_NotNullDateTime();
            filter.RuleFor(x => x.End).Rule_NotNullDateTime();

            if (string.IsNullOrWhiteSpace(filter.OrderByColumn))
                filter.OrderByColumn = "my_query.WinLossAmount";
            else
                filter.OrderByColumn = "my_query." + filter.OrderByColumn;

            string groupBy = filter.IsGroupAllGameOne ? " PlayerID " : " PlayerID , GameID ";
            string gameIdQuery = filter.IsGroupAllGameOne ? " 0 as GameID " : " GameID ";

            string extraWhere = "";


            if (filter.PlayerID.HasValue)
                extraWhere += " AND PlayerID = @PlayerID ";

            if (filter.MerchantID.HasValue)
            {
                extraWhere += " AND MerchantID = @MerchantID ";
            }
            else
            {
                extraWhere += @" AND MerchantID in
		                            (
			                            SELECT MerchantID FROM [PGS_Master].[ThirdParty].[VendorConfigMerchantMapping] with (NOLOCK)
			                            WHERE VendorConfigID = @VendorConfigID
		                            )  ";
            }

            //如果有篩選gameID則優先用gameID過濾
            if (filter.GameID.HasValue)
            {
                extraWhere += " AND GameID = @GameID ";
            }
            else if (filter.GameVendorID.HasValue)
            {
                extraWhere += " AND GameID in ( SELECT GameID FROM [PGS_Master].[dbo].[GameList] WITH (NOLOCK) where GameVendorType = @GameVendorID ) ";
            }

            string sql = $@"SELECT my_query.* FROM
                            (
	                            SELECT TOP 5000  p.MerchantID , sub.GameID , p.PlayerID , p.PlayerName ,sub.BetAmount , sub.PayoutAmount, sub.WinCount , (cast(sub.WinCount as decimal(18,4)) / cast(sub.TotalRound as decimal(18,4))) * 100  as WinRate , sub.TotalRound , sub.WinLossAmount , sub.RTP  FROM 
	                            (
		                            SELECT PlayerID , {gameIdQuery} , sum(WinCount) as WinCount , sum(BetAmount) as BetAmount, sum(PayoutAmount) as PayoutAmount,
                                           sum([WinCount] + [LoseCount] + [DrawCount]) as TotalRound , sum(WinAmount - LoseAmount) as WinLossAmount , sum(payoutAmount) / NULLIF(sum(BetAmount) ,0) * 100 as RTP
		                            FROM [PGS_StatReport].[dbo].[PlayerGameData] with (NOLOCK)
		                            WHERE StatTime >= @Begin AND StatTime < @End 
                                    {extraWhere}
		                            GROUP BY {groupBy}
	                            ) as sub
	                            INNER JOIN [PGS_Master].[dbo].[Player] as p WITH(NOLOCK) on sub.PlayerID = p.PlayerID
                            ) as my_query";

            var query = GetPagingEntitiesBySQLScript<PlayerWinLossWinRateReportModel>(sql, filter , filter);
            return query;
        }


        public List<PlayerGameDataEntity> GetByFilter(QueryPlayerGameDataRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<PlayerGameDataEntity>(sql, filter);
            return query;
        }


        public PagedList<PlayerGameDataEntity> GetPagingByFilter(QueryPlayerGameDataRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<PlayerGameDataEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryPlayerGameDataRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            return sql;
        }

    }


}
