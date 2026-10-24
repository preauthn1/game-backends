
-- =============================================
-- Author:  jason
-- Create date: 20190502
-- Description: 无极限-代理提现
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Unlimit_ConvertOrder]
(
	@intUserID int,
	@intType int = 0,		-- 1:支付宝 2:银联
	@chvIPaddress varchar(15),
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
	declare @logsdate datetime = dateadd(day,1,dateadd(week,datediff(week,0,getdate()),0))
	declare @logedate datetime = dateadd(day,5,@logsdate)


	declare @bonusamount bigint
	declare @preconvertamount bigint
	declare @withdrawlimitamount bigint
	declare @realwithdrawamount bigint
	declare @withdrawtax bigint


	/*  换算成分  */
	set @lngAmount = @lngAmount * 100


	/*  取水池金额扣掉已提领  */
	select	@bonusamount = [PoolAmount] - [WithDrawAmount]
	from [dbo].[Unlimit_Pool] with (nolock)
	where
		userid = @intUserID

	set @bonusamount = isnull(@bonusamount,0)


	/*  给上层回馈  */
	declare @upperuserbonus bigint
	set @upperuserbonus = round(cast(@lngAmount as float) * 5 / 100,0)

	/*  上层用户  */
	declare @topuserid int
	select	@topuserid = topuserid
	from [dbo].[Unlimit_UserInfo] with (nolock)
	where
		userid = @intUserID


	/*  取用户绑定数据  */
	declare @alipayname varchar(64)
	declare @alipayaccount varchar(64)
	declare @unionpaybank varchar(5)
	declare @unionpayaccount varchar(64)
	declare @unionpayname varchar(64)

	--select	@alipayname = [RealName],
	--		@alipayaccount = [AlipayAccount],
	--		@unionpaybank = [UnionpayBank],
	--		@unionpayaccount = [UnionpayAccount],
	--		@unionpayname = [UnionpayRealName]
	--from centerdb.users.userinfo with (nolock)
	--where
	--	userid = @intUserID


	--if(@intType not in (1,2))
	--begin
	--	set @chvMsg = '操作错误 : 提现类型错误'
	--	return -1
	--end

	--if(@intType = 1 and (@alipayname is null or @alipayaccount is null))
	--begin
	--	set @chvMsg = '操作错误 : 您未绑定支付宝'
	--	return -1
	--end

	--if(@intType = 2 and (@unionpaybank is null or @unionpayaccount is null or @unionpayname is null))
	--begin
	--	set @chvMsg = '操作错误 : 您未绑定银联卡'
	--	return -1
	--end

	if(@lngAmount <= 0)
	begin
		set @chvMsg = '操作错误 : 提现金额错误'
		return -1
	end

	if(@bonusamount <= 0)
	begin
		set @chvMsg = '操作错误 : 您无可提现金额'
		return -1
	end

	if((@bonusamount - @lngAmount) < 0)
	begin
		set @chvMsg = '操作错误 : 超过可提现金额'
		return -1
	end

	if(@nowsecs < @minsecs or @nowsecs > @maxsecs)
	begin
		set @chvMsg = '操作错误 : 目前不是可提现时间'
		return -1
	end

	--if(@days not in (1,3,4,5,6,7))
	--begin
	--	set @chvMsg = '操作错误 : 目前不是可提现日期'
	--	return -1
	--end


	declare @temp_orderid table (orderid bigint)	

	insert into [dbo].[Unlimit_WithDrawLog]
		([UserID],[WithDrawAmount],[WithdrawType],[Type],[Status],[CreateTime],[TaxAmount],[PoolAmount],
			[RealName],[AlipayAccount],[UnionpayBank],[UnionpayAccount],[UnionpayRealName],ipaddress)
	output inserted.id into @temp_orderid
	values
		(@intUserID,@lngAmount,2,0,0,@now,@withdrawtax,@bonusamount,@alipayname,@alipayaccount,@unionpaybank,@unionpayaccount,@unionpayname,@chvIPaddress)

	if(@@rowcount <> 1)
	begin
		set @chvMsg = '操作错误 : 提现申请数据异常'
		return -1
	end

	update [dbo].[Unlimit_Pool]
	set [WithDrawAmount] = [WithDrawAmount] + @lngAmount
	where
		userid = @intUserID

	if(@@rowcount <> 1)
	begin
		set @chvMsg = '操作错误 : 更新申请数据异常'
		return -1
	end

	if(@topuserid is not null)
	begin
		insert into [dbo].[Unlimit_WithdrawBonus]
			([RelationID],[UserID],[Amount],[Status])
		select	orderid,@topuserid,@upperuserbonus,0
		from @temp_orderid

		if(@@rowcount <> 1)
		begin
			set @chvMsg = '操作错误 : 上线返利数据异常'
			return -1
		end
	end


	set @chvMsg = '操作完成 : 提现申请成功'
	return 1
END








