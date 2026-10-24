-- =============================================
-- Author:  Jason
-- Create date: 2018/03/29
-- Description: 渠道管理-渠道數據統計-日報表查詢
--	2018/05/14	Jason		#243 運營需求調整日/月報表撈取條件 註解@PlatFormID & @AreaID預設值及新增AND PlatFormID > 0 AND AreaID > 0 AND GameID = 0 AND ServerID = 0
--	2018/05/15	Jason		#243 調整統計BUG
--  2018/05/23  Emily       #347 日報/月報表增加註冊用戶人數/註冊用戶充值人數/ARPU計算調整
--	2018/05/30	Jason		#378 排序調整
--  2018/06/04  Emily	    #399 統計日報月報可用時間排序
--  2018/06/19  Emily	    #485 TotalOrderUser配合前端改為Total_OrderUser
--	2018/06/21	Jason		#503 area增加status狀態, 提供後台相關統計不顯示該渠道 新增AreaStatus狀態確認是否為顯示
--	2019/03/25	Jason		新增全服匯總及新欄位
--	2019/03/29	Jason		新增排序
--	2019/07/29	CCC			当日新增充值金额重複計算到代理充值BUG
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetAreaDataStat_Day]
(
	@intAdminID		int=null,
	@intPlatformID	INT = NULL,
	@intAreaID		INT = NULL,
	@dtStatTime		DATETIME = NULL,
	@dtEndTime		DATETIME = NULL,
	@tnyColumns		TINYINT = 99,			--0:充值 1:兌換
	@tnySort		TINYINT = 1,			--0:降序 1:升序
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intPageCount	INT = 0		OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON


	--渠道篩選
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT,AreaID INT)
	INSERT INTO @Temp_PlatformID Select PlatformID,AreaID From CenterDB.Area.AreaAdminUser where AdminID=@intAdminID



	DECLARE @PlatformID		INT = CASE WHEN @intPlatformID = 0 THEN NULL ELSE @intPlatformID END
	DECLARE @AreaID			INT = CASE WHEN @intAreaID = 0 THEN NULL ELSE @intAreaID END
	DECLARE @LogStartTime	DATETIME
	DECLARE @LogEndTime		DATETIME
	DECLARE @TempLogSTime	DATETIME

	CREATE TABLE #Temp_TotalData (PlatformID INT,AreaID INT,StatTime DATETIME)

	--SET @PlatFormID		= ISNULL(@intPlatformID,0)
	--SET @AreaID			= ISNULL(@intAreaID,0)
	SET @LogStartTime	= CASE	WHEN @dtStatTime IS NULL THEN DATEADD(DAY,-1,DATEDIFF(DAY,0,GETDATE())) ELSE DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime)) END
	SET @LogEndTime		= CASE	WHEN @dtEndTime IS NULL THEN DATEADD(DAY,0,DATEDIFF(DAY,0,GETDATE())) ELSE DATEADD(DAY,0,DATEDIFF(DAY,0,@dtEndTime)) END
	SET @TempLogSTime	= @LogStartTime

	WHILE (@TempLogSTime <= @LogEndTime)
	BEGIN
		INSERT INTO #Temp_TotalData
		SELECT	A.PlatformID,A.AreaID,@TempLogSTime
		FROM [Area].[Area] AS A WITH (NOLOCK)
			INNER JOIN @Temp_PlatformID AS B ON A.PlatformID = B.PlatformID AND A.AreaID = B.AreaID
		WHERE
		ISNULL(@PlatFormID,A.PlatformID) = A.PlatFormID
		AND ISNULL(@AreaID,A.AreaID) = A.AreaID
		AND AreaStatus = 1
		--AND PlatformID in (select platformID from @Temp_PlatformID)

		SET @TempLogSTime	= DATEADD(DAY,1,@TempLogSTime)
	END

	SELECT	A.*,B.TotalRegCount,D.[TotalBindCount],
			[OneDayKeep] = --CAST(ROUND(CAST(C.[OneDayKeep] AS FLOAT)/B.[TotalRegCount]*100,2) AS NVARCHAR(10)) + '%',
							CASE WHEN B.[TotalRegCount] = 0 THEN '0.00%' ELSE CAST(ROUND(CAST(C.[OneDayKeep] AS FLOAT)/B.[TotalRegCount]*100,2) AS NVARCHAR(10)) + '%' END,
			[Third] = --CAST(ROUND(CAST(C.[Third] AS FLOAT)/B.[TotalRegCount]*100,2) AS NVARCHAR(10)) + '%'
							CASE WHEN B.[TotalRegCount] = 0 THEN '0.00%' ELSE CAST(ROUND(CAST(C.[Third] AS FLOAT)/B.[TotalRegCount]*100,2) AS NVARCHAR(10)) + '%' END
	INTO #T 
	FROM [Stat].[AreaDataDay] AS A WITH (NOLOCK)
		LEFT JOIN [Stat].[RegUserLog] AS B WITH (NOLOCK) ON A.StatTime = B.StatTime AND A.PlatFormID = B.PlatformID AND A.AreaID = B.AreaID
		LEFT JOIN [Stat].[KeepLog] AS C WITH (NOLOCK) ON A.StatTime = C.StatTime AND A.PlatFormID = C.PlatformID AND A.AreaID = C.AreaID
		LEFT JOIN [Stat].[UserBindLog] AS D WITH (NOLOCK) ON A.StatTime = D.StatTime AND A.PlatFormID = D.PlatformID AND A.AreaID = D.AreaID
	WHERE
		ISNULL(@PlatFormID,A.PlatformID) = A.PlatFormID
		AND ISNULL(@AreaID,A.AreaID) = A.AreaID
		AND A.PlatFormID > 0 AND A.AreaID > 0 AND GameID = 0 AND ServerID = 0
		AND ISNULL(@LogStartTime,A.StatTime) <= A.StatTime
		AND ISNULL(@LogEndTime,A.StatTime) >= A.StatTime
		AND A.PlatformID in (select platformID from @Temp_PlatformID)

	--查詢DAU
	DECLARE @DAU INT
	DECLARE @DAULogStartDate DATETIME
	DECLARE @DAULogEndDate DATETIME
	DECLARE @LogPlatFormID INT
	DECLARE @LogAreaID INT

	SET @DAULogStartDate = CASE WHEN @dtStatTime IS NULL THEN DATEADD(DAY,0,DATEDIFF(DAY,0,GETDATE())) ELSE DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime)) END
	SET @DAULogEndDate = CASE WHEN @dtEndTime IS NULL THEN DATEADD(DAY,1,DATEDIFF(DAY,0,GETDATE())) ELSE DATEADD(DAY,0,DATEDIFF(DAY,0,@dtEndTime)) END
	SET @LogPlatFormID		= ISNULL(@intPlatformID,0)
	SET @LogAreaID			= ISNULL(@intAreaID,0)

	--SELECT	@DAU = SUM([TotalCount])
	--FROM [Stat].[DAULog] WITH (NOLOCK)
	--WHERE
	--	(@LogPlatFormID = PlatFormID)
	--	AND (@LogAreaID = AreaID)
	--	AND GameID = 0
	--	AND StatTime >= @DAULogStartDate
	--	AND StatTime <= @DAULogEndDate


	/*  --回傳查詢資料  */
	----------------------------------------------------------------------------------------------------------------------
	DECLARE @TotalOrder BIGINT
	DECLARE @TotalOrderUser INT
	DECLARE @TotalAgentChargeAmount BIGINT
	DECLARE @TotalAgentChargeUser INT
	DECLARE @TotalConvertAmount BIGINT
	DECLARE @TotalConvertUser INT
	DECLARE @TotalRegCount INT
	DECLARE @TotalLoginMac INT
	DECLARE @TotalLoginIP INT
	DECLARE @TotalBindUser INT
	DECLARE @OneDayKeep INT
	DECLARE @OneThirdDayKeep INT
	DECLARE @RegOrderUser INT
	DECLARE @TotalNewCharge BIGINT

	SELECT	@intPageCount = COUNT(1),
			--充值
			@TotalOrder = SUM(oldcharge + newcharge),
			--註冊用戶充值人數
			@RegOrderUser = SUM(NewOrderUser),
			--充值人數
			@TotalOrderUser = SUM(Total_OrderUser),
			--幣商充值
			@TotalAgentChargeAmount = SUM([NewAgentChargeAmount] + [OldAgentChargeAmount]),
			--幣商充值人數
			@TotalAgentChargeUser =  SUM([NewAgentChargeUser] + [OldAgentChargeUser]),
			--兌換
			@TotalConvertAmount = SUM(oldConvert + newConvert),
			--兌換人數
			@TotalConvertUser = SUM([TotalConvertUser]),
			--活躍人數
			@DAU = SUM([PCCount] + [iOSCount] + [AndRoidCount]),
			--註冊人數
			@TotalRegCount = SUM(TotalRegCount),
			--註冊設備數
			@TotalLoginMac = SUM([OldLoginMac] + [NewLoginMac]),
			--註冊IP數
			@TotalLoginIP = SUM(OldLoginIP + NewLoginIP),
			--綁定手機樹
			@TotalBindUser = SUM([TotalBindCount]),
			--當日新增充值金額
			@TotalNewCharge = SUM(ISNULL(newcharge,0))
	FROM (
			SELECT DISTINCT 
					PlatformID = ISNULL(A.PlatformID,B.PlatformID),
					AreaID = ISNULL(A.AreaID,B.AreaID),
					StatTime = ISNULL(A.stattime,B.StatTime),
					oldcharge = ISNULL(oldcharge,0),
					newcharge = ISNULL(newcharge,0),
					oldConvert = ISNULL(oldConvert,0),
					newConvert = ISNULL(newConvert,0),
					LoginCount = ISNULL((oldLogin + NewLogin),0),
					OldLogin = ISNULL(OldLogin,0),
					NewLogin = ISNULL(NewLogin,0),
					LoginMac = ISNULL(LoginMac,0),
					[OldLoginMac] = ISNULL([OldLoginMac],0),
					[NewLoginMac] = ISNULL([NewLoginMac],0),
					LoginIP = ISNULL(LoginIP,0),
					[OldLoginIP] = ISNULL([OldLoginIP],0),
					[NewLoginIP] = ISNULL([NewLoginIP],0),
					[GameTax] = ABS(ISNULL([GameTax],0)),									
					[GameTaxDetail] = ISNULL([GameTaxDetail],''),
					ARPU = CASE	WHEN Total_OrderUser IS NULL THEN 0
								WHEN (Total_OrderUser) > 0 THEN ABS([GameTax])/Total_OrderUser
							ELSE 0 END,
					OldUser_ARPU = CASE	WHEN Total_OrderUser IS NULL THEN 0
										WHEN (Total_OrderUser) > 0 THEN ABS([GameTax])/Total_OrderUser
									ELSE 0 END,
					NewUser_ARPU = CASE	WHEN Total_OrderUser IS NULL THEN 0
										WHEN (Total_OrderUser) > 0 THEN ABS([GameTax])/Total_OrderUser
									ELSE 0 END,
					Total_OrderUser = ISNULL(Total_OrderUser,0),
					OldOrderUser = ISNULL((Total_OrderUser - NewOrderUser),0),
					NewOrderUser = ISNULL(NewOrderUser,0),
					[RobotTaxAmount] = ISNULL([RobotTaxAmount],0),
					TotalRegCount = ISNULL(A.TotalRegCount,0),

					[NewAgentChargeAmount] = ISNULL(A.[NewAgentChargeAmount],0),
					[OldAgentChargeAmount] = ISNULL(A.[OldAgentChargeAmount],0),
					[NewAgentChargeUser] = ISNULL(A.[NewAgentChargeUser],0),
					[OldAgentChargeUser] = ISNULL(A.[OldAgentChargeUser],0),
					[TotalConvertUser] = ISNULL(A.[TotalConvertUser],0),

					[PCCount] = ISNULL(A.[PCCount],0),
					[iOSCount] = ISNULL(A.[iOSCount],0),
					[AndRoidCount] = ISNULL(A.[AndRoidCount],0),
					[TotalBindCount] = ISNULL(A.[TotalBindCount],0),
					[OneDayKeep] = ISNULL(A.[OneDayKeep],0),
					[Third] = ISNULL(A.[Third],0)
			FROM #Temp_TotalData AS B
				LEFT JOIN #T AS A WITH (NOLOCK) ON B.StatTime = A.StatTime AND B.PlatFormID = A.PlatFormID AND B.AreaID = A.AreaID
			WHERE 
			A.PlatformID in (select platformID from @Temp_PlatformID)
		) AS A


	


	;WITH CTE
	AS (
		SELECT	ROW_NUMBER() OVER (ORDER BY CASE	WHEN @tnyColumns = 99 AND @tnySort = 0 THEN (AreaID)
													WHEN @tnyColumns = 99 AND @tnySort = 1 THEN 0 - (AreaID)
													WHEN @tnyColumns = 0 AND @tnySort = 0 THEN (oldcharge + newcharge)
													WHEN @tnyColumns = 0 AND @tnySort = 1 THEN 0 - (oldcharge + newcharge)
													WHEN @tnyColumns = 1 AND @tnySort = 0 THEN (oldConvert + newConvert)
													WHEN @tnyColumns = 1 AND @tnySort = 1 THEN (0 - (oldConvert + newConvert))
													WHEN @tnyColumns = 2 AND @tnySort = 0 THEN DATEDIFF(DAY,0,StatTime)
													WHEN @tnyColumns = 2 AND @tnySort = 1 THEN 0 - DATEDIFF(DAY,0,StatTime)
													WHEN @tnyColumns = 2 AND @tnySort = 0 THEN DATEDIFF(DAY,0,StatTime)
													WHEN @tnyColumns = 2 AND @tnySort = 1 THEN 0 - DATEDIFF(DAY,0,StatTime)
													WHEN @tnyColumns = 3 AND @tnySort = 0 THEN (DAU)
													WHEN @tnyColumns = 3 AND @tnySort = 1 THEN 0 - (DAU)
													WHEN @tnyColumns = 4 AND @tnySort = 0 THEN ([OldLoginMac] + [NewLoginMac])
													WHEN @tnyColumns = 4 AND @tnySort = 1 THEN 0 - ([OldLoginMac] + [NewLoginMac])
													WHEN @tnyColumns = 5 AND @tnySort = 0 THEN (OldLoginIP + NewLoginIP)
													WHEN @tnyColumns = 5 AND @tnySort = 1 THEN 0 - (OldLoginIP + NewLoginIP)
													WHEN @tnyColumns = 6 AND @tnySort = 0 THEN ([TotalBindCount])
													WHEN @tnyColumns = 6 AND @tnySort = 1 THEN 0 - ([TotalBindCount])
													END DESC,(oldConvert + newConvert)) AS rowNum,*
				--ROW_NUMBER() OVER (ORDER BY A.StatTime DESC,A.PlatformID ASC,A.AreaID ASC) AS rowNum,*
		FROM (
				SELECT DISTINCT 
					PlatformID = ISNULL(B.PlatformID,A.PlatformID),
					AreaID = ISNULL(B.AreaID,A.AreaID),
					StatTime = CONVERT(NVARCHAR(10),B.stattime,23),
					oldcharge = ISNULL(oldcharge,0),
					newcharge = ISNULL(newcharge,0),
					oldConvert = ISNULL(oldConvert,0),
					newConvert = ISNULL(newConvert,0),
					LoginCount = ISNULL((oldLogin + NewLogin),0),
					OldLogin = ISNULL(OldLogin,0),
					NewLogin = ISNULL(NewLogin,0),
					LoginMac = ISNULL(LoginMac,0),
					[OldLoginMac] = ISNULL([OldLoginMac],0),
					[NewLoginMac] = ISNULL([NewLoginMac],0),
					LoginIP = ISNULL(LoginIP,0),
					[OldLoginIP] = ISNULL([OldLoginIP],0),
					[NewLoginIP] = ISNULL([NewLoginIP],0),
					[GameTax] = ABS(ISNULL([GameTax],0)),									
					[GameTaxDetail] = ISNULL([GameTaxDetail],''),
					ARPU = CASE	WHEN Total_OrderUser IS NULL THEN 0
								WHEN (Total_OrderUser) > 0 THEN ABS([GameTax])/Total_OrderUser
							ELSE 0 END,
					OldUser_ARPU = CASE	WHEN Total_OrderUser IS NULL THEN 0
										WHEN (Total_OrderUser) > 0 THEN ABS([GameTax])/Total_OrderUser
									ELSE 0 END,
					NewUser_ARPU = CASE	WHEN Total_OrderUser IS NULL THEN 0
										WHEN (Total_OrderUser) > 0 THEN ABS([GameTax])/Total_OrderUser
									ELSE 0 END,
					Total_OrderUser = ISNULL(Total_OrderUser,0),
					OldOrderUser = ISNULL((Total_OrderUser - NewOrderUser),0),
					NewOrderUser = ISNULL(NewOrderUser,0),
					[RobotTaxAmount] = ISNULL([RobotTaxAmount],0),
					TotalRegCount = ISNULL(A.TotalRegCount,0),
					C.AreaName,
					P.Name as PlatformName,

					TotalOrder = 0,
					TotalOrderUser = ISNULL(Total_OrderUser,0),
					TotalAgentChargeAmount = ISNULL([NewAgentChargeAmount] + [OldAgentChargeAmount],0),
					TotalAgentChargeUser =  ISNULL([NewAgentChargeUser] + [OldAgentChargeUser],0),
					TotalConvertAmount = ISNULL(oldConvert,0) + ISNULL(newConvert,0),
					TotalConvertUser = ISNULL([TotalConvertUser],0),
					DAU = ISNULL([PCCount] + [iOSCount] + [AndRoidCount],0),
					TotalRegCountNew = ISNULL(A.TotalRegCount,0),
					TotalLoginMac = ISNULL(LoginMac,0),
					TotalLoginIP = ISNULL(LoginIP,0),
					[TotalBindCount] = ISNULL([TotalBindCount],0),
					[OneDayKeep] = ISNULL([OneDayKeep],0),
					[Third] = ISNULL([Third],0),
					TotalNewCharge = ISNULL(newcharge,0) + ISNULL([NewAgentChargeAmount],0)
				FROM #Temp_TotalData AS B
					LEFT JOIN #T AS A WITH (NOLOCK) ON B.StatTime = A.StatTime AND B.PlatFormID = A.PlatFormID AND B.AreaID = A.AreaID
					LEFT JOIN Area.Area C with (nolock) on B.AreaID=C.AreaID
					LEFT JOIN Area.Platform P with (nolock) on B.PlatformID=P.PlatformID					
					WHERE A.PlatformID in (select platformID from @Temp_PlatformID)
			) AS A
		)
		SELECT	rowNum = 0,
				PlatformID = 0,
				AreaID = 0,
				StatTime = '1900-01-01',
				oldcharge = @TotalOrder,
				newcharge = 0,
				oldConvert = @TotalConvertAmount,
				newConvert = 0,
				LoginCount = 0,
				OldLogin = 0,
				NewLogin = 0,
				LoginMac = 0,
				[OldLoginMac] = 0,
				[NewLoginMac] = 0,
				LoginIP = 0,
				[OldLoginIP] = 0,
				[NewLoginIP] = 0,
				[GameTax] = 0,
				[GameTaxDetail] = '',
				ARPU = 0,
				OldUser_ARPU = 0,
				NewUser_ARPU = 0,
				Total_OrderUser = @TotalOrderUser,
				OldOrderUser = 0,
				NewOrderUser = @RegOrderUser,
				[RobotTaxAmount] = 0,
				TotalRegCount = @TotalRegCount,
				AreaName = '',
				PlatformName = '',
				TotalOrder = @TotalOrder,
				Total_OrderUser = @TotalOrderUser,
				TotalAgentChargeAmount = @TotalAgentChargeAmount,
				TotalAgentChargeUser = @TotalAgentChargeUser,
				TotalConvertAmount = @TotalConvertAmount,
				[TotalConvertUser] = @TotalConvertUser,
				DAU = @DAU,
				TotalRegCount = @TotalRegCount,
				TotalLoginMac = @TotalLoginMac,
				TotalLoginIP = @TotalLoginIP,
				TotalBindUser = @TotalBindUser,
				OneDayKeep = '',
				OneThirdDayKeep = '',
				TotalNewCharge = @TotalNewCharge
		UNION ALL
		SELECT * FROM CTE WHERE rowNum BETWEEN @intPageSize * (@intPageIndex -1) + 1 AND @intPageSize * @intPageIndex
		ORDER BY rowNum ASC
	----------------------------------------------------------------------------------------------------------------------

	DROP TABLE #Temp_TotalData
	DROP TABLE #T
END



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetAreaDataStat_Day] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetAreaDataStat_Day] TO [xbuser]
    AS [dbo];

