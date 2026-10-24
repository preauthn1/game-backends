-- =============================================
-- Author:  Jason
-- Create date: 2018/05/28
-- Description:	回傳渠道用戶登陸人次 每日報表 後台第三方帳號使用 #526
-- =============================================
CREATe PROCEDURE [dbo].[PrWeb_Stat_GetLogonCountByArea]
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
	SELECT	StatTime = DATEADD(DAY,0,DATEDIFF(DAY,0,a.[CreateTime])),
			a.AreaID,
			[TotalCount] = count(distinct a.[UserID]),
			[iOS] = count(case when a.[HDType] = 3 then a.[HDType] else null end),
			[Android] = count(case when a.[HDType] = 2 then a.[HDType] else null end),
			[IsBind] = count(case when c.userid is null then null else c.userid end),
			[IsNew] = count(case when b.createtime < @StatTime then null else b.userid end),
			[IPCount] = count(distinct a.[IPAddress])
	FROM [Users].[UserLogonLog] as a WITH (NOLOCK)
		inner join [Users].[UserRegLog] as b with (nolock) on a.userid = b.userid
		left join [Users].[UserBindLog] as c with (nolock) on a.userid = c.userid
	WHERE
		(@StatTime IS NULL OR a.[CreateTime] >= @StatTime)
		AND (@EndTime IS NULL OR a.[CreateTime] < @EndTime)
	GROUP BY DATEADD(DAY,0,DATEDIFF(DAY,0,a.[CreateTime])),a.areaid
	------------------------------------------------------


END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetLogonCountByArea] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetLogonCountByArea] TO [xbuser]
    AS [dbo];

