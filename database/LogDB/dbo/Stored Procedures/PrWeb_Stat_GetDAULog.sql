-- =============================================
-- Author:  Jason
-- Create date: 2018/05/24
-- Description:	回傳DAU紀錄 #349
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetDAULog]
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
	SELECT	[StatTime],[PlatformID],[AreaID],[GameID],[PCCount],[AndroidCount],[IOSCount],[TotalCount]
	FROM [Stat].[StatisticsDAU] WITH (NOLOCK)
	WHERE
		(@StatTime IS NULL OR [StatTime] >= @StatTime)
		AND (@EndTime IS NULL OR [StatTime] < @EndTime)
		--AND [PlatformID] = 0 AND [AreaID] = 0 AND [GameID] = 0
	------------------------------------------------------


END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetDAULog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetDAULog] TO [xbuser]
    AS [dbo];

