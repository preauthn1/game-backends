-- =============================================
-- Author:  Jason
-- Create date: 2018/05/24
-- Description:	遊戲稅收紀錄
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_StatGameAmountLog]
(
	@dtStatTime DATETIME = NULL
)
AS
BEGIN
	
	SET NOCOUNT ON

	SET	@dtStatTime = ISNULL(@dtStatTime,GETDATE())

	DECLARE @LogStartTime	DATETIME
	DECLARE @LogEndTime		DATETIME

	DECLARE @Temp_GameUserAmountLog TABLE ([StatTime] DATETIME,[PlatformID] INT,[AreaID] INT,[GameID] INT,[UserID] INT,[TotalAmount] BIGINT,[TotalTaxAmount] BIGINT)
	DECLARE @Temp_GameUserAmount	TABLE ([StatTime] DATETIME,[PlatformID] INT,[AreaID] INT,[GameID] INT,[UserAmount] BIGINT,[SystemAmount] BIGINT,[UserTaxAmount] BIGINT,[SystemTaxAmount] BIGINT)

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

	

	/*  先取資料暫存  */
	------------------------------------------------------------------------------------------------------
	INSERT INTO @Temp_GameUserAmountLog
	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrWeb_Stat_GetGameUserAmountLog]
		@dtStatTime		= @LogStartTime,		
		@dtEndTime		= @LogEndTime

	INSERT INTO @Temp_GameUserAmount
	SELECT	A.[StatTime],
			A.[PlatformID],
			A.[AreaID],
			[GameID],
			[UserAmount] = SUM(CASE	WHEN B.UserType <> 2 THEN ISNULL([TotalAmount],0) ELSE 0 END),
			[SystemAmount] = SUM(CASE	WHEN B.UserType = 2 THEN ISNULL([TotalAmount],0) ELSE 0 END),
			[UserTaxAmount] = SUM(CASE	WHEN B.UserType <> 2 THEN ISNULL([TotalTaxAmount],0) ELSE 0 END),
			[SystemTaxAmount] = SUM(CASE	WHEN B.UserType = 2 THEN ISNULL([TotalTaxAmount],0) ELSE 0 END)
	FROM @Temp_GameUserAmountLog AS A
		INNER JOIN [Users].[UserInfo] AS B WITH (NOLOCK) ON A.UserID = B.UserID
	GROUP BY A.[StatTime],A.[PlatformID],A.[AreaID],[GameID]
	------------------------------------------------------------------------------------------------------


	/*  更新統計數據  */
	------------------------------------------------------------------------------------------------------
	MERGE [Stat].[GameAmountLog] AS A
	USING (
				SELECT	* FROM @Temp_GameUserAmount
		) AS B ON (A.[StatTime] = B.[StatTime] AND A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID] AND A.GameID = B.GameID)
	WHEN MATCHED THEN
		UPDATE SET	A.[UserAmount] = B.[UserAmount],
					A.[SystemAmount] = B.[SystemAmount],
					A.[UserTaxAmount] = B.[UserTaxAmount],
					A.[SystemTaxAmount] = B.[SystemTaxAmount]
	WHEN NOT MATCHED THEN
		INSERT ([StatTime],[PlatformID],[AreaID],[GameID],[UserAmount],[SystemAmount],[UserTaxAmount],[SystemTaxAmount])
		VALUES
			(
				[StatTime],[PlatformID],[AreaID],[GameID],[UserAmount],[SystemAmount],[UserTaxAmount],[SystemTaxAmount]
			);
	------------------------------------------------------------------------------------------------------

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_StatGameAmountLog] TO [xbuser]
    AS [dbo];

