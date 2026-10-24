-- =============================================
-- Author:  Jason
-- Create date: 2018/05/28
-- Description:	回傳用戶注冊綁定紀錄 #349
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetBindDetailLog]
(
	@dtStatTime		DATETIME = NULL,		
	@dtEndTime		DATETIME = NULL,
	@LogonBind		INT = 0			OUTPUT
)
AS
BEGIN
	
	SET NOCOUNT ON

	DECLARE @StatTime		DATETIME = @dtStatTime		
	DECLARE @EndTime		DATETIME = @dtEndTime
	DECLARE @LogStartTime	DATETIME = DATEADD(DAY,-1,DATEDIFF(DAY,0,GETDATE()))
	DECLARE @LogEndTime		DATETIME = DATEADD(DAY,0,DATEDIFF(DAY,0,GETDATE()))

	SELECT	@LogonBind = COUNT(DISTINCT A.UserID)
	FROM [Users].[UserBindLog] AS A WITH (NOLOCK)
		INNER JOIN [Users].[UserLogonLog] AS B ON A.UserID = B.UserID
	WHERE
		A.[CreateTime] >= @LogStartTime
		AND A.[CreateTime] < @LogEndTime


	/*  回傳紀錄  */
	------------------------------------------------------
	SELECT	DISTINCT A.UserID,
			RegTime = DATEADD(DAY,0,DATEDIFF(DAY,0,B.CreateTime)),
			BindTime = DATEADD(DAY,0,DATEDIFF(DAY,0,C.CreateTime))
	FROM (
			SELECT DISTINCT UserID
			FROM [Users].[UserRegLog] AS A WITH (NOLOCK)
			WHERE
				(@StatTime IS NULL OR A.[CreateTime] >= @StatTime)
				AND (@EndTime IS NULL OR A.[CreateTime] < @EndTime)
			UNION
			SELECT DISTINCT UserID
			FROM [Users].[UserBindLog] AS A WITH (NOLOCK)
			WHERE
				(@StatTime IS NULL OR A.[CreateTime] >= @StatTime)
				AND (@EndTime IS NULL OR A.[CreateTime] < @EndTime)
		) AS A
		LEFT JOIN [Users].[UserRegLog] AS B WITH (NOLOCK) ON A.UserID = B.UserID
		LEFT JOIN [Users].[UserBindLog] AS C WITH (NOLOCK) ON A.UserID = C.UserID
	------------------------------------------------------


END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetBindDetailLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetBindDetailLog] TO [xbuser]
    AS [dbo];

