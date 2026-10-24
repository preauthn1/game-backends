using Dapper;
using game.Entity;
using target.Core.Repository;
using target.Models;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace target.DA.Repository
{

    public class MerchantGameSummaryDataRepository : FormatIntegerPrimaryKeyRepository<MerchantGameSummaryDataEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.StatReportDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[MerchantGameSummaryData]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "StatTime"; }
        }

        public PagedList<QueryAgentDailyGameReportModel> GetAgentDailyGameReportPagedList(QueryAgentDailyGameSummaryReportRequestParameterModel filter)
        {
            filter.RuleFor(x => x.PageIndex).Rule_NotZeroNotMinus();
            filter.RuleFor(x => x.PageSize).Rule_NotZeroNotMinus();
            filter.RuleFor(x => x.MerchantID).Rule_IsNotNullNotZeroNotMinus();

            string extraWhere = "";

            if(filter.GameIDs.IsNotEmptyNull())
            {
                extraWhere = " AND GameID in @GameIDs ";
            }

            string sql = $@"
SELECT final_query.* FROM
(
	SELECT sub_q.* , join_player.totalPlayers from 
	(
		SELECT CAST(DATEADD(hour , @timezone , statTime) as date) StatDate  , GameID ,  MerchantID , sum(TotalRound) as TotalRound ,sum(BetAmount) as BetAmount, sum(PayoutAmount) as PayoutAmount , sum(WinLossAmount) as WinLossAmount
		FROM [PGS_StatReport].[dbo].[MerchantGameSummaryData] WITH (NOLOCK) 
		WHERE StatTime >= @Begin AND StatTime < @End {extraWhere} AND MerchantID in
		(
			SELECT ChildMerchantID 
			FROM [PGS_Master].[dbo].[MerchantHierarchyChildMapping] WITH (NOLOCK) 
			WHERE MerchantID = @HierarchyMerchantID
		)
		GROUP BY MerchantID , GameID , CAST(DATEADD(hour , @timezone , statTime) as date)
	) as sub_q
	LEFT JOIN 
	(
		SELECT CAST(DATEADD(hour , @timezone , statTime) as date) StatDate  , GameID  , MerchantID , count(distinct PlayerID) as totalPlayers
		FROM [PGS_StatReport].[dbo].[PlayerGameData] WITH (NOLOCK) 
		WHERE StatTime >= @Begin AND StatTime < @End {extraWhere} AND MerchantID in
		(
			SELECT ChildMerchantID 
			FROM [PGS_Master].[dbo].[MerchantHierarchyChildMapping] WITH (NOLOCK) 
			WHERE MerchantID = @HierarchyMerchantID
		)
		GROUP BY MerchantID , GameID , CAST(DATEADD(hour , @timezone , statTime) as date)
	) as join_player on sub_q.MerchantID = join_player.MerchantID AND sub_q.GameID = join_player.GameID AND sub_q.StatDate = join_player.StatDate
) as final_query";

            filter.OrderByColumn = " MerchantID , StatDate , GameID   ";

            var param = new
            {
                HierarchyMerchantID = filter.MerchantID,
                Timezone = filter.TimeZone,
                Begin = filter.Begin.Value,
                End = filter.End.Value,
                GameIDs = filter.GameIDs,
            };

            var query = GetPagingEntitiesBySQLScript<QueryAgentDailyGameReportModel>(sql, filter, param);
            return query;
        }

        public List<EachCurrencyStatReportTotalSumQueryModel> GetEachCurrencyTotalSumReportByVendorConfigID(int vendorConfigID, DateTime beginUTC, DateTime endUTC)
        {
            string sql = @"
                SELECT query.Currency , SUM(query.TotalRound) as TotalRound,  SUM(query.BetAmount) as BetAmount , SUM(query.WinLossAmount) as WinLossAmount FROM 
                (
	                SELECT stat_DB.* , m.Name as MerchantName , m.Currency FROM
	                (
		                SELECT MerchantID , sum(TotalRound) as TotalRound , sum(BetAmount) as BetAmount, sum(WinLossAmount) as WinLossAmount
		                FROM [PGS_StatReport].[dbo].[MerchantGameSummaryData] WITH (NOLOCK)
		                WHERE StatTime >= @begin AND StatTime < @end  AND (GameID / 10000 * 10000) in
		                (
			                SELECT VendorID FROM [PGS_Master].[ThirdParty].[VendorConfig]
			                WHERE VendorConfigID = @vendorConfigID
		                )
		                GROUP BY MerchantID 
	                ) as stat_DB
	                INNER JOIN [PGS_Master].[dbo].[Merchant] as m on stat_DB.MerchantID = m.MerchantID
	                LEFT JOIN [PGS_Master].[dbo].[MerchantHierarchy] as mh on m.MerchantID = mh.MerchantID
	                WHERE m.MerchantID in
	                (
		                SELECT MerchantID FROM [PGS_Master].[ThirdParty].[VendorConfigMerchantMapping] WIT (NOLOCK)
		                WHERE VendorConfigID = @vendorConfigID
	                )
                ) as query
                GROUP BY query.Currency
                order by sum(query.TotalRound) desc ";

            var param = new
            {
                begin = beginUTC,
                end = endUTC,
                vendorConfigID = vendorConfigID
            };

            var query = QueryByDapper<EachCurrencyStatReportTotalSumQueryModel>(sql, param);
            return query.ToList();
        }

        public List<EachMerchantDailyStatReportQueryModel> GetEachMerchantTotalSumReportForFinanceOverview(DateTime beginUTC, DateTime endUTC)
        {
            string sql = @"SELECT stat_DB.* , m.Name as MerchantName , m.CreateTime as MerchantCreateTime , m.Currency , mh.RootMerchantID FROM
                            (
	                            SELECT MerchantID , sum(TotalRound) as TotalRound , sum(BetAmount) as BetAmount, sum(WinLossAmount) as WinLossAmount
	                            FROM [PGS_StatReport].[dbo].[MerchantGameSummaryData] WITH (NOLOCK)
	                            WHERE StatTime >= @begin AND StatTime < @end 
	                            GROUP BY MerchantID 
                            ) as stat_DB
                            INNER JOIN [PGS_Master].[dbo].[Merchant] as m on stat_DB.MerchantID = m.MerchantID
                            LEFT JOIN [PGS_Master].[dbo].[MerchantHierarchy] as mh on m.MerchantID = mh.MerchantID
                            WHERE m.Type = @merchantType ";

            var param = new
            {
                begin = beginUTC,
                end = endUTC,
                merchantType = MerchantTypeEnum.Plaform_ProductionEnvironment.ToInteger()
            };

            var query = QueryByDapper<EachMerchantDailyStatReportQueryModel>(sql, param);
            return query.ToList();
        }


        public List<EachMerchantDailyStatReportQueryModel> GetEachMerchantDailyReportForFinanceOverview(DateTime beginUTC, DateTime endUTC)
        {
            string sql = $@"SELECT stat_DB.* , m.Currency , mh.RootMerchantID FROM
                            (
	                            SELECT MerchantID , CAST(DATEADD(hour , 8 , StatTime) as date) as Date, sum(TotalRound) as TotalRound , sum(BetAmount) as BetAmount, sum(WinLossAmount) as WinLossAmount
	                            FROM [PGS_StatReport].[dbo].[MerchantGameSummaryData] WITH (NOLOCK)
	                            WHERE StatTime >= @begin AND StatTime < @end 
	                            GROUP BY MerchantID , CAST(DATEADD(hour , 8 , StatTime) as date)
                            ) as stat_DB
                            INNER JOIN [PGS_Master].[dbo].[Merchant] as m on stat_DB.MerchantID = m.MerchantID
                            LEFT JOIN [PGS_Master].[dbo].[MerchantHierarchy] as mh on m.MerchantID = mh.MerchantID
                            WHERE m.Type = @MerchantType ";

            var param = new
            {
                begin = beginUTC,
                end = endUTC,
                MerchantType = MerchantTypeEnum.Plaform_ProductionEnvironment.ToInteger()
            };

            var query = QueryByDapper<EachMerchantDailyStatReportQueryModel>(sql, param);
            return query.ToList();
        }


        /// <summary>
        /// 查詢每個VendorConfig(遊戲商服務器)的遊戲報表 (全日期加總、不分日期)
        /// </summary>
        public List<EachVendorConfigGameStatisticReportModel> GetEachVendorConfigGameDailyReportGroupByGame(DateTime beginUTC, DateTime endUTC, bool isExcludeJackpotMerchant = true)
        {
            string extraWhere = string.Empty;

            //排除拉JP商戶
            if (isExcludeJackpotMerchant)
            {
                extraWhere = $" AND stat.MerchantID not in ( SELECT MerchantID FROM [PGS_Master].[dbo].[Merchant] WITH (NOLOCK) WHERE Type = {MerchantTypeEnum.JackpotMerchant.ToInteger()} ) ";
            }

            string sql = $@" select vendor.VendorConfigID , stat.GameID , sum(stat.TotalRound) as TotalRound, sum(stat.BetAmount) as BetAmount , sum(stat.PayoutAmount)  as PayoutAmount
                            from
                            (
	                            select stat.MerchantID, stat.GameID , sum([TotalRound]) as [TotalRound] , sum(BetAmount) as BetAmount , sum(PayoutAmount) as PayoutAmount
	                            from [PGS_StatReport].[dbo].[MerchantGameSummaryData] as stat WITH (NOLOCK)
	                            where stat.StatTime >= @begin AND stat.StatTime < @end {extraWhere}
	                            group by stat.MerchantID , stat.GameID
                            ) as stat
                            left join [PGS_Master].[dbo].[GameList] as g WITH (NOLOCK) on stat.GameID = g.GameID
                            left join [PGS_Master].[ThirdParty].[VendorConfigMerchantMapping] as vendor WITH (NOLOCK) on g.GameVendorType = vendor.VendorID AND stat.MerchantID = vendor.MerchantID
                            group by vendor.VendorConfigID , stat.GameID";

            var param = new
            {
                begin = beginUTC,
                end = endUTC,
            };

            var query = QueryByDapper<EachVendorConfigGameStatisticReportModel>(sql, param);
            return query.ToList();
        }


        public List<VendorConfigReportSummaryGroupByMerchantGameModel> GetVendorConfigStatReportGroupByTargetCondition(int vendorConfigID_or_ServerID, DateTime beginUTC, DateTime endUTC, TableColumnEnum groupBy)
        {
            string groupByColumn = groupBy == TableColumnEnum.GameID ? "GameID" : "MerchantID";

            string whereQuery = @"AND MerchantID in
	                            (	
		                            SELECT MerchantID FROM [PGS_Master].[ThirdParty].[VendorConfigMerchantMapping] WITH (NOLOCK)
		                            WHERE VendorConfigID = @vendorConfigID
	                            )
                                AND GameID in 
                                (
                                    SELECT GameID FROM [PGS_Master].[dbo].[GameList] WITH (NOLOCK)
                                    WHERE GameVendorType in
                                    (
	                                    SELECT VendorID FROM [PGS_Master].[ThirdParty].[VendorConfig] WITH (NOLOCK)
	                                    WHERE VendorConfigID = @vendorConfigID
                                    )
                                )";

            //target serverID的話 查詢條件不一樣
            if (RegexRuleHelper.IsValidServerID(vendorConfigID_or_ServerID))
            {
                whereQuery = @"AND MerchantID in
	                            (	
		                            SELECT MerchantID FROM [PGS_Master].[dbo].[ServerMerchantMapping] WITH (NOLOCK)
		                            WHERE ServerID = @vendorConfigID
	                            )
                                AND GameID in 
                                (
                                    SELECT GameID FROM [PGS_Master].[dbo].[ServerList] WITH (NOLOCK)
                                    WHERE ServerID = @vendorConfigID
                                )";
            }

            string sql = $@" SELECT stat.{groupByColumn} , 
                                    sum(stat.TotalRound) as TotalRound, 
                                    sum(stat.BetAmount) as BetAmount , 
                                    sum(stat.PayoutAmount)  as PayoutAmount ,
                                    sum(stat.WinLossAmount)  as WinLossAmount
                            from
                            (
	                            select stat.MerchantID , stat.GameID , 
                                       sum([TotalRound]) as [TotalRound] , 
                                       sum(BetAmount) as BetAmount , 
                                       sum(PayoutAmount) as PayoutAmount , 
                                       sum(WinLossAmount) as WinLossAmount

	                            from [PGS_StatReport].[dbo].[MerchantGameSummaryData] as stat WITH (NOLOCK)
	                            where stat.StatTime >= @begin AND stat.StatTime < @end {whereQuery}
	                            group by stat.MerchantID , stat.GameID
                            ) as stat
                            group by stat.{groupByColumn} ";

            var param = new
            {
                begin = beginUTC,
                end = endUTC,
                vendorConfigID = vendorConfigID_or_ServerID
            };

            var query = QueryByDapper<VendorConfigReportSummaryGroupByMerchantGameModel>(sql, param);
            return query.ToList();
        }



        /// <summary>
        /// 查詢每個VendorConfig(遊戲商服務器)的遊戲報表
        /// </summary>
        public List<MerchantGameSummaryRTPModel> GetEachVendorConfigGameDailyReport(DateTime beginUTC, DateTime endUTC, bool isExcludeJackpotMerchant = true)
        {
            string extraWhere = string.Empty;

            //排除拉JP商戶
            if (isExcludeJackpotMerchant)
            {
                extraWhere = $" AND stat.MerchantID not in ( SELECT MerchantID FROM [PGS_Master].[dbo].[Merchant] WITH (NOLOCK) WHERE Type = {MerchantTypeEnum.JackpotMerchant.ToInteger()} ) ";
            }

            string sql = $@" select vendor.VendorConfigID , stat.Date , stat.GameID , sum(stat.TotalRound) as TotalRound, sum(stat.BetAmount) as BetAmount , sum(stat.PayoutAmount)  as PayoutAmount
                            from
                            (
	                            select stat.MerchantID , cast(DATEADD(hour , 8 , stat.StatTime) as date) as [Date], stat.GameID , sum([TotalRound]) as [TotalRound] , sum(BetAmount) as BetAmount , sum(PayoutAmount) as PayoutAmount
	                            from [PGS_StatReport].[dbo].[MerchantGameSummaryData] as stat WITH (NOLOCK)
	                            where stat.StatTime >= @begin AND stat.StatTime < @end {extraWhere}
	                            group by stat.MerchantID , cast(DATEADD(hour , 8 , stat.StatTime) as date) , stat.GameID
                            ) as stat
                            left join [PGS_Master].[dbo].[GameList] as g WITH (NOLOCK) on stat.GameID = g.GameID
                            left join [PGS_Master].[ThirdParty].[VendorConfigMerchantMapping] as vendor WITH (NOLOCK) on g.GameVendorType = vendor.VendorID AND stat.MerchantID = vendor.MerchantID
                            group by vendor.VendorConfigID ,  stat.Date , stat.GameID";

            var param = new
            {
                begin = beginUTC,
                end = endUTC,
            };

            var query = QueryByDapper<MerchantGameSummaryRTPModel>(sql, param);
            return query.ToList();
        }

        /// <summary>
        /// 遊戲商對帳用
        /// </summary>
        public BetRecordSummaryReportModel GetSummaryForVendorReportDataCheck(List<int> merchantIDs, List<int> gameIDs, DateTime begin, DateTime end)
        {
            string extraWhere = merchantIDs.IsEmptyOrNull() ? "" : " AND MerchantID in @merchantIDs";
            string extraWhereGameID = gameIDs.IsEmptyOrNull() ? "" : " AND GameID in @gameIDs";
            string sql = $@" SELECT SUM(BetAmount) as TotalBetAmount ,
                                    SUM(WinLossAmount) as TotalWinLoss  ,
                                    SUM(ValidAmount) as TotalValidBetAmount  ,
                                    SUM(PayoutAmount) as TotalPayoutAmount 
                            FROM [dbo].[MerchantGameSummaryData] WITH (NOLOCK)
                            WHERE StatTime >= @begin AND StatTime < @end {extraWhere} {extraWhereGameID} ";

            var filter = new
            {
                gameIDs = gameIDs,
                merchantIDs = merchantIDs,
                begin = begin,
                end = end
            };

            var query = QueryByDapper<BetRecordSummaryReportModel>(sql, filter);
            var result = query.FirstOrDefault();
            result.AvailableColumns = new List<GameVendorReportApiAvailableColumnsEnum>
            {
                GameVendorReportApiAvailableColumnsEnum.TotalPayoutAmount,
                GameVendorReportApiAvailableColumnsEnum.TotalValidBetAmount,
                GameVendorReportApiAvailableColumnsEnum.TotalBetAmount,
                GameVendorReportApiAvailableColumnsEnum.TotalWinLoss
            };

            return result;
        }

        public List<MerchantGameSummaryDataEntity> GetDailySumOnDuration(
           DateTime beginUTC,
           DateTime endUTC,
           double timeZone , 
           string extraWhere = " AND MerchantID in ( SELECT MerchantID FROM [PGS_Master].[dbo].[Merchant] WITH(NOLOCK) WHERE Type = 20) ")
        {
            string selectAmountColumns = @"                                
                                SUM(TotalRound) as TotalRound, 
                                SUM(TotalRound) as TotalRound, 
                                SUM([BetPlayer]) as [BetPlayer] , 
                                SUM(WinAmount) as WinAmount , 
                                SUM(LoseAmount) as LoseAmount , 
                                SUM(TaxAmount) as TaxAmount ,
                                SUM(FlowAmount) as FlowAmount , 
                                SUM(JackpotAmount) as JackpotAmount ,
                                SUM(BetAmount) as BetAmount ,
                                SUM(PayoutAmount) as PayoutAmount  ,
                                SUM(ValidAmount) as ValidAmount  ,
                                SUM(WinLossAmount) as WinLossAmount  ";

            string sql = $@" SELECT
                                CAST(DATEADD(hour , {timeZone.ToString()} , StatTime) as date) as StatTime,
                                {selectAmountColumns} 

                            FROM [dbo].[MerchantGameSummaryData] WITH (NOLOCK)
                            WHERE StatTime >= @begin AND StatTime < @end {extraWhere} 
                            GROUP BY CAST( DATEADD(hour , {timeZone.ToString()} , StatTime) as date) ";

            var filter = new
            {
                begin = beginUTC,
                end = endUTC,
            };

            var query = QueryByDapper<MerchantGameSummaryDataEntity>(sql, filter, timeoutSeconds: 60);
            return query;
        }



        public List<MerchantGameSummaryDataEntity> GetEachGameEachMerchantDailySumOnDuration(
            List<int> merchantIDs,
            List<int> gameIDs,
            DateTime begin,
            DateTime end,
            double timeZone,
            string selectAmountColumns = "")
        {
            bool isFilterMerchantIDs = merchantIDs.IsEmptyOrNull() || merchantIDs.Count() >= 500 ? false : true;
            string extraWhereGameID = gameIDs.IsEmptyOrNull() ? "" : " AND GameID in @gameIDs";
            string extraWhere = isFilterMerchantIDs ? " AND MerchantID in @merchantIDs" : "";

            if (string.IsNullOrWhiteSpace(selectAmountColumns))
            {
                selectAmountColumns = @"                                
                                SUM(TotalRound) as TotalRound, 
                                SUM(TotalRound) as TotalRound, 
                                SUM([BetPlayer]) as [BetPlayer] , 
                                SUM(WinAmount) as WinAmount , 
                                SUM(LoseAmount) as LoseAmount , 
                                SUM(TaxAmount) as TaxAmount ,
                                SUM(FlowAmount) as FlowAmount , 
                                SUM(JackpotAmount) as JackpotAmount ,
                                SUM(BetAmount) as BetAmount ,
                                SUM(PayoutAmount) as PayoutAmount  ,
                                SUM(ValidAmount) as ValidAmount  ,
                                SUM(WinLossAmount) as WinLossAmount  ";
            }

            string sql = $@" SELECT
                                CAST(DATEADD(hour , {timeZone.ToString()} , StatTime) as date) as StatTime,
                                MerchantID , 
                                GameID , 
                                {selectAmountColumns} 

                            FROM [dbo].[MerchantGameSummaryData] WITH (NOLOCK)
                            WHERE StatTime >= @begin AND StatTime < @end {extraWhere} {extraWhereGameID} 
                            GROUP BY CAST( DATEADD(hour , {timeZone.ToString()} , StatTime) as date) , MerchantID , GameID";

            var filter = new
            {
                gameIDs = gameIDs,
                merchantIDs = isFilterMerchantIDs ? merchantIDs : null,
                begin = begin,
                end = end,
            };

            var query = QueryByDapper<MerchantGameSummaryDataEntity>(sql, filter, timeoutSeconds: 60);

            if (merchantIDs.IsNotEmptyNull() && merchantIDs.Count() >= 500)
            {
                query = query
                    .Where(x => merchantIDs.Contains(x.MerchantID))
                    .ToList();
            }

            return query;
        }


        public List<MerchantGameSummaryDataEntity> GetEachGameTotalSumOnDuration(List<int> merchantIDs, List<int> gameIDs, DateTime begin, DateTime end)
        {
            string extraWhere = merchantIDs.IsEmptyOrNull() ? "" : " AND MerchantID in @merchantIDs";
            string extraWhereGameID = gameIDs.IsEmptyOrNull() ? "" : " AND GameID in @gameIDs";
            string sql = $@" SELECT MerchantID , GameID , 

                                SUM(TotalRound) as TotalRound, 
                                SUM([BetPlayer]) as [BetPlayer] , 
                                SUM(WinAmount) as WinAmount , 
                                SUM(LoseAmount) as LoseAmount , 
                                SUM(TaxAmount) as TaxAmount ,
                                SUM(FlowAmount) as FlowAmount , 
                                SUM(JackpotAmount) as JackpotAmount ,
                                SUM(BetAmount) as BetAmount ,
                                SUM(PayoutAmount) as PayoutAmount  ,
                                SUM(ValidAmount) as ValidAmount  ,
                                SUM(WinLossAmount) as WinLossAmount  

                            FROM [dbo].[MerchantGameSummaryData] WITH (NOLOCK)
                            WHERE StatTime >= @begin AND StatTime < @end {extraWhere} {extraWhereGameID} 
                            GROUP BY MerchantID , GameID";

            var filter = new
            {
                gameIDs = gameIDs,
                merchantIDs = merchantIDs,
                begin = begin,
                end = end
            };

            var query = QueryByDapper<MerchantGameSummaryDataEntity>(sql, filter);
            return query;
        }

        public List<MerchantGameSummaryDataEntity> GetEachMerchantTotalSumOnDuration(List<int> merchantIDs, DateTime begin, DateTime end, List<int> gameIDs = null)
        {
            string filterGamesWhere = string.Empty;
            string filterMerchantWhere = string.Empty;

            var config = new DbQueryConfig
            {
                ConnectTimeoutSeconds = null,
            };

            if (gameIDs.IsNotEmptyNull())
            {
                filterGamesWhere = " AND GameID in @gameIDs ";
            }

            if (merchantIDs.IsNotEmptyNull())
            {
                GenerateWhereSQLResult largeQueryColumns = GenerateInQuery_Or_LargeIDsQuery(" @merchantIDs ", merchantIDs);
                filterMerchantWhere = $" AND MerchantID in {largeQueryColumns.WhereSQL} ";
                config.TempIDsAppendSQL = largeQueryColumns.TempIDsAppendSQL;
            }

            string sql = $@" SELECT MerchantID , 

                                SUM(TotalRound) as TotalRound, 
                                SUM([BetPlayer]) as [BetPlayer] , 
                                SUM(WinAmount) as WinAmount , 
                                SUM(LoseAmount) as LoseAmount , 
                                SUM(TaxAmount) as TaxAmount ,
                                SUM(FlowAmount) as FlowAmount , 
                                SUM(JackpotAmount) as JackpotAmount ,
                                SUM(BetAmount) as BetAmount ,
                                SUM(PayoutAmount) as PayoutAmount  ,
                                SUM(ValidAmount) as ValidAmount  ,
                                SUM(WinLossAmount) as WinLossAmount  

                            FROM [dbo].[MerchantGameSummaryData] WITH (NOLOCK)
                            WHERE StatTime >= @begin AND StatTime < @end {filterMerchantWhere} {filterGamesWhere}
                            GROUP BY MerchantID";

            var filter = new
            {
                merchantIDs = merchantIDs,
                begin = begin,
                end = end,
                gameIDs = gameIDs
            };

            var query = QueryByDapper<MerchantGameSummaryDataEntity>(sql, filter , config);
            return query;
        }

        public List<MerchantGameSummaryDataEntity> GetEachMerchantTotalSumOnDurationByHierarchyMerchant(int hierarchyMerchantID, DateTime begin, DateTime end)
        {
            string hierarchySQL = AppHelper.GenerateMerchantHierarchyWhereSQL(hierarchyMerchantID);
            string sql = $@" SELECT MerchantID , 

                                SUM(TotalRound) as TotalRound, 
                                SUM([BetPlayer]) as [BetPlayer] , 
                                SUM(WinAmount) as WinAmount , 
                                SUM(LoseAmount) as LoseAmount , 
                                SUM(TaxAmount) as TaxAmount ,
                                SUM(FlowAmount) as FlowAmount , 
                                SUM(JackpotAmount) as JackpotAmount ,
                                SUM(BetAmount) as BetAmount ,
                                SUM(PayoutAmount) as PayoutAmount  ,
                                SUM(ValidAmount) as ValidAmount  ,
                                SUM(WinLossAmount) as WinLossAmount  

                            FROM [dbo].[MerchantGameSummaryData] WITH (NOLOCK)
                            WHERE StatTime >= @begin AND StatTime < @end {hierarchySQL}
                            GROUP BY MerchantID";

            var filter = new
            {
                begin = begin,
                end = end
            };

            var query = QueryByDapper<MerchantGameSummaryDataEntity>(sql, filter);
            return query;
        }

        public List<QueryDirectAgentSummaryReportByHierarchyChildDataModel> GetDirectAgentSummaryReportByHierarchyChildJoin(QueryDirectAgentSummaryReportByHierarchyChildRequestModel filter)
        {
            string extraWhere = "  mapping.MerchantID = @TargetMerchantID ";

            string filterGameWhere = string.Empty;

            //請參考該欄位的註解說明 (說明很複雜)
            if (filter.IsQueryDirectAgentDetailOnly)
            {
                extraWhere = @" mapping.MerchantID in 
		                            (	
			                            SELECT MerchantID from [PGS_Master].[dbo].[MerchantHierarchy] WITH (NOLOCK)
			                            where ParentMerchantID = @targetMerchantID
		                            )";

            }

            if (filter.SpecificLayerOnly.HasValue)
            {
                int filterLength = 7 * filter.SpecificLayerOnly.Value;  //14 = 二級  21 = 三級
                string hierarchyKeyword = $"{filter.TargetMerchantID}/";

                extraWhere = $@" mapping.MerchantID in 
		                            (	
			                            SELECT MerchantID from [PGS_Master].[dbo].[MerchantHierarchy] WITH (NOLOCK)
			                            where LEN(Hierarchy) = {filterLength} AND Hierarchy like '%{hierarchyKeyword}%'
		                            )";
            }

            if (filter.GameIDs.IsNotEmptyNull())
            {
                filterGameWhere = " AND GameID in @GameIDs ";
            }

            string sql = $@"SELECT 
                                join_summary.OwnerMerchantID as MerchantID,
                                sum(join_summary.TotalRound) as TotalRound,
                                sum(join_summary.BetAmount) as BetAmount,
                                sum(join_summary.ValidAmount) as ValidAmount,
                                sum(join_summary.PayoutAmount) as PayoutAmount,
                                sum(join_summary.WinLossAmount) as WinLossAmount,
                                sum(join_summary.EventCostAmount) as EventCostAmount

                            FROM 
                            (
	                            SELECT 

		                            mapping.MerchantID as OwnerMerchantID , 
		                            mapping.ChildMerchantID as ChildMerchantID , 
		                            ISNULL(join_data.TotalRound , 0) as TotalRound,
		                            ISNULL(join_data.BetAmount , 0) as BetAmount,
		                            ISNULL(join_data.ValidAmount , 0) as ValidAmount,
		                            ISNULL(join_data.PayoutAmount , 0) as PayoutAmount,
		                            ISNULL(join_data.WinLossAmount , 0) as WinLossAmount,
		                            ISNULL(join_event_data.EventCostAmount , 0) as EventCostAmount

	                            FROM [PGS_Master].[dbo].[MerchantHierarchyChildMapping] as mapping WITH (NOLOCK)
	                            left join
	                            (
		                            SELECT 

                                        summary.MerchantID as MerchantID , 		
		                                sum(summary.TotalRound ) as TotalRound,
		                                sum(summary.BetAmount ) as BetAmount,
		                                sum(summary.ValidAmount) as ValidAmount,
		                                sum(summary.PayoutAmount) as PayoutAmount,
		                                sum(summary.WinLossAmount) as WinLossAmount

		                            FROM [PGS_StatReport].[dbo].[MerchantGameSummaryData] as summary with (NOLOCK)
		                            where StatTime >= @begin AND StatTime < @end {filterGameWhere}
		                            group by summary.MerchantID

	                            ) as join_data on mapping.ChildMerchantID = join_data.MerchantID


                                left join
	                            (
		                            SELECT 

                                        event_summary.MerchantID as MerchantID , 		
		                                sum(event_summary.TotalAmount) as EventCostAmount

		                            FROM [PGS_StatReport].[dbo].[MerchantEventSummaryData] as event_summary with (NOLOCK)
		                            where StatTime >=  @begin AND StatTime < @end 
		                            group by event_summary.MerchantID

	                            ) as join_event_data on mapping.ChildMerchantID = join_event_data.MerchantID

	                            where {extraWhere} 	
                            ) as join_summary
                            group by join_summary.OwnerMerchantID
                            order by sum(join_summary.BetAmount) desc";

            var query = QueryByDapper<QueryDirectAgentSummaryReportByHierarchyChildDataModel>(sql, filter);
            return query;
        }

        public List<AgentHierarchyGameSummaryReportDataModel> GetAgentHierarchyGameSummaryReport(QueryAgentHierarchyGameSummaryReportRequestModel filter)
        {
            string extraWhere = "  mapping.MerchantID = @TargetMerchantID ";

            string extraWhereGameIDs = " ";

            //請參考該欄位的註解說明
            if (filter.IsQueryStraightCustomerOnly)
            {
                extraWhere = @" mapping.MerchantID = @TargetMerchantID AND mapping.ChildMerchantID = @TargetMerchantID  ";
            }

            if (filter.GameIDs.IsNotEmptyNull())
            {
                extraWhereGameIDs = " AND GameID in @GameIDs ";
            }

            string sql = $@"SELECT 
	                            join_summary.GameID as GameID,
                                join_summary.OwnerMerchantID as MerchantID,
                                sum(join_summary.TotalRound) as TotalRound,
                                sum(join_summary.BetAmount) as BetAmount,
                                sum(join_summary.ValidAmount) as ValidAmount,
                                sum(join_summary.PayoutAmount) as PayoutAmount,
                                sum(join_summary.WinLossAmount) as WinLossAmount
                            FROM 
                            (
	                            SELECT 	
		                            join_data.GameID as GameID , 
		                            mapping.MerchantID as OwnerMerchantID , 
		                            mapping.ChildMerchantID as ChildMerchantID , 
		                            ISNULL(join_data.TotalRound , 0) as TotalRound,
		                            ISNULL(join_data.BetAmount , 0) as BetAmount,
		                            ISNULL(join_data.ValidAmount , 0) as ValidAmount,
		                            ISNULL(join_data.PayoutAmount , 0) as PayoutAmount,
		                            ISNULL(join_data.WinLossAmount , 0) as WinLossAmount

	                            FROM [PGS_Master].[dbo].[MerchantHierarchyChildMapping] as mapping WITH (NOLOCK)
	                            left join
	                            (
		                            SELECT 

			                            summary.GameID as GameID,
                                        summary.MerchantID as MerchantID , 		
		                                sum(summary.TotalRound ) as TotalRound,
		                                sum(summary.BetAmount ) as BetAmount,
		                                sum(summary.ValidAmount) as ValidAmount,
		                                sum(summary.PayoutAmount) as PayoutAmount,
		                                sum(summary.WinLossAmount) as WinLossAmount

		                            FROM [PGS_StatReport].[dbo].[MerchantGameSummaryData] as summary with (NOLOCK)
		                            where StatTime >= @Begin AND StatTime < @End {extraWhereGameIDs}
		                            group by summary.GameID , summary.MerchantID

	                            ) as join_data on mapping.ChildMerchantID = join_data.MerchantID

	                            where {extraWhere} AND join_data.GameID is not null

                            ) as join_summary
                            group by join_summary.GameID , join_summary.OwnerMerchantID
                            order by sum(join_summary.BetAmount) desc";

            var query = QueryByDapper<AgentHierarchyGameSummaryReportDataModel>(sql, filter);
            return query;
        }

        /// <summary>
        /// 列出該代理樹狀底下的每個代理數字
        /// </summary>
        public List<AgentHierarchySummaryReportV3Model> ListTargetHierarchyAgentGameSummaryAndSort(int hierarchyMerchantID , DateTime begin , DateTime end , List<int> gameIDs , string orderByColumn = "")
        {
            string extraWhere = "   ";

            if(gameIDs.IsNotEmptyNull())
            {
                extraWhere = " AND GameID in @gameIDs ";
            }

            string sql = $@"
SELECT * FROM
(
	SELECT MerchantID ,  sum(TotalRound) as TotalRound ,  sum(BetAmount) as BetAmount , sum(PayoutAmount) as PayoutAmount, sum(WinLossAmount) as WinLossAmount 
	FROM [PGS_StatReport].[dbo].[MerchantGameSummaryData] WITH (NOLOCK)
	WHERE StatTime >= @begin AND StatTime < @end {extraWhere} AND MerchantID in
	(
		SELECT ChildMerchantID from [PGS_Master].[dbo].[MerchantHierarchyChildMapping] WITH (NOLOCK)
		WHERE MerchantID = @hierarchyMerchantID
	)
	GROUP BY MerchantID
) as sub
ORDER BY {orderByColumn} ";

            var param = new
            {
                begin = begin ,
                end = end,
                hierarchyMerchantID = hierarchyMerchantID,
                gameIDs = gameIDs,
            };

            var query = QueryByDapper<AgentHierarchySummaryReportV3Model>(sql, param);
            return query;
        }



        public List<MerchantGameSummaryDataEntity> GetByFilter(QueryMerchantGameSummaryDataRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<MerchantGameSummaryDataEntity>(sql, filter);
            return query;
        }


        public PagedList<MerchantGameSummaryDataEntity> GetPagingByFilter(QueryMerchantGameSummaryDataRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<MerchantGameSummaryDataEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryMerchantGameSummaryDataRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in @GameIDs ";

            if (filter.MerchantIDs.IsNotEmptyNull())
                sql += " AND MerchantID in @MerchantIDs ";

            if (filter.StatTimeBegin.HasValue)
                sql += " AND StatTime >= @StatTimeBegin ";

            if (filter.StatTimeEnd.HasValue)
                sql += " AND StatTime < @StatTimeEnd ";

            return sql;
        }

    }


}
