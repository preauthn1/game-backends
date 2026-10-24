-- =============================================
-- Author:  Jason
-- Create date: 2018/05/24
-- Description:	活躍用戶統計
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_StatDAULog]
(
	@dtStatTime	DATETIME = NULL
)
AS
BEGIN
SET NOCOUNT ON

	SET	@dtStatTime = ISNULL(@dtStatTime,GETDATE())

	DECLARE @LogStartTime	DATETIME
	DECLARE @LogEndTime		DATETIME

	CREATE TABLE #Temp_DAULog ([StatTime] DATETIME,[PlatformID] INT,[AreaID] INT,[GameID] INT,[PCCount] INT,[AndroidCount] INT,[IOSCount] INT,[TotalCount] INT)

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
	INSERT INTO #Temp_DAULog
	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrWeb_Stat_GetDAULog]
		@dtStatTime = @LogStartTime,
		@dtEndTime = @LogEndTime
	-------------------------------------------------------------------------------------------


	/*  更新資料  */
	-------------------------------------------------------------------------------------------
	MERGE [Stat].[DAULog] AS A
	USING (
				SELECT	* FROM #Temp_DAULog
		) AS B ON (A.[StatTime] = B.[StatTime] AND A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID] AND A.GameID = B.GameID)
	WHEN MATCHED THEN
		UPDATE SET	A.[PCCount] = B.[PCCount],
					A.[AndroidCount] = B.[AndroidCount],
					A.[IOSCount] = B.[IOSCount],
					A.[TotalCount] = B.[TotalCount]
	WHEN NOT MATCHED THEN
		INSERT ([StatTime],[PlatformID],[AreaID],[GameID],[PCCount],[AndroidCount],[IOSCount],[TotalCount])
		VALUES
			(
				[StatTime],[PlatformID],[AreaID],[GameID],[PCCount],[AndroidCount],[IOSCount],[TotalCount]
			);
	-------------------------------------------------------------------------------------------


	DROP TABLE #Temp_DAULog

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_StatDAULog] TO [xbuser]
    AS [dbo];

