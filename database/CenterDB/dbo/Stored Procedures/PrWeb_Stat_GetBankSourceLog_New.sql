-- =============================================
-- Author:  jason
-- Create date: <保險箱流向統計>
-- Description:	<金幣流向統計>  原作法是ORM取資料表資料顯示 新增PlatformID AreaID 對應 寫到新的[Stat].[StatisticsBankSourceLog_New]
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetBankSourceLog_New]
	@intAdminID	int,	
	@intPlatformID	INT=NULL,
	@intAreaID		INT=NULL,
	@intSourceType	INT=NULL,			--0:全部 12000:C存款 12001:Q取款 13000:G管理員增加保險箱金幣
										--13001:G管理員減少保險箱金幣 13002:G管理員風所用互扣保險箱金幣 13003:G管理員解鎖用戶還原保險箱金幣
	@intChangeType	INT=NULL,			--0:全部 1:流出 2:流通 3:流入
	@dtBeginTime	DATETIME=NULL,		--开始时间范围
	@dtEndTime		DATETIME=NULL		--结束时间范围
AS
BEGIN

	SET NOCOUNT ON

	--平台篩選
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT)
	INSERT INTO @Temp_PlatformID Select PlatformID From CenterDB.Area.PlatformAdminUser where AdminID=@intAdminId


	DECLARE @BeginTime	DATETIME = ISNULL(@dtBeginTime,'1900-01-01 00:00:00.000')
	DECLARE @EndTime	DATETIME = ISNULL(@dtEndTime,'2100-01-01 00:00:00.000')
	
	DECLARE @Total_In		BIGINT
	DECLARE @Total_Out		BIGINT
	DECLARE @Total_Dual		BIGINT
	DECLARE @Total_String	VARCHAR(100)

	SET @intSourceType = ISNULL(@intSourceType,0)

	SET @intChangeType = CASE	WHEN @intChangeType IS NULL OR @intChangeType = 0 THEN 999
								WHEN @intChangeType = 3 THEN 0
							ELSE @intChangeType END

	DECLARE @Temp_Total_New TABLE
			([StatTime] VARCHAR(10),[PlatformID] VARCHAR(32),[AreaID] VARCHAR(32),[SourceID] VARCHAR(32),[SourceName] VARCHAR(32),
			[ChangeType] INT,SourceType VARCHAR(10),[Amount] BIGINT)

	/*  新作法 必須帶入PlatformID and AreaID  */
	--------------------------------------------------------------------------------------------------------------------
	/*
	IF(@intPlatformID IS NULL) AND (@intAreaID IS NULL)
	BEGIN
		INSERT INTO @Temp_Total_New
		SELECT	[StatTime] = CONVERT(VARCHAR(10),[StatTime],23),
				[PlatformID] = CAST(0 AS VARCHAR(32)),
				[AreaID] = CAST(0 AS VARCHAR(32)),
				A.[SourceID],
				[SourceName],
				[ChangeType],
				SourceType = CASE	WHEN B.[ChangeType] = 2 THEN '流通'
									WHEN B.[ChangeType] = 0 THEN '流入'
									WHEN B.[ChangeType] = 1 THEN '流出'
								ELSE '' END,
				[Amount]
		FROM [Stat].[BankSourceLog] AS A WITH (NOLOCK)
			INNER JOIN [Stat].[BankSourceType] AS B WITH (NOLOCK) ON A.SourceID = B.SourceID
		WHERE
			([StatTime] >= @BeginTime)
			AND ([StatTime] <= @dtEndTime)
			AND (A.[SourceID] = @intSourceType OR @intSourceType = 0)
			AND ([ChangeType] = @intChangeType OR @intChangeType = 999)

		SELECT	@Total_In = SUM([Amount]) FROM @Temp_Total_New WHERE [ChangeType] = 0
		SELECT	@Total_Out = SUM([Amount]) FROM @Temp_Total_New WHERE [ChangeType] = 1
		SELECT	@Total_Dual = @Total_In - @Total_Out

		SELECT	@Total_String = '总流入: ' + CAST(ROUND(ISNULL(@Total_In,0) * 1.00,2) AS VARCHAR(30)) + CHAR(13)
								+ '总流出: ' + CAST(ROUND(ISNULL(@Total_Out,0) * 1.00,2) AS VARCHAR(30)) + CHAR(13)
								+ '净流入:' + CAST(ROUND(ISNULL(@Total_Dual,0) * 1.00,2) AS VARCHAR(30))


		SELECT	[StatTime],
				[PlatformID],
				[AreaID],
				[SourceID],
				[SourceName],
				SourceType,
				[Amount]
		FROM @Temp_Total_New
		--UNION ALL
		--SELECT '','','','','','',@Total_String
	END
	*/

	--IF(@intPlatformID IS NOT NULL AND @intPlatformID > 0) AND (@intAreaID IS NOT NULL AND @intAreaID > 0)
	--BEGIN
		INSERT INTO @Temp_Total_New
		SELECT	[StatTime] = CONVERT(VARCHAR(10),[StatTime],23),
				[PlatformID] = CAST(A.[PlatformID] AS VARCHAR(32)),
				[AreaID] = CAST(A.[AreaID] AS VARCHAR(32)),
				[SourceID] = CAST(A.[SourceID] AS VARCHAR(32)),
				[SourceName],
				[ChangeType],
				SourceType = CASE	WHEN B.[ChangeType] = 2 THEN '流通'
									WHEN B.[ChangeType] = 0 THEN '流入'
									WHEN B.[ChangeType] = 1 THEN '流出'
								ELSE '' END,
				[Amount] = [Amount]
		FROM [Stat].[BankSourceLog_New] AS A WITH (NOLOCK)
			INNER JOIN [Stat].[BankSourceType] AS B WITH (NOLOCK) ON A.SourceID = B.SourceID
		WHERE
			([StatTime] >= @BeginTime)
			AND ([StatTime] <= @dtEndTime)
			AND (@intPlatformID is null or [PlatformID] = @intPlatformID)
			AND (@intAreaID is null or [AreaID] = @intAreaID)
			AND (A.[SourceID] = @intSourceType OR @intSourceType = 0)
			AND ([ChangeType] = @intChangeType OR @intChangeType = 999)
			AND PlatformID in (select platformid from @Temp_PlatformID)

		SELECT	@Total_In = SUM([Amount]) FROM @Temp_Total_New WHERE [ChangeType] = 0
		SELECT	@Total_Out = SUM([Amount]) FROM @Temp_Total_New WHERE [ChangeType] = 1
		SELECT	@Total_Dual = @Total_In - @Total_Out

		SELECT	@Total_String = '总流入: ' + CAST(ROUND(ISNULL(@Total_In,0) * 1.00,2) AS VARCHAR(30)) + CHAR(13)
								+ '总流出: ' + CAST(ROUND(ISNULL(@Total_Out,0) * 1.00,2) AS VARCHAR(30)) + CHAR(13)
								+ '净流入:' + CAST(ROUND(ISNULL(@Total_Dual,0) * 1.00,2) AS VARCHAR(30))

		SELECT	[StatTime],
				[PlatformID],
				[AreaID],
				[SourceID],
				[SourceName],
				SourceType,
				[Amount]
		FROM @Temp_Total_New
		--UNION ALL
		--SELECT '','','','','','',@Total_String
	--END
	--------------------------------------------------------------------------------------------------------------------
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetBankSourceLog_New] TO [GameUser]
    AS [dbo];

