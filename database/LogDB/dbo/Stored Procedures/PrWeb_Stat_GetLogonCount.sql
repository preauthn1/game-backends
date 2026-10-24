-- =============================================
-- Author:  Jason
-- Create date: 2018/05/28
-- Description:	回傳用戶登陸人次 #349
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetLogonCount]
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
	SELECT	StatTime = DATEADD(DAY,0,DATEDIFF(DAY,0,[CreateTime])),
			TotalCount = COUNT([UserID])
	FROM [Users].[UserLogonLog] WITH (NOLOCK)
	WHERE
		(@StatTime IS NULL OR [CreateTime] >= @StatTime)
		AND (@EndTime IS NULL OR [CreateTime] < @EndTime)
	GROUP BY DATEADD(DAY,0,DATEDIFF(DAY,0,[CreateTime]))
	------------------------------------------------------


END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetLogonCount] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetLogonCount] TO [xbuser]
    AS [dbo];

