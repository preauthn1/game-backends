-- =============================================
-- Author:  Jason
-- Create date: 2018/05/28
-- Description:	回傳用戶綁定統計紀錄 #349
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetBindLog]
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
	SELECT	[StatTime],[PlatformID],[AreaID],[TotalBindCount],[SameDayReg],[iOSCount],[AndRoidCount],[PCCount]
	FROM [Stat].[StatisticsUserBind] WITH (NOLOCK)
	WHERE
		(@StatTime IS NULL OR [StatTime] >= @StatTime)
		AND (@EndTime IS NULL OR [StatTime] < @EndTime)
	------------------------------------------------------


END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetBindLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetBindLog] TO [xbuser]
    AS [dbo];

