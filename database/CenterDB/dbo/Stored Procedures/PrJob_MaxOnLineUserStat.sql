-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<房间最高在线人数统计>
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_MaxOnLineUserStat]
AS
SET NOCOUNT ON

--大厅最高在线
INSERT INTO Stat.DayMaxHallOnLineUserLog
        ( PlatformID,
		  TotalCount ,
          PCCount ,
          IOSCount ,
          AndroidCount ,
          StatTime
        )
SELECT P.PlatformID,S.TotalCount,PCCount,IOSCount,AndroidCount,StatTime
FROM (select PlatformID from Area.[Platform] union all select 0 PlatformID) as P 
cross apply (
	SELECT TOP 1 PlatformID,TotalCount,PCCount,IOSCount,AndroidCount,StatTime
	FROM Stat.HallOnLineUserLog
	WHERE CONVERT(VARCHAR(10), DATEADD(DAY, -1, GETDATE()), 120) = CONVERT(VARCHAR(10), StatTime, 120)
	and PlatformID = P.PlatformID
	ORDER BY TotalCount DESC
) as S



--总游戏最高在线
INSERT INTO Stat.DayMaxTotalGameOnLineLog
        ( PlatformID,
		  TotalCount ,
          PCCount ,
          IOSCount ,
          AndroidCount ,
          StatTime
        )
SELECT P.PlatformID,S.TotalCount,PCCount,IOSCount,AndroidCount,StatTime
FROM (select PlatformID from Area.[Platform] union all select 0 PlatformID) as P 
cross apply (
	SELECT TOP 1 PlatformID,TotalCount,PCCount,IOSCount,AndroidCount,StatTime
	FROM Stat.TotalGameOnLineLog
	WHERE CONVERT(VARCHAR(10), DATEADD(DAY, -1, GETDATE()), 120) = CONVERT(VARCHAR(10), StatTime, 120)
	and PlatformID = P.PlatformID
	ORDER BY TotalCount DESC
) as S



--房间最高在线
DECLARE @intServerID INT
DECLARE cur_servermax CURSOR FAST_FORWARD READ_ONLY FOR
SELECT ServerID
FROM ServerInfo.GameServerList

OPEN cur_servermax
FETCH NEXT FROM cur_servermax INTO @intServerID
WHILE @@FETCH_STATUS = 0
BEGIN		
	INSERT INTO Stat.DayMaxServerOnLineUserLog
	        ( P.PlatformID,
			  ServerID ,
	          TotalCount ,
	          PCCount ,
	          IOSCount ,
	          AndroidCount ,
	          StatTime
	        )
	SELECT P.PlatformID,S.ServerID, S.TotalCount,PCCount,IOSCount,AndroidCount,StatTime
	FROM (select PlatformID from Area.[Platform] union all select 0 PlatformID) as P 
	cross apply (
		SELECT TOP 1 ServerID,PlatformID,TotalCount,PCCount,IOSCount,AndroidCount,StatTime
		FROM Stat.ServerOnLineUserLog
		WHERE CONVERT(VARCHAR(10), DATEADD(DAY, -1, GETDATE()), 120) = CONVERT(VARCHAR(10), StatTime, 120)
		and PlatformID = P.PlatformID
		and ServerID = @intServerID 
		ORDER BY TotalCount DESC
	) as S		
	
	FETCH NEXT FROM cur_servermax INTO @intServerID
END

CLOSE cur_servermax
DEALLOCATE cur_servermax


--游戏最高在线
DECLARE @intGameID int
DECLARE cur_gamemax CURSOR FAST_FORWARD READ_ONLY FOR 
SELECT GameID FROM ServerInfo.GameList

OPEN cur_gamemax
FETCH NEXT FROM cur_gamemax INTO @intGameID

WHILE @@FETCH_STATUS = 0
BEGIN
	
	INSERT INTO Stat.DayMaxGameOnLineUserLog
	        ( PlatformID,
			  GameID ,
	          TotalCount ,
	          PCCount ,
	          IOSCount ,
	          AndroidCount ,
	          StatTime
	        )
	SELECT P.PlatformID,S.GameID, S.TotalCount,PCCount,IOSCount,AndroidCount,StatTime
	FROM (select PlatformID from Area.[Platform] union all select 0 PlatformID) as P 
	cross apply (
		SELECT TOP 1 GameID,PlatformID,TotalCount,PCCount,IOSCount,AndroidCount,StatTime
		FROM Stat.GameOnLineUserLog
		WHERE CONVERT(VARCHAR(10), DATEADD(DAY, -1, GETDATE()), 120) = CONVERT(VARCHAR(10), StatTime, 120)
		and PlatformID = P.PlatformID
		and GameID = @intGameID
		ORDER BY TotalCount DESC
	) as S
	
	FETCH NEXT FROM cur_gamemax INTO @intGameID	
END

CLOSE cur_gamemax
DEALLOCATE cur_gamemax

RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_MaxOnLineUserStat] TO [xbuser]
    AS [dbo];

