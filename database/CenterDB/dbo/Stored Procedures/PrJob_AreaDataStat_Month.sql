-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<统计渠道日报表相关数据>
--	2018/03/20	Jason		調整取LOGDB寫法及新增變數接參數
--	2018/03/28	Jason		調整報表統計方式 修正原本以@dtStatTime 取當天日期做統計會造成統計資料不正確問題
--	2018/03/29	Jason		調整報表統計流程
--	2018/05/16	Jason		#257 報表統計日月報表調整每30分鐘執行一次計算至當下時間
--	2018/05/25	Jason		#349 報表 - 建立統計數據報表提供API回傳 新增TotalConvertUser,TotalConvertUser,Total_OrderCounts,NewOrderCounts
--	2018/06/29	jason		調整convert條件 取消createtime並改用lastupdatetime判斷
--	2018/07/03	alan		充值統計新增代理充值
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_AreaDataStat_Month]
(
	@dtStatTime		DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @bitIsStatLast bit	--是否需要统计跨天时上一天的统计数据
	DECLARE @LogStartTime		DATETIME
	DECLARE @LogEndTime			DATETIME
	DECLARE	@LogMonthStartTime	DATETIME
	DECLARE	@LogMonthEndTime	DATETIME
	DECLARE @LogStatus			TINYINT = 3
	--DECLARE @SourceTypeID		INT = 10000
	
	SET @dtStatTime			= ISNULL(@dtStatTime,GETDATE())

	IF (DATEPART(HOUR,@dtStatTime) = 0) AND (DATEPART(MINUTE,@dtStatTime) < 10)
	BEGIN
		SET @LogStartTime		= DATEADD(DAY,-1,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogEndTime			= DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogMonthStartTime	= DATEADD(MONTH,DATEDIFF(MONTH,0,@LogStartTime),0)
		SET @LogMonthEndTime	= DATEADD(MONTH,1,DATEADD(MONTH,DATEDIFF(MONTH,0,@LogStartTime),0))
	END
	ELSE
	BEGIN
		SET @LogStartTime		= DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogEndTime			= DATEADD(DAY,1,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogMonthStartTime	= DATEADD(MONTH,DATEDIFF(MONTH,0,@LogStartTime),0)
		SET @LogMonthEndTime	= DATEADD(MONTH,1,DATEADD(MONTH,DATEDIFF(MONTH,0,@LogStartTime),0))
	END


	--CREATE TABLE #Temp_AreaData_Month
	--	(	
	--		[StatTime]				DATETIME NULL,
	--		[PlatformID]			INT NULL DEFAULT ((1)),
	--		[AreaID]				INT NULL,
	--		[GameID]				INT NULL,
	--		[ServerID]				INT NULL,
	--		[OldLogin]				BIGINT NULL DEFAULT ((0)),
	--		[NewLogin]				BIGINT NULL DEFAULT ((0)),
	--		[OldCharge]				BIGINT NULL DEFAULT ((0)),
	--		[NewCharge]				BIGINT NULL DEFAULT ((0)),
	--		[OldConvert]			BIGINT NULL DEFAULT ((0)),
	--		[NewConvert]			BIGINT NULL DEFAULT ((0)),
	--		[OldConvertTax]			BIGINT NULL DEFAULT ((0)),
	--		[NewConvertTax]			BIGINT NULL DEFAULT ((0)),
	--		[LoginIP]				BIGINT NULL DEFAULT ((0)),
	--		[OldLoginIP]			BIGINT NULL DEFAULT ((0)),
	--		[NewLoginIP]			BIGINT NULL DEFAULT ((0)),
	--		[LoginMac]				BIGINT NULL DEFAULT ((0)),
	--		[OldLoginMac]			BIGINT NULL DEFAULT ((0)),
	--		[NewLoginMac]			BIGINT NULL DEFAULT ((0)),
	--		[GameTax]				BIGINT NULL DEFAULT ((0)),
	--		[GameTaxDetail]			VARCHAR(MAX) NULL DEFAULT (''),
	--		[TotalOrderCounts]		BIGINT NULL DEFAULT ((0)),
	--		[NewOrderCounts]		BIGINT NULL DEFAULT ((0)),
	--		[iOSCount]				INT NULL DEFAULT ((0)),
	--		[NewiOSCount]			INT NULL DEFAULT ((0)),
	--		[AndRoidCount]			INT NULL DEFAULT ((0)),
	--		[NewAndRoidCount]		INT NULL DEFAULT ((0)),
	--		[PCCount]				INT NULL DEFAULT ((0)),
	--		[NewPCCount]			INT NULL DEFAULT ((0)),
	--		[AmountTotal]			BIGINT NULL DEFAULT ((0)),
	--		[NewAmountTotal]		BIGINT NULL DEFAULT ((0)),
	--		[AmountAvg]				BIGINT NULL DEFAULT ((0)),
	--		[NewAmountAvg]			BIGINT NULL DEFAULT ((0)),
	--		[TaxAmountTotal]		BIGINT NULL DEFAULT ((0)),
	--		[NewTaxAmountTotal]		BIGINT NULL DEFAULT ((0)),
	--		[TaxAmountAvg]			BIGINT NULL DEFAULT ((0)),
	--		[NewTaxAmountAvg]		BIGINT NULL DEFAULT ((0)),
	--		[GameTimeMax]			INT NULL DEFAULT ((0)),
	--		[GameTimeMin]			INT NULL DEFAULT ((0)),
	--		[GameTimeAvg]			INT NULL DEFAULT ((0)),
	--		[RobotTaxAmount]		BIGINT NULL DEFAULT ((0)),
	--		[TotalConvertUser]		BIGINT NULL DEFAULT ((0)),
	--		[TotalConvertCounts]	BIGINT NULL DEFAULT ((0)),
	--		[Total_OrderUser]		BIGINT NULL DEFAULT ((0)),
	--		[NewOrderUser]			BIGINT NULL DEFAULT ((0))
	--	)

	CREATE TABLE #Temp_GameLogStatData
		(
			[StatTime]				DATETIME NOT NULL,
			[PlatformID]			INT NULL,
			[AreaID]				INT NULL,
			[GameID]				INT NULL,
			[ServerID]				INT NULL,
			[UserCount]				INT NULL,
			[NewUser]				INT NULL,
			[IPCount]				INT NULL,
			[NewIPCount]			INT NULL,
			[MacCount]				INT NULL,
			[NewMacCount]			INT NULL,
			[iOSCount]				INT NULL,
			[NewiOSCount]			INT NULL,
			[AndRoidCount]			INT NULL,
			[NewAndRoidCount]		INT NULL,
			[PCCount]				INT NULL,
			[NewPCCount]			INT NULL,
			[AmountTotal]			BIGINT NULL,
			[NewAmountTotal]		BIGINT NULL,
			[AmountAvg]				BIGINT NULL,
			[NewAmountAvg]			BIGINT NULL,
			[TaxAmountTotal]		BIGINT NULL,
			[NewTaxAmountTotal]		BIGINT NULL,
			[TaxAmountAvg]			BIGINT NULL,
			[NewTaxAmountAvg]		BIGINT NULL,
			[GameTimeMax]			INT NULL,
			[GameTimeMin]			INT NULL,
			[GameTimeAvg]			INT NULL,
			[RobotTaxAmount]		BIGINT NULL DEFAULT ((0))
		)

	


	/*  --取LogDB以統計好的數據暫存  */
	----------------------------------------------------------------------------------------------
	INSERT INTO #Temp_GameLogStatData
	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrPs_GetStatisticsData]  --有登入過遊戲的帳號統計
		@intTypeID = 2,
		@dtStatTime = @LogMonthStartTime,
		@dtEndTime = @LogMonthEndTime
	----------------------------------------------------------------------------------------------


	/*  --统计充值数据  */
	----------------------------------------------------------------------------------------------
	--取充值紀錄暫存
	CREATE TABLE #TempCharge (PlatformID INT,AreaID INT,NewOrderUser INT,TotalOrderUser INT,NewOrderCounts INT,TotalOrderCounts INT,OldCharge INT,NewCharge INT)
	
	INSERT INTO #TempCharge
	SELECT	A.PlatformID,
			U.AreaID,
			NewOrderUser = COUNT(DISTINCT CASE WHEN u.CreateTime >= @LogMonthStartTime THEN u.UserID ELSE NULL END),
			TotalOrderUser = COUNT(DISTINCT u.UserID),
			NewOrderCounts = SUM(CASE WHEN u.CreateTime >= @LogMonthStartTime THEN 1 ELSE 0 END),
			TotalOrderCounts = COUNT(L.LogID),
			OldCharge = ISNULL(SUM(CASE WHEN u.CreateTime < @LogMonthStartTime THEN UpdateAmount/100 ELSE 0 END),0),
			NewCharge = ISNULL(SUM(CASE WHEN u.CreateTime >= @LogMonthStartTime THEN UpdateAmount/100 ELSE 0 END),0)
	FROM Logdb.Money.MoneyLog AS L WITH (NOLOCK)
		INNER JOIN Users.UserInfo AS u WITH (NOLOCK) ON L.UserID=U.UserID
		INNER JOIN Area.Area AS A WITH (NOLOCK) ON u.AreaID = A.AreaID
	WHERE
		L.CreateTime >= @LogMonthStartTime
		AND L.CreateTime < @LogMonthEndTime
		AND L.SourceTypeID >=11001 and L.SourceTypeID <=11020 --此段範圍都屬充值類型
	GROUP BY A.PlatformID,U.AreaID
	UNION ALL
	SELECT	A.PlatformID,
			0 AS AreaID,
			NewOrderUser = COUNT(DISTINCT CASE WHEN u.CreateTime >= @LogMonthStartTime THEN u.UserID ELSE NULL END),
			TotalOrderUser = COUNT(DISTINCT u.UserID),
			NewOrderCounts = SUM(CASE WHEN u.CreateTime >= @LogMonthStartTime THEN 1 ELSE 0 END),
			TotalOrderCounts = COUNT(L.LogID),
			OldCharge = ISNULL(SUM(CASE WHEN u.CreateTime < @LogMonthStartTime THEN UpdateAmount/100 ELSE 0 END),0),
			NewCharge = ISNULL(SUM(CASE WHEN u.CreateTime >= @LogMonthStartTime THEN UpdateAmount/100 ELSE 0 END),0)
	FROM Logdb.Money.MoneyLog AS L WITH (NOLOCK)
		INNER JOIN Users.UserInfo AS u WITH (NOLOCK) ON L.UserID=U.UserID
		INNER JOIN Area.Area AS A WITH (NOLOCK) ON u.AreaID = A.AreaID
	WHERE
		L.CreateTime >= @LogMonthStartTime
		AND L.CreateTime < @LogMonthEndTime
		AND L.SourceTypeID >=11001 and L.SourceTypeID <=11020 --此段範圍都屬充值類型
	GROUP BY A.PlatformID
	UNION ALL
	SELECT	0 AS PlatformID,
			0 AS AreaID,
			NewOrderUser = COUNT(DISTINCT CASE WHEN u.CreateTime >= @LogMonthStartTime THEN u.UserID ELSE NULL END),
			TotalOrderUser = COUNT(DISTINCT u.UserID),
			NewOrderCounts = SUM(CASE WHEN u.CreateTime >= @LogMonthStartTime THEN 1 ELSE 0 END),
			TotalOrderCounts = COUNT(L.LogID),
			OldCharge = ISNULL(SUM(CASE WHEN u.CreateTime < @LogMonthStartTime THEN UpdateAmount/100 ELSE 0 END),0),
			NewCharge = ISNULL(SUM(CASE WHEN u.CreateTime >= @LogMonthStartTime THEN UpdateAmount/100 ELSE 0 END),0)
	FROM Logdb.Money.MoneyLog AS L WITH (NOLOCK)
		INNER JOIN Users.UserInfo AS u WITH (NOLOCK) ON L.UserID=U.UserID
		INNER JOIN Area.Area AS A WITH (NOLOCK) ON u.AreaID = A.AreaID
	WHERE
		L.CreateTime >= @LogMonthStartTime
		AND L.CreateTime < @LogMonthEndTime
		AND L.SourceTypeID >=11001 and L.SourceTypeID <=11020 --此段範圍都屬充值類型

	----------------------------------------------------------------------------------------------





	/*  --统计代理充值数据  */
	----------------------------------------------------------------------------------------------	
	SELECT	A.PlatformID,
			U.AreaID,
			OldAgentChargeUser = COUNT(DISTINCT CASE WHEN u.CreateTime < @LogMonthStartTime THEN u.UserID ELSE NULL END),
			NewAgentChargeUser = COUNT(DISTINCT CASE WHEN u.CreateTime >= @LogMonthStartTime THEN u.UserID ELSE NULL END),
			OldAgentChargeAmount = ISNULL(SUM(CASE WHEN u.CreateTime < @LogMonthStartTime THEN UpdateAmount ELSE 0 END),0),
			NewAgentChargeAmount = ISNULL(SUM(CASE WHEN u.CreateTime >= @LogMonthStartTime THEN UpdateAmount ELSE 0 END),0)
    INTO #TempAgentCharge
	FROM Logdb.Money.MoneyLog AS L WITH (NOLOCK)
		INNER JOIN Users.UserInfo AS u WITH (NOLOCK) ON L.UserID=U.UserID
		INNER JOIN Area.Area AS A WITH (NOLOCK) ON u.AreaID = A.AreaID
	WHERE
		L.CreateTime >= @LogMonthStartTime
		AND L.CreateTime < @LogMonthEndTime
		AND L.SourceTypeID = 11008
	GROUP BY A.PlatformID,U.AreaID
	
	----------------------------------------------------------------------------------------------







	/*  --统计兑换数量与手续费  */
	----------------------------------------------------------------------------------------------

	CREATE TABLE #TempConvert (PlatformID INT,AreaID INT,OldConvert INT,NewConvert INT,OldConvertTax INT,NewConvertTax INT,TotalConvertUser INT,NewConvertUser INT,TotalConvertCounts INT)


	INSERT INTO #TempConvert
	SELECT	A.PlatformID,
			U.AreaID,
			SUM(CASE WHEN u.CreateTime < @LogMonthStartTime AND L.Status = 3 THEN Amount ELSE 0 END) AS OldConvert,
			SUM(CASE WHEN u.CreateTime >= @LogMonthStartTime AND L.Status = 3 THEN Amount ELSE 0 END) AS NewConvert,
			SUM(CASE WHEN u.CreateTime < @LogMonthStartTime AND L.Status = 3 THEN TaxAmount ELSE 0 END) AS OldConvertTax,
			SUM(CASE WHEN u.CreateTime > @LogMonthStartTime AND L.Status = 3 THEN TaxAmount ELSE 0 END) AS NewConvertTax,
			COUNT(DISTINCT L.UserID) AS TotalConvertUser,
			COUNT(DISTINCT CASE WHEN u.CreateTime >= @LogMonthStartTime AND L.Status = 3 THEN L.UserID ELSE NULL END) AS NewConvertUser,
			COUNT(L.UserID) AS TotalConvertCounts
	FROM [Logdb].[Money].[ConvertLog] AS L WITH (NOLOCK)
		INNER JOIN Users.UserInfo AS u WITH (NOLOCK) ON L.UserID=U.UserID
		INNER JOIN Area.Area AS A WITH (NOLOCK) ON u.AreaID = A.AreaID
	WHERE
		L.[LastUpdateTime] >= @LogMonthStartTime
		AND L.[LastUpdateTime] < @LogMonthEndTime
		AND L.Status=3
	GROUP BY A.PlatformID,U.AreaID
	UNION ALL
	SELECT	A.PlatformID,
			0 AS AreaID,
			SUM(CASE WHEN u.CreateTime < @LogMonthStartTime AND L.Status = 3 THEN Amount ELSE 0 END) AS OldConvert,
			SUM(CASE WHEN u.CreateTime >= @LogMonthStartTime AND L.Status = 3 THEN Amount ELSE 0 END) AS NewConvert,
			SUM(CASE WHEN u.CreateTime < @LogMonthStartTime AND L.Status = 3 THEN TaxAmount ELSE 0 END) AS OldConvertTax,
			SUM(CASE WHEN u.CreateTime > @LogMonthStartTime AND L.Status = 3 THEN TaxAmount ELSE 0 END) AS NewConvertTax,
			COUNT(DISTINCT L.UserID) AS TotalConvertUser,
			COUNT(DISTINCT CASE WHEN u.CreateTime >= @LogMonthStartTime AND L.Status = 3 THEN L.UserID ELSE NULL END) AS NewConvertUser,
			COUNT(L.UserID) AS TotalConvertCounts
	FROM [Logdb].[Money].[ConvertLog]  AS L WITH (NOLOCK)
		INNER JOIN Users.UserInfo AS u WITH (NOLOCK) ON L.UserID=U.UserID
		INNER JOIN Area.Area AS A WITH (NOLOCK) ON u.AreaID = A.AreaID
	WHERE
		L.[LastUpdateTime] >= @LogMonthStartTime
		AND L.[LastUpdateTime] < @LogMonthEndTime
		AND L.Status=3
	GROUP BY A.PlatformID
	UNION ALL
	SELECT	0 AS PlatformID,
			0 AS AreaID,
			SUM(CASE WHEN u.CreateTime < @LogMonthStartTime AND L.Status = 3 THEN Amount ELSE 0 END) AS OldConvert,
			SUM(CASE WHEN u.CreateTime >= @LogMonthStartTime AND L.Status = 3 THEN Amount ELSE 0 END) AS NewConvert,
			SUM(CASE WHEN u.CreateTime < @LogMonthStartTime AND L.Status = 3 THEN TaxAmount ELSE 0 END) AS OldConvertTax,
			SUM(CASE WHEN u.CreateTime > @LogMonthStartTime AND L.Status = 3 THEN TaxAmount ELSE 0 END) AS NewConvertTax,
			COUNT(DISTINCT L.UserID) AS TotalConvertUser,
			COUNT(DISTINCT CASE WHEN u.CreateTime >= @LogMonthStartTime AND L.Status = 3 THEN L.UserID ELSE NULL END) AS NewConvertUser,
			COUNT(L.UserID) AS TotalConvertCounts
	FROM [Logdb].[Money].[ConvertLog]  AS L WITH (NOLOCK)
		INNER JOIN Users.UserInfo AS u WITH (NOLOCK) ON L.UserID=U.UserID
		INNER JOIN Area.Area AS A WITH (NOLOCK) ON u.AreaID = A.AreaID
	WHERE
		L.[LastUpdateTime] >= @LogMonthStartTime
		AND L.[LastUpdateTime] < @LogMonthEndTime
		AND L.Status=3
	----------------------------------------------------------------------------------------------


	/*  --统计税收  */
	----------------------------------------------------------------------------------------------
	CREATE TABLE #TempGameTaxDetail (PlatformID INT,AreaID INT,GameName NVARCHAR(128),UserTaxAmount BIGINT,RobotTaxAmount BIGINT)
	INSERT INTO #TempGameTaxDetail

	--20181222 alan調整作法
	SELECT PlatformID,AreaID,GameName,UserTaxAmount,RobotTaxAmount from (
		select g.platformid,g.areaid, g.gameid, GL.GameName,
		SUM(case when u.UserType <> 2 then G.TaxAmount else 0 end) as UserTaxAmount,
		--COUNT(case when u.UserType <> 2 then g.TaxAmount else null end) as userTaxCount,
		SUM(case when u.UserType = 2 then G.TaxAmount else 0 end) as RobotTaxAmount
		from logdb.game.GameLog G 
		left join users.UserInfo U on g.UserID=u.UserID
		left join ServerInfo.GameList GL on G.GameID=GL.GameID
		where LogoutTime >= @LogMonthStartTime and LogoutTime < @LogMonthEndTime
		group by G.platformid, G.areaid, G.gameid, GL.GameName
	) as A
	----------------------------------------------------------------------------------------------




	/*  --更新用戶統計資料   A.GameID=0 AND A.ServerID=0 */
	----------------------------------------------------------------------------------------------

	SELECT StatTime=@LogMonthStartTime,
		A.PlatformID,
		A.Areaid, 
		0 as GameID,
		0 as ServerID,	
		[TotalOrderUser] = ISNULL(B.TotalOrderUser,0),
		[NewOrderUser] = ISNULL(B.[NewOrderUser],0),
		[TotalOrderCounts] = ISNULL(B.[TotalOrderCounts],0),
		[NewOrderCounts] = ISNULL(B.[NewOrderCounts],0),
		[OldCharge] = ISNULL(B.[OldCharge],0),
		[NewCharge] = ISNULL(B.[NewCharge],0),

		[OldConvert] = ISNULL(C.[OldConvert],0),
		[NewConvert] = ISNULL(C.[NewConvert],0),
		[OldConvertTax] = ISNULL(C.[OldConvertTax],0),
		[NewConvertTax] = ISNULL(C.[NewConvertTax],0),

		TotalConvertUser = ISNULL(C.TotalConvertUser,0),
		NewConvertUser = ISNULL(C.NewConvertUser,0),
		TotalConvertCounts = ISNULL(C.TotalConvertCounts,0),

		NewAgentChargeUser = ISNULL(D.NewAgentChargeUser,0),
		OldAgentChargeUser = ISNULL(D.OldAgentChargeUser,0),
		NewAgentChargeAmount = ISNULL(D.NewAgentChargeAmount,0),
		OldAgentChargeAmount = ISNULL(D.OldAgentChargeAmount,0)

	INTO #TempUserData
	FROM Area.Area A WITH(NOLOCK)
	LEFT JOIN #TempCharge AS B  ON A.PlatformID = B.PlatformID AND A.AreaID = B.AreaID	--充值紀錄
	LEFT JOIN #TempConvert AS C ON A.PlatformID = C.PlatformID AND A.AreaID = C.AreaID	--兌換紀錄及手續費
	LEFT JOIN #TempAgentCharge AS D ON A.PlatformID = D.PlatformID AND A.AreaID = D.AreaID	--代理充值紀錄



	MERGE Stat.AreaDataMonth AS A
	USING
	(
		SELECT	* FROM #TempUserData
	) AS B 
	ON (A.[StatTime] = B.[StatTime] AND A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID] AND A.GameID=0 AND A.ServerID=0)
	WHEN MATCHED THEN
		UPDATE SET	

				A.Total_OrderUser = B.TotalOrderUser, --今日不重複充值人數
				A.NewOrderUser = B.NewOrderUser,      --新用戶不重複充值人數
				A.[TotalOrderCounts] = B.[TotalOrderCounts], --總付費筆數
				A.[NewOrderCounts] = B.[NewOrderCounts],     --新用戶付費筆數
				A.[OldCharge] = B.[OldCharge], --舊用戶充值金額
				A.[NewCharge] = B.[NewCharge], --新用戶充值金額

				A.TotalConvertUser = B.TotalConvertUser, --兌換去重用戶數
				A.NewConvertUser = B.NewConvertUser,     --新用戶兌換去重用戶數
				A.TotalConvertCounts = B.TotalConvertCounts, --兌換筆數
				A.[OldConvert] = B.[OldConvert],       --舊用戶兌換金額
				A.[NewConvert] = B.[NewConvert],       --新用戶兌換金額
				A.[OldConvertTax] = B.[OldConvertTax], --舊用戶兌換手續費金額
				A.[NewConvertTax] = B.[NewConvertTax], --新用戶兌換手續費金額

				A.NewAgentChargeUser = B.NewAgentChargeUser, --新用戶代理充值不重複人數
				A.OldAgentChargeUser = B.OldAgentChargeUser, --舊用戶代理充值不重複人數
				A.NewAgentChargeAmount = B.NewAgentChargeAmount, --新用戶代理充值金額
				A.OldAgentChargeAmount = B.OldAgentChargeAmount --舊用戶代理充值金額




	WHEN NOT MATCHED THEN
		INSERT
			(
				[StatTime],[PlatformID],[AreaID],[GameID],[ServerID],[OldCharge],[NewCharge],
				[OldConvert],[NewConvert],[OldConvertTax],[NewConvertTax],
				[TotalOrderCounts],[NewOrderCounts],	
				TotalConvertUser,NewConvertUser,TotalConvertCounts,Total_OrderUser,NewOrderUser,
				NewAgentChargeUser,OldAgentChargeUser,NewAgentChargeAmount,OldAgentChargeAmount
			)
		VALUES
			(
				[StatTime],[PlatformID],[AreaID],[GameID],[ServerID],[OldCharge],[NewCharge],
				[OldConvert],[NewConvert],[OldConvertTax],[NewConvertTax],
				[TotalOrderCounts],[NewOrderCounts],	
				TotalConvertUser,NewConvertUser,TotalConvertCounts,TotalOrderUser,NewOrderUser,
				NewAgentChargeUser,OldAgentChargeUser,NewAgentChargeAmount,OldAgentChargeAmount
			);

	----------------------------------------------------------------------------------------------













	/*  --更新遊戲統計資料  */
	----------------------------------------------------------------------------------------------
	--INSERT INTO #Temp_AreaData	
	SELECT	[StatTime] = @LogMonthStartTime,
			A.PlatformID,
			A.AreaID,
			A.GameID,
			A.ServerID,
			[OldLogin] = ISNULL(A.[UserCount],0) - ISNULL(A.[NewUser],0),
			[NewLogin] = ISNULL(A.[NewUser],0),

			[OldCharge] = ISNULL(B.[OldCharge],0),
			[NewCharge] = ISNULL(B.[NewCharge],0),
			[OldConvert] = ISNULL(C.[OldConvert],0),
			[NewConvert] = ISNULL(C.[NewConvert],0),
			[OldConvertTax] = ISNULL(C.[OldConvertTax],0),
			[NewConvertTax] = ISNULL(C.[NewConvertTax],0),
			[LoginIP] = ISNULL(A.[IPCount],0),
			[OldLoginIP] = ISNULL(A.[IPCount],0) - ISNULL(A.[NewIPCount],0),
			[NewLoginIP] = ISNULL(A.[NewIPCount],0),
			[LoginMac] = ISNULL(A.[MacCount],0),
			[OldLoginMac] = ISNULL(A.[MacCount],0) - ISNULL(A.[NewMacCount],0),
			[NewLoginMac] = ISNULL(A.[NewMacCount],0),
			[UserTaxAmount] = ISNULL((SELECT SUM(UserTaxAmount) FROM #TempGameTaxDetail WHERE AreaID=A.AreaID),0),
			[GameTaxDetail] = ISNULL(CAST((SELECT T.GameName,T.UserTaxAmount FROM #TempGameTaxDetail AS T WHERE  AreaID=A.AreaID  FOR XML PATH('GameTaxDetail'),TYPE) AS VARCHAR(MAX)),''),
			[RobotTaxAmount] = ISNULL((SELECT SUM(RobotTaxAmount) FROM #TempGameTaxDetail WHERE AreaID=A.AreaID),0),
			[TotalOrderCounts] = ISNULL(B.[TotalOrderCounts],0),
			[NewOrderCounts] = ISNULL(B.[NewOrderCounts],0),
			[iOSCount] = ISNULL(A.[iOSCount],0),
			[NewiOSCount] = ISNULL(A.[NewiOSCount],0),
			[AndRoidCount] = ISNULL(A.[AndRoidCount],0),
			[NewAndRoidCount] = ISNULL(A.[NewAndRoidCount],0),
			[PCCount] = ISNULL(A.[PCCount],0),
			[NewPCCount] = ISNULL(A.[NewPCCount],0),
			[AmountTotal] = ISNULL(A.[AmountTotal],0),
			[NewAmountTotal] = ISNULL(A.[NewAmountTotal],0),
			[AmountAvg] = ISNULL(A.[AmountAvg],0),
			[NewAmountAvg] = ISNULL(A.[NewAmountAvg],0),
			[TaxAmountTotal] = ISNULL(A.[TaxAmountTotal],0),
			[NewTaxAmountTotal] = ISNULL(A.[NewTaxAmountTotal],0),
			[TaxAmountAvg] = ISNULL(A.[TaxAmountAvg],0),
			[NewTaxAmountAvg] = ISNULL(A.[NewTaxAmountAvg],0),
			[GameTimeMax] = ISNULL(A.[GameTimeMax],0),
			[GameTimeMin] = ISNULL(A.[GameTimeMin],0),
			[GameTimeAvg] = ISNULL(A.[GameTimeAvg],0),

			TotalConvertUser = ISNULL(C.TotalConvertUser,0),
			NewConvertUser = ISNULL(C.NewConvertUser,0),
			TotalConvertCounts = ISNULL(C.TotalConvertCounts,0),
			[TotalOrderUser] = ISNULL(B.TotalOrderUser,0),
			[NewOrderUser] = ISNULL(B.[NewOrderUser],0),

			NewAgentChargeUser = ISNULL(D.NewAgentChargeUser,0),
			OldAgentChargeUser = ISNULL(D.OldAgentChargeUser,0),
			NewAgentChargeAmount = ISNULL(D.NewAgentChargeAmount,0),
			OldAgentChargeAmount = ISNULL(D.OldAgentChargeAmount,0)

	INTO #Temp_AreaData_Month
	FROM 
	#Temp_GameLogStatData AS A --曾經登入遊戲的用戶統計
	LEFT JOIN #TempCharge AS B ON A.PlatformID = B.PlatformID AND A.AreaID = B.AreaID	--充值紀錄
	LEFT JOIN #TempConvert AS C ON A.PlatformID = C.PlatformID AND A.AreaID = C.AreaID	--兌換紀錄及手續費
	LEFT JOIN #TempAgentCharge AS D ON A.PlatformID = D.PlatformID AND A.AreaID = D.AreaID	--代理充值紀錄


	MERGE Stat.AreaDataMonth AS A
	USING (
			SELECT	* FROM #Temp_AreaData_Month
		) AS B ON (A.[StatTime] = B.[StatTime] AND A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID] AND A.[GameID] = B.[GameID] AND A.[ServerID] = B.[ServerID])
	WHEN MATCHED THEN
		UPDATE SET	A.[OldLogin] = B.[OldLogin],
					A.[NewLogin] = B.[NewLogin],
					A.[LoginIP] = B.[LoginIP],
					A.[OldLoginIP] = B.[OldLoginIP],
					A.[NewLoginIP] = B.[NewLoginIP],
					A.[LoginMac] = B.[LoginMac],
					A.[OldLoginMac] = B.[OldLoginMac],
					A.[NewLoginMac] = B.[NewLoginMac],
					A.[iOSCount] = B.[iOSCount],
					A.[NewiOSCount] = B.[NewiOSCount],
					A.[AndRoidCount] = B.[AndRoidCount],
					A.[NewAndRoidCount] = B.[NewAndRoidCount],
					A.[PCCount] = B.[PCCount],
					A.[NewPCCount] = B.[NewPCCount],

					A.[OldCharge] = B.[OldCharge],
					A.[NewCharge] = B.[NewCharge],
					A.[OldConvert] = B.[OldConvert],
					A.[NewConvert] = B.[NewConvert],
					A.[OldConvertTax] = B.[OldConvertTax],
					A.[NewConvertTax] = B.[NewConvertTax],
					A.[TotalOrderCounts] = B.[TotalOrderCounts],
					A.[NewOrderCounts] = B.[NewOrderCounts],

					A.[AmountTotal] = B.[AmountTotal],
					A.[NewAmountTotal] = B.[NewAmountTotal],
					A.[AmountAvg] = B.[AmountAvg],
					A.[NewAmountAvg] = B.[NewAmountAvg],

					A.[GameTax] = B.[UserTaxAmount],
					A.[RobotTaxAmount] = B.[RobotTaxAmount],
					A.[GameTaxDetail] = B.[GameTaxDetail],

					A.[TaxAmountTotal] = B.[TaxAmountTotal],
					A.[NewTaxAmountTotal] = B.[NewTaxAmountTotal],
					A.[TaxAmountAvg] = B.[TaxAmountAvg],
					A.[NewTaxAmountAvg] = B.[NewTaxAmountAvg],
					A.[GameTimeMax] = B.[GameTimeMax],
					A.[GameTimeMin] = B.[GameTimeMin],
					A.[GameTimeAvg] = B.[GameTimeAvg],

					A.TotalConvertUser = B.TotalConvertUser,
					A.NewConvertUser = B.NewConvertUser,     --新用戶兌換去重用戶數
					A.TotalConvertCounts = B.TotalConvertCounts,
					A.Total_OrderUser = B.TotalOrderUser,
					A.NewOrderUser = B.NewOrderUser,
					
					A.NewAgentChargeUser = B.NewAgentChargeUser, --新用戶代理充值不重複人數
					A.OldAgentChargeUser = B.OldAgentChargeUser, --舊用戶代理充值不重複人數
					A.NewAgentChargeAmount = B.NewAgentChargeAmount, --新用戶代理充值金額
					A.OldAgentChargeAmount = B.OldAgentChargeAmount --舊用戶代理充值金額

	WHEN NOT MATCHED THEN
		INSERT
			(
				[StatTime],[PlatformID],[AreaID],[GameID],[ServerID],[OldLogin],[NewLogin],[OldCharge],[NewCharge],
				[OldConvert],[NewConvert],[OldConvertTax],[NewConvertTax],[LoginIP],[OldLoginIP],[NewLoginIP],
				[LoginMac],[OldLoginMac],[NewLoginMac],[GameTax],[GameTaxDetail],[TotalOrderCounts],[NewOrderCounts],
				[iOSCount],[NewiOSCount],[AndRoidCount],[NewAndRoidCount],[PCCount],[NewPCCount],
				[AmountTotal],[NewAmountTotal],[AmountAvg],[NewAmountAvg],[TaxAmountTotal],[NewTaxAmountTotal],[TaxAmountAvg],
				[NewTaxAmountAvg],[GameTimeMax],[GameTimeMin],[GameTimeAvg],[RobotTaxAmount],TotalConvertUser,NewConvertUser,TotalConvertCounts,Total_OrderUser,NewOrderUser,
				NewAgentChargeUser,OldAgentChargeUser,NewAgentChargeAmount,OldAgentChargeAmount
			)
		VALUES
			(
				[StatTime],[PlatformID],[AreaID],[GameID],[ServerID],[OldLogin],[NewLogin],[OldCharge],[NewCharge],
				[OldConvert],[NewConvert],[OldConvertTax],[NewConvertTax],[LoginIP],[OldLoginIP],[NewLoginIP],
				[LoginMac],[OldLoginMac],[NewLoginMac],[UserTaxAmount],[GameTaxDetail],[TotalOrderCounts],[NewOrderCounts],
				[iOSCount],[NewiOSCount],[AndRoidCount],[NewAndRoidCount],[PCCount],[NewPCCount],
				[AmountTotal],[NewAmountTotal],[AmountAvg],[NewAmountAvg],[TaxAmountTotal],[NewTaxAmountTotal],[TaxAmountAvg],
				[NewTaxAmountAvg],[GameTimeMax],[GameTimeMin],[GameTimeAvg],[RobotTaxAmount],TotalConvertUser,NewConvertUser,TotalConvertCounts,TotalOrderUser,NewOrderUser,
				NewAgentChargeUser,OldAgentChargeUser,NewAgentChargeAmount,OldAgentChargeAmount
			);
	


	--20181222 alan关闭(没用到)
	/*
	MERGE Stat.AreaDataMonth_Area AS A
	USING (
			SELECT	* FROM #Temp_AreaData_Month
		) AS B ON (A.[StatTime] = B.[StatTime] AND A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID] AND A.[GameID] = B.[GameID] AND A.[ServerID] = B.[ServerID])
	WHEN MATCHED THEN
		UPDATE SET	A.[OldLogin] = B.[OldLogin],
					A.[NewLogin] = B.[NewLogin],
					A.[OldCharge] = B.[OldCharge],
					A.[NewCharge] = B.[NewCharge],
					A.[OldConvert] = B.[OldConvert],
					A.[NewConvert] = B.[NewConvert],
					A.[OldConvertTax] = B.[OldConvertTax],
					A.[NewConvertTax] = B.[NewConvertTax],
					A.[LoginIP] = B.[LoginIP],
					A.[OldLoginIP] = B.[OldLoginIP],
					A.[NewLoginIP] = B.[NewLoginIP],
					A.[LoginMac] = B.[LoginMac],
					A.[OldLoginMac] = B.[OldLoginMac],
					A.[NewLoginMac] = B.[NewLoginMac],
					A.[GameTax] = B.[GameTax],
					A.[GameTaxDetail] = B.[GameTaxDetail],
					A.[TotalOrderCounts] = B.[TotalOrderCounts],
					A.[NewOrderCounts] = B.[NewOrderCounts],
					A.[iOSCount] = B.[iOSCount],
					A.[NewiOSCount] = B.[NewiOSCount],
					A.[AndRoidCount] = B.[AndRoidCount],
					A.[NewAndRoidCount] = B.[NewAndRoidCount],
					A.[PCCount] = B.[PCCount],
					A.[NewPCCount] = B.[NewPCCount],
					A.[AmountTotal] = B.[AmountTotal],
					A.[NewAmountTotal] = B.[NewAmountTotal],
					A.[AmountAvg] = B.[AmountAvg],
					A.[NewAmountAvg] = B.[NewAmountAvg],
					A.[TaxAmountTotal] = B.[TaxAmountTotal],
					A.[NewTaxAmountTotal] = B.[NewTaxAmountTotal],
					A.[TaxAmountAvg] = B.[TaxAmountAvg],
					A.[NewTaxAmountAvg] = B.[NewTaxAmountAvg],
					A.[GameTimeMax] = B.[GameTimeMax],
					A.[GameTimeMin] = B.[GameTimeMin],
					A.[GameTimeAvg] = B.[GameTimeAvg],
					A.[RobotTaxAmount] = B.[RobotTaxAmount],
					A.TotalConvertUser = B.TotalConvertUser,
					A.TotalConvertCounts = B.TotalConvertCounts,
					A.Total_OrderUser = B.Total_OrderUser,
					A.NewOrderUser = B.NewOrderUser
	WHEN NOT MATCHED THEN
		INSERT
			(
				[StatTime],[PlatformID],[AreaID],[GameID],[ServerID],[OldLogin],[NewLogin],[OldCharge],[NewCharge],
				[OldConvert],[NewConvert],[OldConvertTax],[NewConvertTax],[LoginIP],[OldLoginIP],[NewLoginIP],
				[LoginMac],[OldLoginMac],[NewLoginMac],[GameTax],[GameTaxDetail],[TotalOrderCounts],[NewOrderCounts],
				[iOSCount],[NewiOSCount],[AndRoidCount],[NewAndRoidCount],[PCCount],[NewPCCount],
				[AmountTotal],[NewAmountTotal],[AmountAvg],[NewAmountAvg],[TaxAmountTotal],[NewTaxAmountTotal],[TaxAmountAvg],
				[NewTaxAmountAvg],[GameTimeMax],[GameTimeMin],[GameTimeAvg],[RobotTaxAmount],TotalConvertUser,TotalConvertCounts,Total_OrderUser,NewOrderUser
			)
		VALUES
			(
				[StatTime],[PlatformID],[AreaID],[GameID],[ServerID],[OldLogin],[NewLogin],[OldCharge],[NewCharge],
					[OldConvert],[NewConvert],[OldConvertTax],[NewConvertTax],[LoginIP],[OldLoginIP],[NewLoginIP],
					[LoginMac],[OldLoginMac],[NewLoginMac],[GameTax],[GameTaxDetail],[TotalOrderCounts],[NewOrderCounts],
					[iOSCount],[NewiOSCount],[AndRoidCount],[NewAndRoidCount],[PCCount],[NewPCCount],
					[AmountTotal],[NewAmountTotal],[AmountAvg],[NewAmountAvg],[TaxAmountTotal],[NewTaxAmountTotal],[TaxAmountAvg],
					[NewTaxAmountAvg],[GameTimeMax],[GameTimeMin],[GameTimeAvg],[RobotTaxAmount],TotalConvertUser,TotalConvertCounts,Total_OrderUser,NewOrderUser
			);
	*/
	----------------------------------------------------------------------------------------------

	DROP TABLE #TempCharge
	--DROP TABLE #ConvertLog
	--DROP TABLE #MoneyLog
	DROP TABLE #TempGameTaxDetail
	DROP TABLE #TempConvert
	DROP TABLE #Temp_AreaData_Month
	DROP TABLE #Temp_GameLogStatData
	DROP TABLE #TempUserData

END





GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_AreaDataStat_Month] TO [xbuser]
    AS [dbo];

