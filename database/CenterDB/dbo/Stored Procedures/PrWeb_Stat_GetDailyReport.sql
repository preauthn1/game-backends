-- =============================================
-- Author:  Jason
-- Create date: 2018/05/29
-- Description:	每日報表 API使用 #349
--	2018/06/26	新增代理充值資料
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetDailyReport]
(
 @dtStatTime	DATETIME = NULL
)
AS
BEGIN
	
	SET NOCOUNT ON

	SET	@dtStatTime = ISNULL(@dtStatTime,GETDATE())

	DECLARE @LogStartTime	DATETIME = DATEADD(DAY,-1,DATEDIFF(DAY,0,@dtStatTime))
	DECLARE @LogEndTime		DATETIME = DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
	DECLARE @LogLastTime	DATETIME = DATEADD(DAY,-2,DATEDIFF(DAY,0,@dtStatTime))
	DECLARE @LogonBindCount	INT
	DECLARE @TodayTotalTaxAmount	FLOAT
	DECLARE @TodayTotalAmount		FLOAT
	DECLARE @LastTotalTaxAmount		FLOAT
	DECLARE @LastTotalAmount		FLOAT

	DECLARE @Dates	TABLE (StatTime DATETIME)	
	DECLARE @Temp_LTV	TABLE (StatTime DATETIME,UserAmount FLOAT,UserTaxAmount FLOAT,SystemAmount FLOAT,SystemTaxAmount FLOAT)
	DECLARE @Temp_LogonCount	TABLE ([StatTime] DATETIME,TotalCount INT)
	DECLARE @Temp_RegBindLog	TABLE (UserID INT,RegTime DATETIME,BindTime DATETIME)
	DECLARE @Temp_TodayRegBindChargeDetail	TABLE (StatTime DATETIME,RegCharge INT,RegBind INT,BindCharge INT)
	DECLARE @Temp_LastRegBindChargeDetail	TABLE (StatTime DATETIME,RegCharge INT,RegBind INT,BindCharge INT)
	DECLARE @Temp_DAUCount		TABLE (StatTime DATETIME,PlatformID INT,TotalCount INT)
	DECLARE @Temp_MaxOnlineLog	TABLE (StatTime DATETIME,TotalCount INT)
	DECLARE @Temp_MaxGameOnlineLog	TABLE (StatTime DATETIME,GameID INT,TotalCount INT)

	DECLARE @Temp_TodayGameData	TABLE (StatTime DATETIME,GameID INT,Tax FLOAT,[SystemAmount] FLOAT,MAXCCU INT,DAU INT,[UserTaxAmount] FLOAT,[UserTaxSystemAmount] FLOAT)
	DECLARE @Temp_LastGameData	TABLE (StatTime DATETIME,GameID INT,Tax FLOAT,[SystemAmount] FLOAT,MAXCCU INT,DAU INT,[UserTaxAmount] FLOAT,[UserTaxSystemAmount] FLOAT)

	DECLARE @Temp_TodayKeepLog	TABLE (StatTime DATETIME,[1] FLOAT,[3] FLOAT,[5] FLOAT,[7] FLOAT)
	DECLARE @Temp_LastKeepLog	TABLE (StatTime DATETIME,[1] FLOAT,[3] FLOAT,[5] FLOAT,[7] FLOAT)

	DECLARE @Temp_TodayData		TABLE
		(
			StatTime				DATETIME,
			TotalRegCount			INT,
			RegBind					INT,
			TotalLoginCount			INT,
			DAU						INT,
			iOSLoginCount			INT,
			AndroidLoginCount		INT,
			NewLoginIP				INT,
			NewInAppChargeAmount	BIGINT,
			OldInAppChargeAmount	BIGINT,
			NewAgentChargeAmount	BIGINT,
			OldAgentChargeAmount	BIGINT,
			NewInAppChargeUserCount	INT,
			OldInAppChargeUserCount	INT,
			NewAgentChargeUserCount	INT,
			OldAgentChargeUserCount	INT,
			InAppChargeTimes		INT,
			AgentChargeTimes		INT,
			NewConvertAmount		BIGINT,
			OldConvertAmount		BIGINT,
			ConvertUserCount		BIGINT,
			OneDayKeep				FLOAT,
			ThirdKeep				FLOAT,
			FifthKeep				FLOAT,
			WeekKeep				FLOAT,
			RegCharge				INT,
			BindCharge				INT,
			UserTaxAmount			BIGINT,
			SystemAmount			BIGINT,
			MaxOnlineCount			INT
		)

	DECLARE @Temp_LastdayData		TABLE
		(
			StatTime				DATETIME,
			TotalRegCount			INT,
			RegBind					INT,
			TotalLoginCount			INT,
			DAU						INT,
			iOSLoginCount			INT,
			AndroidLoginCount		INT,
			NewLoginIP				INT,
			NewInAppChargeAmount	BIGINT,
			OldInAppChargeAmount	BIGINT,
			NewAgentChargeAmount	BIGINT,
			OldAgentChargeAmount	BIGINT,
			NewInAppChargeUserCount	INT,
			OldInAppChargeUserCount	INT,
			NewAgentChargeUserCount	INT,
			OldAgentChargeUserCount	INT,
			InAppChargeTimes		INT,
			AgentChargeTimes		INT,
			NewConvertAmount		BIGINT,
			OldConvertAmount		BIGINT,
			ConvertUserCount		BIGINT,
			OneDayKeep				FLOAT,
			ThirdKeep				FLOAT,
			FifthKeep				FLOAT,
			WeekKeep				FLOAT,
			RegCharge				INT,
			BindCharge				INT,
			UserTaxAmount			BIGINT,
			SystemAmount			BIGINT,
			MaxOnlineCount			INT
		)

	declare @temp_todayagentcharge table (stattime datetime,userid int,amount bigint)
	declare @temp_lastagentcharge table (stattime datetime,userid int,amount bigint)

	/*  取資料暫存  */
	-------------------------------------------------------------------------------------------
	INSERT INTO @Dates
	VALUES (@LogLastTime),(@LogStartTime)

	INSERT INTO @Temp_MaxOnlineLog
	SELECT	DATEADD(DAY,0,DATEDIFF(DAY,0,[StatTime])),
			[TotalCount]
	FROM [Stat].[DayMaxHallOnLineUserLog] WITH (NOLOCK)
	WHERE
		[StatTime] >= @LogLastTime
		AND [StatTime] < @LogEndTime
		AND PlatformID = 0

	INSERT INTO @Temp_MaxGameOnlineLog
	SELECT	DATEADD(DAY,0,DATEDIFF(DAY,0,[StatTime])),
			GameID,
			[TotalCount]
	FROM [Stat].[DayMaxGameOnLineUserLog] WITH (NOLOCK)
	WHERE
		[StatTime] >= @LogLastTime
		AND [StatTime] < @LogEndTime
		AND PlatformID = 0


	INSERT INTO @Temp_LTV
	SELECT	StatTime,
			SUM(UserAmount) * 1.00 / 100,
			SUM(UserTaxAmount) * 1.00 / 100,
			SUM(SystemAmount) * 1.00 / 100,
			SUM(SystemTaxAmount) * 1.00 / 100
	FROM [Stat].[GameAmountLog] WITH (NOLOCK)
	WHERE
		[StatTime] >= @LogLastTime
		AND [StatTime] < @LogEndTime
	GROUP BY StatTime

	INSERT INTO @Temp_LogonCount
	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrWeb_Stat_GetLogonCount]
		@dtStatTime		= @LogLastTime,	
		@dtEndTime		= @LogEndTime

	INSERT INTO @Temp_DAUCount
	SELECT	StatTime,
			PlatformID,
			SUM(TotalCount)
	FROM [Stat].[DAULog]
	WHERE
		[StatTime] >= @LogLastTime
		AND [StatTime] < @LogEndTime
	GROUP BY StatTime,PlatformID

	INSERT INTO @Temp_RegBindLog
	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrWeb_Stat_GetBindDetailLog]
		@dtStatTime		= @LogLastTime,	
		@dtEndTime		= @LogEndTime,
		@LogonBind		= @LogonBindCount	OUTPUT


	insert into @temp_todayagentcharge
	select	@LogStartTime,userid,amount = [UpdateAmount]
	from [LogDB].[Money].[MoneyLog] with (nolock)
	where
		[CreateTime] >= @LogStartTime
		and [CreateTime] < @LogEndTime
		and sourcetypeid = 11008

	insert into @temp_lastagentcharge
	select	@LogStartTime,userid,amount = [UpdateAmount]
	from [LogDB].[Money].[MoneyLog] with (nolock)
	where
		[CreateTime] >= @LogLastTime
		and [CreateTime] < @LogStartTime
		and sourcetypeid = 11008

	declare @todaynewuseragentchargeamount float
	declare @todayolduseragentchargeamount float
	declare @todaynewuseragentchargecount int
	declare @todayolduseragentchargecount int
	declare @todayagentchargecount int

	select	@todaynewuseragentchargeamount = sum(case when b.createtime >= @LogStartTime then amount else 0 end),
			@todayolduseragentchargeamount = sum(case when b.createtime < @LogStartTime then amount else 0 end),
			@todaynewuseragentchargecount = count(case when b.createtime >= @LogStartTime then a.userid else null end),
			@todayolduseragentchargecount = count(case when b.createtime < @LogStartTime then a.userid else null end),
			@todayagentchargecount = count(a.userid)
	from @temp_todayagentcharge as a
		inner join [Users].[UserInfo] as b with (nolock) on a.userid = b.userid

	set @todaynewuseragentchargeamount = isnull(@todaynewuseragentchargeamount,0)
	set @todayolduseragentchargeamount = isnull(@todayolduseragentchargeamount,0)
	set @todaynewuseragentchargecount = isnull(@todaynewuseragentchargecount,0)
	set @todayolduseragentchargecount = isnull(@todayolduseragentchargecount,0)
	set @todayagentchargecount = isnull(@todayagentchargecount,0)

	declare @lastnewuseragentchargeamount float
	declare @lastolduseragentchargeamount float
	declare @lastnewuseragentchargecount int
	declare @lastolduseragentchargecount int
	declare @lastagentchargecount int

	select	@lastnewuseragentchargeamount = sum(case when b.createtime >= @LogStartTime then amount else 0 end),
			@lastolduseragentchargeamount = sum(case when b.createtime < @LogStartTime then amount else 0 end),
			@lastnewuseragentchargecount = count(case when b.createtime >= @LogStartTime then a.userid else null end),
			@lastolduseragentchargecount = count(case when b.createtime < @LogStartTime then a.userid else null end),
			@lastagentchargecount = count(a.userid)
	from @temp_lastagentcharge as a
		inner join [Users].[UserInfo] as b with (nolock) on a.userid = b.userid

	set @lastnewuseragentchargeamount = isnull(@lastnewuseragentchargeamount,0)
	set @lastolduseragentchargeamount = isnull(@lastolduseragentchargeamount,0)
	set @lastnewuseragentchargecount = isnull(@lastnewuseragentchargecount,0)
	set @lastolduseragentchargecount = isnull(@lastolduseragentchargecount,0)
	set @lastagentchargecount = isnull(@lastagentchargecount,0)

	INSERT INTO @Temp_TodayRegBindChargeDetail
	SELECT	StatTime = @LogStartTime,
			RegCharge = SUM(CASE WHEN RegTime IS NOT NULL AND ChargeTime IS NOT NULL AND RegTime = ChargeTime THEN 1 ELSE 0 END),
			RegBind = SUM(CASE WHEN RegTime IS NOT NULL AND BindTime IS NOT NULL AND RegTime = BindTime THEN 1 ELSE 0 END),
			BindCharge = SUM(CASE WHEN BindTime IS NOT NULL AND ChargeTime IS NOT NULL AND BindTime = ChargeTime THEN 1 ELSE 0 END)
	FROM (
			
			SELECT	A.UserID,
					StatTime = @LogStartTime,
					RegTime = DATEADD(DAY,0,DATEDIFF(DAY,0,B.RegTime)),
					BindTime = DATEADD(DAY,0,DATEDIFF(DAY,0,B.BindTime)),
					ChargeTime = DATEADD(DAY,0,DATEDIFF(DAY,0,C.CreateTime))
			FROM (
					SELECT DISTINCT UserID
					FROM @Temp_RegBindLog
					UNION
					SELECT	DISTINCT UserID
					FROM [Pay].[OrderList] WITH (NOLOCK)
					WHERE
						CreateTime >= @LogStartTime
						AND CreateTime < @LogEndTime
						AND Status = 2
				) AS A
				LEFT JOIN @Temp_RegBindLog AS B ON A.UserID = B.UserID AND ((B.RegTime >= @LogStartTime AND B.RegTime < @LogEndTime) OR (BindTime >= @LogStartTime AND BindTime < @LogEndTime))
				LEFT JOIN [Pay].[OrderList] AS C WITH (NOLOCK) ON A.UserID = C.UserID AND CreateTime >= @LogStartTime AND CreateTime < @LogEndTime AND Status = 2
		) AS A

	INSERT INTO @Temp_LastRegBindChargeDetail
	SELECT	StatTime = @LogLastTime,
			RegCharge = SUM(CASE WHEN RegTime IS NOT NULL AND ChargeTime IS NOT NULL AND RegTime = ChargeTime THEN 1 ELSE 0 END),
			RegBind = SUM(CASE WHEN RegTime IS NOT NULL AND BindTime IS NOT NULL AND RegTime = BindTime THEN 1 ELSE 0 END),
			BindCharge = SUM(CASE WHEN BindTime IS NOT NULL AND ChargeTime IS NOT NULL AND BindTime = ChargeTime THEN 1 ELSE 0 END)
	FROM (
			
			SELECT	A.UserID,
					StatTime = @LogLastTime,
					RegTime = DATEADD(DAY,0,DATEDIFF(DAY,0,B.RegTime)),
					BindTime = DATEADD(DAY,0,DATEDIFF(DAY,0,B.BindTime)),
					ChargeTime = DATEADD(DAY,0,DATEDIFF(DAY,0,C.CreateTime))
			FROM (
					SELECT DISTINCT UserID
					FROM @Temp_RegBindLog
					UNION
					SELECT	DISTINCT UserID
					FROM [Pay].[OrderList] WITH (NOLOCK)
					WHERE
						CreateTime >= @LogLastTime
						AND CreateTime < @LogStartTime
						AND Status = 2
				) AS A
				LEFT JOIN @Temp_RegBindLog AS B ON A.UserID = B.UserID AND ((B.RegTime >= @LogLastTime AND B.RegTime < @LogStartTime) OR (BindTime >= @LogLastTime AND BindTime < @LogStartTime))
				LEFT JOIN [Pay].[OrderList] AS C WITH (NOLOCK) ON A.UserID = C.UserID AND CreateTime >= @LogLastTime AND CreateTime < @LogStartTime AND Status = 2
		) AS A


	INSERT INTO @Temp_TodayKeepLog
	SELECT	StatTime,
			[1] = SUM([1]),
			[3] = SUM([3]),
			[5] = SUM([5]),
			[7] = SUM([7])
	FROM (
			SELECT	StatTime = @LogStartTime,
					[1] = CASE WHEN DATEDIFF(DAY,StatTime,@LogStartTime) = 1 AND RegCount > 0 THEN ROUND((OneDayKeep * 1.00 / RegCount) * 100,2) ELSE 0 END,
					[3] = CASE WHEN DATEDIFF(DAY,StatTime,@LogStartTime) = 3 AND RegCount > 0 THEN ROUND((Third * 1.00 / RegCount) * 100,2) ELSE 0 END,
					[5] = CASE WHEN DATEDIFF(DAY,StatTime,@LogStartTime) = 5 AND RegCount > 0 THEN ROUND((Fifth * 1.00 / RegCount) * 100,2) ELSE 0 END,
					[7] = CASE WHEN DATEDIFF(DAY,StatTime,@LogStartTime) = 7 AND RegCount > 0 THEN ROUND((WeekKeep * 1.00 / RegCount) * 100,2) ELSE 0 END
			FROM [Stat].[KeepLog] WITH (NOLOCK)
			WHERE
				DATEDIFF(DAY,StatTime,@LogStartTime) IN (1,3,5,7)
				AND PlatformID = 0 AND AreaID = 0
		) AS A
	GROUP BY StatTime


	INSERT INTO @Temp_LastKeepLog
	SELECT	StatTime,
			[1] = SUM([1]),
			[3] = SUM([3]),
			[5] = SUM([5]),
			[7] = SUM([7])
	FROM (
			SELECT	StatTime = @LogLastTime,
					[1] = CASE WHEN DATEDIFF(DAY,StatTime,@LogLastTime) = 1 AND RegCount > 0 THEN ROUND((OneDayKeep * 1.00 / RegCount) * 100,2) ELSE 0 END,
					[3] = CASE WHEN DATEDIFF(DAY,StatTime,@LogLastTime) = 3 AND RegCount > 0 THEN ROUND((Third * 1.00 / RegCount) * 100,2) ELSE 0 END,
					[5] = CASE WHEN DATEDIFF(DAY,StatTime,@LogLastTime) = 5 AND RegCount > 0 THEN ROUND((Fifth * 1.00 / RegCount) * 100,2) ELSE 0 END,
					[7] = CASE WHEN DATEDIFF(DAY,StatTime,@LogLastTime) = 7 AND RegCount > 0 THEN ROUND((WeekKeep * 1.00 / RegCount) * 100,2) ELSE 0 END
			FROM [Stat].[KeepLog] WITH (NOLOCK)
			WHERE
				DATEDIFF(DAY,StatTime,@LogLastTime) IN (1,3,5,7)
				AND PlatformID = 0 AND AreaID = 0
		) AS A
	GROUP BY StatTime

	INSERT INTO @Temp_TodayData
		(
			StatTime,TotalRegCount,RegBind,TotalLoginCount,DAU,iOSLoginCount,AndroidLoginCount,NewLoginIP,NewInAppChargeAmount,OldInAppChargeAmount,
			NewAgentChargeAmount,OldAgentChargeAmount,NewInAppChargeUserCount,OldInAppChargeUserCount,NewAgentChargeUserCount,OldAgentChargeUserCount,
			InAppChargeTimes,AgentChargeTimes,NewConvertAmount,OldConvertAmount,ConvertUserCount,OneDayKeep,ThirdKeep,FifthKeep,WeekKeep,
			RegCharge,BindCharge,UserTaxAmount,SystemAmount,MaxOnlineCount
		)
	SELECT	@LogStartTime,
			ISNULL(C.TotalRegCount,0),
			ISNULL(H.RegBind,0),
			ISNULL(G.TotalCount,0),
			ISNULL(B.TotalCount,0),
			ISNULL(A.[iOSCount],0),
			ISNULL(A.[AndRoidCount],0),
			ISNULL(A.[NewLoginIP],0),
			ISNULL(A.[NewCharge],0) * 1.00 /100,
			ISNULL(A.[OldCharge],0) * 1.00 /100,
			@todaynewuseragentchargeamount * 1.00 / 100,
			@todayolduseragentchargeamount * 1.00 / 100,
			ISNULL(A.[NewOrderUser],0),
			ISNULL(A.[Total_OrderUser],0) - ISNULL(A.[NewOrderUser],0),
			@todaynewuseragentchargecount,
			@todayolduseragentchargecount,
			ISNULL(A.[TotalOrderCounts],0),
			@todayagentchargecount,
			ISNULL(A.NewConvert,0) * 1.00 / 100,
			ISNULL(A.OldConvert,0) * 1.00 / 100,
			ISNULL(A.[TotalConvertUser],0),
			ISNULL(F.[1],0),
			ISNULL(F.[3],0),
			ISNULL(F.[5],0),
			ISNULL(F.[7],0),
			ISNULL(H.RegCharge,0),
			ISNULL(H.BindCharge,0),
			ISNULL(D.UserTaxAmount,0) * 1.00 / 100,
			ISNULL(D.SystemAmount,0) * 1.00 / 100,
			ISNULL(I.TotalCount,0)
	FROM @Dates AS Z
		LEFT JOIN [Stat].[AreaDataDay] AS A WITH (NOLOCK) ON Z.StatTime = A.StatTime AND A.platformID = 0 AND A.AreaID = 0 AND A.GameID = 0 AND A.ServerID = 0
		LEFT JOIN @Temp_DAUCount AS B ON A.StatTime = B.StatTime AND A.PlatformID = B.PlatformID
		LEFT JOIN [Stat].[RegUserLog] AS C WITH (NOLOCK) ON A.StatTime = C.StatTime AND A.PlatformID = C.PlatformID AND C.AreaID = 0
		LEFT JOIN @Temp_LTV AS D ON A.StatTime = D.StatTime
		LEFT JOIN @Temp_TodayKeepLog AS F ON A.StatTime = F.StatTime
		LEFT JOIN @Temp_LogonCount AS G ON A.StatTime = G.StatTime
		LEFT JOIN @Temp_TodayRegBindChargeDetail AS H ON A.StatTime = H.StatTime
		LEFT JOIN @Temp_MaxOnlineLog AS I ON A.StatTime = I.StatTime
	WHERE
		Z.[StatTime] = @LogStartTime


	INSERT INTO @Temp_LastdayData
		(
			StatTime,TotalRegCount,RegBind,TotalLoginCount,DAU,iOSLoginCount,AndroidLoginCount,NewLoginIP,NewInAppChargeAmount,OldInAppChargeAmount,
			NewAgentChargeAmount,OldAgentChargeAmount,NewInAppChargeUserCount,OldInAppChargeUserCount,NewAgentChargeUserCount,OldAgentChargeUserCount,
			InAppChargeTimes,AgentChargeTimes,NewConvertAmount,OldConvertAmount,ConvertUserCount,OneDayKeep,ThirdKeep,FifthKeep,WeekKeep,
			RegCharge,BindCharge,UserTaxAmount,SystemAmount,MaxOnlineCount
		)
	SELECT	@LogLastTime,
			ISNULL(C.TotalRegCount,0),
			ISNULL(H.RegBind,0),
			ISNULL(G.TotalCount,0),
			ISNULL(B.TotalCount,0),
			ISNULL(A.[iOSCount],0),
			ISNULL(A.[AndRoidCount],0),
			ISNULL(A.[NewLoginIP],0),
			ISNULL(A.[NewCharge],0) * 1.00 / 100,
			ISNULL(A.[OldCharge],0) * 1.00 / 100,
			@lastnewuseragentchargeamount * 1.00 / 100,
			@lastolduseragentchargeamount * 1.00 / 100,
			ISNULL(A.[NewOrderUser],0),
			ISNULL(A.[Total_OrderUser],0) - ISNULL(A.[NewOrderUser],0),
			@lastnewuseragentchargecount,
			@lastolduseragentchargecount,
			ISNULL(A.[TotalOrderCounts],0),
			@lastagentchargecount,
			ISNULL(A.NewConvert,0) * 1.00 / 100,
			ISNULL(A.OldConvert,0) * 1.00 / 100,
			ISNULL(A.[TotalConvertUser],0),
			ISNULL(F.[1],0),
			ISNULL(F.[3],0),
			ISNULL(F.[5],0),
			ISNULL(F.[7],0),
			ISNULL(H.RegCharge,0),
			ISNULL(H.BindCharge,0),
			ISNULL(D.UserTaxAmount,0) * 1.00 / 100,
			ISNULL(D.SystemAmount,0) * 1.00 / 100,
			ISNULL(I.TotalCount,0)
	FROM @Dates AS Z
		LEFT JOIN [Stat].[AreaDataDay] AS A WITH (NOLOCK) ON Z.StatTime = A.StatTime AND A.platformID = 0 AND A.AreaID = 0 AND A.GameID = 0 AND A.ServerID = 0
		LEFT JOIN @Temp_DAUCount AS B ON A.StatTime = B.StatTime AND A.PlatformID = B.PlatformID
		LEFT JOIN [Stat].[RegUserLog] AS C WITH (NOLOCK) ON A.StatTime = C.StatTime AND A.PlatformID = C.PlatformID AND C.AreaID = 0
		LEFT JOIN @Temp_LTV AS D ON A.StatTime = D.StatTime
		LEFT JOIN @Temp_LastKeepLog AS F ON A.StatTime = F.StatTime
		LEFT JOIN @Temp_LogonCount AS G ON A.StatTime = G.StatTime
		LEFT JOIN @Temp_LastRegBindChargeDetail AS H ON A.StatTime = H.StatTime
		LEFT JOIN @Temp_MaxOnlineLog AS I ON A.StatTime = I.StatTime
	WHERE
		Z.[StatTime] = @LogLastTime


	--暫存個別遊戲數據
	INSERT INTO @Temp_TodayGameData
	SELECT	A.StatTime,
			A.GameID,
			[Tax] = SUM(A.[UserTaxAmount]) * 1.00 / 100,
			[SystemAmount] = SUM(A.[SystemAmount]) * 1.00 / 100,
			[MAXCCU] = MAX(E.TotalCount),
			[DAU] = SUM(B.TotalCount),
			[UserTaxAmount] = SUM(A.[UserTaxAmount]) * 1.00 / 100,
			[UserTaxSystemAmount] = SUM(A.[UserTaxAmount] + A.[SystemAmount]) * 1.00 / 100
	FROM [Stat].[GameAmountLog] AS A WITH (NOLOCK)
		INNER JOIN [Stat].[DAULog] AS B WITH (NOLOCK) ON A.StatTime = B.StatTime AND A.PlatformID = B.PlatformID AND A.AreaID = B.AreaID AND A.GameID = B.GameID
		INNER JOIN [ServerInfo].[GameList] AS C WITH (NOLOCK) ON A.GameID = C.GameID
		LEFT JOIN @Temp_MaxGameOnlineLog AS E ON A.StatTime = E.StatTime AND A.GameID = E.GameID
	WHERE
		A.[StatTime] >= @LogStartTime
		AND A.[StatTime] < @LogEndTime
	GROUP BY A.StatTime,A.GameID


	INSERT INTO @Temp_LastGameData
	SELECT	A.StatTime,
			A.GameID,
			[Tax] = SUM(A.[UserTaxAmount]) * 1.00 / 100,
			[SystemAmount] = SUM(A.[SystemAmount]) * 1.00 / 100,
			[MAXCCU] = MAX(E.TotalCount),
			[DAU] = SUM(B.TotalCount),
			[UserTaxAmount] = SUM(A.[UserTaxAmount]) * 1.00 / 100,
			[UserTaxSystemAmount] = SUM(A.[UserTaxAmount] + A.[SystemAmount]) * 1.00 / 100
	FROM [Stat].[GameAmountLog] AS A WITH (NOLOCK)
		INNER JOIN [Stat].[DAULog] AS B WITH (NOLOCK) ON A.StatTime = B.StatTime AND A.PlatformID = B.PlatformID AND A.AreaID = B.AreaID AND A.GameID = B.GameID
		INNER JOIN [ServerInfo].[GameList] AS C WITH (NOLOCK) ON A.GameID = C.GameID
		LEFT JOIN @Temp_MaxGameOnlineLog AS E ON A.StatTime = E.StatTime AND A.GameID = E.GameID
	WHERE
		A.[StatTime] >= @LogLastTime
		AND A.[StatTime] < @LogStartTime
	GROUP BY A.StatTime,A.GameID


	--取系統總稅收及總輸贏
	SELECT	@TodayTotalTaxAmount = SUM(UserTaxAmount),
			@TodayTotalAmount = SUM(SystemAmount)
	FROM @Temp_LTV
	WHERE
		StatTime = @LogStartTime

	SELECT	@LastTotalTaxAmount = SUM(UserTaxAmount),
			@LastTotalAmount = SUM(SystemAmount)
	FROM @Temp_LTV
	WHERE
		StatTime = @LogLastTime
	-------------------------------------------------------------------------------------------


	/*  更新資料  */
	-------------------------------------------------------------------------------------------
	--回傳平台資料
	SELECT				
			--[最高在线]
			[TodayMaxOnline] = ISNULL(A.MaxOnlineCount,0),
			[DiffMaxOnline] = ISNULL(A.MaxOnlineCount,0) - ISNULL(B.MaxOnlineCount,0),
			--[注冊人數]
			[TodayRegCount] = ISNULL(A.TotalRegCount,0),
			[DiffRegCount] = ISNULL(A.TotalRegCount,0) - ISNULL(B.TotalRegCount,0),
			--[注冊綁定人數]
			[TodayRegBindCount] = ISNULL(A.RegBind,0),
			[DiffRegBindCount] = ISNULL(A.RegBind,0) - ISNULL(B.RegBind,0),
			--[綁定總數]
			[TodayLogonBind] = @LogonBindCount,
			--[今日登錄]
			[TodayLogonTimes] = ISNULL(A.TotalLoginCount,0),
			[DiffLogonTimes] = ISNULL(A.TotalLoginCount,0) - ISNULL(B.TotalLoginCount,0),
			--[活躍人數]
			[TodayDAU] = ISNULL(A.DAU,0),
			[DiffDAU] = ISNULL(A.DAU,0) - ISNULL(B.DAU,0),
			--[iOS登錄總數]
			[TodayiOSLoginCount] = ISNULL(A.iOSLoginCount,0),
			[DiffiOSLoginCount] = ISNULL(A.iOSLoginCount,0) - ISNULL(B.iOSLoginCount,0),
			--[AndRoid登錄總數]
			[TodayAndroidLoginCount] = ISNULL(A.AndroidLoginCount,0),
			[DiffAndroidLoginCount] = ISNULL(A.AndroidLoginCount,0) - ISNULL(B.AndroidLoginCount,0),
			--[今日新進入IP]
			[TodayNewLoginIP] = ISNULL(A.NewLoginIP,0),
			[DiffNewLoginIP] = ISNULL(A.NewLoginIP,0) - ISNULL(B.NewLoginIP,0),
			
			--[充值總數]
			--[TodayCharge] = (ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0)) + (ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0)),
			--[DiffCharge] = (ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0)) + (ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0))
			--				- (ISNULL(B.NewInAppChargeAmount,0) + ISNULL(B.OldInAppChargeAmount,0)) + (ISNULL(B.NewAgentChargeAmount,0) + ISNULL(B.OldAgentChargeAmount,0)),
			/*這個數字已經包含代理*/
			[TodayCharge] = (ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0)),
			[DiffCharge] = (ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0)) - (ISNULL(B.NewInAppChargeAmount,0) + ISNULL(B.OldInAppChargeAmount,0)),

			
			--[新用戶充值]
			--[TodayNewCharge] = (ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.NewAgentChargeAmount,0)),
			--[DiffNewCharge] = (ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.NewAgentChargeAmount,0)) - (ISNULL(B.NewInAppChargeAmount,0) + ISNULL(B.NewAgentChargeAmount,0)),			

			[TodayNewCharge] = (ISNULL(A.NewInAppChargeAmount,0)),
			[DiffNewCharge] = (ISNULL(A.NewInAppChargeAmount,0)) - (ISNULL(B.NewInAppChargeAmount,0)),			

			--[老用戶充值]
			--[TodayOldCharge] = (ISNULL(A.OldInAppChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0)),
			--[DiffOldCharge] = (ISNULL(A.OldInAppChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0)) - (ISNULL(B.OldInAppChargeAmount,0) + ISNULL(B.OldAgentChargeAmount,0)),					
			[TodayOldCharge] = (ISNULL(A.OldInAppChargeAmount,0)),
			[DiffOldCharge] = (ISNULL(A.OldInAppChargeAmount,0)) - (ISNULL(B.OldInAppChargeAmount,0)),					
			

			--[在線充值]
			--[TodayInAppCharge] = (ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0)),
			--[DiffInAppCharge] = (ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0)) - (ISNULL(B.NewInAppChargeAmount,0) + ISNULL(B.OldInAppChargeAmount,0)),			
			--[新用戶在線充值]
			--[TodayNewInAppChargeAmount] = ISNULL(A.NewInAppChargeAmount,0),
			--[DiffNewInAppChargeAmount] = (ISNULL(A.NewInAppChargeAmount,0) - ISNULL(B.NewInAppChargeAmount,0)),
			--[老用戶在線充值]
			--[TodayOldInAppChargeAmount] = ISNULL(A.OldInAppChargeAmount,0),
			--[DiffOldInAppChargeAmount] = (ISNULL(A.OldInAppChargeAmount,0) - ISNULL(B.OldInAppChargeAmount,0)),

			--[代理充值]
			[TodayAgentChargeAmount] = (ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0)),
			[DiffAgentChargeAmount] = (ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0)) - (ISNULL(B.NewAgentChargeAmount,0) + ISNULL(B.OldAgentChargeAmount,0)),
			--[新用戶代理充值]
			[TodayNewAgentChargeAmount] = ISNULL(A.NewAgentChargeAmount,0),
			[DiffNewAgentChargeAmount] = ISNULL(A.NewAgentChargeAmount,0) - ISNULL(B.NewAgentChargeAmount,0),
			--[老用戶代理充值]
			[TodayOldAgentChargeAmount] = ISNULL(A.OldAgentChargeAmount,0),
			[DiffOldAgentChargeAmount] = ISNULL(A.OldAgentChargeAmount,0) - ISNULL(B.OldAgentChargeAmount,0),
			--[充值人數]
			[TodayChargeUserCount] =  (ISNULL(A.NewInAppChargeUserCount,0) + ISNULL(A.OldInAppChargeUserCount,0)) + (ISNULL(A.NewAgentChargeUserCount,0) + ISNULL(A.OldAgentChargeUserCount,0)),
			[DiffChargeUserCount] = (ISNULL(A.NewInAppChargeUserCount,0) + ISNULL(A.OldInAppChargeUserCount,0)) + (ISNULL(A.NewAgentChargeUserCount,0) + ISNULL(A.OldAgentChargeUserCount,0))
									- (ISNULL(B.NewInAppChargeUserCount,0) + ISNULL(B.OldInAppChargeUserCount,0)) + (ISNULL(B.NewAgentChargeUserCount,0) + ISNULL(B.OldAgentChargeUserCount,0)),
			--[新用戶充值人數]
			[TodayNewChargeUserCount] =  (ISNULL(A.NewInAppChargeUserCount,0) + ISNULL(A.NewAgentChargeUserCount,0)),
			[DiffNewChargeUserCount] =  (ISNULL(A.NewInAppChargeUserCount,0) + ISNULL(A.NewAgentChargeUserCount,0)) - (ISNULL(B.NewInAppChargeUserCount,0) + ISNULL(B.NewAgentChargeUserCount,0)),
			--[老用戶充值人數]
			[TodayOldChargeUserCount] =  (ISNULL(A.OldInAppChargeUserCount,0) + ISNULL(A.OldAgentChargeUserCount,0)),
			[DiffOldChargeUserCount] =  (ISNULL(A.OldInAppChargeUserCount,0) + ISNULL(A.OldAgentChargeUserCount,0)) - (ISNULL(B.OldInAppChargeUserCount,0) + ISNULL(B.OldAgentChargeUserCount,0)),
			--[在線充值人數]
			[TodayInAppChargeUserCount] = (ISNULL(A.NewInAppChargeUserCount,0) + ISNULL(A.OldInAppChargeUserCount,0)),
			[DiffInAppChargeUserCount] = (ISNULL(A.NewInAppChargeUserCount,0) + ISNULL(A.OldInAppChargeUserCount,0)) - (ISNULL(B.NewInAppChargeUserCount,0) + ISNULL(B.OldInAppChargeUserCount,0)),
			--[新用戶在線充值人數]
			[TodayNewInAppChargeUserCount] = ISNULL(A.NewInAppChargeUserCount,0),
			[DiffNewInAppChargeUserCount] = (ISNULL(A.NewInAppChargeUserCount,0) - ISNULL(B.NewInAppChargeUserCount,0)),
			--[老用戶在線充值人數]
			[TodayOldInAppChargeUserCount] = ISNULL(A.OldInAppChargeUserCount,0),
			[DiffOldInAppChargeUserCount] = (ISNULL(A.OldInAppChargeUserCount,0) - ISNULL(B.OldInAppChargeUserCount,0)),
			--[代理充值人數]
			[TodayAgentChargeUserCount] = (ISNULL(A.NewAgentChargeUserCount,0) + ISNULL(A.OldAgentChargeUserCount,0)),
			[DiffAgentChargeUserCount] = (ISNULL(A.NewAgentChargeUserCount,0) + ISNULL(A.OldAgentChargeUserCount,0)) - (ISNULL(B.NewAgentChargeUserCount,0) + ISNULL(B.OldAgentChargeUserCount,0)),
			--[新用戶代理充值人數]
			[TodayNewAgentChargeUserCount] = ISNULL(A.NewAgentChargeUserCount,0),
			[DiffNewAgentChargeUserCount] = ISNULL(A.NewAgentChargeUserCount,0) - ISNULL(B.NewAgentChargeUserCount,0),
			--[老用戶代理充值人數]
			[TodayOldAgentChargeUserCount] = ISNULL(A.OldAgentChargeUserCount,0),
			[DiffOldAgentChargeUserCount] = ISNULL(A.OldAgentChargeUserCount,0) - ISNULL(B.OldAgentChargeUserCount,0),
			--[充值筆數]
			[TodayChargeTimes] = ISNULL(A.InAppChargeTimes,0) + ISNULL(A.AgentChargeTimes,0),
			[DiffChargeTimes] = (ISNULL(A.InAppChargeTimes,0) + ISNULL(A.AgentChargeTimes,0)) - (ISNULL(B.InAppChargeTimes,0) + ISNULL(B.AgentChargeTimes,0)),
			--[在線充值筆數]
			[TodayInAppChargeTimes] = ISNULL(A.InAppChargeTimes,0),
			[DiffInAppChargeTimes] = (ISNULL(A.InAppChargeTimes,0) - ISNULL(B.InAppChargeTimes,0)),
			--[代理充值筆數]
			[TodayAgentChargeTimes] = ISNULL(A.AgentChargeTimes,0),
			[DiffAgentChargeTimes] = (ISNULL(A.AgentChargeTimes,0) - ISNULL(B.AgentChargeTimes,0)),
			--[平均每付費用戶收入ARPPU]
			[TodayARPPU] = CASE WHEN ((ISNULL(A.NewInAppChargeUserCount,0) + ISNULL(A.OldInAppChargeUserCount,0)) + (ISNULL(A.NewAgentChargeUserCount,0) + ISNULL(A.OldAgentChargeUserCount,0))) > 0
									THEN ((ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0)) + (ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0))) * 1.00 / 
											((ISNULL(A.NewInAppChargeUserCount,0) + ISNULL(A.OldInAppChargeUserCount,0)) + (ISNULL(A.NewAgentChargeUserCount,0) + ISNULL(A.OldAgentChargeUserCount,0)))
								ELSE 0 END,
			[DiffARPPU] = CASE WHEN ((ISNULL(A.NewInAppChargeUserCount,0) + ISNULL(A.OldInAppChargeUserCount,0)) + (ISNULL(A.NewAgentChargeUserCount,0) + ISNULL(A.OldAgentChargeUserCount,0))) > 0
									AND ((ISNULL(B.NewInAppChargeUserCount,0) + ISNULL(B.OldInAppChargeUserCount,0)) + (ISNULL(B.NewAgentChargeUserCount,0) + ISNULL(B.OldAgentChargeUserCount,0))) > 0
									THEN ((ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0)) + (ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0))) * 1.00 / 
											((ISNULL(A.NewInAppChargeUserCount,0) + ISNULL(A.OldInAppChargeUserCount,0)) + (ISNULL(A.NewAgentChargeUserCount,0) + ISNULL(A.OldAgentChargeUserCount,0))) - 
										((ISNULL(B.NewInAppChargeAmount,0) + ISNULL(B.OldInAppChargeAmount,0)) + (ISNULL(B.NewAgentChargeAmount,0) + ISNULL(B.OldAgentChargeAmount,0))) * 1.00 / 
											((ISNULL(B.NewInAppChargeUserCount,0) + ISNULL(B.OldInAppChargeUserCount,0)) + (ISNULL(B.NewAgentChargeUserCount,0) + ISNULL(B.OldAgentChargeUserCount,0)))
								WHEN ((ISNULL(A.NewInAppChargeUserCount,0) + ISNULL(A.OldInAppChargeUserCount,0)) + (ISNULL(A.NewAgentChargeUserCount,0) + ISNULL(A.OldAgentChargeUserCount,0))) > 0
									AND ((ISNULL(B.NewInAppChargeUserCount,0) + ISNULL(B.OldInAppChargeUserCount,0)) + (ISNULL(B.NewAgentChargeUserCount,0) + ISNULL(B.OldAgentChargeUserCount,0))) <= 0
									THEN ((ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0)) + (ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0))) * 1.00 / 
											((ISNULL(A.NewInAppChargeUserCount,0) + ISNULL(A.OldInAppChargeUserCount,0)) + (ISNULL(A.NewAgentChargeUserCount,0) + ISNULL(A.OldAgentChargeUserCount,0))) - 0
								WHEN ((ISNULL(A.NewInAppChargeUserCount,0) + ISNULL(A.OldInAppChargeUserCount,0)) + (ISNULL(A.NewAgentChargeUserCount,0) + ISNULL(A.OldAgentChargeUserCount,0))) <= 0
									AND ((ISNULL(B.NewInAppChargeUserCount,0) + ISNULL(B.OldInAppChargeUserCount,0)) + (ISNULL(B.NewAgentChargeUserCount,0) + ISNULL(B.OldAgentChargeUserCount,0))) > 0
									THEN 0 - ((ISNULL(B.NewInAppChargeAmount,0) + ISNULL(B.OldInAppChargeAmount,0)) + (ISNULL(B.NewAgentChargeAmount,0) + ISNULL(B.OldAgentChargeAmount,0))) * 1.00 / 
											((ISNULL(B.NewInAppChargeUserCount,0) + ISNULL(B.OldInAppChargeUserCount,0)) + (ISNULL(B.NewAgentChargeUserCount,0) + ISNULL(B.OldAgentChargeUserCount,0)))
								ELSE 0 END,
			--[平均每用戶收入]
			[TodayARPU] = CASE	WHEN ISNULL(A.DAU,0) > 0
									THEN ((ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0)) + (ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0))) * 1.00 / A.DAU
								ELSE 0 END,
			[DiffARPU] = CASE	WHEN ISNULL(A.DAU,0) > 0 AND ISNULL(B.DAU,0) > 0
									THEN ((ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0)) + (ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0))) * 1.00 / A.DAU -
										((ISNULL(B.NewInAppChargeAmount,0) + ISNULL(B.OldInAppChargeAmount,0)) + (ISNULL(B.NewAgentChargeAmount,0) + ISNULL(B.OldAgentChargeAmount,0))) * 1.00 / B.DAU
								WHEN ISNULL(A.DAU,0) > 0 AND ISNULL(B.DAU,0) <= 0
									THEN ((ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0)) + (ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0))) * 1.00 / A.DAU - 0
								WHEN ISNULL(A.DAU,0) <= 0 AND ISNULL(B.DAU,0) > 0
									THEN 0 - ((ISNULL(B.NewInAppChargeAmount,0) + ISNULL(B.OldInAppChargeAmount,0)) + (ISNULL(B.NewAgentChargeAmount,0) + ISNULL(B.OldAgentChargeAmount,0))) * 1.00 / B.DAU
								ELSE 0 END,
			--[付費率]
			[TodayPayRate] = CASE	WHEN ISNULL(A.DAU,0) > 0 THEN ((ISNULL(A.NewInAppChargeUserCount,0) + ISNULL(A.OldInAppChargeUserCount,0)) + (ISNULL(A.NewAgentChargeUserCount,0) + ISNULL(A.OldAgentChargeUserCount,0))) * 1.00 / A.DAU * 100
								ELSE 0 END,
			[DiffPayRate] = CASE	WHEN ISNULL(A.DAU,0) > 0 AND ISNULL(B.DAU,0) > 0
										THEN (ISNULL(A.NewInAppChargeUserCount,0) + ISNULL(A.OldInAppChargeUserCount,0)) + (ISNULL(A.NewAgentChargeUserCount,0) + ISNULL(A.OldAgentChargeUserCount,0))  * 1.00 / A.DAU * 100
											- (ISNULL(B.NewInAppChargeUserCount,0) + ISNULL(B.OldInAppChargeUserCount,0)) + (ISNULL(B.NewAgentChargeUserCount,0) + ISNULL(B.OldAgentChargeUserCount,0)) * 1.00 / A.DAU * 100
									WHEN ISNULL(A.DAU,0) > 0 AND ISNULL(B.DAU,0) <= 0
										THEN ((ISNULL(A.NewInAppChargeUserCount,0) + ISNULL(A.OldInAppChargeUserCount,0)) + (ISNULL(A.NewAgentChargeUserCount,0) + ISNULL(A.OldAgentChargeUserCount,0)) * 1.00 / A.DAU * 100) - 0
									WHEN ISNULL(A.DAU,0) <= 0 AND ISNULL(B.DAU,0) > 0
										THEN (0 - (ISNULL(B.NewInAppChargeUserCount,0) + ISNULL(B.OldInAppChargeUserCount,0)) + (ISNULL(B.NewAgentChargeUserCount,0) + ISNULL(B.OldAgentChargeUserCount,0)) * 1.00 / B.DAU) * 100
								ELSE 0 END,
			--[人均貢獻]
			[TodayAvgLTV] = CASE	WHEN ISNULL(A.DAU,0) > 0
										THEN (ISNULL(A.UserTaxAmount,0) + ISNULL(A.SystemAmount,0)) * 1.00 / ISNULL(A.DAU,0)
									ELSE 0 END,
			[DiffAvgLTV] = CASE	WHEN ISNULL(A.DAU,0) > 0 AND ISNULL(B.DAU,0) > 0
									THEN ((ISNULL(A.UserTaxAmount,0) + ISNULL(A.SystemAmount,0)) * 1.00 / ISNULL(A.DAU,0)) - ((ISNULL(B.UserTaxAmount,0) + ISNULL(B.SystemAmount,0)) * 1.00 / ISNULL(B.DAU,0))
								WHEN ISNULL(A.DAU,0) > 0 AND ISNULL(B.DAU,0) <= 0
									THEN (ISNULL(A.UserTaxAmount,0) + ISNULL(A.SystemAmount,0)) * 1.00 / ISNULL(A.DAU,0) - 0
								WHEN ISNULL(A.DAU,0) <= 0 AND ISNULL(B.DAU,0) > 0
									THEN 0- (ISNULL(B.UserTaxAmount,0) + ISNULL(B.SystemAmount,0)) * 1.00 / ISNULL(B.DAU,0)
								ELSE 0 END,
			--[兌換]
			[TodayConvertAmount] = ABS(ISNULL(A.NewConvertAmount,0) + ISNULL(A.OldConvertAmount,0)),
			[DiffConvertAmount] = ((ISNULL(A.NewConvertAmount,0) + ISNULL(A.OldConvertAmount,0)) - (ISNULL(B.NewConvertAmount,0) + ISNULL(B.OldConvertAmount,0))),
			--[兌換人數]
			[TodayConvertUser] = ISNULL(A.ConvertUserCount,0),
			[DiffConvertUser] = (ISNULL(A.ConvertUserCount,0) - ISNULL(B.ConvertUserCount,0)),
			--[營收比] = (充值-兌換) /充值
			[TodayChargeConvertRate] = CASE WHEN (ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0) + ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0)) > 0
										THEN ((ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0) + ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0)) -
											(ISNULL(A.NewConvertAmount,0) + ISNULL(A.OldConvertAmount,0))) * 1.00 /
											(ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0) + ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0))
										ELSE 0 END,
			[DiffChargeConvertRate] = CASE WHEN (ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0) + ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0)) > 0
												AND (ISNULL(B.NewInAppChargeAmount,0) + ISNULL(B.OldInAppChargeAmount,0) + ISNULL(B.NewAgentChargeAmount,0) + ISNULL(B.OldAgentChargeAmount,0)) > 0
												THEN ((ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0) + ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0)) +
													(ISNULL(A.NewConvertAmount,0) + ISNULL(A.OldConvertAmount,0))) * 1.00 /
													(ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0) + ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0)) -
														((ISNULL(B.NewInAppChargeAmount,0) + ISNULL(B.OldInAppChargeAmount,0) + ISNULL(B.NewAgentChargeAmount,0) + ISNULL(B.OldAgentChargeAmount,0)) +
														(ISNULL(B.NewConvertAmount,0) + ISNULL(B.OldConvertAmount,0))) * 1.00 /
														(ISNULL(B.NewInAppChargeAmount,0) + ISNULL(B.OldInAppChargeAmount,0) + ISNULL(B.NewAgentChargeAmount,0) + ISNULL(B.OldAgentChargeAmount,0))
											WHEN (ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0) + ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0)) > 0
												AND (ISNULL(B.NewInAppChargeAmount,0) + ISNULL(B.OldInAppChargeAmount,0) + ISNULL(B.NewAgentChargeAmount,0) + ISNULL(B.OldAgentChargeAmount,0)) <= 0
												THEN  ((ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0) + ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0)) +
													(ISNULL(A.NewConvertAmount,0) + ISNULL(A.OldConvertAmount,0))) * 1.00 /
													(ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0) + ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0)) - 0
											WHEN (ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0) + ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0)) <= 0
												AND (ISNULL(B.NewInAppChargeAmount,0) + ISNULL(B.OldInAppChargeAmount,0) + ISNULL(B.NewAgentChargeAmount,0) + ISNULL(B.OldAgentChargeAmount,0)) > 0
												THEN 0 - ((ISNULL(B.NewInAppChargeAmount,0) + ISNULL(B.OldInAppChargeAmount,0) + ISNULL(B.NewAgentChargeAmount,0) + ISNULL(B.OldAgentChargeAmount,0)) +
													(ISNULL(B.NewConvertAmount,0) + ISNULL(B.OldConvertAmount,0))) * 1.00 /
													(ISNULL(B.NewInAppChargeAmount,0) + ISNULL(B.OldInAppChargeAmount,0) + ISNULL(B.NewAgentChargeAmount,0) + ISNULL(B.OldAgentChargeAmount,0))
										ELSE 0 END,
			--[營收] = 充值 - 兌換 
			--[TodayIncome] =  (ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0) + ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0)) - (ISNULL(A.NewConvertAmount,0) + ISNULL(A.OldConvertAmount,0)) * 1.00 / 100,
			--[DiffIncome] =  ((ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0) + ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0)) - (ISNULL(A.NewConvertAmount,0) + ISNULL(A.OldConvertAmount,0))
			--					- (ISNULL(B.NewInAppChargeAmount,0) + ISNULL(B.OldInAppChargeAmount,0) + ISNULL(B.NewAgentChargeAmount,0) + ISNULL(B.OldAgentChargeAmount,0)) - (ISNULL(B.NewConvertAmount,0) + ISNULL(B.OldConvertAmount,0))) * 1.00 / 100,
			
			[TodayIncome] = 
			(ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0) + ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0)) + (ISNULL(A.NewConvertAmount,0) + ISNULL(A.OldConvertAmount,0)),				
			
			[DiffIncome] =  
			((ISNULL(A.NewInAppChargeAmount,0) + ISNULL(A.OldInAppChargeAmount,0) + ISNULL(A.NewAgentChargeAmount,0) + ISNULL(A.OldAgentChargeAmount,0)) + (ISNULL(A.NewConvertAmount,0) + ISNULL(A.OldConvertAmount,0)))
			- ((ISNULL(B.NewInAppChargeAmount,0) + ISNULL(B.OldInAppChargeAmount,0) + ISNULL(B.NewAgentChargeAmount,0) + ISNULL(B.OldAgentChargeAmount,0)) + (ISNULL(B.NewConvertAmount,0) + ISNULL(B.OldConvertAmount,0))),
								
						
			
			--[代理進貨]
			[TodayAgentOrder] = 0,
			[DiffAgentOrder] = 0,
			--[昨日留存]
			[TodayOneDayKeep] = ISNULL(A.OneDayKeep,0),
			[DiffOneDayKeep] = ISNULL(A.OneDayKeep,0) - ISNULL(B.OneDayKeep,0),
			--[三日留存]
			[TodayThirdKeep] = ISNULL(A.ThirdKeep,0),
			[DiffThirdKeep] = ISNULL(A.ThirdKeep,0) - ISNULL(B.ThirdKeep,0),
			--[五日留存]
			[TodayFifthKeep] = ISNULL(A.FifthKeep,0),
			[DiffFifthKeep] = ISNULL(A.FifthKeep,0) - ISNULL(B.FifthKeep,0),
			--[七日留存]
			[TodayWeekKeep] = ISNULL(A.WeekKeep,0),
			[DiffWeekKeep] = ISNULL(A.WeekKeep,0) - ISNULL(B.WeekKeep,0),			
			--[注充比例]
			[TodayRegCharge] = CASE WHEN ISNULL(A.TotalRegCount,0) > 0 THEN (ISNULL(A.RegCharge,0) * 1.00 / ISNULL(A.TotalRegCount,0)) * 100
								ELSE 0 END,
			[DiffRegCharge] = CASE	WHEN ISNULL(A.TotalRegCount,0) > 0 AND ISNULL(B.TotalRegCount,0) > 0
										THEN ((ISNULL(A.RegCharge,0) * 1.00 / ISNULL(A.TotalRegCount,0)) * 100) - ((ISNULL(B.RegCharge,0) * 1.00 / ISNULL(B.TotalRegCount,0)) * 100)
									WHEN ISNULL(A.TotalRegCount,0) > 0 AND ISNULL(B.TotalRegCount,0) <= 0
										THEN ((ISNULL(A.RegCharge,0) * 1.00 / ISNULL(A.TotalRegCount,0)) * 100) - 0
									WHEN ISNULL(A.TotalRegCount,0) <= 0 AND ISNULL(B.TotalRegCount,0) > 0
										THEN (0 - ((ISNULL(A.RegCharge,0) * 1.00 / ISNULL(A.TotalRegCount,0)) * 100))
								ELSE 0 END,
			--[注綁比例]
			[TodayRegBind] = CASE WHEN ISNULL(A.TotalRegCount,0) > 0 THEN (ISNULL(A.RegBind,0) * 1.00 / ISNULL(A.TotalRegCount,0)) * 100
								ELSE 0 END,
			[DiffRegBind] = CASE	WHEN ISNULL(A.TotalRegCount,0) > 0 AND ISNULL(B.TotalRegCount,0) > 0
										THEN ((ISNULL(A.RegBind,0) * 1.00 / ISNULL(A.TotalRegCount,0)) * 100) - ((ISNULL(B.RegBind,0) * 1.00 / ISNULL(B.TotalRegCount,0)) * 100)
									WHEN ISNULL(A.TotalRegCount,0) > 0 AND ISNULL(B.TotalRegCount,0) <= 0
										THEN ((ISNULL(A.RegBind,0) * 1.00 / ISNULL(A.TotalRegCount,0)) * 100) - 0
									WHEN ISNULL(A.TotalRegCount,0) <= 0 AND ISNULL(B.TotalRegCount,0) > 0
										THEN (0 - ((ISNULL(A.RegBind,0) * 1.00 / ISNULL(A.TotalRegCount,0)) * 100))
								ELSE 0 END,
			--[綁充比例]
			[TodayBindCharge] = CASE WHEN ISNULL(A.TotalRegCount,0) > 0 THEN (ISNULL(A.BindCharge,0) * 1.00 / ISNULL(A.TotalRegCount,0)) * 100
									ELSE 0 END,
			[DiffBindCharge] = CASE	WHEN ISNULL(A.TotalRegCount,0) > 0 AND ISNULL(B.TotalRegCount,0) > 0
										THEN ((ISNULL(A.BindCharge,0) * 1.00 / ISNULL(A.TotalRegCount,0)) * 100) - ((ISNULL(B.BindCharge,0) * 1.00 / ISNULL(B.TotalRegCount,0)) * 100)
									WHEN ISNULL(A.TotalRegCount,0) > 0 AND ISNULL(B.TotalRegCount,0) <= 0
										THEN ((ISNULL(A.BindCharge,0) * 1.00 / ISNULL(A.TotalRegCount,0)) * 100) - 0
									WHEN ISNULL(A.TotalRegCount,0) <= 0 AND ISNULL(B.TotalRegCount,0) > 0
										THEN (0 - ((ISNULL(A.BindCharge,0) * 1.00 / ISNULL(A.TotalRegCount,0)) * 100))
								ELSE 0 END,
			--[系統總稅收]
			[TodayTotalTaxAmount] = (ISNULL(@TodayTotalTaxAmount,0)),
			[DiffTotalTaxAmount] = (ISNULL(@TodayTotalTaxAmount,0) - ISNULL(@LastTotalTaxAmount,0)),
			--[系統總輸贏]
			[TodayWinLoseAmount] = (ISNULL(@TodayTotalAmount,0)),
			[DiffWinLoseAmount] = (ISNULL(@TodayTotalAmount,0) - ISNULL(@LastTotalAmount,0))
	FROM @Dates AS Z
		LEFT JOIN @Temp_TodayData AS A ON Z.StatTime = A.StatTime
		LEFT JOIN @Temp_LastdayData AS B ON Z.StatTime = DATEADD(DAY,1,B.StatTime)
	WHERE
		Z.StatTime = @LogStartTime


	--回傳遊戲資料

	SELECT	C.GameName,
			--遊戲稅收
			[TodayTaxAmount] = SUM(ISNULL(A.Tax,0)),
			[DiffTaxAmount] = SUM(ISNULL(A.Tax,0)) - SUM(ISNULL(B.Tax,0)),
			--遊戲系統輸贏
			[TodaySystemAmount] = SUM(ISNULL(A.SystemAmount,0)),
			[DiffSystemAmount] = SUM(ISNULL(A.SystemAmount,0)) - SUM(ISNULL(B.SystemAmount,0)),
			--遊戲最高在线
			[TodayMAXCCU] = SUM(ISNULL(A.MAXCCU,0)),
			[DiffMAXCCU] = SUM(ISNULL(A.MAXCCU,0)) - SUM(ISNULL(B.MAXCCU,0)),
			--遊戲活躍人數
			[TodayDAU] = SUM(ISNULL(A.DAU,0)),
			[DiffDAU] = SUM(ISNULL(A.DAU,0)) - SUM(ISNULL(B.DAU,0)),
			--人均稅收貢獻
			[TodayARPPU] = CASE WHEN SUM(ISNULL(A.DAU,0)) <= 0 THEN SUM(ISNULL(A.[UserTaxAmount],0))
								 WHEN SUM(ISNULL(A.DAU,0)) > 0 THEN SUM(ISNULL(A.[UserTaxAmount],0)) / SUM(ISNULL(A.DAU,0))
								ELSE 0 END,
			[DiffARPPU] = CASE	WHEN SUM(ISNULL(A.DAU,0)) > 0 AND SUM(ISNULL(B.DAU,0)) > 0 THEN SUM(ISNULL(A.[UserTaxAmount],0)) / SUM(ISNULL(A.DAU,0)) - SUM(ISNULL(B.[UserTaxAmount],0)) / SUM(ISNULL(B.DAU,0))
								WHEN SUM(ISNULL(A.DAU,0)) > 0 AND SUM(ISNULL(B.DAU,0)) <= 0 THEN SUM(ISNULL(A.[UserTaxAmount],0)) / SUM(ISNULL(A.DAU,0)) - 0
								WHEN SUM(ISNULL(A.DAU,0)) <= 0 AND SUM(ISNULL(B.DAU,0)) > 0 THEN 0 - SUM(ISNULL(B.[UserTaxAmount],0)) / SUM(ISNULL(B.DAU,0))
								ELSE 0 END,
			--人均貢獻
			[TodayARPU] = CASE WHEN SUM(ISNULL(A.DAU,0)) <= 0 THEN SUM(ISNULL(A.UserTaxSystemAmount,0))
								 WHEN SUM(ISNULL(A.DAU,0)) > 0 THEN SUM(ISNULL(A.UserTaxSystemAmount,0)) / SUM(ISNULL(A.DAU,0))
								ELSE 0 END,
			[DiffARPU] = CASE	WHEN SUM(ISNULL(A.DAU,0)) > 0 AND SUM(ISNULL(B.DAU,0)) > 0 THEN SUM(ISNULL(A.UserTaxSystemAmount,0)) / SUM(ISNULL(A.DAU,0)) - SUM(ISNULL(B.UserTaxSystemAmount,0)) / SUM(ISNULL(B.DAU,0))
								WHEN SUM(ISNULL(A.DAU,0)) > 0 AND SUM(ISNULL(B.DAU,0)) <= 0 THEN SUM(ISNULL(A.UserTaxSystemAmount,0)) / SUM(ISNULL(A.DAU,0)) - 0
								WHEN SUM(ISNULL(A.DAU,0)) <= 0 AND SUM(ISNULL(B.DAU,0)) > 0 THEN 0 - SUM(ISNULL(B.UserTaxSystemAmount,0)) / SUM(ISNULL(B.DAU,0) )
								ELSE 0 END,
			--遊戲稅收貢獻佔比
			[TodayARPPURate] = CASE	WHEN ISNULL(@TodayTotalTaxAmount,0) > 0 AND ISNULL(@LastTotalTaxAmount,0) > 0
										THEN (SUM(ISNULL(A.[UserTaxAmount],0)) * 1.00 / ISNULL(@TodayTotalTaxAmount,0)) * 100
									WHEN ISNULL(@TodayTotalTaxAmount,0) > 0 AND ISNULL(@LastTotalTaxAmount,0) <= 0
										THEN (SUM(ISNULL(A.[UserTaxAmount],0)) * 1.00 / ISNULL(@TodayTotalTaxAmount,0) - 0) * 100
									WHEN ISNULL(@TodayTotalTaxAmount,0) <= 0 AND ISNULL(@LastTotalTaxAmount,0) > 0
										THEN (0 - SUM(ISNULL(A.[UserTaxAmount],0)) * 1.00 / ISNULL(@LastTotalTaxAmount,0)) * 100
								ELSE 0 END,
			[DiffARPPURate] = CASE	WHEN ISNULL(@TodayTotalTaxAmount,0) > 0 AND ISNULL(@LastTotalTaxAmount,0) > 0
										THEN ((SUM(ISNULL(A.[UserTaxAmount],0)) * 1.00 / ISNULL(@TodayTotalTaxAmount,0) * 100) - (SUM(ISNULL(B.[UserTaxAmount],0)) * 1.00 / ISNULL(@LastTotalTaxAmount,0)) * 100)
									WHEN ISNULL(@TodayTotalTaxAmount,0) > 0 AND ISNULL(@LastTotalTaxAmount,0) <= 0
										THEN (SUM(ISNULL(A.[UserTaxAmount],0)) * 1.00 / ISNULL(@TodayTotalTaxAmount,0) - 0) * 100
									WHEN ISNULL(@TodayTotalTaxAmount,0) <= 0 AND ISNULL(@LastTotalTaxAmount,0) > 0
										THEN (0 - SUM(ISNULL(B.[UserTaxAmount],0)) * 1.00 / ISNULL(@LastTotalTaxAmount,0)) * 100
								ELSE 0 END,
			--遊戲貢獻佔比
			[TodayARPURate] = CASE WHEN (ISNULL(@TodayTotalTaxAmount,0) + ISNULL(@TodayTotalAmount,0)) <> 0
									THEN (SUM(ISNULL(A.Tax,0)) + SUM(ISNULL(A.SystemAmount,0))) * 1.00 / (ISNULL(@TodayTotalTaxAmount,0) + ISNULL(@TodayTotalAmount,0)) * 100
									ELSE 0 END,
			[DiffARPURate] = CASE	WHEN (ISNULL(@TodayTotalTaxAmount,0) + ISNULL(@TodayTotalAmount,0)) <> 0 AND (ISNULL(@LastTotalTaxAmount,0) + ISNULL(@LastTotalAmount,0)) <> 0
										THEN ((SUM(ISNULL(A.Tax,0)) + SUM(ISNULL(A.SystemAmount,0))) * 1.00 / (ISNULL(@TodayTotalTaxAmount,0) + ISNULL(@TodayTotalAmount,0)) * 100) -
											((SUM(ISNULL(B.Tax,0)) + SUM(ISNULL(B.SystemAmount,0))) * 1.00 / (ISNULL(@LastTotalTaxAmount,0) + ISNULL(@LastTotalAmount,0)) * 100)
									WHEN (ISNULL(@TodayTotalTaxAmount,0) + ISNULL(@TodayTotalAmount,0)) <> 0 AND (ISNULL(@LastTotalTaxAmount,0) + ISNULL(@LastTotalAmount,0)) = 0
										THEN ((SUM(ISNULL(A.Tax,0)) + SUM(ISNULL(A.SystemAmount,0))) * 1.00 / (ISNULL(@TodayTotalTaxAmount,0) + ISNULL(@TodayTotalAmount,0)) - 0) * 100
									WHEN (ISNULL(@TodayTotalTaxAmount,0) + ISNULL(@TodayTotalAmount,0)) = 0 AND (ISNULL(@LastTotalTaxAmount,0) + ISNULL(@LastTotalAmount,0)) <> 0
										THEN (0 - (SUM(ISNULL(B.Tax,0)) + SUM(ISNULL(B.SystemAmount,0))) * 1.00 / (ISNULL(@LastTotalTaxAmount,0) + ISNULL(@LastTotalAmount,0))) * 100
									ELSE 0 END
	FROM [ServerInfo].[GameList] AS C
		LEFT JOIN @Temp_TodayGameData AS A ON C.GameID = A.GameID
		LEFT JOIN @Temp_LastGameData AS B ON C.GameID = B.GameID
	GROUP BY C.GameID,C.GameName
	ORDER BY C.GameID ASC 

	-------------------------------------------------------------------------------------------

	



END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetDailyReport] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetDailyReport] TO [xbuser]
    AS [dbo];

