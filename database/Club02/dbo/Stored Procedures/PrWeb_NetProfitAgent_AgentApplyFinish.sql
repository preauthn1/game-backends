-- =============================================
-- Author:  jason
-- Create date: 2019/04/19
-- Description: USER 审核代理申请(净利润)
-- 2019/04/25	jason		由原本上级可审核下级申请改为只有总代理可审核
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_NetProfitAgent_AgentApplyFinish]
(
	@intAdminID int = null,				--后台管理者ID
	@intUserID		int = null,			--用户 ID
	@intTargetUserID	int,			--申请用户ID
	@chvMsg			varchar(250)	OUTPUT
)
AS
BEGIN

	set nocount on


	declare @orderid int
	declare @nowtime datetime = getdate()
	declare @applystatus	tinyint
	declare @topuserid int
	declare @rates tinyint = 0
	declare @targetuserstatus tinyint
	declare @targetuserplatform int
	declare @targetlv int
	declare @targerpermission int


	if(@intUserID is not null)
	begin
		set @chvMsg = '操作错误 : 目前不提供代理审核申请单'
		return -1
	end

	if exists (select top 1 1 from [dbo].[AgentAdminUser] with (nolock) where id = @intAdminID and [AdminType] not in (0,2) and status = 1)
	begin
		set @chvMsg = '操作错误 : 您无权限审核代理申请'
		return -1
	end


	/*  取原申请单内容/申请者权限  */
	select	@orderid = ID,
			@topuserid = b.[TopUserID],
			@applystatus = a.status,
			@targetuserstatus = b.status,
			@targetuserplatform = [PlatformID],
			@targetlv = stagelevel,
			@targerpermission = permission
	from [dbo].[NetProfitAgent_Apply] as a with (nolock)
		inner join [dbo].[NetProfitAgent_UserInfo] as b with (nolock) on a.userid = b.userid
	where
		a.userid = @intTargetUserID


	/*  取出用户关连  */
	declare @topuserrelation varchar(max)

	select	@topuserrelation = [Relations]
	from (
				select	topuserid
				from [dbo].[NetProfitAgent_UserInfo] with (nolock)
				where
					userid = @intTargetUserID
			) as a
			inner join [dbo].[NetProfitAgent_UserInfo] as b with (nolock) on a.topuserid = b.UserID

	declare @temp_upuser table (userid int)
	insert into @temp_upuser
	select	cast(value as int)
	from [dbo].[fn_Split](@topuserrelation,',')

	
	if(@intAdminID is null) or (@orderid is null)
	begin
		set @chvMsg = '操作错误 : 您没有权限进行此操作'
		return -1
	end

	if(@targetuserstatus = 2)
	begin
		set @chvMsg = '操作错误 : 该用户已是代理身份'
		return -1
	end

	if (@targerpermission = 0)
	begin
		set @chvMsg = '操作错误 : 该用户账号已停权'
		return -1
	end

	update [dbo].[NetProfitAgent_Apply]
	set status = 1,
		[UpdateTime] = @nowtime,
		[Updater] = case when @intAdminID is not null then @intAdminID else [Updater] end,
		[AgentUpdater] = case when @intUserID is not null then @intUserID else [AgentUpdater] end
	where
		ID = @orderid

	if (@@rowcount <> 1)
	begin
		set @chvMsg = '操作错误 : 审核用户申请失败'
		return -1
	end

	update [dbo].[NetProfitAgent_UserInfo]
	set [AgentCreateTime] = @nowtime,
		[Status] = 2,
		[UpdateTime] = @nowtime
	where
		userid = @intTargetUserID

	if (@@rowcount <> 1)
	begin
		set @chvMsg = '操作错误 : 更新用户数据失败'
		return -1
	end

	if(@topuserid is not null)
	begin
		update [dbo].[NetProfitAgent_UserInfo]
		set [SumSubAgent] = [SumSubAgent] + 1,
			[SumSubUser] = [SumSubUser] - 1
		where
			userid in (select userid from @temp_upuser)

		if (@@rowcount = 0)
		begin
			set @chvMsg = '操作错误 : 更新用户数据失败'
			return -1
		end
	end

	if not exists (select top 1 1 from [dbo].[NetProfitAgent_Pool] with (nolock) where userid = @intTargetUserID)
	begin
		insert into [dbo].[NetProfitAgent_Pool]
			(UserID,[PoolAmount],[WithDrawAmount],[Rate],[CreateTime])
		values
			(@intTargetUserID,0,0,@rates,getdate())

		if (@@rowcount <> 1)
		begin
			set @chvMsg = '操作错误 : 更新用户纪录失败'
			return -1
		end
	end

	set @chvMsg = '操作完成 : 更新用户申请完成'
	return 1
END
