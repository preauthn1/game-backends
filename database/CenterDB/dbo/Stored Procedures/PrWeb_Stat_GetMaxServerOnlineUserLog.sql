-- =============================================
-- Author:  alan
-- Create date: 2019/03/27
-- Description:	房間最高在線人數
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetMaxServerOnlineUserLog]
@intAdminID		int,
@intPlatformID  int,
@intGameID		int=null,
@intServerID	int=null,
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
		ELSE SUM(L.TotalCount) END as TotalCount,

		CASE WHEN @intOPUserType=10  -- 聯運
		THEN 0 
		ELSE SUM(L.IOSCount) END as IOSCount,

		CASE WHEN @intOPUserType=10  -- 聯運
		THEN 0 
		ELSE SUM(L.AndroidCount) END as AndroidCount,

		L.StatTime, 
		0 as PlatformId,
		'全部' as  PlatformName,
		L.ServerID,
		S.ServerName,
		G.GameName
		from  [Stat].DayMaxServerOnLineUserLog L with (nolock) 
		left join CenterDB.ServerInfo.GameServerList S on L.ServerID = S.ServerID
		left join CenterDB.Area.Platform P with (nolock) on L.PlatformID=P.PlatformID
		left join CenterDB.ServerInfo.GameList G with (nolock) on G.GameID = S.GameID
		where 
		L.PlatformID in (select PlatformId from @Temp_PlatformID)
		AND (@dtStartTime IS NULL OR StatTime >= @dtStartTime)
		AND (@dtEndTime IS NULL OR StatTime <= @dtEndTime)
		AND (@intServerID IS NULL OR L.ServerID = @intServerID)
		GROUP BY Stattime,L.ServerID,S.ServerName,G.GameName
		ORDER BY Stattime DESC,G.GameName ASC, L.ServerID ASC
	--個別
	else
		SELECT
		CASE WHEN @intOPUserType=10  -- 聯運
		THEN 0 
		ELSE L.TotalCount END as TotalCount,

		CASE WHEN @intOPUserType=10  -- 聯運
		THEN 0 
		ELSE L.IOSCount END as IOSCount,

		CASE WHEN @intOPUserType=10  -- 聯運
		THEN 0 
		ELSE L.AndroidCount END as AndroidCount,

		L.PlatformID,
		P.Name as PlatformName,
		S.ServerName,
		G.GameName
		from  [Stat].DayMaxServerOnLineUserLog L with (nolock) 
		left join CenterDB.ServerInfo.GameServerList S on L.ServerID = S.ServerID
		left join CenterDB.Area.Platform P with (nolock) on L.PlatformID=P.PlatformID
		left join CenterDB.ServerInfo.GameList G with (nolock) on G.GameID = S.GameID
		where 
		L.PlatformID in (select PlatformId from @Temp_PlatformID)
		AND (@dtStartTime IS NULL OR StatTime >= @dtStartTime)
		AND (@dtEndTime IS NULL OR StatTime <= @dtEndTime)
		AND (@intServerID IS NULL OR L.ServerID = @intServerID)
		ORDER BY Stattime DESC,G.GameName ASC, L.ServerID ASC
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetMaxServerOnlineUserLog] TO [GameUser]
    AS [dbo];

