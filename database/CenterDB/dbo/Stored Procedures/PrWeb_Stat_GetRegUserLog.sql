-- =============================================
-- Author:  alan
-- Create date: 2019/03/27
-- Description:	註冊帐号统计
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetRegUserLog]
@intAdminID		int,
@intPlatformID  int,
@dtStartTime	datetime=null,
@dtEndTime		datetime=null
AS
BEGIN


	--平台篩選
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT)
	DECLARE @intOPUserType int

	select @intOPUserType=UserType from AdminSys.Admin.Users with (nolock) where UserId=@intAdminID

	--撈出該用戶可讀取的全部平台
	IF @intPlatformID=0
		INSERT INTO @Temp_PlatformID Select PlatformId From CenterDB.Area.PlatformAdminUser where AdminID=@intAdminID
	else
		--針對某渠道篩選
		INSERT INTO @Temp_PlatformID Select PlatformId From CenterDB.Area.PlatformAdminUser where AdminID=@intAdminID and PlatformID=@intPlatformID

	--加總
	if @intPlatformID=0

		SELECT
		CASE WHEN @intOPUserType=10  -- 聯運
		THEN 0 
		ELSE SUM(L.TotalRegCount) END as TotalRegCount,

		CASE WHEN @intOPUserType=10  -- 聯運
		THEN 0 
		ELSE SUM(L.IOSRegCount) END as IOSRegCount,
		SUM(L.AndroidRegCount) as AndroidRegCount,
		L.StatTime,
		0 as PlatformId,
		'全部' as PlatformName --P.Name as PlatformName
		from  Stat.RegUserLog L with (nolock) 
		left join CenterDB.Area.Platform P with (nolock) on L.PlatformID=P.PlatformID
		where 
		L.PlatformID in (select PlatformId from @Temp_PlatformID)
		AND (@dtStartTime IS NULL OR StatTime >= @dtStartTime)
		AND (@dtEndTime IS NULL OR StatTime <= @dtEndTime)
		AND AreaID=0 --畫面沒有篩選渠道
		GROUP BY Stattime
		ORDER BY Stattime DESC
	
	--個別平台
	else
		SELECT
		CASE WHEN @intOPUserType=10  -- 聯運
		THEN 0 
		ELSE L.TotalRegCount END as TotalRegCount,

		CASE WHEN @intOPUserType=10  -- 聯運
		THEN 0 
		ELSE L.IOSRegCount END as IOSRegCount,

		L.AndroidRegCount as AndroidRegCount,
		L.StatTime,
		L.PlatformID,
		P.Name as PlatformName
		from  Stat.RegUserLog L with (nolock) 
		left join CenterDB.Area.Platform P with (nolock) on L.PlatformID=P.PlatformID
		where 
		L.PlatformID in (select PlatformId from @Temp_PlatformID)
		AND (@dtStartTime IS NULL OR StatTime >= @dtStartTime)
		AND (@dtEndTime IS NULL OR StatTime <= @dtEndTime)
		AND AreaID=0 --畫面沒有篩選渠道
		ORDER BY Stattime DESC
END


----------------------------------------

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetRegUserLog] TO [GameUser]
    AS [dbo];

