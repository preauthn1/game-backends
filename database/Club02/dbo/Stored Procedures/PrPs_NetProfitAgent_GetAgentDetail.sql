-- =============================================
-- Author:  jason
-- Create date: 2019/04/15
-- Description: USER 推广信息(净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_NetProfitAgent_GetAgentDetail]
(
	@intUserID		int,	--用户 ID
	@chvMsg			varchar(100)	OUTPUT
)
AS
BEGIN

	set nocount on

	declare @StartDate	datetime = dateadd(day,0,datediff(day,0,getdate()))
	declare @EndDate	datetime = dateadd(day,1,datediff(day,0,getdate()))
	declare @lastDate	datetime = dateadd(day,-1,datediff(day,0,getdate()))
	
	declare @myrate tinyint	--分成比例 *
	declare @sumbonusAmount	bigint --总利润 *
	declare @poolamount	bigint --可提现金额 *
	declare @newagenttoday int --今日新增代理 *
	declare @newagentlast int	--昨日新增代理人 * 
	declare @totalagentuser int	--所有代理人*
	declare @newusertoday	int --今日新增玩家 *
	declare @chargelast		bigint	--昨日充值
	declare @convertlast	bigint	--昨日兑换
	declare @bonusamountlast bigint --昨日净利润
	declare @chargetoday	bigint	--今日充值
	declare @converttoday	bigint	--今日兑换
	declare @bonusamounttoday	bigint --今日净利润
	declare @payusertoday		int		--今日付费用户
	declare @payuserlast		int		--昨日付费用户
	declare @storageamount		bigint	--库存金币
	declare @qrurl				varchar(250)	--我的推广连结
	declare @withdrawlimit		tinyint	--提现限制日期
	declare @withdrawsecs		int	--提现倒数秒数


	if not exists (select top 1 1 from [dbo].[NetProfitAgent_UserInfo] with (nolock) where userid = @intUserID)
	begin
		declare @temp_table table
			([rate] tinyint,[sumbonusAmount] bigint,[poolamount] bigint,[newagenttoday] int,[newagentlast] int,[totalagentuser] int,
			[newusertoday] int,[chargelast] bigint,[convertlast] bigint,[bonusamountlast] bigint,[chargetoday] bigint,
			[converttoday] bigint,[bonusamounttoday] bigint,[payusertoday] int,[payuserlast] int,[storageamount] bigint,
			[qrurl] varchar(250),[withdrawsecs] int
			)

		select * from @temp_table
		set @chvMsg = '操作错误 : 用户不存在'
		return -1
	end


	/*  取分成比例/总利润/可提现金额  */
	select	@myrate = [Rate],
			@sumbonusAmount = [PoolAmount],
			@poolamount = ([PoolAmount] - [WithDrawAmount])
	from [dbo].[NetProfitAgent_Pool] with (nolock)
	where
		[UserID] = @intUserID

	set @myrate = isnull(@myrate,0)


	/*  取今日新增代理/昨日新增代理/所有代理人/今日新增玩家  */
	select	@newagenttoday = sum(case when status = 2 and AgentCreateTime >= @StartDate and AgentCreateTime < @EndDate then 1 else 0 end),
			@newagentlast = sum(case when status = 2 and AgentCreateTime >= @lastDate and AgentCreateTime < @StartDate then 1 else 0 end),
			@totalagentuser = sum(case when status = 2 then 1 else 0 end),
			@newusertoday = sum(case when status = 1 and CreateTime >= @StartDate and CreateTime < @EndDate then 1 else 0 end)
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		TopUserID = @intUserID


	/*  取昨日及今日充值 兑换 净利润数据  */
	-------------------------------------------------------------------------------------------
	declare @temp_totaluser	table (userid int)

	insert into @temp_totaluser
	select	userid
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		TopUserID = @intUserID
	union
	select @intUserID

	--今日
	select	@chargetoday = sum([ChargeAmount]),
			@converttoday = sum([ConvertAmount]),
			@bonusamounttoday = sum([BonusAmount]),
			@payusertoday = count(distinct a.userid)
	from @temp_totaluser as a
		inner join [dbo].[NetProfitAgent_UserTaxLog] as b with (nolock) on a.userid = b.userid
	where
		[LogDate] = @StartDate

	--昨日
	select	@chargelast = sum([ChargeAmount]),
			@convertlast = sum([ConvertAmount]),
			@bonusamountlast = sum([BonusAmount]),
			@payuserlast = count(distinct a.userid)
	from @temp_totaluser as a
		inner join [dbo].[NetProfitAgent_UserTaxLog] as b with (nolock) on a.userid = b.userid
	where
		[LogDate] = @lastDate
	-------------------------------------------------------------------------------------------


	/*  库存金币  */
	select	@storageamount = sum(isnull(c.amount,0) + isnull(d.amount,0))
	from @temp_totaluser as a
		left join CenterDB.[Money].[Cash] as c with (nolock) on a.userid = c.userid
		left join CenterDB.[Money].[Bank] as d with (nolock) on a.userid = d.userid

	/*  取推广连结/提现限制日期  */
	select	@qrurl = b.Domain + '?a=' + c.QRCode,
			@withdrawlimit = a.WithDrawDayLimit
	from [dbo].[NetProfitAgent_UserInfo] as a with (nolock)
		inner join [dbo].[DomainSetting] as b with (nolock) on a.PlatformID = b.PlatformID
		inner join [dbo].[UserAgentQRCode] as c with (nolock) on a.userid = c.userid
	where
		a.userid = @intUserID

	/*  取提现倒数  */
	select	top 1 
			@withdrawsecs = datediff(second,dateadd(day,@withdrawlimit,isnull(isnull([Updatetime],[CreateTime]),getdate())),getdate())
			--isnull(isnull([Updatetime],[CreateTime]),getdate())  --取上一次日期
	from [dbo].[NetProfitAgent_WithDrawLog] with (nolock)
	where
		userid = @intUserID
	order by id desc


	select	[rate] = @myrate,								--我的分成比例
			[sumbonusAmount] = isnull(@sumbonusAmount,0),	--总利润
			[poolamount] = isnull(@poolamount,0),			--可提现金额
			[newagenttoday] = isnull(@newagenttoday,0),		--今日新增代理
			[newagentlast] = isnull(@newagentlast,0),		--昨日新增代理
			[totalagentuser] = isnull(@totalagentuser,0),	--所有代理
			[newusertoday] = isnull(@newusertoday,0),		--今日新增玩家
			[chargelast] = isnull(@chargelast,0),			--昨日充值
			[convertlast] = isnull(@convertlast,0),			--昨日兑换
			[bonusamountlast] = isnull(@bonusamountlast,0),	--昨日净利润
			[chargetoday] = isnull(@chargetoday,0),			--今日充值
			[converttoday] = isnull(@converttoday,0),		--今日兑换
			[bonusamounttoday] = isnull(@bonusamounttoday,0), --今日净利润
			[payusertoday] = isnull(@payusertoday,0),		--今日付费用户
			[payuserlast] = isnull(@payuserlast,0),			--昨日付费用户
			[storageamount] = isnull(@storageamount,0),		--库存金币
			[qrurl] = @qrurl,								--我的推广连结
			[withdrawsecs] = isnull(@withdrawsecs,0)		--提现倒数计时

	set @chvMsg = '操作完成 : 查询用户数据'
	return 1
END





