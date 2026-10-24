-- =============================================
-- Author:  jason
-- Create date: 2019/04/19
-- Description: 全民代理后台 总代理手动新增一级代理 (净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_NetProfitAgent_AddNewAgent]
(
	@intAdminID		int,
	@intUserID		int,			--用户 ID
	@chvMsg			varchar(100)		OUTPUT
)
AS
BEGIN

	set nocount on

	
	declare @adminplatformid int
	declare @userplatformid int
	declare @userareaid int
	declare @userstatus int
	declare @usernickname varchar(32)
	declare @userpwd varchar(32)

	if not exists (select top 1 1 from [dbo].[NetProfitAgent_UserInfo] with (nolock) where userid = @intUserID)
	begin
		select	@userplatformid = platformid,
				@userareaid = a.areaid,
				@userstatus = case when @intUserID is not null then 1 else null end,
				@usernickname = a.NickName,
				@userpwd = [REDACTED]
		from [CenterDB].[Users].[UserInfo] as a with (nolock)
			inner join CenterDB.area.area as b with (nolock) on a.areaid = b.areaid
		where
			userid = @intUserID
	end
	begin
		select	@userplatformid = platformid,
				@userareaid = areaid,
				@userstatus = status,
				@usernickname = NickName,
				@userpwd = [REDACTED]
		from [dbo].[NetProfitAgent_UserInfo] with (nolock)
		where
			userid = @intUserID
	end

	select @adminplatformid = PlatformID
	from [dbo].[AgentPlatformRelations] with (nolock)
	where
		adminid = @intAdminID
		and platformid = @userplatformid


	if(@adminplatformid is null and @userplatformid is null)
	begin
		set @chvMsg = '操作错误 : 用户不存在'
		return -1
	end

	if (@userstatus = 2)
	begin
		set @chvMsg = '操作错误 : 用户已是代理'
		return -1
	end


	if not exists (select top 1 1 from [dbo].[NetProfitAgent_UserInfo] with (nolock) where userid = @intUserID)
	begin
		EXEC [dbo].[PrWeb_UserAgent_AddUserQRCode]
			@intUserID = @intUserID,
			@intplatformid  = @userplatformid,
			@intareaid = @userareaid,
			@chvMsg = @chvMsg OUTPUT

		declare @temp_rate int
		declare @daylimit int

		select	@temp_rate = rate,@daylimit = [WithDrawDayLimit]
		from [dbo].[NetProfitAgent_SystemRate] with (nolock)

		if not exists (select top 1 1 from [dbo].[NetProfitAgent_UserInfo] with (nolock) where userid = @intUserID)
		begin
			insert into [dbo].[NetProfitAgent_UserInfo]
				([UserID],[UserLevel],[StageLevel],[SumSubUser],[SumSubAgent],[WithdrawDayLimit],[Status],[PlatformID],[AreaID],[CreateTime],[NickName],[Password],[Rate],
					[SumChargeAmount],[SumConvertAmount],[SumSubChargeAmount],[SumSubConvertAmount],[SumWithdrawAmount],[SumSubWithdrawAmount],[CountWithdraw],[CountSubwithdraw],[Permission],Relations)
			values
				(@intUserID,1,1,0,0,@daylimit,0,@userplatformid,@userareaid,getdate(),@usernickname,@userpwd,@temp_rate,
					0,0,0,0,0,0,0,0,1,cast(@intUserID as varchar))
		end

		if not exists (select top 1 1 from [dbo].[NetProfitAgent_Pool] with (nolock) where userid = @intUserID)
		begin
			insert into [dbo].[NetProfitAgent_Pool] ([UserID],[PoolAmount],[WithDrawAmount],[Rate],[CreateTime])
			values (@intUserID,0,0,@temp_rate,getdate())
		end

	end
	
	/* 代玩家提出申请  */
	EXEC [dbo].[PrPs_NetProfitAgent_AgentApply]
		@intUserID = @intUserID,
		@chvMsg = @chvMsg	OUTPUT

	
	EXEC [dbo].[PrWeb_NetProfitAgent_AgentApplyFinish]
		@intAdminID = @intAdminID,
		@intTargetUserID = @intUserID,			--申请用户ID
		@chvMsg = @chvMsg	OUTPUT

	if exists (select top 1 1 from [dbo].[NetProfitAgent_UserInfo] with (nolock) where userid = @intUserID and status <> 2)
	begin
		set @chvMsg = '操作错误 : 总代理添加一级代理失败'
		return -1
	end


	set @chvMsg = '操作完成 : 完成'
	return 1
END





