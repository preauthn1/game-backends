-- =============================================
-- Author:  jason
-- Create date: 20190321
-- Description:	全平台金幣異動歷程 - 後台功能 統計分析-系統總報表-日總計
--	2019/03/27	Jason		新增PlatformID AreaID 對應 寫到新的[Stat].[StatisticsMoneySourceLog_New]
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetDailyMoneyLog]
(
	@intAdminID int,
	@intPlatformID    INT = NULL,
	@intAreaID      INT = NULL,
	@dtStartTime		DATETIME,
	@dtEndTime			DATETIME
)	
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @StatDate	DATETIME = DATEADD(DAY,0,CONVERT(VARCHAR(10),GETDATE(),23))
	DECLARE @LastDate	DATETIME = DATEADD(DAY,-1,CONVERT(VARCHAR(10),GETDATE(),23))
	SET @intPlatformID	= ISNULL(@intPlatformID,0)
	SET @intAreaID	= ISNULL(@intAreaID,0)

	/*  先取日期區間  */
	-------------------------------------------------------------------------------------------------------------
	DECLARE @Temp_StatTime TABLE (StatTime DATETIME)

	INSERT INTO @Temp_StatTime
	SELECT	DISTINCT [stattime]
	FROM [Stat].[SumMoneyDaily] WITH (NOLOCK)
	WHERE
		[stattime] >= @dtStartTime
		AND [stattime] <= @dtEndTime
	-------------------------------------------------------------------------------------------------------------


	/* 取出admin對應平台  */
	-------------------------------------------------------------------------------------------------------------
	--平台篩選
	CREATE TABLE #Temp_PlatformID (PlatformID INT,AreaID INT, PRIMARY KEY(PlatformID, AreaID)) 
	DECLARE @intOPUserType int

	--select @intOPUserType=UserType from AdminSys.Admin.Users with (nolock) where UserId=@intAdminID

	--撈出該用戶可讀取的全部平台
	IF @intPlatformID=0
	BEGIN
		INSERT INTO #Temp_PlatformID
		Select a.PlatformId,b.AreaID From CenterDB.Area.PlatformAdminUser as a
			inner join area.area as b on a.PlatformID = b.PlatformID
		where a.AdminID=@intAdminID
	END
	else
	BEGIN
		--針對某渠道篩選
		INSERT INTO #Temp_PlatformID
		Select a.PlatformId,b.AreaID
		From CenterDB.Area.PlatformAdminUser as a
			inner join area.area as b with (nolock) on a.PlatformID = b.PlatformID
		where a.AdminID=@intAdminID and a.PlatformID=@intPlatformID and (@intAreaID = AreaID or @intAreaID = 0)
	END
	-------------------------------------------------------------------------------------------------------------


	--/*	取出平台對應渠道  */
	---------------------------------------------------------------------------------------------------------------
	--DECLARE @Temp_AreaID	TABLE(PlatformID INT,AreaID INT)

	--INSERT INTO @Temp_AreaID
	--SELECT	A.PlatformID,B.AreaID
	--FROM #Temp_PlatformID AS A
	--	INNER JOIN [Area].[Area] AS B WITH (NOLOCK) ON A.PlatformID = B.PlatformID
	---------------------------------------------------------------------------------------------------------------


	/*  取出註冊數  */
	-------------------------------------------------------------------------------------------------------------
	DECLARE @Temp_RegInfo TABLE ([stattime] DATETIME,[PlatformID] INT,[AreaID] INT,[TotalRegCount] BIGINT)
	INSERT INTO @Temp_RegInfo
	SELECT	[StatTime],
			A.[PlatformID],
			B.[AreaID],
			[TotalRegCount]
	FROM [Stat].[RegUserLog] AS A WITH (NOLOCK)
		INNER JOIN #Temp_PlatformID AS B ON A.PlatformID = B.[PlatformID] AND A.AreaID = B.AreaID
	WHERE
		(@dtStartTime IS NULL OR A.StatTime >= @dtStartTime)
		AND (@dtEndTime IS NULL OR A.StatTime <= @dtEndTime)
	-------------------------------------------------------------------------------------------------------------


	/*  取充值兌換記錄  */
	-------------------------------------------------------------------------------------------------------------
	DECLARE @Temp_ChangeInfo_New	TABLE ([stattime] DATETIME,[PlatformID] INT,[AreaID] INT,[Charge] BIGINT,[Convert] BIGINT,[Rate] FLOAT)

	INSERT INTO @Temp_ChangeInfo_New
	SELECT	[stattime],
			[PlatformID],
			[AreaID],
			[Charge],
			[Convert] = ABS([Convert]),
			[Rate] = CASE WHEN [Charge] > 0 THEN ROUND(CAST(ABS([Convert]) AS FLOAT) / [Charge],2) ELSE 1 END
	FROM (
			SELECT	[StatTime],A.[PlatformID],A.[AreaID],
					[Charge] = SUM(CASE WHEN SourceID >= 11000 AND SourceID <= 11099 THEN Amount ELSE 0 END),
					[Convert] = SUM(CASE WHEN SourceID = 14000 THEN Amount ELSE 0 END)
			FROM [Stat].[MoneySourceLog_New] AS A WITH (NOLOCK)
				INNER JOIN #Temp_PlatformID AS B ON A.PlatformID = B.[PlatformID] AND A.AreaID = B.AreaID
			WHERE
				(@dtStartTime IS NULL OR StatTime >= @dtStartTime)
				AND (@dtEndTime IS NULL OR StatTime <= @dtEndTime)
			GROUP BY [StatTime],A.[PlatformID],A.[AreaID]
		) AS A
	-------------------------------------------------------------------------------------------------------------


	/*  取當日金幣變化及歷史金幣變化組合  */
	-------------------------------------------------------------------------------------------------------------
	DECLARE @Temp_MoneyDiff_New TABLE ([stattime] DATETIME,[PlatformID] INT,AreaID INT,DiffAmount BIGINT)

	INSERT INTO @Temp_MoneyDiff_New
	SELECT	[stattime] =@StatDate,
			[PlatformID],[AreaID],
			DiffAmount = SUM(Amount) - SUM(LastAmount)
	FROM (
			SELECT	[PlatformID],A.[AreaID],
					Amount = (ISNULL(B.Amount,0) + ISNULL(C.Amount,0)),
					LastAmount = 0
			FROM [Users].[UserInfo] AS A WITH (NOLOCK)
				INNER JOIN #Temp_PlatformID AS Z ON A.AreaID = Z.AreaID
				LEFT JOIN [Money].[Cash] AS B WITH (NOLOCK) ON A.UserID = B.UserID
				LEFT JOIN [Money].[Bank] AS C WITH (NOLOCK) ON A.UserID = C.UserID
			WHERE
				A.UserType <> 2
			UNION ALL
			SELECT	A.[PlatformID],A.[AreaID],
					Amount = 0,
					LastAmount = ([usercashamount] + [userbankamount])
			FROM [Stat].[SumMoneyDaily] AS A WITH (NOLOCK)
				INNER JOIN #Temp_PlatformID AS Z ON A.AreaID = Z.AreaID
			WHERE
				A.[stattime] = @LastDate
		) AS A
	GROUP BY [PlatformID],[AreaID]
	UNION ALL
	SELECT	[stattime],
			A.[PlatformID],A.[AreaID],
			Amount = SUM([usercashdiff]) + SUM([userbankdiff])
	FROM [Stat].[SumMoneyDaily] AS A WITH (NOLOCK)
		INNER JOIN #Temp_PlatformID AS Z ON A.AreaID = Z.AreaID
	WHERE
		(@dtStartTime IS NULL OR StatTime >= @dtStartTime)
		AND (@dtEndTime IS NULL OR StatTime <= @dtEndTime)
		AND [stattime] <> @StatDate
	GROUP BY A.[PlatformID],A.[AreaID],[stattime]
	-------------------------------------------------------------------------------------------------------------


	/*  回傳結果  */
	-------------------------------------------------------------------------------------------------------------
	--IF(@intPlatformID = 0)
	--BEGIN
		SELECT	[stattime] = CONVERT(VARCHAR(10),A.[stattime],23),
				[TotalRegCount] = SUM(ISNULL([TotalRegCount],0)),
				[Charge] = SUM(ISNULL(ROUND(CAST([Charge] AS FLOAT) / 100 * 1.00,2),0.00)),
				[Convert] = SUM(ISNULL(ROUND(CAST([Convert] AS FLOAT) / 100 * 1.00,2),0.00)),
				[Rate] =	CASE WHEN SUM([Charge]) > 0 THEN
							ROUND((SUM(ISNULL(ROUND(CAST([Convert] AS FLOAT) / 100 * 1.00,2),0.00))
							/ SUM(ISNULL(ROUND(CAST([Charge] AS FLOAT) / 100 * 1.00,2),0.00)))
								* 100,2) ELSE 0 END,
				DiffAmount = SUM(ISNULL(ROUND(CAST(DiffAmount AS FLOAT) / 100 * 1.00,2),0.00))
		FROM @Temp_StatTime AS A
			LEFT JOIN @Temp_RegInfo AS B ON A.StatTime = B.StatTime
			LEFT JOIN @Temp_ChangeInfo_New AS C ON A.StatTime = C.stattime AND B.PlatformID = C.PlatformID AND B.AreaID = C.AreaID
			LEFT JOIN @Temp_MoneyDiff_New AS D ON A.StatTime = D.stattime AND B.PlatformID = D.PlatformID AND B.AreaID = D.AreaID
			LEFT JOIN #Temp_PlatformID AS Z ON B.PlatformID = Z.PlatformID AND B.AreaID = Z.AreaID
		GROUP BY CONVERT(VARCHAR(10),A.[stattime],23)
		ORDER BY CONVERT(VARCHAR(10),A.[stattime],23) DESC
	--END
	--ELSE
	--BEGIN
	--	SELECT	[stattime] = CONVERT(VARCHAR(10),A.[stattime],23),
	--			B.[PlatformID],B.[AreaID],
	--			[TotalRegCount] = ISNULL([TotalRegCount],0),
	--			[Charge] = ISNULL(ROUND(CAST([Charge] AS FLOAT) / 100 * 1.00,2),0.00),
	--			[Convert] = ISNULL(ROUND(CAST([Convert] AS FLOAT) / 100 * 1.00,2),0.00),
	--			[Rate] = ISNULL([Rate],0.00),
	--			DiffAmount = ISNULL(ROUND(CAST(DiffAmount AS FLOAT) / 100 * 1.00,2),0.00)
	--	FROM @Temp_StatTime AS A
	--		LEFT JOIN @Temp_RegInfo AS B ON A.StatTime = B.StatTime
	--		LEFT JOIN @Temp_ChangeInfo_New AS C ON A.StatTime = C.stattime AND B.PlatformID = C.PlatformID AND B.AreaID = C.AreaID
	--		LEFT JOIN @Temp_MoneyDiff_New AS D ON A.StatTime = D.stattime AND B.PlatformID = D.PlatformID AND B.AreaID = D.AreaID
	--		INNER JOIN #Temp_PlatformID AS Z ON B.PlatformID = Z.PlatformID AND B.AreaID = Z.AreaID 
	--	WHERE
	--		B.PlatformID = @intPlatformID
	--	ORDER BY CONVERT(VARCHAR(10),A.[stattime],23) DESC,[PlatformID],B.[AreaID]
	--END
	-------------------------------------------------------------------------------------------------------------

--	IF(@intPlatformID IS NULL OR @intAreaID IS NULL)
--	BEGIN
--		RETURN -1
--	END


--	INSERT INTO @Temp_StatTime
--	SELECT	DISTINCT [stattime]
--	FROM [Stat].[SumMoneyDaily] WITH (NOLOCK)
--	WHERE
--		[stattime] >= @dtStartTime
--		AND [stattime] <= @dtEndTime

--	--註冊數
--	CREATE TABLE #Temp_Reg_New ([stattime] DATETIME,[PlatformID] INT,[AreaID] INT,[TotalRegCount] BIGINT)
--	INSERT INTO #Temp_Reg_New
--	SELECT	[StatTime],
--			[PlatformID],
--			[AreaID],
--			[TotalRegCount]
--	FROM [Stat].[RegUserLog] WITH (NOLOCK)
--	WHERE
--		[PlatformID] = @intPlatformID
--		AND [AreaID] = @intAreaID
--		AND [stattime] >= @dtStartTime
--		AND [stattime] <= @dtEndTime

--	--取充值兌換記錄
----	CREATE TABLE #Temp_ChangeInfo ([stattime] DATETIME,[Charge] BIGINT,[Convert] BIGINT,[Rate] FLOAT)
--	CREATE TABLE #Temp_ChangeInfo_New ([stattime] DATETIME,[PlatformID] INT,[AreaID] INT,[Charge] BIGINT,[Convert] BIGINT,[Rate] FLOAT)

--	/*  20190327 因應Stat.MoneySourceLog 需要新增PlatformID AreaID 故讀取新表 */
--	-------------------------------------------------------------------------------------------------------------
--	IF(@intPlatformID IS NOT NULL AND @intPlatformID > 0) AND (@intAreaID IS NOT NULL AND @intAreaID > 0)
--	BEGIN
--		INSERT INTO #Temp_ChangeInfo_New
--		SELECT	[stattime],[PlatformID],[AreaID],
--				[Charge],
--				[Convert] = ABS([Convert]),
--				[Rate] = CASE WHEN [Charge] > 0 THEN ROUND(CAST(ABS([Convert]) AS FLOAT) / [Charge],2) ELSE 1 END
--		FROM (
--				SELECT	[StatTime],[PlatformID],[AreaID],
--						[Charge] = SUM(CASE WHEN SourceID >= 11000 AND SourceID < 11999 THEN Amount ELSE 0 END),
--						[Convert] = SUM(CASE WHEN SourceID >= 14000 AND SourceID < 14999 THEN Amount ELSE 0 END)
--				FROM [Stat].[MoneySourceLog_New] WITH (NOLOCK)
--				WHERE
--					[stattime] >= @dtStartTime
--					AND [stattime] <= @dtEndTime
--					AND [PlatformID] = @intPlatformID
--					AND [AreaID] = @intAreaID
--					GROUP BY [StatTime],[PlatformID],[AreaID]
--			) AS A
--	END
--	-------------------------------------------------------------------------------------------------------------


--	--取當日金幣變化及歷史金幣變化組合
--	--CREATE TABLE #Temp_MoneyDiff ([stattime] DATETIME,DiffAmount BIGINT)
--	CREATE TABLE #Temp_MoneyDiff_New ([stattime] DATETIME,[PlatformID] INT,AreaID INT,DiffAmount BIGINT)

--	INSERT INTO #Temp_MoneyDiff_New
--	SELECT	[stattime] =@StatDate,
--			[PlatformID],[AreaID],
--			DiffAmount = SUM(Amount) - SUM(LastAmount)
--	FROM (
--			SELECT	[PlatformID],A.[AreaID],
--					Amount = (ISNULL(B.Amount,0) + ISNULL(C.Amount,0)),
--					LastAmount = 0
--			FROM [Users].[UserInfo] AS A WITH (NOLOCK)
--				INNER JOIN [Area].[Area] AS Z WITH (NOLOCK) ON A.AreaID = Z.AreaID
--				LEFT JOIN [Money].[Cash] AS B WITH (NOLOCK) ON A.UserID = B.UserID
--				LEFT JOIN [Money].[Bank] AS C WITH (NOLOCK) ON A.UserID = C.UserID
--			WHERE
--				A.UserType <> 2
--				AND [PlatformID] = @intPlatformID
--				AND A.AreaID = @intAreaID
--			UNION ALL
--			SELECT	[PlatformID],A.[AreaID],
--					Amount = 0,
--					LastAmount = ([usercashamount] + [userbankamount])
--			FROM [Stat].[SumMoneyDaily] AS A WITH (NOLOCK)
--			WHERE
--				[stattime] = @LastDate
--				AND [PlatformID] = @intPlatformID
--				AND A.AreaID = @intAreaID
--		) AS A
--	GROUP BY [PlatformID],[AreaID]
--	UNION ALL
--	SELECT	[stattime],
--			[PlatformID],A.[AreaID],
--			Amount = SUM([usercashdiff]) + SUM([userbankdiff])
--	FROM [Stat].[SumMoneyDaily] AS A WITH (NOLOCK)
--	WHERE
--		[stattime] >= @dtStartTime
--		AND [stattime] <= @dtEndTime
--		AND [stattime] <> @StatDate
--		AND [PlatformID] = @intPlatformID
--		AND A.AreaID = @intAreaID
--	GROUP BY [PlatformID],[AreaID],[stattime]

--	--回傳結果
--	SELECT	[stattime] = CONVERT(VARCHAR(10),A.[stattime],23),
--			B.[PlatformID],B.[AreaID],
--			[TotalRegCount] = ISNULL([TotalRegCount],0),
--			[Charge] = ISNULL(ROUND(CAST([Charge] AS FLOAT) / 100 * 1.00,2),0.00),
--			[Convert] = ISNULL(ROUND(CAST([Convert] AS FLOAT) / 100 * 1.00,2),0.00),
--			[Rate] = ISNULL([Rate],0.00),
--			DiffAmount = ISNULL(ROUND(CAST(DiffAmount AS FLOAT) / 100 * 1.00,2),0.00)
--	FROM @Temp_StatTime AS A
--		LEFT JOIN #Temp_Reg_New AS B ON A.StatTime = B.StatTime
--		LEFT JOIN #Temp_ChangeInfo_New AS C ON A.StatTime = C.stattime
--		LEFT JOIN #Temp_MoneyDiff_New AS D ON A.StatTime = D.stattime
--	ORDER BY CONVERT(VARCHAR(10),A.[stattime],23) DESC,[PlatformID],C.[AreaID]

--	DROP TABLE #Temp_Reg_New
--	DROP TABLE #Temp_MoneyDiff_New
--	DROP TABLE #Temp_ChangeInfo_new
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetDailyMoneyLog] TO [GameUser]
    AS [dbo];

