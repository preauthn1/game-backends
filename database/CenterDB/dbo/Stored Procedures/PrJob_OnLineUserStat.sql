-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<统计在线人数>
--	2018/03/20	Jason		調整寫法
--	2018/07/17	jason		調整robot判斷 從robotlist改users.userinfo.usertype
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_OnLineUserStat]
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @dtDate DATETIME
	SET @dtDate = GETDATE()

	/*  寫入空紀錄  */
	------------------------------------------------------------------------------------------------
	INSERT INTO stat.HallOnLineUserLog(PlatformID, TotalCount ,PCCount ,IOSCount ,AndroidCount ,StatTime)
	SELECT PlatformID,0,0,0,0,@dtDate 
	FROM Area.[Platform]  as P
	------------------------------------------------------------------------------------------------


	/*  --各平台大厅在线  */
	------------------------------------------------------------------------------------------------
	--UPDATE L
	--SET L.TotalCount = S.TotalCount,
	--L.PCCount= S.PCCount,
	--L.IOSCount= S.IOSCount,
	--L.AndroidCount= S.AndroidCount
	--FROM stat.HallOnLineUserLog  as L
	--INNER JOIN (
	--	SELECT	A.PlatformID,
	--			COUNT(1) as TotalCount,
	--			ISNULL(SUM(case when LL.HDType = 1 THEN 1 ELSE 0 END),0) as PCCount,
	--			ISNULL(SUM(case when LL.HDType = 3 THEN 1 ELSE 0 END),0) as IOSCount,
	--			ISNULL(SUM(case when LL.HDType = 2 THEN 1 ELSE 0 END),0) as AndroidCount			
	--	FROM Game.HallUserList as LL
	--	inner join Users.UserInfo as U with(nolock) ON LL.UserID = U.UserID
	--	inner join Area.Area as A ON U.AreaID=A.AreaID
	--	WHERE LL.UserID NOT IN(select UserID from Robot.RobotList)
	--	GROUP BY PlatformID
	--) as S ON L.PlatformID=S.PlatformID
	--WHERE StatTime = @dtDate

	DECLARE @PlatformCount	TABLE (PlatformID INT,TotalCount INT,PCCount INT,IOSCount INT,AndroidCount INT)

	INSERT INTO @PlatformCount
	SELECT	A.PlatformID,
			COUNT(1) AS TotalCount,
			ISNULL(SUM(CASE WHEN LL.HDType = 1 THEN 1 ELSE 0 END),0) AS PCCount,
			ISNULL(SUM(CASE WHEN LL.HDType = 3 THEN 1 ELSE 0 END),0) AS IOSCount,
			ISNULL(SUM(CASE WHEN LL.HDType = 2 THEN 1 ELSE 0 END),0) AS AndroidCount			
	FROM Game.HallUserList AS LL WITH (NOLOCK)
		INNER JOIN Users.UserInfo AS U WITH (NOLOCK) ON LL.UserID = U.UserID AND U.UserType <> 2
		INNER JOIN Area.Area AS A WITH (NOLOCK) ON U.AreaID=A.AreaID
	GROUP BY PlatformID

	UPDATE L
	SET L.TotalCount = S.TotalCount,
	L.PCCount= S.PCCount,
	L.IOSCount= S.IOSCount,
	L.AndroidCount= S.AndroidCount
	FROM stat.HallOnLineUserLog AS L
		INNER JOIN @PlatformCount AS S ON L.PlatformID=S.PlatformID
	WHERE
		StatTime = @dtDate
	
	
	--所有平台
	INSERT INTO stat.HallOnLineUserLog(PlatformID, TotalCount ,PCCount ,IOSCount ,AndroidCount ,StatTime)
	SELECT 0,SUM(TotalCount),SUM(PCCount),SUM(IOSCount),SUM(AndroidCount),@dtDate
	FROM  stat.HallOnLineUserLog WITH (NOLOCK)
	WHERE
		StatTime = @dtDate



	--游戏房间在线统计
	INSERT INTO stat.ServerOnLineUserLog(PlatformID, ServerID,TotalCount ,PCCount ,IOSCount ,AndroidCount ,StatTime)
	SELECT PlatformID,ServerID,0,0,0,0,@dtDate 
	FROM Area.[Platform] AS P WITH (NOLOCK)
		LEFT JOIN ServerInfo.GameServerList AS GS WITH (NOLOCK) ON 1=1

	DECLARE @PlatformServerCount	TABLE (PlatformID INT,ServerID INT,TotalCount INT,PCCount INT,IOSCount INT,AndroidCount INT)

	INSERT INTO @PlatformServerCount
	SELECT	A.PlatformID,
			ServerID,
			COUNT(1) AS TotalCount,
			ISNULL(SUM(CASE WHEN LL.HDType = 1 THEN 1 ELSE 0 END),0) AS PCCount,
			ISNULL(SUM(CASE WHEN LL.HDType = 3 THEN 1 ELSE 0 END),0) AS IOSCount,
			ISNULL(SUM(CASE WHEN LL.HDType = 2 THEN 1 ELSE 0 END),0) AS AndroidCount			
	FROM Game.GameUserList AS LL WITH (NOLOCK)
		INNER JOIN Users.UserInfo AS U WITH (NOLOCK) ON LL.UserID = U.UserID AND U.UserType <> 2
		INNER JOIN Area.Area AS A WITH (NOLOCK) ON U.AreaID=A.AreaID
	GROUP BY PlatformID,ServerID

	--UPDATE L
	--SET L.TotalCount = S.TotalCount,
	--L.PCCount= S.PCCount,
	--L.IOSCount= S.IOSCount,
	--L.AndroidCount= S.AndroidCount
	--FROM stat.ServerOnLineUserLog  as L
	--INNER JOIN (
	--	SELECT	A.PlatformID,
	--			ServerID,
	--			COUNT(1) as TotalCount,
	--			ISNULL(SUM(case when LL.HDType = 1 THEN 1 ELSE 0 END),0) as PCCount,
	--			ISNULL(SUM(case when LL.HDType = 3 THEN 1 ELSE 0 END),0) as IOSCount,
	--			ISNULL(SUM(case when LL.HDType = 2 THEN 1 ELSE 0 END),0) as AndroidCount			
	--	FROM Game.GameUserList as LL
	--	INNER JOIN Users.UserInfo as U with(nolock) ON LL.UserID = U.UserID
	--	INNER JOIN Area.Area as A ON U.AreaID=A.AreaID
	--	WHERE LL.UserID NOT IN(select UserID from Robot.RobotList)
	--	GROUP BY PlatformID,ServerID
	--) as S ON L.PlatformID=S.PlatformID and L.ServerID = S.ServerID
	--WHERE StatTime = @dtDate

	UPDATE L
	SET L.TotalCount = S.TotalCount,
		L.PCCount= S.PCCount,
		L.IOSCount= S.IOSCount,
		L.AndroidCount= S.AndroidCount
	FROM stat.ServerOnLineUserLog AS L
		INNER JOIN @PlatformServerCount AS S ON L.PlatformID=S.PlatformID AND L.ServerID = S.ServerID
	WHERE
		StatTime = @dtDate


	--所有平台
	INSERT INTO stat.ServerOnLineUserLog(PlatformID,ServerID, TotalCount ,PCCount ,IOSCount ,AndroidCount ,StatTime)
	SELECT 0,ServerID,SUM(TotalCount),SUM(PCCount),SUM(IOSCount),SUM(AndroidCount),@dtDate
	FROM  stat.ServerOnLineUserLog WITH (NOLOCK)
	WHERE StatTime = @dtDate
	GROUP BY ServerID


	--游戏在线统计
	INSERT INTO stat.GameOnLineUserLog(PlatformID, GameID,TotalCount ,PCCount ,IOSCount ,AndroidCount ,StatTime)
	SELECT PlatformID,GameID,0,0,0,0,@dtDate 
	FROM (SELECT PlatformID FROM Area.[Platform] WITH (NOLOCK) UNION ALL SELECT 0 AS PlatformID) AS P
		LEFT JOIN ServerInfo.GameList AS G ON 1=1


	DECLARE @GameCount	TABLE	(PlatformID INT,GameID INT,TotalCount INT,PCCount INT,IOSCount INT,AndroidCount INT)

	INSERT INTO @GameCount
	SELECT	LL.PlatformID,
			GS.GameID,
			SUM(TotalCount) AS TotalCount,
			SUM(PCCount) AS PCCount,
			SUM(IOSCount) AS IOSCount,
			SUM(AndroidCount) AS AndroidCount			
	FROM Stat.ServerOnLineUserLog AS LL WITH (NOLOCK)
		INNER JOIN ServerInfo.GameServerList AS GS WITH (NOLOCK) ON LL.ServerID = GS.ServerID
	WHERE
		LL.StatTime = @dtDate
	GROUP BY LL.PlatformID,GS.GameID

	--UPDATE L
	--SET L.TotalCount = S.TotalCount,
	--L.PCCount= S.PCCount,
	--L.IOSCount= S.IOSCount,
	--L.AndroidCount= S.AndroidCount
	--FROM stat.GameOnLineUserLog as L
	--INNER JOIN (
	--	SELECT	LL.PlatformID,
	--			GS.GameID,
	--			SUM(TotalCount) as TotalCount,
	--			SUM(PCCount) as PCCount,
	--			SUM(IOSCount) as IOSCount,
	--			SUM(AndroidCount) as AndroidCount			
	--	FROM Stat.ServerOnLineUserLog as LL 
	--	INNER JOIN ServerInfo.GameServerList as GS ON LL.ServerID = GS.ServerID
	--	where LL.StatTime = @dtDate
	--	GROUP BY LL.PlatformID,GS.GameID
	--) as S ON L.PlatformID = S.PlatformID AND L.GameID = S.GameID
	--where L.StatTime = @dtDate

	UPDATE L
	SET L.TotalCount = S.TotalCount,
		L.PCCount= S.PCCount,
		L.IOSCount= S.IOSCount,
		L.AndroidCount= S.AndroidCount
	FROM stat.GameOnLineUserLog AS L
		INNER JOIN @GameCount AS S ON L.PlatformID = S.PlatformID AND L.GameID = S.GameID
	WHERE
		L.StatTime = @dtDate


	--统计所有游戏在线人数
	INSERT INTO stat.TotalGameOnLineLog(PlatformID, TotalCount ,PCCount ,IOSCount ,AndroidCount ,StatTime)
	SELECT	L.PlatformID,
			SUM(TotalCount) AS TotalCount,
			SUM(PCCount) AS PCCount,
			SUM(IOSCount) AS IOSCount,
			SUM(AndroidCount) AS AndroidCount,
			@dtDate 
	FROM (
			SELECT PlatformID FROM Area.[Platform] WITH (NOLOCK) UNION ALL SELECT 0 AS PlatformID
		) AS P
		LEFT JOIN stat.GameOnLineUserLog AS L WITH (NOLOCK) ON P.PlatformID = L.PlatformID AND L.StatTime = @dtDate
	WHERE
		L.StatTime = @dtDate
	GROUP BY L.PlatformID


	--统计渠道总在线人数
	DECLARE @AreaCount	TABLE (AreaID INT,TotalCount INT,PCCount INT,IOSCount INT,AndroidCount INT)
	
	INSERT INTO @AreaCount
	SELECT	AreaID,
			COUNT(1) AS TotalCount,
			ISNULL(SUM(CASE WHEN GU.HDType = 1 THEN 1 ELSE 0 END),0) AS PCCount,
			ISNULL(SUM(CASE WHEN GU.HDType = 3 THEN 1 ELSE 0 END),0) AS IOSCount,
			ISNULL(SUM(CASE WHEN GU.HDType = 2 THEN 1 ELSE 0 END),0) AS AndroidCount			
	FROM Game.GameUserList AS GU WITH (NOLOCK)
		LEFT JOIN Users.UserInfo AS U WITH (NOLOCK) ON GU.UserID = U.UserID AND U.UserType <> 2
	GROUP BY AreaID
	
	INSERT INTO Stat.TotalGameOnLineLogArea(AreaID,TotalCount ,PCCount ,IOSCount ,AndroidCount ,StatTime)
	SELECT A.AreaID,ISNULL(S.TotalCount,0),ISNULL(S.PCCount,0),ISNULL(S.IOSCount,0),ISNULL(S.AndroidCount,0),@dtDate
	FROM Area.Area AS A WITH (NOLOCK)
		LEFT JOIN @AreaCount AS S ON A.AreaID = S.AreaID


	--统计机器人总游戏在线人数
	INSERT INTO Stat.TotalGameOnLineRobotLog(TotalCount,StatTime)
	SELECT COUNT(1),@dtDate
	FROM Game.GameUserList WITH (NOLOCK)

	--统计机器人游戏在线人数
	INSERT INTO Stat.GameOnLineRobotLog(GameID,TotalCount,StatTime)
	SELECT GameID,COUNT(1),@dtDate
	FROM Game.GameUserList AS GU WITH (NOLOCK)
		INNER JOIN Users.UserInfo AS U WITH (NOLOCK) ON GU.UserID = U.UserID AND U.UserType = 2
		LEFT JOIN ServerInfo.GameServerList AS GS WITH (NOLOCK) ON GU.ServerID = GS.ServerID
	GROUP BY GameID

	--统计机器人游戏房间在线人数
	INSERT INTO Stat.ServerOnLineRobotLog(ServerID,TotalCount,StatTime)
	SELECT ServerID,COUNT(1),@dtDate
	FROM Game.GameUserList AS GU WITH (NOLOCK)
	GROUP BY ServerID
	------------------------------------------------------------------------------------------------

	RETURN 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_OnLineUserStat] TO [xbuser]
    AS [dbo];

