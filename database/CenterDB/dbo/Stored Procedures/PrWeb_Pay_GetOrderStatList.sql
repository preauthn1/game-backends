-- =============================================
-- Author:  Jason
-- Create date: 2018/05/17
-- Description: 用戶管理-第三方支付管理-支付渠道報表
-- Changelog:
--	2018/06/25	jason	#515 後台增加代理統計資料
--	2018/07/05	jason	#576 兌換管理與支付渠道統計分別加上提現用戶數(去重) 與 充值用戶數(去重) 增加充值用戶去重數
--	2018//8/9	alan	#661 增加ios android的充值总额
--	2018//8/20	alan	增加兑换率
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Pay_GetOrderStatList]
(
	@intAdminId		INT,
	@intPlatformID	INT = NULL,
	@intAreaID		INT = NULL,
	@dtStatTime		DATETIME = NULL,
	@dtEndTime		DATETIME = NULL,
	@intPayTypeID	INT = NULL,			--0 全部 1 支付宝 2 微信支付 3 银行卡充值 4 QQ支付
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intPageCount	INT = 0		OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @PlatformID INT = @intPlatformID		--平台
	DECLARE @AreaID INT = @intAreaID				--渠道
	DECLARE @StatTime DATETIME = @dtStatTime		--開始日期
	DECLARE @EndTime DATETIME = @dtEndTime			--結束日期
	DECLARE @PayTypeID INT = @intPayTypeID			--支付類型

	DECLARE @yesStatTime datetime --昨日開始日期
	DECLARE @yesEndTime datetime --昨日結束日期
	SET @yesStatTime = CAST(Convert(varchar,DATEAdd(day,-1,GETDATE()),110) as DATETIME)
	SET @yesEndTime = GETDATE()-1

	--今日
	DECLARE @TotalCharge	BIGINT --充值總額
	DECLARE @TotalConvert	BIGINT --兌換總額
	DECLARE @TotalConvertIOS BIGINT --兌換總額IOS
	DECLARE @TotalConvertAndroid BIGINT --兌換總額Android

	DECLARE @TotalAlipay	BIGINT --支付寶總額
	DECLARE @TotalNayChatpay	BIGINT --久聊總額
	DECLARE @TotalCloudPay BIGINT --云闪付
	DECLARE @TotalShanFuPay BIGINT --闪付
	DECLARE @TotalWX	BIGINT --微信總額
	DECLARE @TotalUnionpay	BIGINT --銀行卡總額
	DECLARE @TotalQQ	BIGINT --QQ總額
	DECLARE @TotalIOS BIGINT  --IOS充直總額	
	DECLARE @TotalAndroid BIGINT  --Android 充直總額
	DECLARE @chargeusers int	--充值用户去重
	DECLARE @SuccessCount	BIGINT --成功訂單數
	DECLARE @FailCount	BIGINT --失敗訂單數
	DECLARE @SuccessRate	FLOAT --成功率
	DECLARE @ConvertCount	BIGINT --兌換总数
	DECLARE @ConvertRate	Float --兌換率	

	DECLARE @TotalAgentAmount bigint --代理總額
	DECLARE @TotalAgentIOS bigint --代理總額IOS
	DECLARE @TotalAgentAndroid bigint --代理總額Android

	--昨日
	DECLARE @yesTotalCharge	BIGINT --充值總額
	DECLARE @yesTotalConvert	BIGINT --兌換總額
	DECLARE @yesTotalConvertIOS BIGINT --兌換總額IOS
	DECLARE @yesTotalConvertAndroid BIGINT --兌換總額Android

	DECLARE @yesTotalAlipay	BIGINT --支付寶總額
	DECLARE @yesTotalNayChatpay	BIGINT --久聊總額
	DECLARE @yesTotalCloudPay BIGINT --云闪付
	DECLARE @yesTotalShanFuPay BIGINT --闪付
	DECLARE @yesTotalWX	BIGINT --微信總額
	DECLARE @yesTotalUnionpay	BIGINT --銀行卡總額
	DECLARE @yesTotalQQ	BIGINT --QQ總額
	DECLARE @yesTotalIOS BIGINT  --IOS總額	
	DECLARE @yesTotalAndroid BIGINT  --Android 總額	
	DECLARE @yesChargeusers int	--充值用户去重
	DECLARE @yesSuccessCount	BIGINT --成功訂單數
	DECLARE @yesFailCount	BIGINT --失敗訂單數
	DECLARE @yesSuccessRate	FLOAT --成功率
	DECLARE @yesConvertCount	BIGINT --兌換总数
	DECLARE @yesConvertRate	Float --兌換率	
	DECLARE @yesTotalAgentAmount bigint --代理總額
	DECLARE @yesTotalAgentIOS bigint --代理總額IOS
	DECLARE @yesTotalAgentAndroid bigint --代理總額Android

	DECLARE @HeadLineJson	VARCHAR(MAX)


	DECLARE @Temp_PayType	TABLE (GWID INT,PayTypeID INT,GatewayName VARCHAR(100),TypeName VARCHAR(100))
	DECLARE @Temp_STAT	TABLE (PayTypeID int, GatewayName VARCHAR(100),TypeName VARCHAR(100),TotalCount int, SuccessCount int, SuccessRate FLOAT)
	

	--渠道篩選
	DECLARE @Temp_AreaID TABLE (AreaID	INT)
	INSERT INTO @Temp_AreaID Select AreaID From Area.AreaAdminUser where AdminID=@intAdminID



	--取得所有支付與渠道對應表
	INSERT INTO @Temp_PayType
	SELECT P.GWID,P.TypeID,G.GatewayName,P.TypeName FROM [CenterDB].[Pay].[PayType] as P
	  left join 
	  pay.paygateway as G
	  on p.GWID = G.ID
 

	--計算每個支付渠道的筆數與成功率
	;WITH CTE2 AS
	(
			SELECT 
			a.gwid,a.PayTypeID, 
			count(id) as TotalCount,
			SuccessCount = SUM(CASE	WHEN A.Status = 2 THEN 1 ELSE 0 END),
			SuccessRate = CAST(  (SUM(CASE WHEN A.Status = 2 THEN 1 ELSE 0 END) * 1.00 / COUNT(id))* 100 AS Decimal(18,2)   )
			from 
			[CenterDB].[Pay].[OrderList] AS A WITH (NOLOCK) 
			left join pay.[PayType] as b with (nolock)
			on a.gwid = b.GWID and a.PayTypeID = b.TypeID
			left join Users.UserInfo as U with (nolock) on U.UserID=A.UserID
			where 
			(@StatTime IS NULL OR A.CreateTime >= @StatTime)
			AND (@EndTime IS NULL OR A.CreateTime < @EndTime)
			AND a.gwid is not null
			AND U.AreaID in (select AreaID from @Temp_AreaID)
			group by a.gwid,a.PayTypeID
	)
	Insert @Temp_STAT
	select T.PayTypeID, T.GatewayName,T.TypeName,ISNULL(C.TotalCount,0) as TotalCount,ISNULL(C.SuccessCount,0) as SuccessCount ,ISNULL(C.SuccessRate,0) as SuccessRate from 
	@Temp_PayType  AS T
	full join
	CTE2 as C	
	on 
	C.GWID = T.GWID AND C.PayTypeID = T.PayTypeID 


	SELECT	@intPageCount = COUNT(1)
	FROM @Temp_STAT




	/*  總金額另外計算  */
	------------------------------------------------------------------------------------------
	SELECT	@TotalAlipay = SUM([UpdateAmount]) / 100
	FROM [LogDB].[Money].[MoneyLog] AS A WITH (NOLOCK)
	WHERE
		(@StatTime IS NULL OR A.[CreateTime] >= @StatTime)
		AND (@EndTime IS NULL OR A.[CreateTime] < @EndTime)
		AND [SourceTypeID] = 11001
		AND A.AreaID in (select AreaID from @Temp_AreaID)

	SELECT	@TotalNayChatpay = SUM([UpdateAmount]) / 100
	FROM [LogDB].[Money].[MoneyLog] AS A WITH (NOLOCK)
	WHERE
		(@StatTime IS NULL OR A.[CreateTime] >= @StatTime)
		AND (@EndTime IS NULL OR A.[CreateTime] < @EndTime)
		AND [SourceTypeID] = 11011
		AND A.AreaID in (select AreaID from @Temp_AreaID)

	--雲閃付
	--SELECT	@TotalCloudPay = SUM([UpdateAmount]) / 100
	--FROM [LogDB].[Money].[MoneyLog] AS A WITH (NOLOCK)
	--WHERE
	--	(@StatTime IS NULL OR A.[CreateTime] >= @StatTime)
	--	AND (@EndTime IS NULL OR A.[CreateTime] < @EndTime)
	--	AND [SourceTypeID] = 11012
	--	AND A.AreaID in (select AreaID from @Temp_AreaID)



	--閃付
	select @TotalShanFuPay = SUM(Amount) from pay.OrderList A with (nolock) 
	left join Users.UserInfo as U with (nolock) on U.UserID=A.UserID
	where 
	GWID=7 and Status=2 
	AND (@StatTime IS NULL OR A.CreateTime >= @StatTime)
	AND (@EndTime IS NULL OR A.CreateTime < @EndTime)
	AND U.AreaID in (select AreaID from @Temp_AreaID)


	SELECT @TotalConvert= SUM(ABS(C.amount))/100,
	@TotalConvertAndroid = SUM(CASE WHEN U.HDType=2 THEN ABS( ISNULL(C.Amount,0) ) ELSE 0 END)/100
	from 
	logdb.Money.ConvertLog C with (nolock) 
	LEFT JOIN Users.UserInfo U with (nolock) on C.UserID=U.UserID
	WHERE (@StatTime IS NULL OR C.LastUpdateTime >= @StatTime) and (@EndTime IS NULL OR C.LastUpdateTime < @EndTime) and Status=3
	AND C.AreaID in (select AreaID from @Temp_AreaID)

	SET @TotalConvertIOS = @TotalConvert-@TotalConvertAndroid
		

	SELECT	@TotalWX = SUM([UpdateAmount]) / 100
	FROM [LogDB].[Money].[MoneyLog] AS A WITH (NOLOCK)
	WHERE
		(@StatTime IS NULL OR A.[CreateTime] >= @StatTime)
		AND (@EndTime IS NULL OR A.[CreateTime] < @EndTime)
		AND [SourceTypeID] = 11002
		AND A.AreaID in (select AreaID from @Temp_AreaID)

	SELECT	@TotalUnionpay = SUM([UpdateAmount]) / 100
	FROM [LogDB].[Money].[MoneyLog] AS A WITH (NOLOCK)
	WHERE
		(@StatTime IS NULL OR A.[CreateTime] >= @StatTime)
		AND (@EndTime IS NULL OR A.[CreateTime] < @EndTime)
		AND [SourceTypeID] = 11004
		AND A.AreaID in (select AreaID from @Temp_AreaID)

	SELECT	@TotalQQ = SUM([UpdateAmount]) / 100
	FROM [LogDB].[Money].[MoneyLog] AS A WITH (NOLOCK)
	WHERE
		(@StatTime IS NULL OR A.[CreateTime] >= @StatTime)
		AND (@EndTime IS NULL OR A.[CreateTime] < @EndTime)
		AND [SourceTypeID] = 11003
		AND A.AreaID in (select AreaID from @Temp_AreaID)

	SELECT @TotalAgentAmount = SUM([UpdateAmount]) / 100 ,
	@TotalAgentAndroid = SUM(CASE WHEN U.HDType=2 THEN ISNULL(UpdateAmount,0) ELSE 0 END)/100
	from [LogDB].[Money].[MoneyLog] M with (nolock)
	LEFT JOIN Users.UserInfo U with (nolock) on M.UserID=U.UserID
	where [SourceTypeID] = 11008
	and (@StatTime IS NULL OR M.CreateTime >= @StatTime) AND (@EndTime IS NULL OR M.CreateTime < @EndTime)
	AND M.AreaID in (select AreaID from @Temp_AreaID)

	SET @TotalAgentIOS = @TotalAgentAmount-@TotalAgentAndroid
	

	SET @TotalCharge = ISNULL(@TotalAlipay,0) + ISNULL(@TotalWX,0) + ISNULL(@TotalUnionpay,0) + ISNULL(@TotalQQ,0) + ISNULL(@TotalNayChatpay,0) + isnull(@totalagentamount,0)


	SELECT	@chargeusers = count(distinct userid) 
	from [LogDB].[Money].[MoneyLog] with (nolock) 
	where [SourceTypeID] >= 11001 and [SourceTypeID] < 11099
	AND (@StatTime IS NULL OR [CreateTime] >= @StatTime)
	AND (@EndTime IS NULL OR [CreateTime] < @EndTime)
	AND AreaID in (select AreaID from @Temp_AreaID)

	SET @chargeusers = isnull(@chargeusers,0)
	

	select @TotalIOS = Sum(A.updateamount/100) from [LogDB].[Money].[MoneyLog] A with (nolock)
	left join Users.UserInfo B with (nolock)
	On a.UserID = B.UserID and UserType <> 2
	where A.[SourceTypeID] >= 11001 and A.[SourceTypeID] < 11099
	and (@StatTime IS NULL OR A.[CreateTime] >= @StatTime)
	AND (@EndTime IS NULL OR A.[CreateTime] < @EndTime)
	AND B.HDType=3
	AND A.AreaID in (select AreaID from @Temp_AreaID)
	
	SET @TotalAndroid=@TotalCharge-@TotalIOS
	
	
	--兑换数
	select @ConvertCount=SUM(ABS(amount))/100 from LogDB.Money.ConvertLog with (nolock) 
	where Status = 3 
	and (@StatTime IS NULL OR CreateTime >= @StatTime)
	AND (@EndTime IS NULL OR CreateTime < @EndTime)	
	AND AreaID in (select AreaID from @Temp_AreaID)

	--兑换率
	SET @ConvertRate =  ABS( CAST( @TotalConvert*1.0 /(CASE WHEN @TotalCharge = 0 THEN 1 ELSE @TotalCharge  END ) * 100 AS Decimal(18,2) ))
	


	--計算所有的總付費與成功率
	SELECT	@SuccessCount = SUM(CASE WHEN A.Status = 2 THEN 1 ELSE 0 END),
			@FailCount = SUM(CASE WHEN A.Status in (1,3) THEN 1 ELSE 0 END),
			@SuccessRate =  CAST(  @SuccessCount*1.0 / (@FailCount + @SuccessCount) * 100 AS Decimal(18,2) )

	FROM [CenterDB].[Pay].[OrderList] AS A WITH (NOLOCK)
		LEFT JOIN [Users].[UserInfo] AS B WITH (NOLOCK) ON A.UserID = B.UserID and UserType <> 2
		LEFT JOIN [Area].[Area] AS C WITH (NOLOCK) ON B.AreaID = C.AreaID
		LEFT JOIN [Area].[Platform] AS E WITH (NOLOCK) ON C.PlatformID = E.PlatformID
	WHERE
		(@PlatformID IS NULL OR C.PlatformID = @PlatformID)
		AND (@AreaID IS NULL OR C.AreaID = @AreaID)
		AND (@StatTime IS NULL OR A.CreateTime >= @StatTime)
		AND (@EndTime IS NULL OR A.CreateTime < @EndTime)
		AND A.GWID is NOT NULL
		AND B.AreaID in (select AreaID from @Temp_AreaID)
	------------------------------------------------------------------------------------------






	/*  昨日金額  */
	------------------------------------------------------------------------------------------
	SELECT	@yesTotalAlipay = SUM([UpdateAmount]) / 100
	FROM [LogDB].[Money].[MoneyLog] AS A WITH (NOLOCK)
	WHERE
		(@yesStatTime IS NULL OR A.[CreateTime] >= @yesStatTime)
		AND (@yesEndTime IS NULL OR A.[CreateTime] < @yesEndTime)
		AND [SourceTypeID] = 11001
		AND A.AreaID in (select AreaID from @Temp_AreaID)

	SELECT	@yesTotalNayChatpay = SUM([UpdateAmount]) / 100
	FROM [LogDB].[Money].[MoneyLog] AS A WITH (NOLOCK)
	WHERE
		(@yesStatTime IS NULL OR A.[CreateTime] >= @yesStatTime)
		AND (@yesEndTime IS NULL OR A.[CreateTime] < @yesEndTime)
		AND [SourceTypeID] = 11011
		AND A.AreaID in (select AreaID from @Temp_AreaID)

	--雲閃付
	--SELECT	@yesTotalCloudPay = SUM([UpdateAmount]) / 100
	--FROM [LogDB].[Money].[MoneyLog] AS A WITH (NOLOCK)
	--WHERE
	--	(@yesStatTime IS NULL OR A.[CreateTime] >= @yesStatTime)
	--	AND (@yesEndTime IS NULL OR A.[CreateTime] < @yesEndTime)
	--	AND [SourceTypeID] = 11012
	--	AND A.AreaID in (select AreaID from @Temp_AreaID)		



	--閃付
	select @yesTotalShanFuPay = SUM(Amount) from pay.OrderList A with (nolock) 
	left join Users.UserInfo as U with (nolock) on U.UserID=A.UserID
	where 
	GWID=7 and Status=2 
	AND (@yesStatTime IS NULL OR A.CreateTime >= @yesStatTime)
	AND (@yesEndTime IS NULL OR A.CreateTime < @yesEndTime)
	AND U.AreaID in (select AreaID from @Temp_AreaID)




	SELECT @yesTotalConvert=SUM(ABS(amount))/100,
	@yesTotalConvertAndroid = SUM(CASE WHEN U.HDType=2 THEN ABS( ISNULL(C.Amount,0) ) ELSE 0 END)/100
	from 
	logdb.Money.ConvertLog C with (nolock) 
	LEFT JOIN Users.UserInfo U with (nolock) on C.UserID=U.UserID
	WHERE (@yesStatTime IS NULL OR C.LastUpdateTime >= @yesStatTime) and (@yesEndTime IS NULL OR C.LastUpdateTime < @yesEndTime) and Status=3
	AND C.AreaID in (select AreaID from @Temp_AreaID)

	SET @yesTotalConvertIOS = @yesTotalConvert-@yesTotalConvertAndroid



	SELECT	@yesTotalWX = SUM([UpdateAmount]) / 100
	FROM [LogDB].[Money].[MoneyLog] AS A WITH (NOLOCK)
	WHERE
		(@yesStatTime IS NULL OR A.[CreateTime] >= @yesStatTime)
		AND (@yesEndTime IS NULL OR A.[CreateTime] < @yesEndTime)
		AND [SourceTypeID] = 11002
		AND A.AreaID in (select AreaID from @Temp_AreaID)

	SELECT	@yesTotalUnionpay = SUM([UpdateAmount]) / 100
	FROM [LogDB].[Money].[MoneyLog] AS A WITH (NOLOCK)
	WHERE
		(@yesStatTime IS NULL OR A.[CreateTime] >= @yesStatTime)
		AND (@yesEndTime IS NULL OR A.[CreateTime] < @yesEndTime)
		AND [SourceTypeID] = 11004
		AND A.AreaID in (select AreaID from @Temp_AreaID)


	SELECT	@yesTotalQQ = SUM([UpdateAmount]) / 100
	FROM [LogDB].[Money].[MoneyLog] AS A WITH (NOLOCK)
	WHERE
		(@yesStatTime IS NULL OR A.[CreateTime] >= @yesStatTime)
		AND (@yesEndTime IS NULL OR A.[CreateTime] < @yesEndTime)
		AND [SourceTypeID] = 11003
		AND A.AreaID in (select AreaID from @Temp_AreaID)

	select @yesTotalAgentAmount = SUM([UpdateAmount]) / 100,
	@yesTotalAgentAndroid = SUM(CASE WHEN U.HDType=2 THEN ISNULL(UpdateAmount,0) ELSE 0 END)/100
	from [LogDB].[Money].[MoneyLog] M with (nolock)
	LEFT JOIN Users.UserInfo U with (nolock) on M.UserID=U.UserID
	where [SourceTypeID] = 11008
	AND (@yesStatTime IS NULL OR M.CreateTime >= @yesStatTime)
	AND (@yesEndTime IS NULL OR M.CreateTime < @yesEndTime)
	AND M.AreaID in (select AreaID from @Temp_AreaID)

	SET @yesTotalAgentIOS = @yesTotalAgentAmount-@yesTotalAgentAndroid


	SET @yesTotalCharge = ISNULL(@yesTotalAlipay,0) + ISNULL(@yesTotalWX,0) + ISNULL(@yesTotalUnionpay,0) + ISNULL(@yesTotalQQ,0) + ISNULL(@yesTotalNayChatpay,0) + isnull(@yesTotalagentamount,0)


	SELECT	@yesChargeusers = count(distinct userid) 
		from [LogDB].[Money].[MoneyLog] with(nolock) 
		where [SourceTypeID] >= 11001 and [SourceTypeID] < 11099
		AND (@yesStatTime IS NULL OR [CreateTime] >= @yesStatTime)
		AND (@yesEndTime IS NULL OR [CreateTime] < @yesEndTime)
		AND AreaID in (select AreaID from @Temp_AreaID)

	SET @yesChargeusers = isnull(@yesChargeusers,0)
	

	select @yesTotalIOS = Sum(A.updateamount/100) from [LogDB].[Money].[MoneyLog] A with (nolock)
	left join Users.UserInfo B with (nolock)
	On a.UserID = B.UserID and UserType <> 2
	where A.[SourceTypeID] >= 11001 and A.[SourceTypeID] < 11099
	and (@yesStatTime IS NULL OR A.[CreateTime] >= @yesStatTime)
	AND (@yesEndTime IS NULL OR A.[CreateTime] < @yesEndTime)
	AND B.HDType=3
	AND A.AreaID in (select AreaID from @Temp_AreaID)	

	SET @yesTotalAndroid=@yesTotalCharge - @yesTotalIOS
	
	
	--兑换数
	select @yesConvertCount=SUM(ABS(amount))/100 from LogDB.Money.ConvertLog with (nolock) 
	where Status = 3 
	and (@yesStatTime IS NULL OR CreateTime >= @yesStatTime)
	AND (@yesEndTime IS NULL OR CreateTime < @yesEndTime)	
	AND AreaID in (select AreaID from @Temp_AreaID)

	--兑换率
	SET @yesConvertRate =  ABS( CAST( @yesTotalConvert*1.0 /(CASE WHEN @yesTotalCharge = 0 THEN 1 ELSE @yesTotalCharge  END ) * 100 AS Decimal(18,2) ))
	


	--計算所有的總付費與成功率
	SELECT	@yesSuccessCount = SUM(CASE WHEN A.Status = 2 THEN 1 ELSE 0 END),
			@yesFailCount = SUM(CASE WHEN A.Status in (1,3) THEN 1 ELSE 0 END),
			@yesSuccessRate =  CAST(  @yesSuccessCount*1.0 / (@yesFailCount + @yesSuccessCount) * 100 AS Decimal(18,2) )

	FROM [CenterDB].[Pay].[OrderList] AS A WITH (NOLOCK)
		LEFT JOIN [Users].[UserInfo] AS B WITH (NOLOCK) ON A.UserID = B.UserID and UserType <> 2
		LEFT JOIN [Area].[Area] AS C WITH (NOLOCK) ON B.AreaID = C.AreaID
		LEFT JOIN [Area].[Platform] AS E WITH (NOLOCK) ON C.PlatformID = E.PlatformID
	WHERE
		(@PlatformID IS NULL OR C.PlatformID = @PlatformID)
		AND (@AreaID IS NULL OR C.AreaID = @AreaID)
		AND (@yesStatTime IS NULL OR A.CreateTime >= @yesStatTime)
		AND (@yesEndTime IS NULL OR A.CreateTime < @yesEndTime)
		AND A.GWID is NOT NULL
		AND B.AreaID in (select AreaID from @Temp_AreaID)
	------------------------------------------------------------------------------------------






	SET @HeadLineJson = '{"TotalCharge":' + CAST(CASE WHEN @TotalCharge IS NULL THEN '' ELSE @TotalCharge END AS VARCHAR) +
							',"TotalConvert":'+CAST(CASE WHEN @TotalConvert IS NULL THEN '' ELSE @TotalConvert END AS VARCHAR) +
							',"TotalAlipay":'+CAST(CASE WHEN @TotalAlipay IS NULL THEN '' ELSE @TotalAlipay END AS VARCHAR) +
							--',"TotalCloudpay":'+CAST(CASE WHEN @TotalCloudPay IS NULL THEN '' ELSE @TotalCloudPay END AS VARCHAR) +
							',"TotalShanFuPay":'+CAST(CASE WHEN @TotalShanFuPay IS NULL THEN '' ELSE @TotalShanFuPay END AS VARCHAR) +
							',"TotalNayChatpay":'+CAST(CASE WHEN @TotalNayChatpay IS NULL THEN '' ELSE @TotalNayChatpay END AS VARCHAR) +
							',"TotalWX":'+CAST(CASE WHEN @TotalWX IS NULL THEN '' ELSE @TotalWX END AS VARCHAR)+
							',"TotalUnionpay":' + CAST(CASE WHEN @TotalUnionpay IS NULL THEN '' ELSE @TotalUnionpay END AS VARCHAR) + 
							',"TotalQQ":' + CAST(CASE WHEN @TotalQQ IS NULL THEN '' ELSE @TotalQQ END AS VARCHAR) +
							',"totalagentamount":' + CAST(CASE WHEN @totalagentamount IS NULL THEN '' ELSE @totalagentamount END AS VARCHAR) + 
							',"SuccessCount":' + CAST(CASE WHEN @SuccessCount IS NULL THEN '' ELSE @SuccessCount END AS VARCHAR) +
							',"FailCount":' + CAST(CASE WHEN @FailCount IS NULL THEN '' ELSE @FailCount END AS VARCHAR) + 
							',"SuccessRate":' + CAST(CASE WHEN @SuccessRate IS NULL THEN '' ELSE @SuccessRate END AS VARCHAR) +
							',"TotalChargeUser":' + CAST(CASE WHEN @chargeusers IS NULL THEN '' ELSE @chargeusers END AS VARCHAR) +
							',"TotalIOS":' + CAST(CASE WHEN @TotalIOS IS NULL THEN '' ELSE @TotalIOS END AS VARCHAR) +
							',"TotalAndroid":' + CAST(CASE WHEN @TotalAndroid IS NULL THEN '' ELSE @TotalAndroid END AS VARCHAR) +
							',"TotalConvertIOS":' + CAST(CASE WHEN @TotalConvertIOS IS NULL THEN '' ELSE @TotalConvertIOS END AS VARCHAR) +
							',"TotalConvertAndroid":' + CAST(CASE WHEN @TotalConvertAndroid IS NULL THEN '' ELSE @TotalConvertAndroid END AS VARCHAR) +
							',"TotalAgentIOS":' + CAST(CASE WHEN @TotalAgentIOS IS NULL THEN '' ELSE @TotalAgentIOS END AS VARCHAR) +
							',"TotalAgentAndroid":' + CAST(CASE WHEN @TotalAgentAndroid IS NULL THEN '' ELSE @TotalAgentAndroid END AS VARCHAR) +
							',"ConvertRate":' + CAST(CASE WHEN @ConvertRate IS NULL THEN '' ELSE @ConvertRate END AS VARCHAR) + 
								
							',"yesTotalCharge":' + CAST(CASE WHEN @yesTotalCharge IS NULL THEN '' ELSE @yesTotalCharge END AS VARCHAR) +
							',"yesTotalConvert":'+CAST(CASE WHEN @yesTotalConvert IS NULL THEN '' ELSE @yesTotalConvert END AS VARCHAR) +
							',"yesTotalAlipay":'+ CAST(CASE WHEN @yesTotalAlipay IS NULL THEN '' ELSE @yesTotalAlipay END AS VARCHAR) +
							--',"yesTotalCloudPay":'+ CAST(CASE WHEN @yesTotalCloudPay IS NULL THEN '' ELSE @yesTotalCloudPay END AS VARCHAR) +
							',"yesTotalShanFuPay":'+ CAST(CASE WHEN @yesTotalShanFuPay IS NULL THEN '' ELSE @yesTotalShanFuPay END AS VARCHAR) +
							',"yesTotalNayChatpay":'+CAST(CASE WHEN @yesTotalNayChatpay IS NULL THEN '' ELSE @yesTotalNayChatpay END AS VARCHAR) +
							',"yesTotalWX":'+ CAST(CASE WHEN @yesTotalWX IS NULL THEN '' ELSE @yesTotalWX END AS VARCHAR)+
							',"yesTotalUnionpay":' + CAST(CASE WHEN @yesTotalUnionpay IS NULL THEN '' ELSE @yesTotalUnionpay END AS VARCHAR) + 
							',"yesTotalQQ":' + CAST(CASE WHEN @yesTotalQQ IS NULL THEN '' ELSE @yesTotalQQ END AS VARCHAR) +
							',"yesTotalagentamount":' + CAST(CASE WHEN @yesTotalagentamount IS NULL THEN '' ELSE @yesTotalagentamount END AS VARCHAR) + 
							',"yesSuccessCount":' + CAST(CASE WHEN @yesSuccessCount IS NULL THEN '' ELSE @yesSuccessCount END AS VARCHAR) +
							',"yesFailCount":' + CAST(CASE WHEN @yesFailCount IS NULL THEN '' ELSE @yesFailCount END AS VARCHAR) + 
							',"yesSuccessRate":' + CAST(CASE WHEN @yesSuccessRate IS NULL THEN '' ELSE @yesSuccessRate END AS VARCHAR) +
							',"yesTotalChargeUser":' + CAST(CASE WHEN @yesChargeusers IS NULL THEN '' ELSE @yesChargeusers END AS VARCHAR) +
							',"yesTotalIOS":' + CAST(CASE WHEN @yesTotalIOS IS NULL THEN '' ELSE @yesTotalIOS END AS VARCHAR) +
							',"yesTotalAndroid":' + CAST(CASE WHEN @yesTotalAndroid IS NULL THEN '' ELSE @yesTotalAndroid END AS VARCHAR) +
							',"yesTotalConvertIOS":' + CAST(CASE WHEN @yesTotalConvertIOS IS NULL THEN '' ELSE @yesTotalConvertIOS END AS VARCHAR) +
							',"yesTotalConvertAndroid":' + CAST(CASE WHEN @yesTotalConvertAndroid IS NULL THEN '' ELSE @yesTotalConvertAndroid END AS VARCHAR) +
							',"yesTotalAgentIOS":' + CAST(CASE WHEN @yesTotalAgentIOS IS NULL THEN '' ELSE @yesTotalAgentIOS END AS VARCHAR) +
							',"yesTotalAgentAndroid":' + CAST(CASE WHEN @yesTotalAgentAndroid IS NULL THEN '' ELSE @yesTotalAgentAndroid END AS VARCHAR) +
							',"yesConvertRate":' + CAST(CASE WHEN @yesConvertRate IS NULL THEN '' ELSE @yesConvertRate END AS VARCHAR) + 							
							'}'							

	--取得所有資料
	;WITH CTE AS
	(
		SELECT  ROW_NUMBER() OVER (ORDER BY SuccessRate desc) AS rowNum,
		TypeName,GatewayName,SuccessCount,TotalCount,SuccessRate,TransData='' from 
		@Temp_STAT	
	)
	SELECT rowNum, TypeName,GatewayName, SuccessCount, TotalCount, SuccessRate,TransData FROM 
	CTE 
	WHERE
	rowNum BETWEEN @intPageSize * (@intPageIndex -1) + 1 AND @intPageSize * @intPageIndex
	UNION ALL
	SELECT 0,'','',0,0,0,@HeadLineJson

	--增加最後一筆保存HeadLineJson
	SET @intPageCount = @intPageCount+1
END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Pay_GetOrderStatList] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Pay_GetOrderStatList] TO [xbuser]
    AS [dbo];

