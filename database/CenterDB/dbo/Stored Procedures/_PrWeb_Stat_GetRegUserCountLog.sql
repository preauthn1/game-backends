-- =============================================
-- Author:  Jason
-- Create date: 2018/04/02
-- Description:	統計分析-用戶統計-註冊帳號統計
-- =============================================
CREATE PROCEDURE [dbo].[_PrWeb_Stat_GetRegUserCountLog]
(
	@intPlatformID	INT = NULL,
	@intAreaID		INT = NULL,
	@dtStatTime		DATETIME = NULL,
	@dtEndTime		DATETIME = NULL
)
AS
BEGIN
SET NOCOUNT ON

	DECLARE @PlatformID INT = @intPlatformID
	DECLARE @AreaID		INT = @intAreaID
	DECLARE @LogStatTime DATETIME = @dtStatTime
	DECLARE @LogEndTijme DATETIME = @dtEndTime


	/*  更新資料  */
	-------------------------------------------------------------------------------------------
	SELECT	[PlatformID],
			[AreaID],
			[StatTime],
			[TotalRegCount],
			[IOSRegCount],
			[AndroidRegCount]
	FROM [Stat].[RegUserLog] WITH (NOLOCK)
	WHERE
		(@PlatformID IS NULL OR @PlatformID = [PlatformID])
		AND (@AreaID IS NULL OR @AreaID = [AreaID])
		AND (@LogStatTime IS NULL OR [StatTime] >= @LogStatTime)
		AND (@PlatformID IS NULL OR [StatTime] <= @LogEndTijme)
	-------------------------------------------------------------------------------------------

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[_PrWeb_Stat_GetRegUserCountLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[_PrWeb_Stat_GetRegUserCountLog] TO [xbuser]
    AS [dbo];

