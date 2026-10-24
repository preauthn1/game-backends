-- =============================================
-- Author:  Jason
-- Create date: 2018/06/27
-- Description:	回傳遊戲次數 每日報表 後台第三方帳號使用 #526
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetGameCountByArea]
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
	SELECT	StatTime = DATEADD(DAY,0,DATEDIFF(DAY,0,a.[LogoutTime])),
			a.[AreaID],
			a.gameid,
			[Gamecount] = count(1)
	FROM [Game].[GameLog] as a WITH (NOLOCK)
	WHERE
		(@StatTime IS NULL OR a.[LogoutTime] >= @StatTime)
		AND (@EndTime IS NULL OR a.[LogoutTime] < @EndTime)
	GROUP BY DATEADD(DAY,0,DATEDIFF(DAY,0,a.[LogoutTime])),a.[AreaID],a.gameid
	------------------------------------------------------


END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetGameCountByArea] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetGameCountByArea] TO [xbuser]
    AS [dbo];

