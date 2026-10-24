-- =============================================
-- Author:		Jason
-- Create date: 2018/04/02
-- Description:	回傳每日註冊用戶統計
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_GetRegUserData]
(
	@dtStattime		DATETIME = NULL, 
	@dtEndtime		DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @StatTime	DATETIME = @dtStattime
	DECLARE @EndTime	DATETIME = @dtEndtime


	/*  回傳紀錄  */
	------------------------------------------------------
	SELECT	[StatTime],[PlatformID],[AreaID],[TotalRegCount],[IOSRegCount],[AndroidRegCount],[PCRegCount]
	FROM [Stat].[StatisticsRegUser] WITH (NOLOCK)
	WHERE
		(@StatTime IS NULL OR [StatTime] >= @StatTime)
		AND (@EndTime IS NULL OR [StatTime] < @EndTime)
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetRegUserData] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetRegUserData] TO [xbuser]
    AS [dbo];

