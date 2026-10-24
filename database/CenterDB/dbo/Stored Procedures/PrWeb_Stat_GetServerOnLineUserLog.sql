-- =============================================
-- Author:  alan
-- Create date: 2019/03/27
-- Description:	房間在線人數明細
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetServerOnLineUserLog]
@intAdminID		int,
@intPlatformID  int,
@intGameID		int=null,
@intServerID	int=null,
@dtStartTime	datetime=null,
@dtEndTime		datetime=null
AS
BEGIN


	declare @starttime datetime = isnull(@dtStartTime,'2000-01-01')
	declare @endtime datetime = isnull(@dtEndTime,'2100-01-01')


	--平台篩選
	CREATE TABLE #Temp_PlatformID (PlatformID	INT PRIMARY KEY)
	DECLARE @intOPUserType int

	select @intOPUserType=UserType from AdminSys.Admin.Users with (nolock) where UserId=@intAdminID


	--撈出該用戶可讀取的全部平台
	IF @intPlatformID=0
		INSERT INTO #Temp_PlatformID Select PlatformId From CenterDB.Area.PlatformAdminUser where AdminID=@intAdminID
	else
		--針對某渠道篩選
		INSERT INTO #Temp_PlatformID Select PlatformId From CenterDB.Area.PlatformAdminUser where AdminID=@intAdminID and PlatformID=@intPlatformID
	

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
		from  [Stat].ServerOnLineUserLog L with (nolock) 
		left join CenterDB.ServerInfo.GameServerList S on L.ServerID = S.ServerID
		left join CenterDB.Area.Platform P with (nolock) on L.PlatformID=P.PlatformID
		left join CenterDB.ServerInfo.GameList G with (nolock) on G.GameID = S.GameID
		where
		StatTime >= @starttime AND StatTime < @endtime
		AND L.PlatformID in (select PlatformId from #Temp_PlatformID)		
		AND (@intServerID IS NULL OR L.ServerID = @intServerID)
		GROUP BY Stattime,L.ServerID,S.ServerName,G.GameName
		ORDER BY Stattime DESC,L.ServerID ASC
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
		L.StatTime, 
		L.PlatformID,
		P.Name as PlatformName,
		L.ServerID,
		S.ServerName,
		G.GameName
		from  [Stat].ServerOnLineUserLog L with (nolock) 
		left join CenterDB.ServerInfo.GameServerList S on L.ServerID = S.ServerID
		left join CenterDB.Area.Platform P  with (nolock) on L.PlatformID=P.PlatformID
		left join CenterDB.ServerInfo.GameList G with (nolock) on G.GameID = S.GameID
		where 
		StatTime >= @starttime AND StatTime < @endtime
		AND L.PlatformID in (select PlatformId from #Temp_PlatformID)
		AND (@intServerID IS NULL OR L.ServerID = @intServerID)
		ORDER BY Stattime DESC,G.GameName ASC, L.ServerID ASC
END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetServerOnLineUserLog] TO [GameUser]
    AS [dbo];

