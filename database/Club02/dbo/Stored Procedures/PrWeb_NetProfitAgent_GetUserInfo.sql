-- =============================================
-- Author:  jason
-- Create date: 2019/04/18
-- Description: 全民代理后台 用戶資料(净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_NetProfitAgent_GetUserInfo]
(
	@intAdminID		int = null,			--管理者
	@intUserID		int = null,			--用户 ID
	@chvMsg		varchar(100) OUTPUT
)
AS
BEGIN

	set nocount on

	
	--平台暂存
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT)

	declare @totalamount bigint = 0

	if(@intAdminID is null and @intUserID is null)
	begin
		set @chvMsg = '操作错误 : 用户不存在'
		return -1
	end
	
	if(@intAdminID is not null)
	begin

		--捞出该用户可读取的全部平台
		INSERT INTO @Temp_PlatformID
		Select	B.PlatformID
		From [dbo].[AgentPlatformRelations] as a
			inner join CenterDB.area.area as b with (nolock) on a.PlatformID = B.PlatformID
			inner join CenterDB.area.[platform] as c with (nolock) on b.PlatformID = c.PlatformID
		where
			a.AdminID = @intAdminID
	end
	else if(@intUserID is not null)
	begin
		INSERT INTO @Temp_PlatformID
		select platformid
		from [dbo].[NetProfitAgent_UserInfo] with (nolock)
		where
			userid = @intUserID
	end


	/*  淨利潤比例/等級/總人數  */
	declare @totaluser int
	declare @totalagent int
	declare @stagelevel int
	declare @rate int
	declare @status int

	if(@intAdminID is not null)
	begin
		select	@totaluser = [SumSubUser],
				@totalagent = [SumSubAgent],
				@stagelevel = 0,
				@rate = 0,
				@status = 0
		from (	select	[SumSubUser] = sum(case when status = 1 then 1 else 0 end),
						[SumSubAgent] = sum(case when status = 2 then 1 else 0 end)
				from [dbo].[NetProfitAgent_UserInfo] with (nolock)
				where
					status in (1,2)
					and Relations is not null
			) as a
	end
	else
	begin
		select	@totaluser = [SumSubUser],
				@totalagent = [SumSubAgent],
				@stagelevel = stagelevel,
				@rate = rate,
				@status = status
		from [dbo].[NetProfitAgent_UserInfo] with (nolock)
		where
			userid = @intUserID
	end

	/*  淨利潤/已提領/可提領  */
	declare @poolamount bigint
	declare @withdrawamount bigint
	declare @performedamount bigint

	select	@poolamount = [PoolAmount],
			@withdrawamount = [WithDrawAmount]
	from [dbo].[NetProfitAgent_Pool] with (nolock)
	where
		userid = @intUserID

	select	@performedamount = sum([WithDrawAmount])
	from [dbo].[NetProfitAgent_WithDrawLog] with (nolock)
	where
		userid = @intUserID
		and [Status] = 0


	/*  充值 兌換 庫存顯示  */
	declare @chargeamount bigint
	declare @convertamount bigint
	declare @storageamount bigint
	declare @zerorateusers int

	if(@intUserID is not null)
	begin
		declare @UserIDstr varchar(max)
		select	 @UserIDstr = Relations
		from [dbo].[NetProfitAgent_UserInfo] with (nolock)
		where
			userid = @intUserID

		declare @temp_users table (userid int)
		insert into @temp_users
		select userid
		from [dbo].[NetProfitAgent_UserInfo] with (nolock)
		where
			Relations like '' + @UserIDstr + '%'


		select	@chargeamount = sum(Amount) * 100
		from @temp_users as a
			inner join centerdb.pay.OrderList as b with (nolock) on a.userid = b.userid
		where status = 2 and LastUpdateTime >= '2019-04-27'


		select	@convertamount = abs(sum(isnull(c.UpdateAmount,0)))
		from @temp_users as a
			inner join logdb.money.moneylog as c with (nolock) on a.userid = c.userid
		where
			c.SourceTypeID in (14000,14002) and c.CreateTime >= '2019-04-27'

		select	
				--@chargeamount = sum(isnull(b.SumCharge,0)) + sum(isnull(b.SumAgentCharge,0)),
				--@convertamount = sum(isnull(b.SumConvert,0)),
				@storageamount = sum(isnull(c.amount,0)) + sum(isnull(d.amount,0))
		from @temp_users as a
			left join centerdb.[Users].[UserInfo] as b with (nolock) on a.userid = b.userid
			left join centerdb.money.cash as c with (nolock) on a.userid = c.userid
			left join centerdb.money.bank as d with (nolock) on a.userid = d.userid

		
		select	@zerorateusers = COUNT(1)
		from @temp_users as a
			inner join dbo.NetProfitAgent_UserInfo as b with (nolock) on a.userid = b.userid
		where
			Rate = 0
			and Status = 2

	end

	if(@intAdminID is not null)
	begin

		declare @temp_platformusers table (userid int)
		insert into @temp_platformusers
		select distinct userid
		from @Temp_PlatformID as a
			inner join [dbo].[NetProfitAgent_UserInfo] as b with (nolock) on a.PlatformID = b.PlatformID

		
		select	@chargeamount = sum(Amount) * 100
		from @temp_platformusers as a
			inner join centerdb.pay.OrderList as b with (nolock) on a.userid = b.userid
		where status = 2 and LastUpdateTime >= '2019-04-27'


		select	@convertamount = abs(sum(isnull(c.UpdateAmount,0)))
		from @temp_platformusers as a
			inner join logdb.money.moneylog as c with (nolock) on a.userid = c.userid
		where
			c.SourceTypeID in (14000,14002) and c.CreateTime >= '2019-04-27'

		select	
				--@chargeamount = sum(isnull(b.SumCharge,0)) + sum(isnull(b.SumAgentCharge,0)),
				--@convertamount = sum(isnull(b.SumConvert,0)),
				@storageamount = sum(isnull(c.amount,0)) + sum(isnull(d.amount,0))
		from @temp_platformusers as a
			left join centerdb.[Users].[UserInfo] as b with (nolock) on a.userid = b.userid
			left join centerdb.money.cash as c with (nolock) on a.userid = c.userid
			left join centerdb.money.bank as d with (nolock) on a.userid = d.userid

		select	@zerorateusers = COUNT(1)
		from @temp_platformusers as a
			inner join dbo.NetProfitAgent_UserInfo as b with (nolock) on a.userid = b.userid
		where
			Rate = 0
			and Status = 2
	end



	/*  回傳資料  */
	select	[TotalUser] = isnull(@totaluser,0) + isnull(@totalagent,0),
			[StageLevel] = isnull(@stagelevel,0),
			[Rate] = isnull(@rate,0),
			[PoolAmount] = isnull(@poolamount,0),
			[WithdrawAmount] = isnull(@withdrawamount,0),
			[PerformedAmount] = isnull(@performedAmount ,0),
			[status] = @status,
			[ChargeAmount] = isnull(@chargeamount,0),
			[ConvertAmount] = isnull(@convertamount,0),
			[StorageAmount] = isnull(@storageamount,0),
			[ZeroRateUsers] = isnull(@zerorateusers,0)

	set @chvMsg = '操作完成 : 查询完成'
	return 1
END







