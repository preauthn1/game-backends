-- =============================================
-- Author:		Jason
-- Create date: 2018/05/24
-- Description:	統計遊戲用戶稅收
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_Stat_GameUserAmountLog]
(
	@dtStatTime DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON

	SET @dtStatTime = ISNULL(@dtStatTime,GETDATE())

	DECLARE @LogStartTime			DATETIME
	DECLARE @LogEndTime				DATETIME

	DECLARE @Temp_GameUserAmount	TABLE ([StatTime] DATETIME,[PlatformID] INT,[AreaID] INT,[GameID] INT,[UserID] INT,[TotalAmount] BIGINT,[TotalTaxAmount] BIGINT)


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


	/*  取登入紀錄  */
	------------------------------------------------------
	INSERT INTO @Temp_GameUserAmount
	SELECT	[StatTime] = @LogStartTime,
			[PlatformID],
			[AreaID],
			[GameID],
			[UserID],
			TotalAmount = SUM([Amount]),
			TotalTaxAmount = SUM([TaxAmount])
	FROM [Game].[GameLog] WITH (NOLOCK)
	WHERE
		LogoutTime >= @LogStartTime
		AND LogoutTime <= @LogEndTime
	GROUP BY [PlatformID],[AreaID],[GameID],[UserID]
	------------------------------------------------------


	/*  若已有統計數據則刪除後再重新寫入  */
	------------------------------------------------------
	MERGE [Stat].[StatisticsGameUserAmount] AS A
	USING (
			SELECT	[StatTime],[PlatformID],[AreaID],[GameID],[UserID],[TotalAmount],[TotalTaxAmount]
			FROM @Temp_GameUserAmount
			) AS B ON (A.[StatTime] = B.[StatTime] AND A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID] AND A.GameID = B.GameID AND A.UserID = B.UserID)
	WHEN MATCHED THEN
		UPDATE SET	A.[TotalAmount] = ISNULL(B.[TotalAmount],0),
					A.[TotalTaxAmount] = ISNULL(B.[TotalTaxAmount],0)
	WHEN NOT MATCHED THEN
		INSERT
			([StatTime],[PlatformID],[AreaID],[GameID],[UserID],[TotalAmount],[TotalTaxAmount])
		VALUES
			(
				[StatTime],[PlatformID],[AreaID],[GameID],[UserID],ISNULL(B.[TotalAmount],0),ISNULL(B.[TotalTaxAmount],0)
			);
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_Stat_GameUserAmountLog] TO [xbuser]
    AS [dbo];

