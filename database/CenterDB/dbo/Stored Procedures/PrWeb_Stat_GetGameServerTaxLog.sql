-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<统计游戏税收记录>
--	2018/03/19	Jason	調整寫法改用變數接參數
--	2019/03/27	Jason		新增PlatformID AreaID 對應 寫到新的[Stat].[StatisticsMoneySourceLog_New]
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetGameServerTaxLog]
	@intAdminID		int,
	@intPlatformID	INT=NULL,
	@intAreaID		INT=NULL,
	@intGameID		INT=NULL,			--游戏ID	
	@intServerID	INT=NULL,			--服务器ID
	@dtBeginTime	DATETIME=NULL,		--开始时间范围
	@dtEndTime		DATETIME=NULL		--结束时间范围
AS
BEGIN	
	SET NOCOUNT ON

	--平台篩選
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT)
	INSERT INTO @Temp_PlatformID Select PlatformID From CenterDB.Area.PlatformAdminUser where AdminID=@intAdminID

		DECLARE @GameID		INT = @intGameID
		DECLARE @ServerID	INT = @intServerID
		DECLARE @BeginTime	DATETIME = ISNULL(@dtBeginTime,'1900-01-01 00:00:00.000')
		DECLARE @EndTime	DATETIME = ISNULL(@dtEndTime,'2100-01-01 00:00:00.000')

	/*  原作法  */
	/*
	--------------------------------------------------------------------------------------------------------------------
	IF(@intPlatformID IS NULL) AND (@intAreaID IS NULL)
	BEGIN
		SELECT				
				Max(G.GameName) AS 'GameName'			
				,Max(S.ServerName) AS 'ServerName'
				,L.StatTime
				,SUM(L.Amount) AS Amount
				,SUM(L.RobotAmount) AS RobotAmount
				,SUM(L.RobotTaxAmount) AS RobotTaxAmount
				,SUM(L.TaxAmount) AS TaxAmount
		FROM Stat.MoneySourceLog AS L WITH (NOLOCK)
			INNER JOIN ServerInfo.GameServerList AS S WITH (NOLOCK) ON L.SourceID = S.ServerID
			LEFT JOIN ServerInfo.GameList AS G WITH (NOLOCK) ON S.GameID = G.GameID		
		WHERE  (@GameID IS NULL OR G.GameID=@GameID)
			AND (@ServerID IS NULL OR S.ServerID=@ServerID)
			AND (@BeginTime IS NULL OR L.StatTime>=@BeginTime)
			AND (@EndTime IS NULL OR L.StatTime<=@EndTime)
		GROUP BY L.StatTime,L.SourceID
		ORDER BY L.StatTime DESC
	END
	--------------------------------------------------------------------------------------------------------------------
	*/

	/*  新作法 必須帶入PlatformID and AreaID  */
	--------------------------------------------------------------------------------------------------------------------
	--IF(@intPlatformID IS NOT NULL AND @intPlatformID > 0) AND (@intAreaID IS NOT NULL AND @intAreaID > 0)
	--BEGIN
		SELECT	--L.PlatformID,
				--L.AreaID,			
				Max(G.GameName) AS 'GameName'			
				,Max(S.ServerName) AS 'ServerName'
				,L.StatTime
				,SUM(L.Amount) AS Amount
				,SUM(L.RobotAmount) AS RobotAmount
				,SUM(L.RobotTaxAmount) AS RobotTaxAmount
				,SUM(L.TaxAmount) AS TaxAmount
				,SUM(ISNULL(L.FlowAmount,0)) AS FlowAmount
		FROM Stat.MoneySourceLog_New AS L WITH (NOLOCK)
			INNER JOIN ServerInfo.GameServerList AS S WITH (NOLOCK) ON L.SourceID = S.ServerID
			LEFT JOIN ServerInfo.GameList AS G WITH (NOLOCK) ON S.GameID = G.GameID		
		WHERE  (@GameID IS NULL OR G.GameID=@GameID)
			AND (@ServerID IS NULL OR S.ServerID=@ServerID)
			AND (@BeginTime IS NULL OR L.StatTime>=@BeginTime)
			AND (@EndTime IS NULL OR L.StatTime<=@EndTime)
			AND (@intPlatformID is null or L.PlatformID = @intPlatformID)
			AND (@intAreaID is null or L.AreaID = @intAreaID)
			AND L.PlatformID in (select platformid from @Temp_PlatformID)
		GROUP BY L.StatTime,L.SourceID
		ORDER BY L.StatTime DESC
	--END
	--------------------------------------------------------------------------------------------------------------------

END	

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetGameServerTaxLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetGameServerTaxLog] TO [xbuser]
    AS [dbo];

