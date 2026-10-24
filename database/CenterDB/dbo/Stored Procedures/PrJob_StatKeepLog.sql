-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<统计留存率>
--	2018/03/27	Jason		統計機制改寫為LOGDB統計並直接寫入Center紀錄表
--	2018/04/02	Jason		優化統計語法及修正記錄問題
--	2018/05/22	Jason		調整統計
--	2018/05/23	Jason		#349 建立統計數據報表提供API回傳 新增3 4 5 6 8 9 10日統計
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_StatKeepLog]
(
	@dtStatTime DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON

	SET @dtStatTime = DATEADD(DAY,0,DATEDIFF(DAY,0,GETDATE()))

	CREATE TABLE #Temp_LogDBLog ([PID] INT,[AID] INT,[Reg] INT,[CreateTime] DATETIME,[1] INT,[2] INT,[3] INT,[4] INT,[5] INT,[6] INT,[7] INT,[8] INT,[9] INT,[10] INT,[30] INT)
	CREATE TABLE #Temp_DaysByMonth (StatTime DATETIME,PID INT,AID INT)

	DECLARE @LogStartTime			DATETIME = DATEADD(DAY,-30,DATEADD(DAY,0,@dtStatTime))
	DECLARE @LogEndTime				DATETIME = DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))

	

	/*  先取資料暫存  */
	------------------------------------------------------------------------------------------------------
	INSERT INTO #Temp_LogDBLog
	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrPsWeb_GetStatKeepLog]
		@dtCreateTime = @dtStatTime

	WHILE(@LogStartTime <= @LogEndTime)
	BEGIN
		INSERT INTO #Temp_DaysByMonth 
		SELECT DISTINCT @LogStartTime,[PID],[AID]
		FROM #Temp_LogDBLog AS A
			INNER JOIN [Area].[Area] AS B WITH (NOLOCK) ON 1=1

		SET @LogStartTime	=	DATEADD(DAY,1,@LogStartTime)
	END

	SELECT	*
	INTO #Temp_KeepCountData
	FROM (
			SELECT	DISTINCT ISNULL(A.PID,0) AS [PlatformID],
					ISNULL(A.AID,0) AS [AreaID],
					ISNULL(Reg,0) AS [RegCount],
					ISNULL([1],0) AS [OneDayKeep],
					ISNULL([2],0) AS [TwoDayKeep],
					ISNULL([3],0) AS [Third],
					ISNULL([4],0) AS [Fourth],
					ISNULL([5],0) AS [Fifth],
					ISNULL([6],0) AS [Sixth],
					ISNULL([7],0) AS [WeekKeep],
					ISNULL([8],0) AS [eighth],
					ISNULL([9],0) AS [ninth],
					ISNULL([10],0) AS [tenth],
					ISNULL([30],0) AS [MonthKeep],
					StatTime
			FROM #Temp_DaysByMonth AS A
				LEFT JOIN #Temp_LogDBLog AS B ON A.StatTime = B.CreateTime AND A.[PID] = B.[PID] AND A.[AID] = B.[AID]
			UNION ALL (
						SELECT	A.PID,
								0 AS AID,
								SUM(Reg) AS [RegCount],
								SUM([1]) AS [OneDayKeep],
								SUM([2]) AS [TwoDayKeep],
								SUM([3]) AS [Third],
								SUM([4]) AS [Fourth],
								SUM([5]) AS [Fifth],
								SUM([6]) AS [Sixth],
								SUM([7]) AS [WeekKeep],
								SUM([8]) AS [eighth],
								SUM([9]) AS [ninth],
								SUM([10]) AS [tenth],
								SUM([30]) AS [MonthKeep],
								StatTime
						FROM #Temp_DaysByMonth AS A
							LEFT JOIN #Temp_LogDBLog AS B ON A.StatTime = B.CreateTime AND A.[PID] = B.[PID]
						GROUP BY A.StatTime,A.PID
					)
			UNION ALL (
						SELECT	0 AS PID,
								0 AS AID,
								SUM(Reg) AS [RegCount],
								SUM([1]) AS [OneDayKeep],
								SUM([2]) AS [TwoDayKeep],
								SUM([3]) AS [Third],
								SUM([4]) AS [Fourth],
								SUM([5]) AS [Fifth],
								SUM([6]) AS [Sixth],
								SUM([7]) AS [WeekKeep],
								SUM([8]) AS [eighth],
								SUM([9]) AS [ninth],
								SUM([10]) AS [tenth],
								SUM([30]) AS [MonthKeep],
								StatTime
						FROM #Temp_DaysByMonth AS A
							LEFT JOIN #Temp_LogDBLog AS B ON A.StatTime = B.CreateTime AND A.[PID] = B.[PID]
						GROUP BY A.StatTime
					)
	) AS A
	WHERE StatTime IS NOT NULL
	ORDER BY StatTime,A.[PlatformID],A.[AreaID]
	------------------------------------------------------------------------------------------------------


	/*  更新統計數據  */
	------------------------------------------------------------------------------------------------------
	MERGE [Stat].[KeepLog] AS A
	USING (
				SELECT	* FROM #Temp_KeepCountData
		) AS B ON (A.[StatTime] = B.[StatTime] AND A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID])
	WHEN MATCHED THEN
		UPDATE SET	A.[RegCount] = ISNULL(B.[RegCount],0),
					A.[OneDayKeep] = ISNULL(B.[OneDayKeep],0),
					A.[TwoDayKeep] = ISNULL(B.[TwoDayKeep],0),
					A.[WeekKeep] = ISNULL(B.[WeekKeep],0),
					A.[MonthKeep] = ISNULL(B.[MonthKeep],0),
					A.[Third] = ISNULL(B.[Third],0),
					A.[Fourth] = ISNULL(B.[Fourth],0),
					A.[Fifth] = ISNULL(B.[Fifth],0),
					A.[Sixth] = ISNULL(B.[Sixth],0),
					A.[eighth] = ISNULL(B.[eighth],0),
					A.[ninth] = ISNULL(B.[ninth],0),
					A.[tenth] = ISNULL(B.[tenth],0)
	WHEN NOT MATCHED THEN
		INSERT ([PlatformID],[AreaID],[RegCount],[OneDayKeep],[TwoDayKeep],[WeekKeep],[MonthKeep],[StatTime],
				[Third],[Fourth],[Fifth],[Sixth],[eighth],[ninth],[tenth])
		VALUES
			(
				[PlatformID],[AreaID],ISNULL(B.[RegCount],0),ISNULL(B.[OneDayKeep],0),ISNULL(B.[TwoDayKeep],0),ISNULL(B.[WeekKeep],0),ISNULL(B.[MonthKeep],0),[StatTime],
				ISNULL(B.[Third],0),ISNULL(B.[Fourth],0),ISNULL(B.[Fifth],0),ISNULL(B.[Sixth],0),ISNULL(B.[eighth],0),ISNULL(B.[ninth],0),ISNULL(B.[tenth],0)
			);
	------------------------------------------------------------------------------------------------------

	DROP TABLE #Temp_DaysByMonth
	DROP TABLE #Temp_LogDBLog
	DROP TABLE #Temp_KeepCountData
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_StatKeepLog] TO [xbuser]
    AS [dbo];

