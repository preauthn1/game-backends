-- =============================================
-- Author:  jason
-- Create date: 2019/04/16
-- Description: 全民代理后台 管理者/总代使用 用户停权冻结 (净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_ChangeUserStatus]
(
	@intAdminID int = null,				--后台管理者ID
	@intMode	int = null,				--1:全民代理 2:流水 3:裂变 4:净利润 5:
	@intTargetID	int = null,			--用户 ID
	@intStatus	int = null,				-- 0:停权(冻结) 1:复权(解除冻结)
	@chvMsg varchar(100)	OUTPUT
)
AS
BEGIN

	set nocount on


	if(@intMode is null)
	begin
		set @chvMsg = '操作错误 : 设定的代理模式不存在'
		return -1
	end

	if(@intAdminID is null and @intTargetID is null)
	begin
		set @chvMsg = '操作错误 : 用户不存在'
		return -1
	end

	if(@intStatus is null)
	begin
		set @chvMsg = '操作错误 : 权限设定错误'
		return -1
	end


	if not exists (select top 1 1 from [dbo].[AgentAdminUser] with (nolock) where id = @intAdminID and status = 1 and AdminType in (0,2))
	begin
		set @chvMsg = '操作错误 : 管理者权限错误'
		return -1
	end

	if((@intMode = 1) and not exists (select top 1 1 from [dbo].[UserAgentInfo] with (nolock) where userid = @intTargetID))
		--or ((@intMode = 2) and not exists (select top 1 1 from  with (nolock) where userid = @intTargetID))
		--or ((@intMode = 3) and not exists (select top 1 1 from  with (nolock) where userid = @intTargetID))
		or ((@intMode = 4) and not exists (select top 1 1 from [dbo].[NetProfitAgent_UserInfo] with (nolock) where userid = @intTargetID))
		--or ((@intMode = 5) and not exists (select top 1 1 from  with (nolock) where userid = @intTargetID))
		--or ((@intMode = 6) and not exists (select top 1 1 from  with (nolock) where userid = @intTargetID))
	begin
		set @chvMsg = '操作错误 : 用户不存在'
		return -1
	end


	if(@intMode = 1) and (@intStatus not in(1,2))
	begin
		set @chvMsg = '操作错误 : 调整用户状态错误'
		return -1
	end


	declare @temp_status table (userid int,status int,bf_status int)
	declare @laststatus int
	

	if(@intMode = 1)
	begin
		select @laststatus = permission from [dbo].[UserAgentInfo] with (nolock) where userid = @intTargetID

		if(@laststatus <> @intStatus)
		begin
			update [dbo].[UserAgentInfo]
			set permission = @intStatus
			output inserted.userid,inserted.permission,deleted.permission into @temp_status
			where
				userid = @intTargetID
		end
	end
	--else if (@intMode = 2)
	--begin
	--	xxx
	--end
	--else if (@intMode = 3)
	--begin
	--	xxx
	--end
	else if (@intMode = 4)
	begin
		select @laststatus = permission from [dbo].[NetProfitAgent_UserInfo] with (nolock) where userid = @intTargetID

		if(@laststatus <> @intStatus)
		begin
			update [dbo].[NetProfitAgent_UserInfo]
			set permission = @intStatus
			output inserted.userid,inserted.permission,deleted.permission into @temp_status
			where
				userid = @intTargetID
		end
	end
	--else if (@intMode = 5)
	--begin
	--	xxx
	--end
	--else if (@intMode = 6)
	--begin
	--	xxx
	--end

	if exists (select top 1 1 from @temp_status)
	begin
		insert into [dbo].[Log_SystemAdminChangeUserStatusLog]
			([AdminID],[UserID],[Mode],[Status],[BF_Status],[CreateTime])
		select	@intAdminID,@intTargetID,@intMode,status,bf_status,getdate()
		from @temp_status
	end


	set @chvMsg = '操作完成 : 更新完成'
	return 1
END





