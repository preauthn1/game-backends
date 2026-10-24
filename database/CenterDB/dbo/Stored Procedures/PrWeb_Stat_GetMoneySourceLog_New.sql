-- =============================================
-- Author:  jason
-- Create date: 20190328
-- Description:	<金幣流向統計>  原作法是ORM取資料表資料顯示 新增PlatformID AreaID 對應 寫到新的[Stat].[StatisticsMoneySourceLog_New]
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetMoneySourceLog_New]
	@intAdminID int,
	@intPlatformID	INT=NULL,
	@intAreaID		INT=NULL,
	@intSourceGroup	INT=NULL,	--現金流通類型分組  0:全部 1:充值 2:平台 3:銀行 4:遊戲 5:管理員 6:贈送 7:其他
	@intSourceType	INT = NULL,	--現金流通類型 0:全部 >0:類型前端帶入
	@intChangeType	INT=NULL,			--0:全部 1:流出 2:流通 3:流入
	@dtBeginTime	DATETIME=NULL,		--开始时间范围
	@dtEndTime		DATETIME=NULL		--结束时间范围
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @BeginTime	DATETIME = ISNULL(@dtBeginTime,'1900-01-01 00:00:00.000')
	DECLARE @EndTime	DATETIME = ISNULL(@dtEndTime,'2100-01-01 00:00:00.000')

	DECLARE @Total_In		BIGINT
	DECLARE @Total_Out		BIGINT
	DECLARE @Total_Dual		BIGINT
	DECLARE @Total_String	VARCHAR(100)

	SET @intSourceGroup	=	ISNULL(@intSourceGroup,0)
	SET @intSourceType	=	ISNULL(@intSourceType,0)
	SET @intChangeType	=	ISNULL(@intChangeType,0)

	SET @intPlatformID	= ISNULL(@intPlatformID,0)
	SET @intAreaID		= ISNULL(@intAreaID,0)

	IF(@intChangeType = NULL OR @intChangeType = 0)
	BEGIN
		SET @intChangeType = 999
	END
	ELSE IF (@intChangeType = 3)
	BEGIN
		SET @intChangeType = 0
	END
		--0:全部 1:流出 2:流通 3:流入)


	/* 取出admin對應平台  */
	-------------------------------------------------------------------------------------------------------------
	--平台篩選
	CREATE TABLE #Temp_PlatformID  (PlatformID	INT,AreaID INT, PRIMARY KEY (PlatformID, AreaID))
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


	/*  先取已有的報表日期  */
	--------------------------------------------------------------------------------------------------------------------
	DECLARE @Temp_StatTime TABLE (StatTime DATETIME)
	
	INSERT INTO @Temp_StatTime
	SELECT	DISTINCT [stattime]
	FROM [Stat].[MoneySourceLog_New] WITH (NOLOCK)
	WHERE
		[stattime] >= @BeginTime
		AND [stattime] <= @dtEndTime
	--------------------------------------------------------------------------------------------------------------------


	/*  先取參數所對應的群組及類別  */
	--------------------------------------------------------------------------------------------------------------------
	DECLARE @Temp_TypeGroup	TABLE (GroupID INT,SourceID INT,SourceName VARCHAR(50),[ChangeType] TINYINT)

	INSERT INTO @Temp_TypeGroup
	SELECT	DISTINCT a.Id,b.SourceID,[SourceName],[ChangeType]
	FROM [Stat].[MoneySourceGroup] AS A WITH (NOLOCK)
		INNER JOIN [Stat].[MoneySourceType] AS B WITH (NOLOCK) ON A.Id = B.GroupID
		INNER JOIN [Stat].[MoneySourceLog] AS C WITH (NOLOCK) ON B.SourceID = C.[SourceID]
	WHERE
		(a.Id = @intSourceGroup or @intSourceGroup = 0)
		AND (b.SourceID = @intSourceType OR @intSourceType = 0)
		AND b.SourceID >= 10000
	--------------------------------------------------------------------------------------------------------------------


	/*  回傳平台資料  */
	--------------------------------------------------------------------------------------------------------------------
	IF(@intPlatformID = 0)
	BEGIN
		SELECT	A.StatTime,
				A.SourceID,
				SourceName,
				[ChangeType],
				[Amount] = SUM(ISNULL(C.[Amount],0))
		FROM (
				SELECT	*
				FROM @Temp_StatTime AS A
					LEFT JOIN (SELECT DISTINCT PlatformID FROM #Temp_PlatformID) AS B ON 1=1
					LEFT JOIN @Temp_TypeGroup AS C ON 1=1
			) AS A
			LEFT JOIN [Stat].[MoneySourceLog_New] AS C WITH (NOLOCK) ON A.StatTime = C.[StatTime] AND A.PlatformID = C.PlatformID AND A.SourceID = C.[SourceID]
		GROUP BY A.StatTime,
				A.SourceID,
				SourceName,
				[ChangeType]
	END
	ELSE
	BEGIN
		SELECT	top 10 A.StatTime,
				--A.[PlatformID],
				--A.[AreaID],
				A.SourceID,
				SourceName,
				[ChangeType],
				[Amount] = SUM(ISNULL(C.[Amount],0))
		FROM (
				SELECT	*
				FROM @Temp_StatTime AS A
					LEFT JOIN #Temp_PlatformID AS B ON 1=1
					LEFT JOIN @Temp_TypeGroup AS C ON 1=1
			) AS A
			LEFT JOIN [Stat].[MoneySourceLog_New] AS C WITH (NOLOCK) ON A.StatTime = C.[StatTime] AND A.PlatformID = C.PlatformID AND A.AreaID = C.AreaID AND A.SourceID = C.[SourceID]
		GROUP BY A.StatTime,
				A.SourceID,
				SourceName,
				[ChangeType]
	END
	--------------------------------------------------------------------------------------------------------------------

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetMoneySourceLog_New] TO [GameUser]
    AS [dbo];

