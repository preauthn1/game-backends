
-- =============================================
-- Author:		Jason
-- Create date: 2018/03/30
-- Description:	GameLog統計資訊
--	2018/05/16	Jason		#257 報表統計日月報表調整每30分鐘執行一次計算至當下時間
--	2018/05/30	Jason		#349 調整月報GameTime計算datatype轉換
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_Stat_GameLogStatistics]
(
	@dtStatTime	DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @LogStartTime		DATETIME
	DECLARE @LogEndTime			DATETIME
	DECLARE @LogMonthStartTime	DATETIME
	DECLARE @LogMonthEndTime	DATETIME

	SET @dtStatTime			= ISNULL(@dtStatTime,GETDATE())
																 
															   
																		  
																						 

	IF (DATEPART(HOUR,@dtStatTime) = 0) AND (DATEPART(MINUTE,@dtStatTime) < 10)
	BEGIN
		SET @LogStartTime		= DATEADD(DAY,-1,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogEndTime			= DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogMonthStartTime	= DATEADD(MONTH,DATEDIFF(MONTH,0,@LogStartTime),0)
		SET @LogMonthEndTime	= DATEADD(MONTH,1,DATEADD(MONTH,DATEDIFF(MONTH,0,@LogStartTime),0))
	END
	ELSE
	BEGIN
		SET @LogStartTime		= DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogEndTime			= DATEADD(DAY,1,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogMonthStartTime	= DATEADD(MONTH,DATEDIFF(MONTH,0,@LogStartTime),0)
		SET @LogMonthEndTime	= DATEADD(MONTH,1,DATEADD(MONTH,DATEDIFF(MONTH,0,@LogStartTime),0))
	END

	/*  刪除現有紀錄  */
	----------------------------------------------------------------------------------------------------------------------------------------
	--DELETE FROM [Stat].[StatisticsData_Day] WHERE @LogStartTime = [StatTime]
	--DELETE FROM [Stat].[StatisticsData_Month] WHERE @LogMonthStartTime = [StatTime]
	----------------------------------------------------------------------------------------------------------------------------------------


	/*  統計暫存  */
	----------------------------------------------------------------------------------------------------------------------------------------
	--日報表統計
	SELECT	*
	INTO #Temp_SDD
	FROM (
			SELECT	[StatTime] = @LogStartTime,
					PlatformID = ISNULL(A.PlatformID,0),
					AreaID = ISNULL(A.AreaID,0),
					GameID = ISNULL(GameID,0),
					ServerID = ISNULL(ServerID,0),
					[UserCount] = COUNT(DISTINCT A.UserID),
					[NewUser] = COUNT(DISTINCT (CASE WHEN A.UserID = B.UserID AND [CreateTime] >= @LogStartTime THEN A.UserID ELSE NULL END)),
					[IPCount] = COUNT(DISTINCT A.IPAddress),
					[NewIPCount] = COUNT(DISTINCT (CASE WHEN A.UserID = B.UserID AND [CreateTime] >= @LogStartTime THEN A.IPAddress ELSE NULL END)),
					[MacCount] = COUNT(DISTINCT A.HDCode),
					[NewMacCount] = COUNT(DISTINCT (CASE WHEN A.UserID = B.UserID AND [CreateTime] >= @LogStartTime THEN A.HDCode ELSE NULL END)),
					[iOSCount] = COUNT(DISTINCT (CASE WHEN A.HDType = 3 THEN A.UserID ELSE NULL END)),
					[NewiOSCount] = COUNT(DISTINCT (CASE WHEN A.HDType = 3 AND A.UserID = B.UserID AND [CreateTime] >= @LogStartTime THEN A.UserID ELSE NULL END)),
					[AndRoidCount] = COUNT(DISTINCT (CASE WHEN A.HDType = 2 THEN A.UserID ELSE NULL END)),
					[NewAndRoidCount] = COUNT(DISTINCT (CASE WHEN A.HDType = 2 AND A.UserID = B.UserID AND [CreateTime] >= @LogStartTime THEN A.UserID ELSE NULL END)),
					[PCCount] = COUNT(DISTINCT (CASE WHEN A.HDType = 1 THEN A.UserID ELSE NULL END)),
					[NewPCCount] = COUNT(DISTINCT (CASE WHEN A.HDType = 1 AND A.UserID = B.UserID AND [CreateTime] >= @LogStartTime THEN A.UserID ELSE NULL END)),
					[AmountTotal] = SUM(Amount),
					[NewAmountTotal] = SUM(CASE WHEN A.UserID = B.UserID AND [CreateTime] >= @LogStartTime THEN Amount ELSE 0 END),
					[AmountAvg] = AVG(Amount),
					[NewAmountAvg] = AVG(CASE WHEN A.UserID = B.UserID AND [CreateTime] >= @LogStartTime THEN Amount ELSE 0 END),
					[TaxAmountTotal] = SUM(TaxAmount),
					[NewTaxAmountTotal] = SUM(CASE WHEN A.UserID = B.UserID AND [CreateTime] >= @LogStartTime THEN TaxAmount ELSE 0 END),
					[TaxAmountAvg] = AVG(TaxAmount),
					[NewTaxAmountAvg] = AVG(CASE WHEN A.UserID = B.UserID AND [CreateTime] >= @LogStartTime THEN TaxAmount ELSE 0 END),
					[GameTimeMax] = MAX(CAST([GameTime] AS BIGINT)),
					[GameTimeMin] = MIN(CAST([GameTime] AS BIGINT)),
					[GameTimeAvg] = AVG(CAST([GameTime] AS BIGINT))
			FROM [Game].[GameLog] AS A WITH (NOLOCK)
				INNER JOIN [Users].[UserRegLog] AS B WITH (NOLOCK) ON A.UserID = B.UserID
			WHERE
				LogoutTime >= @LogStartTime
				AND LogoutTime < @LogEndTime
			GROUP BY ROLLUP(A.PlatformID,A.AreaID,GameID,ServerID)
		) AS A


	--月報表統計
	SELECT	*
	INTO #Temp_SDM
	FROM (
			SELECT	[StatTime] = @LogMonthStartTime,
					PlatformID = ISNULL(A.PlatformID,0),
					AreaID = ISNULL(A.AreaID,0),
					GameID = ISNULL(GameID,0),
					ServerID = ISNULL(ServerID,0),
					[UserCount] = COUNT(DISTINCT A.UserID),
					[NewUser] = COUNT(DISTINCT (CASE WHEN A.UserID = B.UserID AND [CreateTime] >= @LogMonthStartTime THEN A.UserID ELSE 0 END)),
					[IPCount] = COUNT(DISTINCT A.IPAddress),
					[NewIPCount] = COUNT(DISTINCT (CASE WHEN A.UserID = B.UserID AND [CreateTime] >= @LogMonthStartTime THEN A.IPAddress ELSE NULL END)),
					[MacCount] = COUNT(DISTINCT A.HDCode),
					[NewMacCount] = COUNT(DISTINCT (CASE WHEN A.UserID = B.UserID AND [CreateTime] >= @LogMonthStartTime THEN A.HDCode ELSE NULL END)),
					[iOSCount] = COUNT(DISTINCT (CASE WHEN A.HDType = 3 THEN A.UserID ELSE NULL END)),
					[NewiOSCount] = COUNT(DISTINCT (CASE WHEN A.HDType = 3 AND A.UserID = B.UserID AND [CreateTime] >= @LogMonthStartTime THEN A.UserID ELSE NULL END)),
					[AndRoidCount] = COUNT(DISTINCT (CASE WHEN A.HDType = 2 THEN A.UserID ELSE NULL END)),
					[NewAndRoidCount] = COUNT(DISTINCT (CASE WHEN A.HDType = 2 AND A.UserID = B.UserID AND [CreateTime] >= @LogMonthStartTime THEN A.UserID ELSE NULL END)),
					[PCCount] = COUNT(DISTINCT (CASE WHEN A.HDType = 1 THEN A.UserID ELSE NULL END)),
					[NewPCCount] = COUNT(DISTINCT (CASE WHEN A.HDType = 1 AND A.UserID = B.UserID AND [CreateTime] >= @LogMonthStartTime THEN A.UserID ELSE NULL END)),
					[AmountTotal] = SUM(Amount),
					[NewAmountTotal] = SUM(CASE WHEN A.UserID = B.UserID AND [CreateTime] >= @LogMonthStartTime THEN Amount ELSE 0 END),
					[AmountAvg] = AVG(Amount),
					[NewAmountAvg] = AVG(CASE WHEN A.UserID = B.UserID AND [CreateTime] >= @LogMonthStartTime THEN Amount ELSE 0 END),
					[TaxAmountTotal] = SUM(TaxAmount),
					[NewTaxAmountTotal] = SUM(CASE WHEN A.UserID = B.UserID AND [CreateTime] >= @LogMonthStartTime THEN TaxAmount ELSE 0 END),
					[TaxAmountAvg] = AVG(TaxAmount),
					[NewTaxAmountAvg] = AVG(CASE WHEN A.UserID = B.UserID AND [CreateTime] >= @LogMonthStartTime THEN TaxAmount ELSE 0 END),
					[GameTimeMax] = MAX(CAST([GameTime] AS BIGINT)),
					[GameTimeMin] = MIN(CAST([GameTime] AS BIGINT)),
					[GameTimeAvg] = AVG(CAST([GameTime] AS BIGINT))
			FROM [Game].[GameLog] AS A WITH (NOLOCK)
				INNER JOIN [Users].[UserRegLog] AS B WITH (NOLOCK) ON A.UserID = B.UserID
			WHERE
				LogoutTime >= @LogMonthStartTime
				AND LogoutTime < @LogMonthEndTime
			GROUP BY ROLLUP(A.PlatformID,A.AreaID,GameID,ServerID)
	) AS A
	----------------------------------------------------------------------------------------------------------------------------------------


	/*  寫入統計資料表  */
	----------------------------------------------------------------------------------------------------------------------------------------
	--日報表
	MERGE [Stat].[StatisticsData_Day] AS A
	USING 
		(
			SELECT	[StatTime],[PlatformID],[AreaID],[GameID],[ServerID],[UserCount],[NewUser],[IPCount],[NewIPCount],[MacCount],[NewMacCount],
					[iOSCount],[NewiOSCount],[AndRoidCount],[NewAndRoidCount],[PCCount],[NewPCCount],[AmountTotal],[NewAmountTotal],[AmountAvg],[NewAmountAvg],
					[TaxAmountTotal],[NewTaxAmountTotal],[TaxAmountAvg],[NewTaxAmountAvg],[GameTimeMax],[GameTimeMin],[GameTimeAvg]
			FROM #Temp_SDD
		) AS B ON (A.[StatTime] = B.[StatTime] AND A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID] AND A.[GameID] = B.[GameID] AND A.[ServerID] = B.[ServerID])
	WHEN MATCHED THEN
		UPDATE SET	A.[UserCount] = B.[UserCount],
					A.[NewUser] = B.[NewUser],
					A.[IPCount] = B.[IPCount],
					A.[NewIPCount] = B.[NewIPCount],
					A.[MacCount] = B.[MacCount],
					A.[NewMacCount] = B.[NewMacCount],
					A.[iOSCount] = B.[iOSCount],
					A.[NewiOSCount] = B.[NewiOSCount],
					A.[AndRoidCount] = B.[AndRoidCount],
					A.[NewAndRoidCount] = B.[NewAndRoidCount],
					A.[PCCount] = B.[PCCount],
					A.[NewPCCount] = B.[NewPCCount],
					A.[AmountTotal] = B.[AmountTotal],
					A.[NewAmountTotal] = B.[NewAmountTotal],
					A.[AmountAvg] = B.[AmountAvg],
					A.[NewAmountAvg] = B.[NewAmountAvg],
					A.[TaxAmountTotal] = B.[TaxAmountTotal],
					A.[NewTaxAmountTotal] = B.[NewTaxAmountTotal],
					A.[TaxAmountAvg] = B.[TaxAmountAvg],
					A.[NewTaxAmountAvg] = B.[NewTaxAmountAvg],
					A.[GameTimeMax] = B.[GameTimeMax],
					A.[GameTimeMin] = B.[GameTimeMin],
					A.[GameTimeAvg] = B.[GameTimeAvg]
	WHEN NOT MATCHED THEN
		INSERT
			(
				[StatTime],[PlatformID],[AreaID],[GameID],[ServerID],[UserCount],[NewUser],[IPCount],[NewIPCount],[MacCount],[NewMacCount],
				[iOSCount],[NewiOSCount],[AndRoidCount],[NewAndRoidCount],[PCCount],[NewPCCount],[AmountTotal],[NewAmountTotal],[AmountAvg],[NewAmountAvg],
				[TaxAmountTotal],[NewTaxAmountTotal],[TaxAmountAvg],[NewTaxAmountAvg],[GameTimeMax],[GameTimeMin],[GameTimeAvg]
			)
		VALUES
			(
				[StatTime],[PlatformID],[AreaID],[GameID],[ServerID],
				ISNULL([UserCount],0),ISNULL([NewUser],0),
				ISNULL([IPCount],0),ISNULL([NewIPCount],0),
				ISNULL([MacCount],0),ISNULL([NewMacCount],0),
				ISNULL([iOSCount],0),ISNULL([NewiOSCount],0),
				ISNULL([AndRoidCount],0),ISNULL([NewAndRoidCount],0),
				ISNULL([PCCount],0),ISNULL([NewPCCount],0),
				ISNULL([AmountTotal],0),ISNULL([NewAmountTotal],0),
				ISNULL([AmountAvg],0),ISNULL([NewAmountAvg],0),
				ISNULL([TaxAmountTotal],0),ISNULL([NewTaxAmountTotal],0),
				ISNULL([TaxAmountAvg],0),ISNULL([NewTaxAmountAvg],0),
				ISNULL([GameTimeMax],0),ISNULL([GameTimeMin],0),ISNULL([GameTimeAvg],0)
			);


	--月報表
	MERGE [Stat].[StatisticsData_Month] AS A
	USING 
		(
			SELECT	[StatTime],[PlatformID],[AreaID],[GameID],[ServerID],[UserCount],[NewUser],[IPCount],[NewIPCount],[MacCount],[NewMacCount],
					[iOSCount],[NewiOSCount],[AndRoidCount],[NewAndRoidCount],[PCCount],[NewPCCount],[AmountTotal],[NewAmountTotal],[AmountAvg],[NewAmountAvg],
					[TaxAmountTotal],[NewTaxAmountTotal],[TaxAmountAvg],[NewTaxAmountAvg],[GameTimeMax],[GameTimeMin],[GameTimeAvg]
			FROM #Temp_SDM
		) AS B ON (A.[StatTime] = B.[StatTime] AND A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID] AND A.[GameID] = B.[GameID] AND A.[ServerID] = B.[ServerID])
	WHEN MATCHED THEN
		UPDATE SET	A.[UserCount] = B.[UserCount],
					A.[NewUser] = B.[NewUser],
					A.[IPCount] = B.[IPCount],
					A.[NewIPCount] = B.[NewIPCount],
					A.[MacCount] = B.[MacCount],
					A.[NewMacCount] = B.[NewMacCount],
					A.[iOSCount] = B.[iOSCount],
					A.[NewiOSCount] = B.[NewiOSCount],
					A.[AndRoidCount] = B.[AndRoidCount],
					A.[NewAndRoidCount] = B.[NewAndRoidCount],
					A.[PCCount] = B.[PCCount],
					A.[NewPCCount] = B.[NewPCCount],
					A.[AmountTotal] = B.[AmountTotal],
					A.[NewAmountTotal] = B.[NewAmountTotal],
					A.[AmountAvg] = B.[AmountAvg],
					A.[NewAmountAvg] = B.[NewAmountAvg],
					A.[TaxAmountTotal] = B.[TaxAmountTotal],
					A.[NewTaxAmountTotal] = B.[NewTaxAmountTotal],
					A.[TaxAmountAvg] = B.[TaxAmountAvg],
					A.[NewTaxAmountAvg] = B.[NewTaxAmountAvg],
					A.[GameTimeMax] = B.[GameTimeMax],
					A.[GameTimeMin] = B.[GameTimeMin],
					A.[GameTimeAvg] = B.[GameTimeAvg]
	WHEN NOT MATCHED THEN
		INSERT
			(
				[StatTime],[PlatformID],[AreaID],[GameID],[ServerID],[UserCount],[NewUser],[IPCount],[NewIPCount],[MacCount],[NewMacCount],
				[iOSCount],[NewiOSCount],[AndRoidCount],[NewAndRoidCount],[PCCount],[NewPCCount],[AmountTotal],[NewAmountTotal],[AmountAvg],[NewAmountAvg],
				[TaxAmountTotal],[NewTaxAmountTotal],[TaxAmountAvg],[NewTaxAmountAvg],[GameTimeMax],[GameTimeMin],[GameTimeAvg]
			)
		VALUES
			(
				[StatTime],[PlatformID],[AreaID],[GameID],[ServerID],
				ISNULL([UserCount],0),ISNULL([NewUser],0),
				ISNULL([IPCount],0),ISNULL([NewIPCount],0),
				ISNULL([MacCount],0),ISNULL([NewMacCount],0),
				ISNULL([iOSCount],0),ISNULL([NewiOSCount],0),
				ISNULL([AndRoidCount],0),ISNULL([NewAndRoidCount],0),
				ISNULL([PCCount],0),ISNULL([NewPCCount],0),
				ISNULL([AmountTotal],0),ISNULL([NewAmountTotal],0),
				ISNULL([AmountAvg],0),ISNULL([NewAmountAvg],0),
				ISNULL([TaxAmountTotal],0),ISNULL([NewTaxAmountTotal],0),
				ISNULL([TaxAmountAvg],0),ISNULL([NewTaxAmountAvg],0),
				ISNULL([GameTimeMax],0),ISNULL([GameTimeMin],0),ISNULL([GameTimeAvg],0)
			);
	----------------------------------------------------------------------------------------------------------------------------------------

	DROP TABLE #Temp_SDD
	DROP TABLE #Temp_SDM

	RETURN
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_Stat_GameLogStatistics] TO [xbuser]
    AS [dbo];

