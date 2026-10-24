-- =============================================
-- Author:  Jason
-- Create date: 2018/04/02
-- Description: 用戶管理-第三方支付管理-支付訂單查詢
-- Changelog:
--			2018/04/03 alan 增加SourceTypeID, Status改回傳原值不進行加工
--							調整Userlevel從userinfo.GrandID取得
--	2018/05/10	Jason		#87 依運營需求調整等級判斷
--	2018/05/25	Harry		#373 新增搜寻条件 @intGWID  移除 @chvTransData
--	2018/06/25	jason		#515 後台增加代理統計資料
--	2018/06/27	jason		聯運需求, 開放後台給第三方查詢特定平台與渠道營運資訊 新增@intAdminID
--  2019/06/15  minglung	代理充值篩選條件支付状态判断修正
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Pay_GetOrderList]
(
	@intAdminID		INT,
	@intPlatformID	INT = NULL,
	@intAreaID		INT = NULL,
	@dtStartTime		DATETIME = NULL,
	@dtEndTime		DATETIME = NULL,
	@dtStartLastUpdateTime		DATETIME = NULL,
	@dtEndLastUpdateTime		DATETIME = NULL,
	@chvUserName	VARCHAR(32) = NULL,
	@intUserID		INT = NULL,
	@tnyStatus		TINYINT = NULL,
	@intOrderID		INT = NULL,
	@chvIPAddress	VARCHAR(15) = NULL,
	@intPayTypeID	INT = NULL,
	@intGWID        INT = NULL,
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intPageCount	INT = 0			OUTPUT,
	@intTotalChargeAmount BIGINT = 0	OUTPUT, --充值金额
	@intTotalConvertAmount	BIGINT = 0	OUTPUT, --兑换金额
	@intTotalChargeUser BIGINT = 0	OUTPUT, --充值人数
	@intTotalConvertUser BIGINT = 0	OUTPUT, --兑换人数
	@intNewTotalChargeAmount BIGINT = 0	OUTPUT, --新增充值金额
	@intNewTotalConvertAmount	BIGINT = 0	OUTPUT, --新增兑换金额
	@intNewTotalChargeUser BIGINT = 0	OUTPUT, --新增充值人数
	@intNewTotalConvertUser BIGINT = 0	OUTPUT  --新增兑换人数
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @PlatformID INT = @intPlatformID		--平台
	DECLARE @AreaID INT = @intAreaID				--渠道
	DECLARE @UserName VARCHAR(32)= @chvUserName		--帳號
	DECLARE @UserID INT = @intUserID				--用戶ID
	DECLARE @Status TINYINT = @tnyStatus			--訂單狀態
	DECLARE @OrderID INT = @intOrderID				--編號
	DECLARE @IPAddress VARCHAR(15) = @chvIPAddress	--IP地址
	DECLARE @GWID INT = @intGWID                    --第三方厂商
	DECLARE @PayTypeID INT = @intPayTypeID			--支付類型

	
	CREATE TABLE #Temp_PlatformID (PlatformID	INT Primary KEY)
	CREATE TABLE #Temp_AreaID (AreaID	INT Primary KEY)

	--筛选平台
	INSERT INTO #Temp_PlatformID
	SELECT	DISTINCT PlatformID
	FROM [dbo].[Fn_GetPlatformAccessList](@intAdminID)
	WHERE
		ISNULL(@intPlatformID,PlatformID) = PlatformID

	--筛选渠道
	IF EXISTS(Select 1 from Area.AreaAdminUser with (nolock) where AdminID=@intAdminID)
		INSERT INTO #Temp_AreaID Select distinct AreaID From Area.AreaAdminUser  with (nolock) where AdminID=@intAdminID
	ELSE
		INSERT INTO #Temp_AreaID Select distinct AreaID From Area.Area  with (nolock) where PlatformID in (Select PlatformID from #Temp_PlatformID)



	--充值金額,充值人數
	SELECT	@intTotalChargeAmount=ISNULL(SUM(UpdateAmount),0), @intTotalChargeUser=Count(Distinct UserID) FROM [LogDB].[Money].[MoneyLog] AS A WITH (NOLOCK)
	WHERE
	(@dtStartTime IS NULL OR A.[CreateTime] >= @dtStartTime)
	AND (@dtEndTime IS NULL OR A.[CreateTime] < @dtEndTime)
	AND [SourceTypeID] >= 11001 AND [SourceTypeID] < 11099
	AND (@AreaID IS NULL OR AreaID = @AreaID)
	AND AreaID IN (Select AreaID From #Temp_AreaID)
	AND PlatformID IN (Select PlatformID From #Temp_PlatformID)



	--兌換金額,兌換充值人數
	SELECT @intTotalConvertAmount=ISNULL(SUM(ABS(amount)) ,0), @intTotalConvertUser=Count(Distinct UserID)
	FROM 
	logdb.Money.ConvertLog  with (nolock) 
	WHERE (@dtStartTime IS NULL OR LastUpdateTime >= @dtStartTime) 
	AND (@dtEndTime IS NULL OR LastUpdateTime < @dtEndTime) and Status=3
	AND (@AreaID IS NULL OR AreaID = @AreaID)
	AND AreaID IN (Select AreaID From #Temp_AreaID)
	AND PlatformID IN (Select PlatformID From #Temp_PlatformID)

	--新增充值金額,新增充值人數
	SELECT	@intNewTotalChargeAmount=ISNULL(SUM(A.UpdateAmount),0), @intNewTotalChargeUser=Count(Distinct A.UserID)	
	FROM [LogDB].[Money].[MoneyLog] AS A WITH (NOLOCK)
	LEFT JOIN Users.UserInfo U WITH (NOLOCK) ON A.UserID=U.UserID
	WHERE
	(@dtStartTime IS NULL OR A.CreateTime >= @dtStartTime)
	AND (@dtEndTime IS NULL OR A.CreateTime < @dtEndTime)
	AND SourceTypeID >= 11001 AND SourceTypeID < 11099
	AND (@dtStartTime IS NULL OR U.CreateTime >= @dtStartTime)
	AND (@dtEndTime IS NULL OR U.CreateTime < @dtEndTime)
	AND (@AreaID IS NULL OR A.AreaID = @AreaID)
	AND A.AreaID IN (Select AreaID From #Temp_AreaID)
	AND PlatformID IN (Select PlatformID From #Temp_PlatformID)

	--新增兌換金額,新增兌換充值人數
	SELECT @intNewTotalConvertAmount=ISNULL(SUM(ABS(amount)) ,0), @intNewTotalConvertUser=Count(Distinct C.UserID)
	FROM 
	LogDB.Money.ConvertLog C  with (nolock) 
	LEFT JOIN Users.UserInfo U WITH (NOLOCK) ON C.UserID=U.UserID
	WHERE (@dtStartTime IS NULL OR LastUpdateTime >= @dtStartTime) 
	AND (@dtEndTime IS NULL OR LastUpdateTime < @dtEndTime) and Status=3
	AND (@dtStartTime IS NULL OR U.[CreateTime] >= @dtStartTime)
	AND (@dtEndTime IS NULL OR U.[CreateTime] < @dtEndTime)
	AND (@AreaID IS NULL OR C.AreaID = @AreaID)
	AND C.AreaID IN (Select AreaID From #Temp_AreaID)
	AND PlatformID IN (Select PlatformID From #Temp_PlatformID)
	


	--暫存數據料表
	select * 
	into #tmp_stat
	from
	(
		SELECT 
				C.PlatformID,
				E.Name as PlatformName,
				C.AreaID,
				A.CreateTime,
				A.LastUpdateTime,
				OrderID = A.Id,
				A.GWID,
				A.PayTypeID,
				GatewayName = ISNULL(F.GatewayName,'其他'),
				D.TypeName,
				D.SourceTypeID,
				A.UserID,
				B.LoginName,
				UserLevel = B.GradeID, 
				A.Amount,
				A.[Status],
				A.[IPAddress],
				A.TransID,
				A.TransData,
				A.ClaimAmount,  --客服補單金額
				B.CreateTime as UserCreateTime
		FROM [CenterDB].[Pay].[OrderList] AS A with (nolock)
			LEFT JOIN [Users].[UserInfo] AS B with (nolock) ON A.UserID = B.UserID
			LEFT JOIN [Area].[Area] AS C with (nolock) ON B.AreaID = C.AreaID
			LEFT JOIN [Area].[Platform] AS E with (nolock) ON C.PlatformID = E.PlatformID
			LEFT JOIN [Pay].[PayType] AS D with (nolock) ON A.GWID = D.GWID AND A.PayTypeID = D.TypeID
			LEFT JOIN [Pay].[PayGateway] AS F with (nolock) ON D.GWID = F.ID
			INNER JOIN #Temp_PlatformID AS Z ON C.PlatformID = Z.PlatformID
		WHERE
					
			(@AreaID IS NULL OR C.AreaID = @AreaID)
					
			AND (
			(@dtStartTime IS NULL OR A.CreateTime >= @dtStartTime) or
			(@dtStartTime IS NULL OR A.LastUpdateTime >= @dtStartTime) --如果隔天手动充值, 也包含进来
			)
			AND (
			(@dtEndTime IS NULL OR A.CreateTime < @dtEndTime) or
			(@dtEndTime IS NULL OR A.LastUpdateTime < @dtEndTime)  --如果隔天手动充值, 也包含进来
			)
					
			AND (@dtStartLastUpdateTime IS NULL OR A.LastUpdateTime >= @dtStartLastUpdateTime)
			AND (@dtEndLastUpdateTime IS NULL OR A.LastUpdateTime < @dtEndLastUpdateTime)

			AND (@UserName IS NULL OR B.LoginName = @UserName)
			AND (@UserID IS NULL OR A.UserID = @UserID)
			AND (@Status IS NULL OR A.Status = @Status)
			AND (@OrderID IS NULL OR A.Id = @OrderID)
			AND (@IPAddress IS NULL OR A.[IPAddress] = @IPAddress)
			AND (@PayTypeID IS NULL OR A.PayTypeID = @PayTypeID)
			AND (@GWID IS NULL OR A.GWID = @GWID)
			AND A.GWID IS NOT NULL
			AND C.AreaID IN (Select AreaID From #Temp_AreaID)

		UNION ALL

		--官方代理充值记录
		SELECT	a.PlatformID
				,E.Name as PlatformName
				,a.AreaID
				,a.[CreateTime]
				,a.CreateTime as LastUpdateTime
				,OrderID = a.logid
				,0 as GWID
				,99 as PayTypeID
				,'' as GatewayName
				,'' as TypeName
				,a.[SourceTypeID]
				,a.[UserID]
				,b.LoginName
				,UserLevel = B.GradeID
				,Amount = [UpdateAmount] / 100
				,2 as [Status]
				,a.[IPAddress]
				,'' as TransID
				,'' as TransData
				,0 as ClaimAmount  --客服補單金額
				,B.CreateTime as UserCreateTime
		FROM [LogDB].[Money].[MoneyLog] as a with (nolock)
			left join [Users].[UserInfo] AS B with (nolock) ON A.UserID = B.UserID
			left join [Area].[Area] AS C with (nolock) ON B.AreaID = C.AreaID
			left join [Area].[Platform] AS E with (nolock) ON C.PlatformID = E.PlatformID
			INNER JOIN #Temp_PlatformID AS Z ON C.PlatformID = Z.PlatformID
		where 
		case when isnull(@PayTypeID,99) = 99 then 11008 else 0 end = SourceTypeID
			AND (@AreaID IS NULL OR a.AreaID = @AreaID)
			AND (@dtStartTime IS NULL OR a.CreateTime >= @dtStartTime)
			AND (@dtEndTime IS NULL OR a.CreateTime < @dtEndTime)
			AND (@dtStartLastUpdateTime IS NULL OR A.CreateTime >= @dtStartLastUpdateTime)
			AND (@dtEndLastUpdateTime IS NULL OR A.CreateTime < @dtEndLastUpdateTime)
			AND (@UserName IS NULL OR LoginName = @UserName)
			AND (@UserID IS NULL OR a.UserID = @UserID)
			AND (@OrderID IS NULL OR A.logId = @OrderID)
			AND (@IPAddress IS NULL OR [IPAddress] = @IPAddress)
			AND a.AreaID IN (Select AreaID From #Temp_AreaID)
			AND (@GWID IS NULL OR @GWID=-1) --如果有篩選第三方, 則不顯示官方代理充值
			AND (@Status is null or @Status = 2) --篩選條件如果是支付成功(=2)才顯代理
			

	) as a

	------------------------


	--取得頁數
	SELECT	@intPageCount = COUNT(1) from #tmp_stat



	;WITH CTE AS
	(
		SELECT	
			ROW_NUMBER() OVER (	
				Order by Case 
				When @dtStartTime is null Then LastUpdateTime 
				When @dtStartLastUpdateTime is null Then CreateTime 
			END DESC) AS rowNum,
			PlatformID,
			PlatformName,
			AreaID,
			CreateTime,
			LastUpdateTime,
			OrderID,
			GWID,
			PayTypeID,
			GatewayName,
			TypeName,
			SourceTypeID,
			UserID,
			LoginName,
			UserLevel,
			Amount,
			[Status],
			[IPAddress],
			TransID,
			TransData,
			ClaimAmount
		FROM #tmp_stat

	)
	SELECT * FROM CTE WHERE rowNum BETWEEN @intPageSize * (@intPageIndex -1) + 1 AND @intPageSize * @intPageIndex

	--如果有篩選支付類型, 總充值金額就以orderlist數據加總
	if (@PayTypeID is not NULL or @GWID is not null)
	begin
		select @intTotalChargeAmount=isnull(sum(amount*100),0) from #tmp_stat
		select @intNewTotalChargeAmount=isnull(sum(amount*100),0) from #tmp_stat where 
			(@dtStartTime IS NULL OR UserCreateTime >= @dtStartTime) AND (@dtEndTime IS NULL OR UserCreateTime < @dtEndTime)		
		select @intNewTotalChargeUser=Count(Distinct UserID) from #tmp_stat where 
			(@dtStartTime IS NULL OR UserCreateTime >= @dtStartTime) AND (@dtEndTime IS NULL OR UserCreateTime < @dtEndTime)		
	end

	drop table #tmp_stat
	drop table #Temp_PlatformID
	drop table #Temp_AreaID
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Pay_GetOrderList] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Pay_GetOrderList] TO [xbuser]
    AS [dbo];

