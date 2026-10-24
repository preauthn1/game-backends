-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<统计用户查询时间范围的游戏统计信息>
--	2018/03/21	Jason		調整查詢語法改為LINKSERVER
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetUserGameInfo]
	@intUserID		INT,				--用户ID
	@dtBeginTime	DATETIME = NULL,	--开始时间范围
	@dtEndTime		DATETIME = NULL		--结束时间范围
AS
BEGIN
	
	SET NOCOUNT ON

	/*  取資料及回傳  */
	-------------------------------------------------------------------------------------------------
	CREATE TABLE #Temp_UserGameLog (ServerID INT,WinCount INT,LoseCount INT,DrawCount INT,Score BIGINT,GameTime INT)

	INSERT INTO #Temp_UserGameLog
	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrPs_GetUserGameLog]
		@intUserID = @intUserID,
		@dtStatTime  = @dtBeginTime,
		@dtEndTime	= @dtEndTime

	SELECT	G.GameID,
			G.GameName AS 'Game.GameName',
			ISNULL(SUM(L.WinCount),0) AS WinCount,
			ISNULL(SUM(L.LoseCount),0) AS LoseCount,
			ISNULL(SUM(L.DrawCount),0) AS DrawCount,
			ISNULL(SUM(L.Score),0) AS Score,
			ISNULL(SUM(L.GameTime),0) AS GameTime
	FROM #Temp_UserGameLog AS L
		INNER JOIN ServerInfo.GameServerList AS GL WITH (NOLOCK) ON L.ServerID = GL.ServerID
		INNER JOIN ServerInfo.GameList AS G WITH (NOLOCK) ON G.GameID = GL.GameID
	GROUP BY G.GameID,G.GameName
	-------------------------------------------------------------------------------------------------
	
	DROP TABLE #Temp_UserGameLog

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetUserGameInfo] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetUserGameInfo] TO [xbuser]
    AS [dbo];

