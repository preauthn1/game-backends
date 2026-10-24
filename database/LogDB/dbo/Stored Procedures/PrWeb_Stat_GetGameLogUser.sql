-- =============================================
-- Author:  Jason
-- Create date: 2018/05/29
-- Description:	回傳用戶登入遊戲紀錄 #351
-- =============================================
create PROCEDURE [dbo].[PrWeb_Stat_GetGameLogUser]
(
	@intPlatformID	INT,
	@intAreaID		INT,
	@dtStatTime		DATETIME
)
AS
BEGIN
	
	SET NOCOUNT ON

	DECLARE @PlatformID		INT = @intPlatformID
	DECLARE @AreaID			INT = @intAreaID
	DECLARE @StatTime		DATETIME = DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
	DECLARE @EndTime		DATETIME = DATEADD(DAY,1,DATEDIFF(DAY,0,@dtStatTime))



	/*  回傳紀錄  */
	------------------------------------------------------
	SELECT	DISTINCT UserID
	FROM [Game].[GameLog] WITH (NOLOCK)
	WHERE
		[PlatformID] = @PlatformID
		AND [AreaID] = @AreaID
		AND [LogoutTime] >= @StatTime
		AND [LogoutTime] < @EndTime
	------------------------------------------------------


END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetGameLogUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetGameLogUser] TO [xbuser]
    AS [dbo];

