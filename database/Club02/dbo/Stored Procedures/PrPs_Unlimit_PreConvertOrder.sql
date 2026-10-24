
-- =============================================
-- Author:  jason
-- Create date: 20190501
-- Description: 无极限-代理预提
--2019/07/11	CCC		取消自動發放金幣
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Unlimit_PreConvertOrder]
(
	@intUserID int,
	@lngAmount bigint = 0,
	@chvMsg	varchar(100) OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON


	declare @days int = datepart(weekday,getdate())
	declare @minsecs int = 32400 -- 9 点
	declare @maxsecs int = 82800 -- 23 点
	declare @now datetime = getdate()
	declare @nowsecs int = datediff(second,convert(varchar(10),@now,23),@now)
	declare @lastpreconverttime datetime
	--declare @logsdate datetime = dateadd(day,1,dateadd(week,datediff(week,0,getdate()),0))
	declare @logsdate datetime = dateadd(day,1,dateadd(week,datediff(week,0,getdate()),0))
	declare @logedate datetime = dateadd(day,7,@logsdate)


	declare @bonusamount bigint
	declare @preconvertamount bigint
	declare @withdrawlimitamount bigint
	declare @realwithdrawamount bigint
	declare @withdrawtax bigint


	/*  换算成分  */
	set @lngAmount = @lngAmount * 100

	/*  取上次预提时间  */
	select	@lastpreconverttime = [CreateTime]
	from (
			select	max([ID]) as ID
			from [dbo].[Unlimit_WithDrawLog] with (nolock)
			where
				userid = @intUserID
				and [WithdrawType] = 1
		) as a
		inner join [dbo].[Unlimit_WithDrawLog] as b with (nolock) on a.id = b.id

	set @lastpreconverttime = isnull(@lastpreconverttime,getdate()-2)


	/*  取已小计结算总金额  */
	select	@bonusamount = sum(isnull([TeamBonusAmount],0)) + sum(isnull([DirectBonusAmount],0))
	from [dbo].[Unlimit_BonusReport] with (nolock)
	where
		userid = @intUserID
		and [LogDate] >= @logsdate
		and [LogDate] < @logedate

	set @bonusamount = isnull(@bonusamount,0)

	/*  取当周已预提金额  */
	select	@preconvertamount = sum([WithDrawAmount])
	from [dbo].[Unlimit_WithDrawLog] with (nolock)
	where
		userid = @intUserID
		and [WithdrawType] = 1	-- 1:预提 2:提领
		and [Status] < 2
		and [CreateTime] >= @logsdate
		and [CreateTime] <= @logedate

	set @preconvertamount = isnull(@preconvertamount,0)
	--当前可提
	set @withdrawlimitamount = round(cast(cast(@bonusamount - @preconvertamount as float) * 0.9 as bigint),0)
	--实际提领金额
	set @realwithdrawamount = round(cast(cast(@lngAmount as float) * 0.9 as bigint),0)
	--提领税收
	set @withdrawtax = isnull(cast(cast(@lngAmount as float) * 0.1 as bigint),0)
	set @realwithdrawamount = @realwithdrawamount - @withdrawtax
	
	if(@lngAmount < 1000)
	begin
		set @chvMsg = '操作错误 : 预提金额最低限制100元'
		return -1
	end

	if(@bonusamount < 1000)
	begin
		set @chvMsg = '操作错误 : 您无可预提金额'
		return -1
	end

	if(@withdrawlimitamount < (@lngAmount))
	begin
		set @chvMsg = '操作错误 : 超过可预提金额'
		return -1
	end

	if(@lngAmount <= 0)
	begin
		set @chvMsg = '操作错误 : 预提金额错误'
		return -1
	end

	if(@days not in (1,3,4,5,6,7))
	begin
		set @chvMsg = '操作错误 : 目前不是可预提日期'
		return -1
	end

	if(@nowsecs < @minsecs or @nowsecs > @maxsecs)
	begin
		set @chvMsg = '操作错误 : 目前不是可预提时间'
		return -1
	end

	if(convert(varchar(10),@now,23) = convert(varchar(10),@lastpreconverttime,23))
	begin
		set @chvMsg = '操作错误 : 您当日已有预提申请'
		return -1
	end


	insert into [dbo].[Unlimit_WithDrawLog]
		([UserID],[WithDrawAmount],[WithdrawType],[Type],[Status],[CreateTime],[TaxAmount],[PoolAmount])
	values
		(@intUserID,@lngAmount,1,0,0,@now,@withdrawtax,@lngAmount)

	if(@@rowcount <> 1)
	begin
		set @chvMsg = '操作错误 : 预提申请数据异常'
		return -1
	end

	
	update [dbo].[Unlimit_Pool]
	set	[WithDrawAmount] = [WithDrawAmount] + @lngAmount
	where
		userid = @intUserID

	if(@@rowcount <> 1)
	begin
		set @chvMsg = '操作错误 : 预提申请数据异常'
		return -1
	end


	/*  取用户最后登入IP  */
	declare @ipaddress varchar(15)
	select	@ipaddress = LastLogonIPAddress
	from centerdb.users.UserInfo with (nolock)
	where
		userid = @intUserID

	/*
	--转账到用户游戏钱包
	declare @intReturnValue int
	declare @chvMemo varchar(200)
	declare @lngSelfAmount bigint
	declare @chvErrMsg varchar(200)
	declare @ingamemoney bigint = (@lngAmount - @withdrawtax)
	SET @intReturnValue = 0
	SET @chvMemo = '无限级代理预提:'+CAST(@intUserID as varchar)+',数量:'+ CAST(((@lngAmount) / 100) as varchar)+',扣除手续费:'+ 
							cast((@withdrawtax / 100) as varchar) + ',实际转入金额:' + cast(((@lngAmount - @withdrawtax) / 100) as varchar)
	EXEC @intReturnValue = [CenterDB].[dbo].[PrPsWeb_UpdateUserBank]
		@intUserID,
		14003,		--转账类型
		@ingamemoney,
		@IPAddress,	
		@chvMemo,
		@lngSelfAmount OUTPUT,
		@chvErrMsg OUTPUT


	DECLARE @NowTime	DATETIME = GETDATE()
	DECLARE @NotifyType	TINYINT = 1
	declare @chvTitle varchar(100) = '无限级代理提现'
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
	*/





	set @chvMsg = '操作完成 : 预提申请成功'
	return 1
END






