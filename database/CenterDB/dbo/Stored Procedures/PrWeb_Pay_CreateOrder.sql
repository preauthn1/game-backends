-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<创建充值订单>
--	2018/03/23	Jason	新增渠道代碼參數 & 變數判斷
--	2018/03/28	Jason	Remark 自動取渠道
--	2018/06/21	alan	檢查充值訂單是否建立頻繁
--	20180904	alan	如果使用九聊支付, GatewayID 直接指定統一支付
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Pay_CreateOrder]
	@intPayTypeID		INT,					--充值类型
	@intUserID			INT,					--账号ID	
	@lngAmount			BIGINT,					--充值金额
	@chvIPAddress		VARCHAR(15),			--IP地址
	@chvTransID			varchar(32)=null,		--第三方标识
	@chvTransData		VARCHAR(max),			--第三方数据
	@intOrderID			INT OUTPUT,				--返回订单ID
	@chvErrMsg			VARCHAR(128) OUTPUT		--返回消息
	
AS
BEGIN


	DECLARE @intLastOrderID int = 0 --最後一筆訂單編號
	DECLARE @dtLastOrderTime datetime = null --最後一筆訂單時間
	--取得近一小時的最後一筆訂單
	SELECT top 1 @intLastOrderID = ID, @dtLastOrderTime = CreateTime from [CenterDB].[Pay].[OrderList] with (nolock)
		where userid = @intUserID and CreateTime > DATEADD(HOUR, -1 ,getdate()) order by id desc
	
	IF @dtLastOrderTime >= DATEADD(second, -10 , GETDATE()) 
	BEGIN
		set @chvErrMsg = '您的充值订单太过频繁, 请稍后10秒再试'
		return -1
	END	

	--檢查3分鐘內是否超過七筆, 如果有, 限制最後一筆時間的五分鐘內不能再進行充值
	IF (SELECT count(id) 
		FROM [CenterDB].[Pay].[OrderList] with (nolock)
		where userid = @intUserID and ID <= @intLastOrderID and CreateTime >= DATEADD(MINUTE, -3 , @dtLastOrderTime)) > 99
    AND DATEDIFF(MINUTE,@dtLastOrderTime,GETDATE())  <= 5
	BEGIN
		set @chvErrMsg = '您的充值订单太过频繁, 请稍后再试'
		return -1
	END		

	--是否正式帳號
	Declare @chvLoginName varchar(32)=''
	select @chvLoginName=LoginName from Users.UserInfo with (nolock) where UserID=@intUserID and UserType=1
	if @@ROWCOUNT > 0
	begin
		--檢查是否被鎖定
		if EXISTS(select 1 from Game.ForbidAccount with (nolock) where AccountID = @chvLoginName)
			BEGIN
				set @chvErrMsg = '您的支付功能已被封锁, 请与客服联系'
				return -1
			END
	end

	IF @lngAmount <=0 
	BEGIN
		set @chvErrMsg = '充值金额不正确'
		return -1
	END
	
	
	DECLARE @intAreaID INT,@chvPayCloseTips VARCHAR(128),@intPlatformID INT
	DECLARE @chvDBIPAddress varchar(50) 
	SELECT @intAreaID=AreaID, @chvDBIPAddress=LastLogonIPAddress from Users.UserInfo with(nolock) where UserID = @intUserID
	SELECT @intPlatformID=PlatformID  FROM [CenterDB].[Area].[Area] WHERE AreaID = @intAreaID

	IF @intAreaID IS NULL
	BEGIN
		SET @chvErrMsg = '用户ID不正确'
		RETURN -1
	END	

	set @chvIPAddress=@chvDBIPAddress	
	
	--判断充值功能是否关闭
	SELECT @chvPayCloseTips = P.PayCloseTip
	FROM Area.[Platform] P INNER JOIN Area.Area as A ON P.PlatformID = A.AreaID	
	WHERE A.AreaID  = @intAreaID AND PayStatus=0
	IF @chvPayCloseTips IS NOT NULL
	BEGIN
		SET @chvErrMsg = @chvPayCloseTips
		RETURN -1
	END
	
	DECLARE @tintTypeStatus TINYINT
	DECLARE @tnyGatewayID	TINYINT
	

	--判断第三方标识是否重复
	if @chvTransID is not null
	begin
		if EXISTS(select 1 from Pay.OrderList where PayTypeID = @intPayTypeID and TransID = @chvTransID) 
		begin
			SET @chvErrMsg = '操作失败:重复的充值订单'
			RETURN -1
		end		
	end
	
	
	--如果使用九聊支付, GatewayID 直接指定統一支付
	IF @intPayTypeID = 6
		SET @tnyGatewayID = 6



	--创建订单,状态只能为待支付
	INSERT INTO Pay.OrderList
		(
			[PayTypeID],
			[UserID],
			[Amount],
			[Status],
			[IPAddress],
			[TransID],
			[TransData],
			[CreateTime],
			[GWID]
		)
	SELECT	@intPayTypeID,
			@intUserID,
			@lngAmount,
			1,
			@chvIPAddress,
			@chvTransID,
			'创建订单:'+@chvTransData,
			GETDATE(),
			@tnyGatewayID
	
	SET @intOrderID = @@IDENTITY 
	SET @chvErrMsg = '操作成功'	
	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Pay_CreateOrder] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Pay_CreateOrder] TO [xbuser]
    AS [dbo];

