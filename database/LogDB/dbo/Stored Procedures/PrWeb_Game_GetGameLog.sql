
-- =============================================
-- Author:		Jason
-- Create date: 2018/03/21
-- Description:	回傳UserGameLog
--	2018/04-13	Jason		調整LogoutTime >= @LogoutStartTime to (@LogoutStartTime IS NULL OR LogoutTime >= @LogoutStartTime)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Game_GetGameLog]
(
	@intServerID		INT = NULL,
	@intUserID			INT = NULL,
	@chvIpAddress		VARCHAR(15) = NULL,
	@tnyHDtype			INT = NULL,
	@chvHDcode			VARCHAR(64) = NULL,
	@dtLoginStartTime	DATETIME = NULL,
	@dtLoginEndTime		DATETIME = NULL,
	@dtLogoutStartTime	DATETIME = NULL,
	@dtLogoutEndTime	DATETIME = NULL,
	@intGameID			INT = NULL,
	@intAreaID			INT = NULL,
	@intPageIndex		INT = 1,
	@intPageSize		INT = 20,
	@intPageCount		INT = 0		OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE	@ServerID			INT = @intServerID
	DECLARE @UserID				INT = @intUserID
	DECLARE @IpAddress			VARCHAR(15) = @chvIpAddress
	DECLARE	@HDtype				TINYINT = @tnyHDtype
	DECLARE @HDcode				VARCHAR(64) = @chvHDcode
	DECLARE @LoginStartTime		DATETIME = @dtLoginStartTime
	DECLARE @LoginEndTime		DATETIME = @dtLoginEndTime
	DECLARE @LogoutStartTime	DATETIME = @dtLogoutStartTime
	DECLARE @LogoutEndTime		DATETIME = @dtLogoutEndTime
	DECLARE @GameID				INT = @intGameID
	DECLARE @AreaID				INT = @intAreaID
	DECLARE	@StartRow			INT = @intPageSize * (@intPageIndex -1) + 1
	DECLARE	@EndRow				INT = @intPageSize * @intPageIndex


	/*  回傳資料  */
	-------------------------------------------------------------------------------------------
	SELECT	@intPageCount = SUM(CASE WHEN [LogID]> 0 THEN 1 ELSE 0 END)
	FROM [Game].[GameLog] WITH (NOLOCK)
	WHERE
		(@ServerID IS NULL OR ServerID = @ServerID)
		AND (@UserID IS NULL OR UserID = @UserID)
		AND (@IpAddress IS NULL OR IPAddress = @IpAddress)
		AND (@HDtype IS NULL OR HDType = @HDtype)
		AND (@HDcode IS NULL OR HDCode = @HDcode)
		AND (@LoginStartTime IS NULL OR LoginTime >= @LoginStartTime)
		AND (@LoginEndTime IS NULL OR LoginTime <= @LoginEndTime)
		AND (@LogoutStartTime IS NULL OR LogoutTime >= @LogoutStartTime)
		AND (@LogoutEndTime IS NULL OR LogoutTime <= @LogoutEndTime)
		AND (@GameID IS NULL OR GameID = @GameID)
		AND (@AreaID IS NULL OR AreaID = @AreaID)

	SELECT	rowNum,[LogID],[UserID],[ServerID],[LoginMoney],[Amount],[TaxAmount],
			[WinCount],[LoseCount],[DrawCount],[GameTime],[IPAddress],
			[HDCode],[HDType],[LoginTime],[LogoutTime],[GameID],[AreaID]
	FROM (
			SELECT	ROW_NUMBER() OVER (ORDER BY LogoutTime DESC) AS rowNum,
					[LogID],[UserID],[ServerID],[LoginMoney],[Amount],[TaxAmount],
					[WinCount],[LoseCount],[DrawCount],[GameTime],[IPAddress],
					[HDCode],[HDType],[LoginTime],[LogoutTime],[GameID],[AreaID]
			FROM [Game].[GameLog] WITH (NOLOCK)
			WHERE
				(@ServerID IS NULL OR ServerID = @ServerID)
				AND (@UserID IS NULL OR UserID = @UserID)
				AND (@IpAddress IS NULL OR IPAddress = @IpAddress)
				AND (@HDtype IS NULL OR HDType = @HDtype)
				AND (@HDcode IS NULL OR HDCode = @HDcode)
				AND (@LoginStartTime IS NULL OR LoginTime >= @LoginStartTime)
				AND (@LoginEndTime IS NULL OR LoginTime <= @LoginEndTime)
				AND (@LogoutStartTime IS NULL OR LogoutTime >= @LogoutStartTime)
				AND (@LogoutEndTime IS NULL OR LogoutTime <= @LogoutEndTime)
				AND (@GameID IS NULL OR GameID = @GameID)
				AND (@AreaID IS NULL OR AreaID = @AreaID)
		) AS A
	WHERE
		rowNum BETWEEN @StartRow AND @EndRow
	-------------------------------------------------------------------------------------------

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_GetGameLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_GetGameLog] TO [xbuser]
    AS [dbo];

