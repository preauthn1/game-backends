-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<统计注册帐号数>
--	2018/04/02	Jason		修正HDTYPE錯誤問題及改為取LOGDB資料
--	2018/04/10	Jason		修正統計流程
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_StatRegLog]
(
	@dtStatTime	DATETIME = NULL
)
AS
BEGIN
SET NOCOUNT ON

	SET	@dtStatTime = ISNULL(@dtStatTime,GETDATE())

	DECLARE @LogStartTime	DATETIME
	DECLARE @LogEndTime		DATETIME

	CREATE TABLE #Temp_RegUser ([StatTime] DATETIME,[PlatformID] INT,[AreaID] INT,[TotalRegCount] INT,[IOSRegCount] INT,[AndroidRegCount] INT,[PCRegCount] INT)
	CREATE TABLE #Temp_DaysByDate ([StatTime] DATETIME,[PlatformID] INT,[AreaID] INT)

	IF (DATEPART(HOUR,@dtStatTime) = 0) AND (DATEPART(MINUTE,@dtStatTime) < 10)
	BEGIN
		SET @LogStartTime		= DATEADD(DAY,-1,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogEndTime			= DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
	END
	ELSE
	BEGIN
		SET @LogStartTime		= DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogEndTime			= DATEADD(DAY,1,DATEDIFF(DAY,0,@dtStatTime))
	END

	/*  取資料暫存  */
	-------------------------------------------------------------------------------------------
	INSERT INTO #Temp_RegUser
	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrPs_GetRegUserData]
		@dtStatTime = @LogStartTime,
		@dtEndtime = @LogEndTime

	INSERT INTO #Temp_DaysByDate 
	SELECT	DISTINCT @LogStartTime,ISNULL(PlatformID,0),ISNULL(AreaID,0)
	FROM [Area].[Area] WITH (NOLOCK)
	GROUP BY ROLLUP(PlatformID,AreaID)

	SELECT	DISTINCT *
	INTO #Temp_RegUserCountData
	FROM (
			SELECT	A.[StatTime],
					ISNULL(A.[PlatformID],0) AS [PlatformID],
					ISNULL(A.[AreaID],0) AS [AreaID],
					SUM(ISNULL([TotalRegCount],0)) AS [TotalRegCount],
					SUM(ISNULL([IOSRegCount],0)) AS [IOSRegCount],
					SUM(ISNULL([AndroidRegCount],0)) AS [AndroidRegCount],
					SUM(ISNULL([PCRegCount],0)) AS [PCRegCount]
			FROM #Temp_DaysByDate AS A
				LEFT JOIN #Temp_RegUser AS B ON A.StatTime = B.[StatTime] AND A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID]
			GROUP BY A.StatTime,A.[PlatformID],A.[AreaID]--ROLLUP(A.StatTime,A.[PlatformID],A.[AreaID])
	) AS A
	WHERE StatTime IS NOT NULL
	ORDER BY StatTime,A.[PlatformID],A.[AreaID]
	-------------------------------------------------------------------------------------------


	/*  更新資料  */
	-------------------------------------------------------------------------------------------
	MERGE [Stat].[RegUserLog] AS A
	USING (
				SELECT	* FROM #Temp_RegUserCountData
		) AS B ON (A.[StatTime] = B.[StatTime] AND A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID])
	WHEN MATCHED THEN
		UPDATE SET	A.[TotalRegCount] = B.[TotalRegCount],
					A.[IOSRegCount] = B.[IOSRegCount],
					A.[AndroidRegCount] = B.[AndroidRegCount],
					A.[PCRegCount] = B.[PCRegCount]
	WHEN NOT MATCHED THEN
		INSERT ([PlatformID],[AreaID],[TotalRegCount],[IOSRegCount],[AndroidRegCount],[PCRegCount],[StatTime])
		VALUES
			(
				[PlatformID],[AreaID],[TotalRegCount],[IOSRegCount],[AndroidRegCount],[PCRegCount],[StatTime]
			);
	-------------------------------------------------------------------------------------------


	DROP TABLE #Temp_RegUser
	DROP TABLE #Temp_DaysByDate
	DROP TABLE #Temp_RegUserCountData
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_StatRegLog] TO [xbuser]
    AS [dbo];

