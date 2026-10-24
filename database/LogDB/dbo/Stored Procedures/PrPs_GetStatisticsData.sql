-- =============================================
-- Author:		Jason
-- Create date: 2018/03/31
-- Description:	回傳每日遊戲統計紀錄
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_GetStatisticsData]
(
	@intTypeID		TINYINT = 1,		--1:Daily  2:Month
	@dtStattime		DATETIME = NULL, 
	@dtEndtime		DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @TypeID		INT = @intTypeID
	DECLARE @StatTime	DATETIME = DATEADD(DAY,0,DATEDIFF(DAY,0,ISNULL(@dtStattime,GETDATE())))
	DECLARE @EndTime	DATETIME = DATEADD(DAY,0,DATEDIFF(DAY,0,ISNULL(@dtEndtime,GETDATE())))


	/*  回傳紀錄  */
	------------------------------------------------------
	IF (@intTypeID = 1)
	BEGIN
		SELECT	[StatTime],[PlatformID],[AreaID],[GameID],[ServerID],[UserCount],[NewUser],[IPCount],[NewIPCount],[MacCount],[NewMacCount],
				[iOSCount],[NewiOSCount],[AndRoidCount],[NewAndRoidCount],[PCCount],[NewPCCount],[AmountTotal],[NewAmountTotal],
				[AmountAvg],[NewAmountAvg],[TaxAmountTotal],[NewTaxAmountTotal],[TaxAmountAvg],[NewTaxAmountAvg],[GameTimeMax],[GameTimeMin],[GameTimeAvg],[RobotTaxAmount]
		FROM [Stat].[StatisticsData_Day] WITH (NOLOCK)
		WHERE
			(@StatTime IS NULL OR [StatTime] >= @StatTime)
			AND (@EndTime IS NULL OR [StatTime] < @EndTime)
	END

	IF (@intTypeID = 2)
	BEGIN
		SELECT	[StatTime],[PlatformID],[AreaID],[GameID],[ServerID],[UserCount],[NewUser],[IPCount],[NewIPCount],[MacCount],[NewMacCount],
				[iOSCount],[NewiOSCount],[AndRoidCount],[NewAndRoidCount],[PCCount],[NewPCCount],[AmountTotal],[NewAmountTotal],
				[AmountAvg],[NewAmountAvg],[TaxAmountTotal],[NewTaxAmountTotal],[TaxAmountAvg],[NewTaxAmountAvg],[GameTimeMax],[GameTimeMin],[GameTimeAvg],[RobotTaxAmount]
		FROM [Stat].[StatisticsData_Month] WITH (NOLOCK)
		WHERE
			(@StatTime IS NULL OR [StatTime] >= @StatTime)
			AND (@EndTime IS NULL OR [StatTime] < @EndTime)
	END
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetStatisticsData] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetStatisticsData] TO [xbuser]
    AS [dbo];

