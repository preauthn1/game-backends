-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<统计游戏税收记录>
--	2018/03/19	Jason		調整語法WHERE判斷方式並新增變數接參數
--	2019/03/25	Jason		新增遊戲獎池
--	2019/03/27	Jason		新增PlatformID AreaID 對應 寫到新的[Stat].[StatisticsMoneySourceLog_New]
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetGameTaxLog]
	@intAdminID		int,
	@intPlatformID	INT=NULL,
	@intAreaID		INT=NULL,
	@intGameID		INT=NULL,			--游戏ID
	@dtBeginTime	DATETIME=NULL,		--开始时间范围
	@dtEndTime		DATETIME=NULL		--结束时间范围
AS
BEGIN


	--平台篩選
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT)
	INSERT INTO @Temp_PlatformID Select PlatformID From CenterDB.Area.PlatformAdminUser where AdminID=@intAdminID


	DECLARE	@GameID		INT = ISNULL(@intGameID,0)
	DECLARE @BeginTime	DATETIME = ISNULL(@dtBeginTime,'1900-01-01 00:00:00.000')
	DECLARE @EndTime	DATETIME = ISNULL(@dtEndTime,'2100-01-01 00:00:00.000')
	DECLARE @Temp_Table	TABLE (GameID INT,[LogDate] DATETIME,Amount BIGINT)

	/*  原作法  */
	/*
	--------------------------------------------------------------------------------------------------------------------
	IF(@intPlatformID IS NULL) AND (@intAreaID IS NULL)
	BEGIN


		--因獎池被個別遊戲獨立切開  以下自行新增
		/*  GameID=2 搶庄牛牛 預設一開始獎池50000 需先扣掉  */
		INSERT INTO @Temp_Table
		SELECT	2,[LogDate],[Amount] = 0-[Amount]
		FROM [Stat].[QZNN_PoolAmountDaily] WITH (NOLOCK)

		SELECT	G.GameID AS 'Game.Id'
				,G.GameName AS 'Game.GameName'
				,G.GameID AS GameID
				,CONVERT(DATE,L.StatTime,120) StatTime
				,SUM(L.Amount) AS Amount
				,SUM(L.RobotAmount) as RobotAmount
				,SUM(L.RobotTaxAmount) as RobotTaxAmount
				,SUM(L.TaxAmount) AS TaxAmount
				,ISNULL(H.Amount,0) AS PoolAmount
		FROM Stat.MoneySourceLog AS L WITH (NOLOCK)
			INNER JOIN ServerInfo.GameServerList AS S WITH (NOLOCK) ON L.SourceID = S.ServerID
			LEFT JOIN ServerInfo.GameList AS G WITH (NOLOCK) ON S.GameID = G.GameID
			LEFT JOIN @Temp_Table AS H ON G.GameID = H.GameID AND L.StatTime = H.LogDate
		WHERE  (@GameID = 0 OR G.GameID=@GameID)
			AND L.StatTime >= @BeginTime
			AND L.StatTime <= @EndTime
		GROUP BY CONVERT(DATE,L.StatTime,120),G.GameID,G.GameName,ISNULL(H.Amount,0)
		ORDER BY CONVERT(DATE,L.StatTime,120) DESC
	END
	--------------------------------------------------------------------------------------------------------------------
	*/

	/*  新作法 必須帶入PlatformID and AreaID  */
	--------------------------------------------------------------------------------------------------------------------
	--IF(@intPlatformID IS NOT NULL AND @intPlatformID > 0) AND (@intAreaID IS NOT NULL AND @intAreaID > 0)
	--BEGIN

		--因獎池被個別遊戲獨立切開  以下自行新增
		/*  GameID=2 搶庄牛牛 預設一開始獎池50000 需先扣掉  */
		INSERT INTO @Temp_Table
		SELECT	2,[LogDate],[Amount] = 0-[Amount]
		FROM [Stat].[QZNN_PoolAmountDaily] WITH (NOLOCK)

		SELECT	--L.PlatformID
				--,L.AreaID
				G.GameID AS 'Game.Id'
				,G.GameName AS 'Game.GameName'
				,G.GameID AS GameID
				,CONVERT(DATE,L.StatTime,120) StatTime
				,SUM(L.Amount) AS Amount
				,SUM(L.RobotAmount) as RobotAmount
				,SUM(L.RobotTaxAmount) as RobotTaxAmount
				,SUM(L.TaxAmount) AS TaxAmount
				,ISNULL(H.Amount,0) AS PoolAmount
				,SUM(ISNULL(L.FlowAmount,0)) AS FlowAmount
		FROM Stat.MoneySourceLog_New AS L WITH (NOLOCK)
			INNER JOIN ServerInfo.GameServerList AS S WITH (NOLOCK) ON L.SourceID = S.ServerID
			LEFT JOIN ServerInfo.GameList AS G WITH (NOLOCK) ON S.GameID = G.GameID
			LEFT JOIN @Temp_Table AS H ON G.GameID = H.GameID AND L.StatTime = H.LogDate
		WHERE  (@GameID = 0 OR G.GameID=@GameID)
			AND L.StatTime >= @BeginTime
			AND L.StatTime <= @EndTime
			AND (@intPlatformID is null or L.PlatformID = @intPlatformID)
			AND (@intAreaID is null or L.AreaID = @intAreaID)
			AND L.PlatformID in (select platformid from @Temp_PlatformID)
		GROUP BY CONVERT(DATE,L.StatTime,120),G.GameID,G.GameName,ISNULL(H.Amount,0)
		ORDER BY CONVERT(DATE,L.StatTime,120) DESC
	--END
	--------------------------------------------------------------------------------------------------------------------
END




GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetGameTaxLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetGameTaxLog] TO [xbuser]
    AS [dbo];

