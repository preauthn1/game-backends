-- =============================================
-- Author:  Jason
-- Create date: 2018/05/28
-- Description:	回傳渠道注冊用戶人數 每日報表 後台第三方帳號使用 #526
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetRegCountByArea]
(
	@dtStatTime		DATETIME = NULL,		
	@dtEndTime		DATETIME = NULL
)
AS
BEGIN
	
	SET NOCOUNT ON

	DECLARE @StatTime		DATETIME = @dtStatTime		
	DECLARE @EndTime		DATETIME = @dtEndTime

	declare @newip int
	select @newip = count(1)
	from [Users].[UserRegLog] as a with (nolock)
	where
		(@StatTime IS NULL OR a.[CreateTime] >= @StatTime)
		AND (@EndTime IS NULL OR a.[CreateTime] < @EndTime)
		and ipaddress not in (select distinct ipaddress from [Users].[UserRegLog] with (nolock) where createtime < @StatTime)


	/*  回傳紀錄  */
	------------------------------------------------------
	SELECT	StatTime = DATEADD(DAY,0,DATEDIFF(DAY,0,a.[CreateTime])),
			a.[AreaID],
			[TotalCount] = count(distinct a.[UserID]),
			[iOS] = count(case when a.[HDType] = 3 then a.[HDType] else null end),
			[Android] = count(case when a.[HDType] = 2 then a.[HDType] else null end),
			[iOSBind] = count(case when a.[HDType] = 3 and c.userid is null then null else c.userid end),
			[AndroidBind] = count(case when a.[HDType] = 2 and c.userid is null then null else c.userid end),
			[IPCount] = count(distinct a.[IPAddress]),
			[NewIPCount] = @newip
	FROM [Users].[UserRegLog] as a WITH (NOLOCK)
		left join [Users].[UserBindLog] as c with (nolock) on a.userid = c.userid
	WHERE
		(@StatTime IS NULL OR a.[CreateTime] >= @StatTime)
		AND (@EndTime IS NULL OR a.[CreateTime] < @EndTime)
	GROUP BY DATEADD(DAY,0,DATEDIFF(DAY,0,a.[CreateTime])),a.[AreaID]
	------------------------------------------------------


END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetRegCountByArea] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetRegCountByArea] TO [xbuser]
    AS [dbo];

