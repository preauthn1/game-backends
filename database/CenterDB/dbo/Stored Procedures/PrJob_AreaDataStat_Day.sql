-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<统计渠道日报表相关数据>
--	2018/03/20	Jason		調整取LOGDB寫法及新增變數接參數
--	2018/03/28	Jason		調整報表統計方式 修正原本以@dtStatTime 取當天日期做統計會造成統計資料不正確問題
--	2018/03/29	Jason		調整報表統計流程
--	2018/05/16	Jason		#257 報表統計日月報表調整每30分鐘執行一次計算至當下時間
--	2018/05/25	Jason		#349 報表 - 建立統計數據報表提供API回傳 新增TotalConvertUser,TotalConvertUser,Total_OrderCounts,NewOrderCounts
--	2018/07/03	alan		充值統計新增代理充值
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_AreaDataStat_Day]
(
	@dtStatTime		DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @bitIsStatLast bit	--是否需要统计跨天时上一天的统计数据
	DECLARE @LogStartTime	DATETIME
	DECLARE @LogEndTime		DATETIME
	DECLARE @LogStatus		TINYINT = 3
	--DECLARE @SourceTypeID	INT = 10000
	
	SET @dtStatTime		= ISNULL(@dtStatTime,GETDATE())

	IF (DATEPART(HOUR,@dtStatTime) = 0) AND (DATEPART(MINUTE,@dtStatTime) < 10)
	BEGIN
		SET @LogStartTime	= DATEADD(DAY,-1,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogEndTime		= DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
		SET @bitIsStatLast = 0
	END
	ELSE
	BEGIN
		SET @LogStartTime	= DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogEndTime		= DATEADD(DAY,1,DATEDIFF(DAY,0,@dtStatTime))
		SET @bitIsStatLast = 0
	END


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
			[AndroidCount]			INT NULL,
			[NewAndroidCount]		INT NULL,
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
		@intTypeID = 1,
		@dtStatTime = @LogStartTime,
		@dtEndTime = @LogEndTime
	----------------------------------------------------------------------------------------------

	



	/*  --统计充值数据  */
	----------------------------------------------------------------------------------------------
	--取充值紀錄暫存
	CREATE TABLE #TempCharge (PlatformID INT,AreaID INT,NewOrderUser INT,TotalOrderUser INT,NewOrderCounts INT,TotalOrderCounts INT,OldCharge INT,NewCharge INT)
	
	INSERT INTO #TempCharge
	SELECT	A.PlatformID,
			U.AreaID,
			NewOrderUser = COUNT(DISTINCT CASE WHEN u.CreateTime >= @LogStartTime THEN u.UserID ELSE NULL END),
			TotalOrderUser = COUNT(DISTINCT u.UserID),
			NewOrderCounts = SUM(CASE WHEN u.CreateTime >= @LogStartTime THEN 1 ELSE 0 END),
			TotalOrderCounts = COUNT(L.LogID),
			OldCharge = ISNULL(SUM(CASE WHEN u.CreateTime < @LogStartTime THEN UpdateAmount ELSE 0 END),0),
			NewCharge = ISNULL(SUM(CASE WHEN u.CreateTime >= @LogStartTime THEN UpdateAmount ELSE 0 END),0)
	FROM Logdb.Money.MoneyLog AS L WITH (NOLOCK)
		INNER JOIN Users.UserInfo AS u WITH (NOLOCK) ON L.UserID=U.UserID
		INNER JOIN Area.Area AS A WITH (NOLOCK) ON u.AreaID = A.AreaID
	WHERE
		L.CreateTime >= @LogStartTime
		AND L.CreateTime < @LogEndTime
		AND L.SourceTypeID >=11001 and L.SourceTypeID <=11020 --此段範圍都屬充值類型
	GROUP BY A.PlatformID,U.AreaID

	UNION ALL
	SELECT	A.PlatformID,
			0 AS AreaID,
			NewOrderUser = COUNT(DISTINCT CASE WHEN u.CreateTime >= @LogStartTime THEN u.UserID ELSE NULL END),
			TotalOrderUser = COUNT(DISTINCT u.UserID),
			NewOrderCounts = SUM(CASE WHEN u.CreateTime >= @LogStartTime THEN 1 ELSE 0 END),
			TotalOrderCounts = COUNT(L.LogID),
			OldCharge = ISNULL(SUM(CASE WHEN u.CreateTime < @LogStartTime THEN UpdateAmount ELSE 0 END),0),
			NewCharge = ISNULL(SUM(CASE WHEN u.CreateTime >= @LogStartTime THEN UpdateAmount ELSE 0 END),0)
	FROM Logdb.Money.MoneyLog AS L WITH (NOLOCK)
		INNER JOIN Users.UserInfo AS u WITH (NOLOCK) ON L.UserID=U.UserID
		INNER JOIN Area.Area AS A WITH (NOLOCK) ON u.AreaID = A.AreaID
	WHERE
		L.CreateTime >= @LogStartTime
		AND L.CreateTime < @LogEndTime
		AND L.SourceTypeID >=11001 and L.SourceTypeID <=11020 --此段範圍都屬充值類型
	GROUP BY A.PlatformID
	UNION ALL
	SELECT	0 AS PlatformID,
			0 AS AreaID,
			NewOrderUser = COUNT(DISTINCT CASE WHEN u.CreateTime >= @LogStartTime THEN u.UserID ELSE NULL END),
			TotalOrderUser = COUNT(DISTINCT u.UserID),
			NewOrderCounts = SUM(CASE WHEN u.CreateTime >= @LogStartTime THEN 1 ELSE 0 END),
			TotalOrderCounts = COUNT(L.LogID),
			OldCharge = ISNULL(SUM(CASE WHEN u.CreateTime < @LogStartTime THEN UpdateAmount ELSE 0 END),0),
			NewCharge = ISNULL(SUM(CASE WHEN u.CreateTime >= @LogStartTime THEN UpdateAmount ELSE 0 END),0)
	FROM Logdb.Money.MoneyLog AS L WITH (NOLOCK)
		INNER JOIN Users.UserInfo AS u WITH (NOLOCK) ON L.UserID=U.UserID
		INNER JOIN Area.Area AS A WITH (NOLOCK) ON u.AreaID = A.AreaID
	WHERE
		L.CreateTime >= @LogStartTime
		AND L.CreateTime < @LogEndTime
		AND L.SourceTypeID >=11001 and L.SourceTypeID <=11020 --此段範圍都屬充值類型
	
	
	----------------------------------------------------------------------------------------------



	/*  --统计代理充值数据  */
	----------------------------------------------------------------------------------------------	
	SELECT	A.PlatformID,
			U.AreaID,
			OldAgentChargeUser = COUNT(DISTINCT CASE WHEN u.CreateTime < @LogStartTime THEN u.UserID ELSE NULL END),
			NewAgentChargeUser = COUNT(DISTINCT CASE WHEN u.CreateTime >= @LogStartTime THEN u.UserID ELSE NULL END),
			OldAgentChargeAmount = ISNULL(SUM(CASE WHEN u.CreateTime < @LogStartTime THEN UpdateAmount ELSE 0 END),0),
			NewAgentChargeAmount = ISNULL(SUM(CASE WHEN u.CreateTime >= @LogStartTime THEN UpdateAmount ELSE 0 END),0)
    INTO #TempAgentCharge
	FROM Logdb.Money.MoneyLog AS L WITH (NOLOCK)
		INNER JOIN Users.UserInfo AS u WITH (NOLOCK) ON L.UserID=U.UserID
		INNER JOIN Area.Area AS A WITH (NOLOCK) ON u.AreaID = A.AreaID
	WHERE
		L.CreateTime >= @LogStartTime
		AND L.CreateTime < @LogEndTime
		AND L.SourceTypeID = 11008
	GROUP BY A.PlatformID,U.AreaID
	
	----------------------------------------------------------------------------------------------



	/*  --统计兑换数量与手续费  */
	----------------------------------------------------------------------------------------------
	
	CREATE TABLE #TempConvert (PlatformID INT,AreaID INT,OldConvert INT,NewConvert INT,OldConvertTax INT,NewConvertTax INT,TotalConvertUser INT, NewConvertUser INT, TotalConvertCounts INT)
	

	INSERT INTO #TempConvert
	SELECT	A.PlatformID,
			U.AreaID,
			SUM(CASE WHEN u.CreateTime < @LogStartTime AND L.Status = 3 THEN Amount ELSE 0 END) AS OldConvert,
			SUM(CASE WHEN u.CreateTime >= @LogStartTime AND L.Status = 3 THEN Amount ELSE 0 END) AS NewConvert,
			SUM(CASE WHEN u.CreateTime < @LogStartTime AND L.Status = 3 THEN TaxAmount ELSE 0 END) AS OldConvertTax,
			SUM(CASE WHEN u.CreateTime > @LogStartTime AND L.Status = 3 THEN TaxAmount ELSE 0 END) AS NewConvertTax,
			COUNT(DISTINCT L.UserID) AS TotalConvertUser,
			COUNT(DISTINCT CASE WHEN u.CreateTime >= @LogStartTime AND L.Status = 3 THEN L.UserID ELSE NULL END) AS NewConvertUser,
			COUNT(L.UserID) AS TotalConvertCounts
	FROM [Logdb].[Money].[ConvertLog] AS L WITH (NOLOCK)

		INNER JOIN Users.UserInfo AS u WITH (NOLOCK) ON L.UserID=U.UserID
		INNER JOIN Area.Area AS A WITH (NOLOCK) ON u.AreaID = A.AreaID
	WHERE
		L.[LastUpdateTime] >= @LogStartTime
		AND L.[LastUpdateTime] < @LogEndTime
		AND L.Status=3
	GROUP BY A.PlatformID,U.AreaID

	UNION ALL
	SELECT	A.PlatformID,
			0 AS AreaID,
			SUM(CASE WHEN u.CreateTime < @LogStartTime AND L.Status = 3 THEN Amount ELSE 0 END) AS OldConvert,
			SUM(CASE WHEN u.CreateTime >= @LogStartTime AND L.Status = 3 THEN Amount ELSE 0 END) AS NewConvert,
			SUM(CASE WHEN u.CreateTime < @LogStartTime AND L.Status = 3 THEN TaxAmount ELSE 0 END) AS OldConvertTax,
			SUM(CASE WHEN u.CreateTime > @LogStartTime AND L.Status = 3 THEN TaxAmount ELSE 0 END) AS NewConvertTax,
			COUNT(DISTINCT L.UserID) AS TotalConvertUser,
			COUNT(DISTINCT CASE WHEN u.CreateTime >= @LogStartTime AND L.Status = 3 THEN L.UserID ELSE NULL END) AS NewConvertUser,
			COUNT(L.UserID) AS TotalConvertCounts
	FROM [Logdb].[Money].[ConvertLog] AS L
		INNER JOIN Users.UserInfo AS u WITH (NOLOCK) ON L.UserID=U.UserID
		INNER JOIN Area.Area AS A WITH (NOLOCK) ON u.AreaID = A.AreaID
	WHERE
		L.[LastUpdateTime] >= @LogStartTime
		AND L.[LastUpdateTime] < @LogEndTime
		AND L.Status=3
	GROUP BY A.PlatformID
	UNION ALL
	SELECT	0 AS PlatformID,
			0 AS AreaID,
			SUM(CASE WHEN u.CreateTime < @LogStartTime AND L.Status = 3 THEN Amount ELSE 0 END) AS OldConvert,
			SUM(CASE WHEN u.CreateTime >= @LogStartTime AND L.Status = 3 THEN Amount ELSE 0 END) AS NewConvert,
			SUM(CASE WHEN u.CreateTime < @LogStartTime AND L.Status = 3 THEN TaxAmount ELSE 0 END) AS OldConvertTax,
			SUM(CASE WHEN u.CreateTime > @LogStartTime AND L.Status = 3 THEN TaxAmount ELSE 0 END) AS NewConvertTax,
			COUNT(DISTINCT L.UserID) AS TotalConvertUser,
			COUNT(DISTINCT CASE WHEN u.CreateTime >= @LogStartTime AND L.Status = 3 THEN L.UserID ELSE NULL END) AS NewConvertUser,
			COUNT(L.UserID) AS TotalConvertCounts
	FROM [Logdb].[Money].[ConvertLog] AS L
		INNER JOIN Users.UserInfo AS u WITH (NOLOCK) ON L.UserID=U.UserID
		INNER JOIN Area.Area AS A WITH (NOLOCK) ON u.AreaID = A.AreaID
	WHERE
		L.[LastUpdateTime] >= @LogStartTime
		AND L.[LastUpdateTime] < @LogEndTime
		AND L.Status=3
	----------------------------------------------------------------------------------------------


	/*  --统计税收  */
	----------------------------------------------------------------------------------------------
	
	CREATE TABLE #TempGameTaxDetail (PlatformID INT, AreaID INT, GameName NVARCHAR(128), 
	UserGameAmount BIGINT, UserTaxAmount BIGINT, RobotGameAmount BIGINT, RobotTaxAmount BIGINT)
	
	INSERT INTO #TempGameTaxDetail(PlatformID, AreaID, GameName, UserGameAmount, UserTaxAmount, RobotGameAmount, RobotTaxAmount)
	--20181222 alan調整作法
	SELECT PlatformID, AreaID, GameName, 
	ISNULL(UserGameAmount,0) as UserGameAmount, 
	ISNULL(UserTaxAmount,0) as UserTaxAmount, 
	ISNULL(RobotGameAmount,0) as RobotGameAmount, 
	ISNULL(RobotTaxAmount,0) as RobotTaxAmount
	from (
		
		SELECT G.PlatformID, G.AreaID, G.GameID, GL.GameName,

		SUM(case when u.UserType <> 2 then G.Amount else 0 end) as UserGameAmount,
		SUM(case when u.UserType <> 2 then G.TaxAmount else 0 end) as UserTaxAmount,
		
		--COUNT(case when u.UserType <> 2 then g.TaxAmount else null end) as userTaxCount,
		SUM(case when u.UserType = 2 then G.Amount else 0 end) as RobotGameAmount,
		SUM(case when u.UserType = 2 then G.TaxAmount else 0 end) as RobotTaxAmount

		from logdb.game.GameLog G with (nolock)
		left join users.UserInfo U on g.UserID=u.UserID
		left join ServerInfo.GameList GL on G.GameID=GL.GameID
		where LogoutTime >= @LogStartTime AND  LogoutTime < @LogEndTime
		group by G.platformid, G.areaid, G.gameid, GL.GameName
	) as A

	----------------------------------------------------------------------------------------------



	/*  --用戶金幣数据  */
	----------------------------------------------------------------------------------------------	
	SELECT	A.PlatformID,
			A.AreaID,
			TotalMoney = SUM(Amount), --身上金幣金額
			TotalBankMoney = SUM(BankAmount) --身上保險箱金額 
	INTO #TempUsersMoney
	From (
		Select U.AreaID, C.Amount,B.Amount as BankAmount
		FROM CENTERDB.Money.Cash AS C WITH (NOLOCK)
		LEFT JOIN CENTERDB.Money.Bank AS B WITH (NOLOCK) on C.UserID=B.UserID
		LEFT JOIN CenterDB.Users.UserInfo U WITH (NOLOCK) on C.UserID=U.UserID
	) U
		INNER JOIN Area.Area AS A WITH (NOLOCK) ON U.AreaID = A.AreaID

	GROUP BY A.PlatformID,A.AreaID
	----------------------------------------------------------------------------------------------







	/*  --更新用戶統計資料   A.GameID=0 AND A.ServerID=0 */
	----------------------------------------------------------------------------------------------

	SELECT StatTime=@LogStartTime,
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
		OldAgentChargeAmount = ISNULL(D.OldAgentChargeAmount,0),

		TotalMoney = ISNULL(M.TotalMoney,0),
		TotalBankMoney = ISNULL(M.TotalBankMoney,0)	
	
	INTO #TempUserData
	FROM Area.Area A WITH(NOLOCK)
	LEFT JOIN #TempCharge AS B  ON A.PlatformID = B.PlatformID AND A.AreaID = B.AreaID	--充值紀錄
	LEFT JOIN #TempConvert AS C ON A.PlatformID = C.PlatformID AND A.AreaID = C.AreaID	--兌換紀錄及手續費
	LEFT JOIN #TempAgentCharge AS D ON A.PlatformID = D.PlatformID AND A.AreaID = D.AreaID	--代理充值紀錄
	LEFT JOIN #TempUsersMoney as M  ON A.PlatformID = M.PlatformID AND A.AreaID = M.AreaID --用戶金幣數量


	MERGE Stat.AreaDataDay AS A
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
				A.OldAgentChargeAmount = B.OldAgentChargeAmount, --舊用戶代理充值金額

				A.TotalMoney = B.TotalMoney,         --用戶金幣金額
				A.TotalBankMoney = B.TotalBankMoney --用戶保險箱金額



	WHEN NOT MATCHED THEN
		INSERT
			(
				[StatTime],[PlatformID],[AreaID],[GameID],[ServerID],[OldCharge],[NewCharge],
				[OldConvert],[NewConvert],[OldConvertTax],[NewConvertTax],
				[TotalOrderCounts],[NewOrderCounts],	
				TotalConvertUser,NewConvertUser,TotalConvertCounts,Total_OrderUser,NewOrderUser,
				NewAgentChargeUser,OldAgentChargeUser,NewAgentChargeAmount,OldAgentChargeAmount,TotalMoney,TotalBankMoney
			)
		VALUES
			(
				[StatTime],[PlatformID],[AreaID],[GameID],[ServerID],[OldCharge],[NewCharge],
				[OldConvert],[NewConvert],[OldConvertTax],[NewConvertTax],
				[TotalOrderCounts],[NewOrderCounts],	
				TotalConvertUser,NewConvertUser,TotalConvertCounts,TotalOrderUser,NewOrderUser,
				NewAgentChargeUser,OldAgentChargeUser,NewAgentChargeAmount,OldAgentChargeAmount,TotalMoney,TotalBankMoney
			);

	----------------------------------------------------------------------------------------------







	
	/*  --更新遊戲統計資料  */
	----------------------------------------------------------------------------------------------
	SELECT	[StatTime] = @LogStartTime,
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
			[UserGameAmount] = ISNULL((SELECT SUM(UserGameAmount) FROM #TempGameTaxDetail WHERE AreaID=A.AreaID),0),
			[UserTaxAmount] = ISNULL((SELECT SUM(UserTaxAmount) FROM #TempGameTaxDetail WHERE AreaID=A.AreaID),0),
			[RobotGameAmount] = ISNULL((SELECT SUM(RobotGameAmount) FROM #TempGameTaxDetail WHERE AreaID=A.AreaID),0),
			[RobotTaxAmount] = ISNULL((SELECT SUM(RobotTaxAmount) FROM #TempGameTaxDetail WHERE AreaID=A.AreaID),0),
			[GameTaxDetail] = ISNULL(CAST((SELECT T.GameName,T.UserTaxAmount FROM #TempGameTaxDetail AS T WHERE AreaID=A.AreaID FOR XML PATH('GameTaxDetail'),TYPE) AS VARCHAR(MAX)),''),
			[TotalOrderCounts] = ISNULL(B.[TotalOrderCounts],0),
			[NewOrderCounts] = ISNULL(B.[NewOrderCounts],0),
			[iOSCount] = ISNULL(A.[iOSCount],0),
			[NewiOSCount] = ISNULL(A.[NewiOSCount],0),
			[AndroidCount] = ISNULL(A.[AndroidCount],0),
			[NewAndroidCount] = ISNULL(A.[NewAndroidCount],0),
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
			OldAgentChargeAmount = ISNULL(D.OldAgentChargeAmount,0),
			TotalMoney = ISNULL(M.TotalMoney,0),
			TotalBankMoney = ISNULL(M.TotalBankMoney,0)


	INTO #Temp_AreaData
	FROM 
	#Temp_GameLogStatData AS A	--曾經登入遊戲的用戶統計
	LEFT JOIN #TempCharge AS B ON A.PlatformID = B.PlatformID AND A.AreaID = B.AreaID	--充值紀錄
	LEFT JOIN #TempConvert AS C ON A.PlatformID = C.PlatformID AND A.AreaID = C.AreaID	--兌換紀錄及手續費
	LEFT JOIN #TempAgentCharge AS D ON A.PlatformID = D.PlatformID AND A.AreaID = D.AreaID	--代理充值紀錄
	LEFT JOIN #TempUsersMoney as M ON A.PlatformID=M.PlatformID AND A.AreaID = M.AreaID --用戶金幣數量


	MERGE Stat.AreaDataDay AS A
	USING (
			SELECT	* FROM #Temp_AreaData
		) AS B ON (A.[StatTime] = B.[StatTime] AND A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID] AND A.[GameID] = B.[GameID] AND A.[ServerID] = B.[ServerID])
	WHEN MATCHED THEN
		UPDATE SET	


				--进游戏登入资讯
				A.[OldLogin] = B.[OldLogin], --舊用戶数登入游戏
				A.[NewLogin] = B.[NewLogin], --新用户数登入游戏
				A.[LoginIP] = B.[LoginIP],	 --登入遊戲IP數
				A.[OldLoginIP] = B.[OldLoginIP], --舊用戶登入遊戲IP數
				A.[NewLoginIP] = B.[NewLoginIP], --新用戶登入遊戲IP數
				A.[LoginMac] = B.[LoginMac],     --登入遊戲用戶機器數
				A.[OldLoginMac] = B.[OldLoginMac], --舊用戶登入遊戲機器數
				A.[NewLoginMac] = B.[NewLoginMac], --新用戶登入遊戲機器數
				A.[iOSCount] = B.[iOSCount],       --IOS註冊數且登入遊戲
				A.[NewiOSCount] = B.[NewiOSCount], --新用戶IOS註冊數且登入遊戲
				A.[AndroidCount] = B.[AndroidCount], --安卓註冊數且登入遊戲
				A.[NewAndroidCount] = B.[NewAndroidCount], --新用戶安卓註冊數且登入遊戲
				A.[PCCount] = B.[PCCount], --PC註冊數且登入遊戲
				A.[NewPCCount] = B.[NewPCCount], --新用戶PC註冊數且登入遊戲


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
				A.OldAgentChargeAmount = B.OldAgentChargeAmount, --舊用戶代理充值金額


				A.[AmountTotal] = B.[AmountTotal],		 --用戶(含机器人)輸贏總金額
				A.[TaxAmountTotal] = B.[TaxAmountTotal], --用戶(含机器人)遊戲稅收總金額
				A.[NewAmountTotal] = B.[NewAmountTotal], --新用戶(含机器人)輸贏總金額
				A.[NewTaxAmountTotal] = B.[NewTaxAmountTotal], --新用戶(含机器人)遊戲稅收總金額
				A.[AmountAvg] = B.[AmountAvg],				--用戶(含机器人)輸贏平均
				A.[TaxAmountAvg] = B.[TaxAmountAvg],		--用戶(含机器人)遊戲稅收平均 
				A.[NewAmountAvg] = B.[NewAmountAvg],		--新用戶(含机器人)輸贏平均
				A.[NewTaxAmountAvg] = B.[NewTaxAmountAvg],  --新用戶(含机器人)遊戲稅收平均

				A.UserGameAmount = B.UserGameAmount,	--用戶(不含機器人)遊戲輸贏
				A.GameTax = B.UserTaxAmount,			--用戶(不含機器人)游戏税收(=系统税收)
				A.RobotGameAmount = B.RobotGameAmount,	--機器人遊戲輸贏 (=系統輸贏)
				A.RobotTaxAmount = B.RobotTaxAmount,	--機器人遊戲稅收
				A.GameTaxDetail = B.GameTaxDetail,		--遊戲稅收內容 

				A.TotalMoney = B.TotalMoney,         --用戶金幣金額
				A.TotalBankMoney = B.TotalBankMoney, --用戶保險箱金額


				A.[GameTimeMax] = B.[GameTimeMax],
				A.[GameTimeMin] = B.[GameTimeMin],
				A.[GameTimeAvg] = B.[GameTimeAvg]


	WHEN NOT MATCHED THEN
		INSERT
			(
				[StatTime],[PlatformID],[AreaID],[GameID],[ServerID],[OldLogin],[NewLogin],[OldCharge],[NewCharge],
				[OldConvert],[NewConvert],[OldConvertTax],[NewConvertTax],[LoginIP],[OldLoginIP],[NewLoginIP],
				[LoginMac],[OldLoginMac],[NewLoginMac],[GameTax],[GameTaxDetail],[TotalOrderCounts],[NewOrderCounts],
				[iOSCount],[NewiOSCount],[AndroidCount],[NewAndroidCount],[PCCount],[NewPCCount],
				[AmountTotal],[NewAmountTotal],[AmountAvg],[NewAmountAvg],[TaxAmountTotal],[NewTaxAmountTotal],[TaxAmountAvg],
				[NewTaxAmountAvg],[GameTimeMax],[GameTimeMin],[GameTimeAvg],[RobotTaxAmount],TotalConvertUser,NewConvertUser,TotalConvertCounts,Total_OrderUser,NewOrderUser,
				NewAgentChargeUser,OldAgentChargeUser,NewAgentChargeAmount,OldAgentChargeAmount,TotalMoney,TotalBankMoney,
				UserGameAmount,RobotGameAmount
			)
		VALUES
			(
				[StatTime],[PlatformID],[AreaID],[GameID],[ServerID],[OldLogin],[NewLogin],[OldCharge],[NewCharge],
				[OldConvert],[NewConvert],[OldConvertTax],[NewConvertTax],[LoginIP],[OldLoginIP],[NewLoginIP],
				[LoginMac],[OldLoginMac],[NewLoginMac],[UserTaxAmount],[GameTaxDetail],[TotalOrderCounts],[NewOrderCounts],
				[iOSCount],[NewiOSCount],[AndroidCount],[NewAndroidCount],[PCCount],[NewPCCount],
				[AmountTotal],[NewAmountTotal],[AmountAvg],[NewAmountAvg],[TaxAmountTotal],[NewTaxAmountTotal],[TaxAmountAvg],
				[NewTaxAmountAvg],[GameTimeMax],[GameTimeMin],[GameTimeAvg],[RobotTaxAmount],TotalConvertUser,NewConvertUser,TotalConvertCounts,TotalOrderUser,NewOrderUser,
				NewAgentChargeUser,OldAgentChargeUser,NewAgentChargeAmount,OldAgentChargeAmount,TotalMoney,TotalBankMoney,
				UserGameAmount,RobotGameAmount
			);
	


	--20181222 alan关闭(没用到)
	/* 
	MERGE Stat.AreaDataDay_Area AS A
	USING (
			SELECT	* FROM #Temp_AreaData
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
					A.[AndroidCount] = B.[AndroidCount],
					A.[NewAndroidCount] = B.[NewAndroidCount],
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
				[iOSCount],[NewiOSCount],[AndroidCount],[NewAndroidCount],[PCCount],[NewPCCount],
				[AmountTotal],[NewAmountTotal],[AmountAvg],[NewAmountAvg],[TaxAmountTotal],[NewTaxAmountTotal],[TaxAmountAvg],
				[NewTaxAmountAvg],[GameTimeMax],[GameTimeMin],[GameTimeAvg],[RobotTaxAmount],TotalConvertUser,TotalConvertCounts,Total_OrderUser,NewOrderUser
			)
		VALUES
			(
				[StatTime],[PlatformID],[AreaID],[GameID],[ServerID],[OldLogin],[NewLogin],[OldCharge],[NewCharge],
					[OldConvert],[NewConvert],[OldConvertTax],[NewConvertTax],[LoginIP],[OldLoginIP],[NewLoginIP],
					[LoginMac],[OldLoginMac],[NewLoginMac],[GameTax],[GameTaxDetail],[TotalOrderCounts],[NewOrderCounts],
					[iOSCount],[NewiOSCount],[AndroidCount],[NewAndroidCount],[PCCount],[NewPCCount],
					[AmountTotal],[NewAmountTotal],[AmountAvg],[NewAmountAvg],[TaxAmountTotal],[NewTaxAmountTotal],[TaxAmountAvg],
					[NewTaxAmountAvg],[GameTimeMax],[GameTimeMin],[GameTimeAvg],[RobotTaxAmount],TotalConvertUser,TotalConvertCounts,Total_OrderUser,NewOrderUser
			);
	*/
	----------------------------------------------------------------------------------------------

	DROP TABLE #TempCharge
	DROP TABLE #TempAgentCharge
	DROP TABLE #TempGameTaxDetail
	DROP TABLE #TempConvert
	DROP TABLE #TempUsersMoney
	DROP TABLE #Temp_AreaData
	DROP TABLE #Temp_GameLogStatData
	DROP TABLE #TempUserData

END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_AreaDataStat_Day] TO [xbuser]
    AS [dbo];

