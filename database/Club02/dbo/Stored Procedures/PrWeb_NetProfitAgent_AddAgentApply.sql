-- =============================================
-- Author:  jason
-- Create date: 2019/04/24
-- Description: 全民代理后台 代理为下线提交代理申请 (净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_NetProfitAgent_AddAgentApply]
(
	@intUserID		int,			--用户 ID
	@intTargetID	int,			-- 下一级代理ID
	@chvMsg			varchar(100)		OUTPUT
)
AS
BEGIN

	set nocount on


	if not exists (select top 1 1 from [dbo].[NetProfitAgent_UserInfo] with (nolock) where userid = @intUserID)
	begin
		set @chvMsg = '操作错误 : 申请用户不存在'
		return -1
	end

	if not exists (select top 1 1 from [dbo].[NetProfitAgent_UserInfo] with (nolock) where userid = @intTargetID)
	begin
		set @chvMsg = '操作错误 : 对象用户不存在'
		return -1
	end

	/*  取上下级资料  */
	declare @topuserplatformid int
	declare @topuserlevel int
	declare @topuserlevelstr varchar(max)
	declare @topuserstatus int
	declare @topuserpermission int
	
	declare @targetuserplatformid int
	declare @targetuserlevel int
	declare @targetlevelstr varchar(max)
	declare @targetuserstatus int
	declare @targetuserpermission int

	select	@topuserplatformid = platformid,
			@topuserlevel = stagelevel,
			@topuserlevelstr = relations,
			@topuserstatus = status,
			@topuserpermission = permission
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		userid = @intUserID

	select	@targetuserplatformid = platformid,
			@targetuserlevel = stagelevel,
			@targetlevelstr = relations,
			@targetuserstatus = status,
			@targetuserpermission = permission
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		userid = @intTargetID


	if(@topuserplatformid is null or @targetuserplatformid is null) or (@topuserplatformid <> @targetuserplatformid)
	begin
		set @chvMsg = '操作错误 : 您的游戏渠道找不到此用户'
		return -1
	end

	if(@topuserlevel < (isnull(@targetuserlevel,101)-1))
	begin
		set @chvMsg = '操作错误 : 您只能对下一级玩家作申请提交'
		return -1
	end

	if(@targetlevelstr not like ''+ @topuserlevelstr + '%')
	begin
		set @chvMsg = '操作错误 : 对象与您无上下线关系'
		return -1
	end

	if(@topuserstatus <> 2)
	begin
		set @chvMsg = '操作错误 : 您无权限提交下级代理申请'
		return -1
	end

	if(@targetuserstatus = 2)
	begin
		set @chvMsg = '操作错误 : 对象已是代理身份'
		return -1
	end

	if (@topuserpermission = 0)
	begin
		set @chvMsg = '操作错误 : 您的账户已被封锁,请连系客服'
		return -1
	end

	if (@targetuserpermission = 0)
	begin
		set @chvMsg = '操作错误 : 用户已被封锁,请连系客服'
		return -1
	end

	if exists (select top 1 1 from [dbo].[NetProfitAgent_Apply] with (nolock) where userid = @intTargetID)
	begin
		set @chvMsg = '操作错误 : 用户已申请过,请前往审核'
		return -1
	end
	
	/* 代玩家提出申请  */
	EXEC [dbo].[PrPs_NetProfitAgent_AgentApply]
		@intUserID = @intTargetID,
		@chvMsg = @chvMsg	OUTPUT


	set @chvMsg = '操作完成 : 完成'
	return 1
END







