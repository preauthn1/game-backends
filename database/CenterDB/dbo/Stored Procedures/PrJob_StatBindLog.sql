-- =============================================
-- Author:  Jason
-- Create date: 2018/05/28
-- Description:	用戶綁定統計 #349
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_StatBindLog]
(
	@dtStatTime	DATETIME = NULL
)
AS
BEGIN
SET NOCOUNT ON

	SET	@dtStatTime = ISNULL(@dtStatTime,GETDATE())

	DECLARE @LogStartTime	DATETIME
	DECLARE @LogEndTime		DATETIME

	DECLARE @Temp_BindLog	TABLE ([StatTime] DATETIME,[PlatformID] INT,[AreaID] INT,[TotalBindCount] INT,[SameDayReg] INT,[IOSCount] INT,[AndroidCount] INT,[PCCount] INT)

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
	INSERT INTO @Temp_BindLog
	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrWeb_Stat_GetBindLog]
		@dtStatTime = @LogStartTime,
		@dtEndTime = @LogEndTime
	-------------------------------------------------------------------------------------------


	/*  更新資料  */
	-------------------------------------------------------------------------------------------
	MERGE [Stat].[UserBindLog] AS A
	USING (
				SELECT	* FROM @Temp_BindLog
		) AS B ON (A.[StatTime] = B.[StatTime] AND A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID])
	WHEN MATCHED THEN
		UPDATE SET	A.[PCCount] = B.[PCCount],
					A.[AndroidCount] = B.[AndroidCount],
					A.[IOSCount] = B.[IOSCount],
					A.[TotalBindCount] = B.[TotalBindCount],
					A.[SameDayReg] = B.[SameDayReg]
	WHEN NOT MATCHED THEN
		INSERT ([StatTime],[PlatformID],[AreaID],[TotalBindCount],[SameDayReg],[IOSCount],[AndroidCount],[PCCount])
		VALUES
			(
				[StatTime],[PlatformID],[AreaID],[TotalBindCount],[SameDayReg],[IOSCount],[AndroidCount],[PCCount]
			);
	-------------------------------------------------------------------------------------------


END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_StatBindLog] TO [xbuser]
    AS [dbo];

