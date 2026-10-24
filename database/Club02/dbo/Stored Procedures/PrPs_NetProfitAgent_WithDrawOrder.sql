-- =============================================
-- Author:  jason
-- Create date: 2019/04/15
-- Description: USER 提现(净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_NetProfitAgent_WithDrawOrder]
(
	@intUserID		int,	--用户 ID
	@intAmount		bigint,	--提现金额
	@chvMsg			varchar(100)	OUTPUT
)
AS
BEGIN

	set nocount on

	declare @poolamount bigint				--水池剩多少金额
	declare @nowtime datetime = getdate()
	declare @topuserid int
	declare @withdrawrate	int
	declare @poolwithdrawamount bigint		--水池提领后剩余金额
	declare @poollastupdatetime datetime
	declare @status tinyint = 0
	declare @Memo varchar(100) = '净利润抽成'
	declare @lastwithdrawdate datetime
	declare @withdrawday int
	declare @withdrawdaylimit datetime
	declare @PlatformID int

	if exists (select top 1 1 from [dbo].[NetProfitAgent_UserInfo] with (nolock) where Permission = 0)
	begin
		set @chvMsg = '操作错误 : 您的账号已被停权,请连系客服'
		return -1
	end

	select	@withdrawrate = [Rate],	
			@PlatformID = [PlatformID]
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		[UserID] = @intUserID

	/*  取出提领最低天数/上次提出申请时间  */
	select	@withdrawdaylimit = dateadd(day,-[WithDrawDayLimit],@nowtime),
			@withdrawday = [WithDrawDayLimit]
	from [dbo].[NetProfitAgent_SystemRate] with (nolock)
	where @PlatformID = PlatformID

	select	top 1 @lastwithdrawdate = [CreateTime]
	from [dbo].[NetProfitAgent_WithDrawLog] with (nolock)
	where
		UserID = @intUserID
	order by id desc

	set @lastwithdrawdate = isnull(@lastwithdrawdate,getdate()-8)

	if(@withdrawdaylimit < @lastwithdrawdate)
	begin
		set @chvMsg = '操作错误 : ' + cast(@withdrawday as varchar) + '日内只能提领一次'
		return -1
	end


	/*  取出用户关连  */
	declare @topuserrelation varchar(max)
	select	@topuserrelation = [Relations]
	from (
			select	topuserid
			from [dbo].[NetProfitAgent_UserInfo] with (nolock)
			where
				userid = @intUserID
		) as a
		inner join [dbo].[NetProfitAgent_UserInfo] as b with (nolock) on a.topuserid = b.UserID

	declare @temp_upuser table (userid int)
	insert into @temp_upuser
	select	cast(value as int)
	from [dbo].[fn_Split](@topuserrelation,',')

	
	/*  取出用户可提领金额  */
	select	@poolamount = [PoolAmount] - [WithDrawAmount],
			@poolwithdrawamount = ([PoolAmount] - [WithDrawAmount] - @intAmount),
			@poollastupdatetime = [UpdateTime]
	from [dbo].[NetProfitAgent_Pool] with (nolock)
	where
		userid = @intUserID


	set @poolamount = isnull(@poolamount,0)

	if((@poolamount - @intAmount) < 0)
	begin
		set @chvMsg = '操作错误 : 输入金额超过可提现金额'
		return -1
	end

	if (@intAmount < 0)
	begin
		set @chvMsg = '操作错误 : 输入金额错误'
		return -1
	end

	if(@poollastupdatetime is null)
	begin
		set @chvMsg = '操作错误 : 尚未到达可提领期限'
		return -1
	end

	begin tran

		insert into [dbo].[NetProfitAgent_WithDrawLog]
			([UserID],[WithDrawAmount],[WithDrawRate],[PoolAmount],[PoolWithDrawAmount],[PoolLastUpdateTime],[Status],[CreateTime],[Memo],[Updatetime])
		values
			(@intUserID,@intAmount,@withdrawrate,@poolamount,@poolwithdrawamount,@poollastupdatetime,@status,@nowtime,@Memo,@nowtime)

		if (@@rowcount <> 1)
		begin
			set @chvMsg = '操作错误 : 提领失败'
			rollback tran
			return -1
		end

		update [dbo].[NetProfitAgent_Pool]
		set [WithDrawAmount] = [WithDrawAmount] + @intAmount,
			[UpdateTime] = @nowtime
		where
			userid = @intUserID

		if (@@rowcount <> 1)
		begin
			set @chvMsg = '操作错误 : 更新个人资料失败'
			rollback tran
			return -1
		end

		update [dbo].[NetProfitAgent_UserInfo]
		set [SumWithDrawAmount] = [SumWithDrawAmount] + @intAmount
		where
			userid = @intUserID
			
		if (@@rowcount <> 1)
		begin
			set @chvMsg = '操作错误 : 更新提领历程失败'
			rollback tran
			return -1
		end

		if exists (select top 1 1 from @temp_upuser)
		begin
			update [dbo].[NetProfitAgent_UserInfo]
			set [SumSubWithDrawAmount] = [SumSubWithDrawAmount] + @intAmount
			where
				userid in (select userid from @temp_upuser)

			if (@@rowcount <> 1)
			begin
				set @chvMsg = '操作错误 : 更新提领历程失败'
				rollback tran
				return -1
			end
		end 

	set @chvMsg = '操作完成 : 提领申请已提出'
	commit tran

	return 1
END







