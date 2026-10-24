-- =============================================
-- Author:  alan
-- Create date: 2018/12/23
-- Description: 日報表總匯查詢
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetAreaDataSummary_Day]
(
	@intPlatformID	INT = null,
	@intAreaID		INT = null,
	@dtStatTime		DATETIME = null,
	@dtEndTime		DATETIME = null,
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 500,
	@intPageCount	INT = 0	output	
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @LogStartTime	DATETIME
	DECLARE @LogEndTime		DATETIME
	DECLARE @TempLogSTime	DATETIME
	SET @LogStartTime	= CASE	WHEN @dtStatTime IS NULL THEN DATEADD(DAY,0,DATEDIFF(DAY,0,GETDATE())) ELSE DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime)) END
	SET @LogEndTime		= CASE	WHEN @dtEndTime IS NULL THEN DATEADD(DAY,1,DATEDIFF(DAY,0,GETDATE())) ELSE DATEADD(DAY,1,DATEDIFF(DAY,0,@dtEndTime)) END
	SET @TempLogSTime	= @LogStartTime

	CREATE TABLE #Temp_TotalData (PlatformID INT, AreaID INT, AreaName varchar(100), StatTime DATETIME)

	WHILE (@TempLogSTime < @LogEndTime)
	BEGIN
		INSERT INTO #Temp_TotalData
		SELECT	PlatformID,AreaID,AreaName,@TempLogSTime
		FROM [Area].[Area] WITH (NOLOCK)
		WHERE
		ISNULL(@intPlatFormID,PlatformID) = PlatFormID
		AND ISNULL(@intAreaID,AreaID) = AreaID

		SET @TempLogSTime = DATEADD(DAY,1,@TempLogSTime)
	END


	--取得當日註冊資訊
	select * into #tmpReg from LogDB.stat.StatisticsRegUser where	@LogStartTime <= StatTime AND @LogEndTime > StatTime and PlatFormID > 0 AND AreaID > 0 
	--取得綁定資訊
	select * into #tmpBind from LogDB.stat.StatisticsUserBind where	@LogStartTime <= StatTime AND @LogEndTime > StatTime and PlatFormID > 0 AND AreaID > 0
	--取得登入遊戲帳號資訊
	select * into #tmpAreaGameData from Stat.AreaDataDay where		@LogStartTime <= StatTime AND @LogEndTime > StatTime and PlatFormID > 0 AND AreaID > 0 AND GameID = 0 AND ServerID = 0
	--取得留存
	select PlatformID, AreaID, StatTime, OneDayKeep as Keep1,Third as Keep3, Fifth as Keep5, WeekKeep as Keep7 into #tmpKeep from Stat.KeepLog where @LogStartTime <= StatTime AND @LogEndTime > StatTime and PlatFormID > 0 AND AreaID > 0

	SELECT	
	P.PlatformID, P.AreaID, P.AreaName, P.StatTime,

	--營收 充值減去兌換
	----------------------------------------------------------
	Income = (A.OldCharge+A.NewCharge) - ABS((A.OldConvert+A.NewConvert)),


	--金幣數量
	----------------------------------------------------------
	A.TotalMoney,     --用戶金幣金額
	A.TotalBankMoney, --用戶保險箱金額

	--有登入遊戲相關用戶資訊
	----------------------------------------------------------
	A.OldLogin, --舊用戶数登入游戏(去重)
	A.NewLogin, --新用户数登入游戏(去重)
	A.LoginIP,    --登入遊戲IP數(去重)
	A.OldLoginIP, --舊用戶登入遊戲IP數(去重)
	A.NewLoginIP, --新用戶登入遊戲IP數(去重)
	A.LoginMac,   --登入遊戲用戶機器數(去重)
	A.OldLoginMac,--舊用戶登入遊戲機器數(去重)
	A.NewLoginMac,--新用戶登入遊戲機器數(去重)
	A.IOSCount,   --蘋果註冊數且登入遊戲(去重)
	A.NewiOSCount,--新用戶蘋果註冊數且登入遊戲(去重)
	A.AndroidCount,    --安卓註冊數且登入遊戲(去重)
	A.NewAndroidCount, --新用戶安卓註冊數且登入遊戲(去重)
	A.PCCount,    --PC註冊數且登入遊戲(去重)
	A.NewPCCount, --新用戶PC註冊數且登入遊戲(去重)


	--充值用戶資訊
	----------------------------------------------------------
	A.Total_OrderUser,  --不重複充值人數
	A.NewOrderUser,     --新用戶不重複充值人數
	A.TotalOrderCounts, --充值成功筆數
	A.NewOrderCounts,   --新用戶充值成功筆數
	TotalCharge=(A.OldCharge+A.NewCharge), --總充值金額
	A.OldCharge,  --舊用戶充值金額 (已經包含代理充值數據)
	A.NewCharge,  --新用戶充值金額 (已經包含代理充值數據)
	A.OldAgentChargeAmount, --舊用戶代理充值金額
	A.NewAgentChargeAmount, --新用戶代理充值金額


	--兌換用戶資訊
	----------------------------------------------------------
	A.TotalConvertUser,   --兌換去重用戶數
	A.NewConvertUser,     --新用戶兌換去重用戶數
	A.TotalConvertCounts, --兌換成功筆數
	TotalConvert=(A.OldConvert+A.NewConvert), --總兌換金額
	A.OldConvert,    --舊用戶兌換金額
	A.NewConvert,    --新用戶兌換金額
	A.OldConvertTax, --舊用戶兌換手續費金額
	A.NewConvertTax, --新用戶兌換手續費金額

	--註冊用戶資訊
	----------------------------------------------------------
	TotalRegCount=ISNULL(R.TotalRegCount,0),	--當日註冊用戶總數
	IOSRegCount=ISNULL(R.IOSRegCount,0),		--當日註冊IOS用戶數 (等同 當日登錄IOS新用戶數)
	AndroidRegCount=ISNULL((R.AndroidRegCount+R.PCRegCount),0), --當日註冊安卓用戶數 (等同 當日登錄安卓新用戶數)
	
	TotalBindUser=ISNULL(B.TotalBindCount,0), --當日綁定用戶數
	BindIOSUser=ISNULL(B.iOSCount,0),         --當日綁定IOS用戶數
	BindAndroidUser=ISNULL((B.AndRoidCount+B.PCCount),0), --當日綁定安卓用戶數

	--登入APP用戶資訊
	----------------------------------------------------------
	LoginAppUser=ISNULL(R.LoginAppUser,0),    --當日登錄app用戶數
	LoginAppIOSUser=ISNULL(R.LoginAppIOSUser,0), --當日登錄app IOS用戶數
	LoginAppAndroidUser=ISNULL((R.LoginAppAndroidUser+R.LoginAppPCUser),0), --當日登錄app 安卓用戶數
	OldLoginAppUser=ISNULL((R.LoginAppUser-R.TotalRegCount),0), --當日登錄app舊用戶數
	OldLoginAppIOSUser= --當日登錄app舊IOS用戶數
		CASE WHEN (R.LoginAppIOSUser-R.IOSRegCount) > 0 THEN ISNULL((R.LoginAppIOSUser-R.IOSRegCount),0) ELSE 0 END, 
	OldLoginAppAndroidUser= --當日登錄app舊安卓用戶數
		CASE WHEN (R.LoginAppAndroidUser+R.LoginAppPCUser)-(R.AndroidRegCount+R.PCRegCount) > 0 THEN ISNULL((R.LoginAppAndroidUser+R.LoginAppPCUser)-(R.AndroidRegCount+R.PCRegCount),0) ELSE 0 END, 


	--遊戲稅收
	----------------------------------------------------------
	A.UserGameAmount,   --用戶(不含機器人)遊戲輸贏
	A.GameTax,			--用戶(不含機器人)游戏税收 (=系统税收)
	A.RobotGameAmount,	--機器人遊戲輸贏 (=系統輸贏)
	A.RobotTaxAmount,	--機器人遊戲稅收
	GameTaxDetail=ISNULL(dbo.Fn_xml2Json(A.GameTaxDetail),''),	--游戏税收內容

	A.AmountTotal,		--用戶(含机器人)輸贏總金額
	A.TaxAmountTotal,	--用戶(含机器人)遊戲稅收總金額
	A.NewAmountTotal,	--新用戶(含机器人)輸贏總金額
	A.NewTaxAmountTotal,--新用戶(含机器人)遊戲稅收總金額
	A.AmountAvg,		--用戶(含机器人)輸贏平均
	A.TaxAmountAvg,		--用戶(含机器人)遊戲稅收平均 
	A.NewAmountAvg,		--新用戶(含机器人)輸贏平均
	A.NewTaxAmountAvg, --新用戶(含机器人)遊戲稅收平均

	A.[GameTimeMax],
	A.[GameTimeMin],
	A.[GameTimeAvg],

	Keep1=ISNULL(K.Keep1,0), --1日留存
	Keep3=ISNULL(K.Keep3,0), --3日留存
	Keep4=ISNULL(K.Keep5,0), --5日留存
	Keep7=ISNULL(K.Keep7,0)  --7日留存
	
	INTO #T
	FROM 
	#Temp_TotalData P WITH (NOLOCK) 
	left join #tmpAreaGameData A WITH (NOLOCK)	ON P.PlatformID=A.PlatformID AND  P.AreaID=A.AreaID and P.StatTime=A.StatTime 
	left join #tmpReg R WITH (NOLOCK)			ON P.PlatformID=R.PlatformID AND  P.AreaID=R.AreaID and P.StatTime=R.StatTime 
	left join #tmpBind B WITH (NOLOCK)			ON P.PlatformID=B.PlatformID AND  P.AreaID=B.AreaID and P.StatTime=B.StatTime 
	left join #tmpKeep K  WITH (NOLOCK)			ON P.PlatformID=K.PlatformID AND  P.AreaID=K.AreaID and P.StatTime=K.StatTime 

	/*  --回傳查詢資料  */
	----------------------------------------------------------------------------------------------------------------------
	SELECT	@intPageCount = COUNT(1)
	FROM (
			SELECT * FROM #T WITH (NOLOCK)
	) AS A
	
	--select @intPageCount
	--select @LogStartTime,@LogEndTime

	;WITH CTE
	AS (
		SELECT ROW_NUMBER() OVER (ORDER BY A.StatTime DESC,A.PlatformID ASC,A.AreaID ASC) AS rowNum, *
		FROM (
				SELECT T.*
				FROM #T AS T

			) AS A
		)
		SELECT * FROM CTE WHERE rowNum BETWEEN @intPageSize * (@intPageIndex -1) + 1 AND @intPageSize * @intPageIndex
	----------------------------------------------------------------------------------------------------------------------

	--DROP TABLE #T
	--drop table #Temp_TotalData
	--drop table #tmpAreaGameData
	--drop table #tmpReg
	--drop table #tmpBind

END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetAreaDataSummary_Day] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetAreaDataSummary_Day] TO [xbuser]
    AS [dbo];

