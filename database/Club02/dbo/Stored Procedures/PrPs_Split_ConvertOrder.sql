
-- =============================================
-- Author:  jason
-- Create date: 20190506
-- Description: 裂变 - 提现纪录 送出提现金额
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Split_ConvertOrder]
(
	@intUserID int,
	@chvIPaddress	varchar(15),
	@lngAmount bigint,
	@chvMsg	varchar(100) OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON
	
	set @lngAmount = @lngAmount * 100

	declare @rewardamount bigint
	declare @poolamount bigint
	declare @withdrawamount bigint
	declare @platformid int
	declare @withdrawlimit int
	declare @lastwithdrawtime datetime
	declare @withdrawdate datetime

	select	@rewardamount = isnull([PoolAmount],0) - isnull([WithDrawAmount],0),
			@poolamount = isnull([PoolAmount],0),
			@withdrawamount = isnull([WithDrawAmount],0)
	from [dbo].[Split_Pool] with (nolock)
	where
		userid = @intUserID

	select	@platformid = platformid 
	from [dbo].[Split_UserInfo] with (nolock)
	where
		userid = @intUserID

	select	top 1 @withdrawlimit = [WithdrawLimit]
	from [dbo].[Split_SystemLevelRate] with (nolock)
	where
		platformid = @platformid
	
	select	@lastwithdrawtime = [CreateTime]
	from [dbo].[Split_WithDrawLog] with (nolock)
	where
		userid = @intUserID
	order by [ID] desc

	set @lastwithdrawtime = isnull(@lastwithdrawtime,getdate()-100)
	set @withdrawdate = dateadd(hour,@withdrawlimit,@lastwithdrawtime)

	if(@platformid is null)
	begin
		set @chvMsg = '操作失败 : 用户不存在'
		return -1
	end

	if(@rewardamount < @lngAmount)
	begin
		set @chvMsg = '操作失败 : 您的奖励金额不足'
		return -1
	end

	if((@rewardamount - @lngAmount) < 0)
	begin
		set @chvMsg = '操作失败 : 您的奖励金额不足'
		return -1
	end

	if(@lngAmount < 0)
	begin
		set @chvMsg = '操作失败 : 提现金额错误'
		return -1
	end

	if(@withdrawdate > getdate())
	begin
		set @chvMsg = '操作失败 : 未达提现冷却时间'
		return -1
	end

	insert into [dbo].[Split_WithDrawLog]
		([UserID],[WithDrawAmount],[PoolAmount],[PoolWithDrawAmount],[CreateTime])
	values
		(@intUserID,@lngAmount,@poolamount,@withdrawamount,getdate())

	if(@@rowcount <> 1)
	begin
		set @chvMsg = '操作失败 : 提现申请失败'
		return -1
	end


	update [dbo].[Split_Pool]
	set [WithDrawAmount] = [WithDrawAmount] + @lngAmount
	where
		userid = @intUserID

	if(@@rowcount <> 1)
	begin
		set @chvMsg = '操作失败 : 更新个人奖励金错误'
		return -1
	end
	


	--转账到用户游戏钱包
	declare @intReturnValue int
	declare @chvMemo varchar(200)
	declare @lngSelfAmount bigint
	declare @chvErrMsg varchar(200)
	SET @intReturnValue = 0
	SET @chvMemo = '裂变提现:'+CAST(@intUserID as varchar)+',数量:'+ CAST((@lngAmount / 100) as varchar)
	EXEC @intReturnValue = [CenterDB].[dbo].[PrPsWeb_UpdateUserBank]
		@intUserID,
		14003,		--转账类型
		@lngAmount,
		@chvIPaddress,	
		@chvMemo,
		@lngSelfAmount OUTPUT,
		@chvErrMsg OUTPUT


	DECLARE @NowTime	DATETIME = GETDATE()
	DECLARE @NotifyType	TINYINT = 1
	declare @chvTitle varchar(100) = '裂变提现'
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

	
	set @chvMsg = '操作成功'
	return 1
END




