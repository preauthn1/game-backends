-- =============================================
-- Author:  Jason
-- Create date: 2018/05/24
-- Description:	回傳GameUserAmount紀錄
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetGameUserAmountLog]
(
	@dtStatTime		DATETIME = NULL,		
	@dtEndTime		DATETIME = NULL
)
AS
BEGIN
	
	SET NOCOUNT ON

	DECLARE @StatTime		DATETIME = @dtStatTime		
	DECLARE @EndTime		DATETIME = @dtEndTime


	/*  回傳紀錄  */
	------------------------------------------------------
	SELECT	[StatTime],[PlatformID],[AreaID],[GameID],[UserID],[TotalAmount],[TotalTaxAmount]
	FROM [Stat].[StatisticsGameUserAmount] WITH (NOLOCK)
	WHERE
		(@StatTime IS NULL OR [StatTime] >= @StatTime)
		AND (@EndTime IS NULL OR [StatTime] < @EndTime)
	------------------------------------------------------


END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetGameUserAmountLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetGameUserAmountLog] TO [xbuser]
    AS [dbo];

