-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 收益提现页面
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserAgent_GetWithdrawOrder]
(
	@intUserID	int,
	@chvMsg		varchar(100) OUTPUT
)
AS
BEGIN

	set nocount on

	declare @temp_performedAmount bigint
	declare @status tinyint = 0
	declare @checkamount bigint
	declare @temp_bonus bigint
	declare @Permission int

	/*  取用户最后登入IP  */
	declare @ipaddress varchar(15)
	select	@ipaddress = LastLogonIPAddress
	from centerdb.users.UserInfo with (nolock)
	where
		userid = @intUserID


	/*  计算已提领金额含已申请  */
	declare @temp_totalconvert bigint
	select	@temp_totalconvert = sum([WithdrawAmount])
	from [dbo].[UserAgentWithDraw] with (nolock)
	where
		userid = @intUserID
		and [Status] in (0,2)

	set @temp_totalconvert = isnull(@temp_totalconvert,0)


	/*  计算可提领金额及总额  */
	select	@temp_bonus = [SumTaxBonus],
			@temp_performedAmount = [SumTaxBonus] - @temp_totalconvert,
			@Permission = Permission
	from [dbo].[UserAgentInfo] with (nolock)
	where
		userid = @intUserID


	/* 确认是否金额正确  */
	if(@temp_performedAmount is null or @temp_bonus is null)
	begin
		set @chvMsg	= '操作失败 : 用户余额不足'
		return -1
	end

	if(@Permission = 0)
	begin
		set @chvMsg	= '操作失败 : 您已被管理員停權,無法提領'
		return -1
	end

	if(@temp_performedAmount <= 0)
	begin
		set @chvMsg	= '操作失败 : 用户余额不足'
		return -1
	end

	/*  当下若有金额则先补提领定单  */
	if(@temp_performedAmount > 0)
	begin
		insert into [dbo].[UserAgentWithDraw]
			([LogDate],[UserID],[WithdrawAmount],[Status])
		values
			(getdate(),@intUserID,(@temp_performedAmount),@status)

		if(@@rowcount <> 1)
		begin
			set @chvMsg	= '操作失败 : 确认新增差额补提领纪录失败请联系客服'
			return -1
		end

		update [dbo].[UserAgentInfo]
		set [SumTaxConvert] = [SumTaxConvert] + @temp_performedAmount,
			@checkamount = [SumTaxBonus] - ([SumTaxConvert] + @temp_performedAmount)
		where
			userid = @intUserID

		if(@@rowcount <> 1)
		begin
			set @chvMsg	= '操作失败 : 确认新增差额数据更新失败请联系客服'
			return -1
		end
	end


	/*  取所有需要处理的定单及未处理总金额  */
	declare @temp_updatelist table (ID bigint,[WithdrawAmount] bigint)
	insert into @temp_updatelist
	select	id,[WithdrawAmount]
	from [dbo].[UserAgentWithDraw] with (nolock)
	where
		UserID = @intUserID
		and [Status] = 0

	declare @temp_updateamount bigint
	select	@temp_updateamount = sum([WithdrawAmount])
	from @temp_updatelist

	declare @temp_nowamount bigint
	select @temp_nowamount = SumTaxBonus - SumTaxConvert
	from [dbo].[UserAgentInfo] with (nolock)
	where
		userid = @intUserID
	
	if(@temp_nowamount < 0)
	begin
		set @chvMsg	= '操作失败 : 提交提领金額不足'
		return -1
	end

	if(@temp_nowamount > 0 and ((@temp_nowamount - @temp_updateamount) > 0))
	begin
		set @chvMsg	= '操作失败 : 提交提领金額不足'
		return -1
	end

	update [dbo].[UserAgentWithDraw]
	set [Status] = 2,
		[UpdateTime] = getdate()
	where
		ID in (select id from @temp_updatelist)

	if(@@rowcount = 0)
	begin
		set @chvMsg	= '操作失败 : 提交提领订单请联系客服'
		return -1
	end

	--转账到用户游戏钱包
	declare @intReturnValue int
	declare @chvMemo varchar(200)
	declare @lngSelfAmount bigint
	declare @chvErrMsg varchar(200)
	SET @intReturnValue = 0
	SET @chvMemo = '全民代理提现:'+CAST(@intUserID as varchar)+',数量:'+ CAST(@temp_updateamount as varchar)
	EXEC @intReturnValue = [CenterDB].[dbo].[PrPsWeb_UpdateUserBank]
		@intUserID,
		14003,		--转账类型
		@temp_updateamount,
		@IPAddress,	
		@chvMemo,
		@lngSelfAmount OUTPUT,
		@chvErrMsg OUTPUT


	DECLARE @NowTime	DATETIME = GETDATE()
	DECLARE @NotifyType	TINYINT = 1
	declare @chvTitle varchar(100) = '全民代理提現通知'
	declare @chvContent varchar(500) = @chvMemo
	declare @intSort int = 1
	declare @intMsgID bigint

	INSERT INTO Centerdb.Message.MessageList
		(	Title,		MsgContent,	Type,	FromUserID,	Sort,		CreateTime	)
	SELECT	@chvTitle,	@chvContent,1,		NULL,		@intSort,	GETDATE()
	
	SET @intMsgID = @@IDENTITY

	INSERT INTO Centerdb.Message.ReceiveUser
		(	MsgID,		UserID,		Status,	ReadTime )
	SELECT	@intMsgID,	@intUserID, 0,		NULL
	

	/*  --邮件通知  */
	----------------------------------------------------------------------
	EXEC NOTIFYLINKEDSERVER.[Notify].[dbo].[PrNs_SendSysMail]
		@intUserID = @intUserID,
		@tnyTypeID = @NotifyType,
		@bintParam = 0,
		@tnyStatus = 0,
		@dtCreateTime = @NowTime,
		@dtLastPostTime = @NowTime
	----------------------------------------------------------------------



	IF @intReturnValue<>1
	BEGIN
		SET @chvErrMsg = '操作失败 : 更新用户金币失败'
		RETURN -1
	END

	set @chvMsg	= '操作完成 : 已进行提现转金币'
	return 1
END








