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
using Lck.Utility.Extensions.Helper;
using target.Utility;
using target.Models.Models.StoreProcedures;
using System.Data.SqlClient;
using System.Data;
using target.Models.Enums;
using target.Models.Models;
using target.Core.Helper;
using target.Models;

namespace target.DA.Repository
{

    /// <summary>
    /// 這個DB 很多都是透過DBA寫SP溝通的。
    /// </summary>
    public class StatReportStoreProcedureRepository
    {
        protected string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.StatReportDB); }
        }


        /// <summary>
        /// 三個數據來源 比對結果
        /// </summary>
        public SqlQueryCommandTempModel<Stat_GameVendorStatSummaryThreeSourceCompareModel> GetGameVendorStatSummary_ComapreThreeSource(QueryGameVendorStatSummaryThreeSourceCompareModel filter)
        {
            string joinSource = "join_daily";
            string joinDailySQL = @"
                            left join 
                            (
	                            select StatDate , 
                                    sum(TotalRound) as TotalRound, 
                                    sum(BetAmount) as BetAmount, 
                                    sum(ValidAmount) as ValidAmount, 
                                    sum(WinLossAmount) as WinLossAmount
	                            from [PGS_StatReport].[dbo].[GameVendorDailyGameSummary] with (NOLOCK)
	                            where VendorConfigID = @VendorConfigID AND MerchantID not in @ExcludeMerchantIDs
	                            group by StatDate
                            ) as join_daily on ms.StatDate = join_daily.StatDate";

            //可以指定不join這張資料表，輕量化查詢
            if(filter.IsSkipJoinDailyTable)
            {
                joinSource = "ms";
                joinDailySQL = "";
            }

            string sql = $@"SELECT 
                                ms.StatDate ,
                                ms.TotalRound as Stat_TotalRound,
                                ms.BetAmount as Stat_BetAmount,
                                ms.WinLossAmount as Stat_WinLossAmount,
                                ms.ValidAmount as Stat_ValidAmount,
                                ms.MerchantCount as MerchantCount,

                                {joinSource}.TotalRound as ThirdSettleDate_TotalRound ,
                                {joinSource}.BetAmount as ThirdSettleDate_BetAmount ,
                                {joinSource}.WinLossAmount as ThirdSettleDate_WinLossAmount ,

                                join_thirdFetch.TotalRound as ThirdFetch_TotalRound ,
                                join_thirdFetch.BetAmount as ThirdFetch_TotalBetAmount ,
                                join_thirdFetch.WinLossAmount as ThirdFetch_TotalWinLoss 
                            FROM 
                            (
	                            select cast(dateadd(HOUR , {filter.AggregativeTimezone} , 
                                    [StatTime]) as date) as StatDate,  
                                    count(distinct MerchantID) as MerchantCount , 
                                    sum([TotalRound]) as [TotalRound] , 
                                    sum(BetAmount) as BetAmount , 
                                    sum(WinLossAmount) as WinLossAmount,
                                    sum(ValidAmount) as ValidAmount
	                            from [PGS_StatReport].[dbo].[MerchantGameSummaryData] with (NOLOCK)
	                            where MerchantID in ( select MerchantID from [PGS_Master].[ThirdParty].[VendorConfigMerchantMapping] WITH (NOLOCK) where VendorConfigID = @VendorConfigID) 
                                      AND MerchantID not in @ExcludeMerchantIDs AND StatTime >= @begin AND StatTime < @End AND GameID >= @MinGameID AND GameID < @MaxGameID
	                            group by cast(dateadd(HOUR , {filter.AggregativeTimezone} , [StatTime]) as date)
                            ) as ms

                            {joinDailySQL}

                            left join 
                            (	
	                            select StatDate ,  sum(TotalRound) as TotalRound , sum(TotalBetAmount) as BetAmount , sum(TotalWinLoss) as WinLossAmount  
	                            from [PGS_GameLog].[WagerFetch].[GameVendorAccountingSummaryFetch] WITH(NOLOCK)
	                            where VendorConfigID = @VendorConfigID AND StatDate >= DATEADD(DAY , -1 , @begin)
	                            group by StatDate  

                            ) as join_thirdFetch on ms.StatDate = join_thirdFetch.StatDate
                            order by ms.StatDate";

            using (var conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Query<Stat_GameVendorStatSummaryThreeSourceCompareModel>(sql, filter);

                var resultModel = new SqlQueryCommandTempModel<Stat_GameVendorStatSummaryThreeSourceCompareModel>
                {
                    SqlWithParameterValue = SQLHelper.ReplaceSqlParameterToValue(sql , filter),
                    ListResult = result.ToList()
                };

                return resultModel;
            }
        }



        public List<Stat_GameVendorBetSummaryResultModel> GetGameVendorBetSummary(QueryGameVendorBetSummaryApiRequestParameterModel model)
        {
            var sql = @"select sub.* , m.ChargeCurrency , CASE WHEN g.GameVendorType = 2001 then 0 else g.GameVendorType END as GameVendorType from 
	                    (
		                    select MerchantID , GameID , sum(BetAmount) as  BetAmount,  sum(TotalRound) as TotalRound, sum(PayoutAmount) as PayoutAmount, 
                                   sum(ValidAmount) as ValidAmount, sum(TaxAmount) as TaxAmount, sum(WinLossAmount) as WinLossAmount
		                    from [PGS_StatReport].[dbo].[MerchantGameSummaryData] with (NOLOCK)
		                    where StatTime >= @begin AND StatTime < @end
		                    group by MerchantID , GameID
	                    ) as sub
	                    left join [PGS_Master].[dbo].[GameList] as g with (NOLOCK) on sub.GameID = g.GameID
	                    left join [PGS_Master].[dbo].[Merchant] as m with (NOLOCK) on sub.MerchantID = m.MerchantID";


            var para = new
            {
                begin = model.Begin,
                end = model.End
            };

            using (var conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Query<Stat_GameVendorBetSummaryResultModel>(sql, para);
                return result.ToList();
            }
        }


        public List<MerchantDailySummaryResultModel> GetMerchantDailySummary(Stat_MerchantDailySummaryParameterModelV2 filter)
        {
            string extraWhere = filter.GameIDs.IsEmptyOrNull() ? "" : " AND GameID in @GameIDs ";

            //因為是從SP改寫 TotalLoginPlayer 新版的意義其實是有 投注的玩家(以前是有登入的玩家)
            string sql = $@"SELECT summary.* 
                            ,ISNULL(PL.[TotalLoginPlayer],0) AS [TotalLoginPlayer]	
                            ,ISNULL(PL.[TotalLoginCount],0) AS [TotalLoginCount]	
                            FROM 
                            (
	                            SELECT CAST(dateadd(HOUR, {filter.Timezone}, [StatTime])  AS DATE) as [DATE] 
		                            ,MS.[MerchantID]	
		                            ,SUM(MS.TotalRound) AS TotalRound
		                            ,SUM(MS.[TaxAmount]) AS [TaxAmount]
		                            ,SUM(MS.[BetAmount]) AS [BetAmount]
		                            ,SUM(MS.[PayoutAmount]) AS [PayoutAmount]
		                            ,SUM(MS.[ValidAmount]) AS [ValidAmount]
		                            ,SUM(MS.[WinLossAmount]) AS [WinLossAmount]
		                            FROM [PGS_StatReport].[dbo].[MerchantGameSummaryData] AS MS WITH(NOLOCK) 
		                            WHERE MS.[StatTime] >= @beginUTC AND MS.StatTime < @endUTC {extraWhere}
		                            AND MS.[MerchantID] in @MerchantIDs
	                            GROUP BY MS.[MerchantID],  CAST(dateadd(HOUR, {filter.Timezone}, [StatTime]) AS DATE)
                            ) as summary 
                            LEFT JOIN
	                            (
		                            SELECT 
			                            CAST(dateadd(HOUR, {filter.Timezone}, [StatTime]) AS DATE) as [DATE],
                                        [MerchantID]	,
			                            COUNT(DISTINCT PL.[PlayerID]) AS [TotalLoginPlayer],
			                            COUNT(DISTINCT PL.[PlayerID]) AS [TotalLoginCount]

		                            FROM [PGS_StatReport].[dbo].[PlayerGameData] AS PL WITH(NOLOCK) 
		                            WHERE PL.[StatTime] >= @beginUTC AND PL.StatTime < @endUTC {extraWhere}
		                            AND PL.[MerchantID] in @MerchantIDs
		                            GROUP BY PL.[MerchantID], CAST(dateadd(HOUR, {filter.Timezone}, [StatTime]) AS DATE)

                            ) AS PL ON PL.[MerchantID] = summary.[MerchantID] AND PL.[DATE] = summary.[DATE]";

            var splitEachDate = filter.DateStart.SplitToDateListByHourRange(filter.DateEnd, 24);

            var param = new
            {
                MerchantIDs = filter.MerchantIDs,
                GameIDs = filter.GameIDs,
                beginUTC = filter.DateStart.AddHours(-1 * filter.Timezone),
                endUTC = filter.DateEnd.AddHours(-1 * filter.Timezone),
                Timezone = filter.Timezone
            };

            using (var conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Query<MerchantDailySummaryResultModel>(sql, param).ToList();

                var listResult = new List<MerchantDailySummaryResultModel>();

                foreach (var date in splitEachDate)
                {
                    DateTime targetDate = date;

                    var matchData = result.FirstOrDefault(x => x.Date == targetDate);

                    var instance = new MerchantDailySummaryResultModel();
                    
                    if(matchData != null)
                    {
                        instance = matchData.ReturnInjecteObject<MerchantDailySummaryResultModel>();
                    }

                    instance.Date = targetDate;
                    listResult.Add(instance);
                }

                return listResult;
            }

            //舊版寫法 呼叫SP (但是不能篩選gameIDs)
            //var spName = "PrWeb_Stat_MerchantDailySummary";           

            //using (var conn = new SqlConnection(ConnectionString))
            //{
            //    var result = conn.Query<MerchantDailySummaryResultModel>(spName, filter, commandType: CommandType.StoredProcedure);
            //    return result.ToList();
            //}
        }

        public List<MerchantDailySummaryMergeOneResultModel> GetMerchantDailySummaryMergeOne(Stat_MerchantDailySummaryParameterModel filter)
        {
            var spName = "PrWeb_Stat_MultipleMerchantDailySummaryMergeOne";

            using (var conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Query<MerchantDailySummaryMergeOneResultModel>(spName, filter, commandType: CommandType.StoredProcedure);
                return result.ToList();
            }
        }

        public List<MerchantAllPlayerWinLossSPResultModel> GetMerchantAllPlayerWinLoss(Stat_MerchantAllPlayerWinLossParameterModel filter)
        {
            var spName = "PrWeb_Stat_MerchantAllPlayerWinLoss";

            using (var conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Query<MerchantAllPlayerWinLossSPResultModel>(spName, filter, commandType: CommandType.StoredProcedure);
                return result.ToList();
            }
        }


        public List<HomePageSummaryViewModel> GetHomePageSummary(QueryHomePageReportRequestViewModel filter)
        {
            var spName = "PrWeb_Stat_HomePageSummary";
            var para = new
            {
                userID = filter.UserID,
                dateStart = filter.Begin,
                dateEnd = filter.End,
                merchantList = filter.MerchantIDs.IsEmptyOrNull() ? null : string.Join(",", filter.MerchantIDs)
            };

            using (var conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Query<HomePageSummaryViewModel>(spName, para, commandType: CommandType.StoredProcedure);
                return result.ToList();
            }
        }

        /// <summary>
        /// 数据报表(商戶總報表?)
        /// </summary>
        public List<Stat_MerchantSummarySPResultModel> GetMerchantSummaryStatReport(DateTime begin, DateTime end, int merchantID , bool isSearchFullInfo = true)
        {
            var para = new
            {
                dateStart = begin,
                dateEnd = end,
                merchantID = merchantID,
            };

            string connString = ReplaceConnectTimeoutInConnString(ConnectionString, SystemConstant.ReportQuery_SqlTimeout);

            string spName = "PrWeb_Stat_MerchantSummary_QuerySingleMerchant";

            //精簡版搜尋的速度較快 (join的table較輕量)
            if(!isSearchFullInfo)
            {
                spName = "PrWeb_Stat_MerchantSummary_QuerySingleMerchant_BriefColumns";
            }

            using (var conn = new SqlConnection(connString))
            {
                var result = conn.Query<Stat_MerchantSummarySPResultModel>(spName, para, commandType: CommandType.StoredProcedure);
                return result.ToList();
            }
        }



        /// <summary>
        /// 遊戲報表
        /// </summary>
        public List<Stat_GameSummarySPResultModel> GetGameSummaryStatReport(DateTime begin, DateTime end, int merchantID, List<int> gameIDs)
        {
            var para = new
            {
                dateStart = begin,
                dateEnd = end,
                merchantID = merchantID,
                gameList = gameIDs.IsEmptyOrNull() ? null : string.Join(",", gameIDs),
            };

            using (var conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Query<Stat_GameSummarySPResultModel>("PrWeb_Stat_GameSummary_QuerySingleMerchant", para, commandType: CommandType.StoredProcedure);
                return result.ToList();
            }
        }

        /// <summary>
        /// 遊戲商日報表報表 group by merchant
        /// </summary>
        public List<Dictionary<string , object>> GetGameVendorDailySummaryStatReport(DateTime begin, DateTime end, int? merchantID)
        {
            var para = new
            {
                beginTime = begin,
                endTime = end,
                merchantID = merchantID,
            };

            using (var conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Query("PrWeb_EazyQuery_GetGameVendorDailyReport_EachMerchant", para, commandType: CommandType.StoredProcedure)
                     .Cast<IDictionary<string, object>>()
                     .Select(it => it.ToDictionary(it => it.Key, it => it.Value))
                     .ToList();

                return result.ToList();
            }
        }

        /// <summary>
        /// 遊戲商日報表報表 group by 代理型商戶
        /// </summary>
        public List<Dictionary<string, object>> GetGameVendorDailySummaryStatReport_HierarchyMerchants(DateTime begin, DateTime end, int? merchantID)
        {
            var para = new
            {
                beginTime = begin,
                endTime = end,
                merchantID = merchantID,
            };

            using (var conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Query("PrWeb_EazyQuery_GetGameVendorDailyReport_EachRootAgent", para, commandType: CommandType.StoredProcedure)
                     .Cast<IDictionary<string, object>>()
                     .Select(it => it.ToDictionary(it => it.Key, it => it.Value))
                     .ToList();

                return result.ToList();
            }
        }


        /// <summary>
        /// 圖表數據
        /// </summary>
        public List<Stat_ChartDataSPResultModel> GetStatReportChartData(DateTime begin, DateTime end, List<int> merchantIDs, ChartDataTypeEnum chartType)
        {
            var para = new
            {
                dateStart = begin,
                dateEnd = end,
                merchantList = merchantIDs.IsEmptyOrNull() ? null : string.Join(",", merchantIDs),
                chartType
            };

            using (var conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Query<Stat_ChartDataSPResultModel>("PrWeb_Stat_MerchantSummary_Chart", para, commandType: CommandType.StoredProcedure);
                return result.ToList();
            }
        }

        private string GetConnectionStringByDbName(string dbName)
        {
            var conn = BaseSettingHelper.GetSettingValue($"ConnectionStrings:{dbName}");
            return conn;
        }

        protected string ReplaceConnectTimeoutInConnString(string connText, int newTimeoutSeconds = 30)
        {
            //滿廢的寫法
            connText = connText.Replace("Connection Timeout=60", $"Connection Timeout={newTimeoutSeconds}");
            connText = connText.Replace("Connection Timeout=45", $"Connection Timeout={newTimeoutSeconds}");
            connText = connText.Replace("Connection Timeout=30", $"Connection Timeout={newTimeoutSeconds}");
            return connText;
        }

    }


}
