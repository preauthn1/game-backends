
-- =============================================
-- Author:  jason
-- Create date: 20190502
-- Description: 后台  无极限- 财务提现审核
--2019/07/11	CCC		預提、提現審核通過自動發放金幣
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Unlimit_WithdrawOrder]
(
	@intAdminID int,
	@lngOrderID bigint,
	@intStatus int = null,
	@chvMsg varchar(100) OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON


	/*  取出订单金额  */
	declare @ordertype int
	declare @orderamount bigint
	declare @withdrawuserid int
	declare @platformid int
	declare @orderidstatus int


	select	@ordertype = [WithdrawType],	-- 1:预提 2:提领
			@withdrawuserid = userid,
			@orderidstatus = [Status]
	from [dbo].[Unlimit_WithDrawLog] with (nolock)
	where
		ID = @lngOrderID

	select	@platformid = platformid
	from [dbo].[Unlimit_UserInfo] with (nolock)
	where
		userid = @withdrawuserid


	/*  取用户最后登入IP  */
	declare @ipaddress varchar(15)
	select	@ipaddress = LastLogonIPAddress
	from centerdb.users.UserInfo with (nolock)
	where
		userid = @withdrawuserid


	if not exists (select top 1 1 from [dbo].[Unlimit_WithDrawLog] with (nolock) where ID = @lngOrderID)
	begin
		set @chvMsg = '操作错误 : 查询不到订单号码'
		return -1
	end

	if(@orderidstatus in(1,2))
	begin
		set @chvMsg = '操作错误 : 订单已调整过'
		return -1
	end

	if(@intStatus is null or @intStatus not in (1,2))
	begin
		set @chvMsg = '操作错误 : 调整订单状态错误'
		return -1
	end

	if not exists (select top 1 1 from [dbo].[AgentAdminUser] with (nolock) where ID = @intAdminID and [AdminType] = 1 and Status = 1)
	begin
		set @chvMsg = '操作错误 : 您无权限审核提现订单,请联系财务'
		return -1
	end

	if not exists (select top 1 1 from [dbo].[AgentPlatformRelations] with (nolock) where AdminID = @intAdminID and Platformid = @platformid)
	begin
		set @chvMsg = '操作错误 : 您无权限审核此平台用户提现订单,请联系管理员'
		return -1
	end
	

	if(@ordertype = 1)
	begin
		select	@orderamount = [PoolAmount]
		from [dbo].[Unlimit_WithDrawLog] with (nolock)
		where
			ID = @lngOrderID
	end
	else if (@ordertype = 2)
	begin
		select	@orderamount = [WithDrawAmount]
		from [dbo].[Unlimit_WithDrawLog] with (nolock)
		where
			ID = @lngOrderID
	end


	if(@intStatus = 1)
	begin
		update [dbo].[Unlimit_WithDrawLog]
		set [Status] = @intStatus,
			[Updatetime] = getdate(),
			[UpdateUser] = @intAdminID
		where
			ID = @lngOrderID

		if @@rowcount <> 1
		begin
			set @chvMsg = '操作错误 : 更新订单状态错误'
			return -1
		end	
		

		--转账到用户游戏钱包
		declare @intReturnValue int
		declare @chvMemo varchar(200)
		declare @lngSelfAmount bigint
		declare @chvErrMsg varchar(200)
		declare @intSourceID int
		declare @realwithdrawamount bigint
		declare @withdrawtax bigint	
		SET @intReturnValue = 0
				
		if @ordertype = 1
		begin
			--無限級代理預提
			set @intSourceID = 14004				
			--提领税收
			set @withdrawtax = isnull(cast(cast(@orderamount as float) * 0.1 as bigint),0)
			--实际提领金额
			set @realwithdrawamount = @orderamount - @withdrawtax

			set @chvMemo = '无限级代理预提:'+CAST(@withdrawuserid as varchar)+',数量:'+ CAST(((@orderamount) / 100) as varchar)+',扣除手续费:'+ 
				cast(cast(@withdrawtax as float) / 100 as varchar) + ',实际转入金额:' + cast(cast(@realwithdrawamount as float) / 100 as varchar)
		end
		else
		begin
			--無限級代理提現
			set @intSourceID = 14005	
			set @realwithdrawamount = @orderamount
			set @chvMemo = '无限级代理提現:' + CAST(@withdrawuserid as varchar)+',数量:'+ CAST((@realwithdrawamount/100) as varchar)
		end
		

		EXEC @intReturnValue = [CenterDB].[dbo].[PrPsWeb_UpdateUserBank]
			@withdrawuserid,
			@intSourceID,		--转账类型
			@realwithdrawamount,
			@IPAddress,	
			@chvMemo,
			@lngSelfAmount OUTPUT,
			@chvErrMsg OUTPUT


		DECLARE @NowTime	DATETIME = GETDATE()
		DECLARE @NotifyType	TINYINT = 1
		declare @chvTitle varchar(100) = '無限級提现通知'
		declare @chvContent varchar(500) = @chvMemo
		declare @intSort int = 1
		declare @intMsgID bigint

		INSERT INTO Centerdb.Message.MessageList
			(	Title,		MsgContent,	Type,	FromUserID,	Sort,		CreateTime	)
		SELECT	@chvTitle,	@chvContent,1,		NULL,		@intSort,	GETDATE()
	
		SET @intMsgID = @@IDENTITY

		INSERT INTO Centerdb.Message.ReceiveUser
			(	MsgID,		UserID,		Status,	ReadTime )
		SELECT	@intMsgID,	@withdrawuserid, 0,		NULL
	

		/*  --邮件通知  */
		----------------------------------------------------------------------
		EXEC NOTIFYLINKEDSERVER.[Notify].[dbo].[PrNs_SendSysMail]
			@intUserID = @withdrawuserid,
			@tnyTypeID = @NotifyType,
			@bintParam = 0,
			@tnyStatus = 0,
			@dtCreateTime = @NowTime,
			@dtLastPostTime = @NowTime
		----------------------------------------------------------------------		
	end
	else if (@intStatus = 2)
	begin
		update [dbo].[Unlimit_WithDrawLog]
		set [Status] = @intStatus,
			[Updatetime] = getdate(),
			[UpdateUser] = @intAdminID
		where
			ID = @lngOrderID

		if @@rowcount <> 1
		begin
			set @chvMsg = '操作错误 : 更新订单状态错误'
			return -1
		end

		/*  拒绝返还金额  */
	
		update [dbo].[Unlimit_Pool]
		set [WithDrawAmount] = [WithDrawAmount] - @orderamount
		where
			userid = @withdrawuserid

		if @@rowcount <> 1
		begin
			set @chvMsg = '操作错误 : 返还用户奖励错误'
			return -1
		end
	end


	set @chvMsg = '操作成功'
	return 1
END











