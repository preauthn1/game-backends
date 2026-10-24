-- =============================================
-- Author:  jason
-- Create date: 2019/04/19
-- Description: 全民代理后台 总代理用户变更密码
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_AdminUserPassword_Update]
(
	@intUserID		int,				--用户 ID
	@chvOldPassword	varchar(32),		--旧密码
	@chvNewPassword varchar(32),		--新密码
	@chvEncrypt		varchar(10),			--加密编码
	@chvMsg			varchar(250)	OUTPUT
)
AS
BEGIN

	set nocount on

	if (@intUserID is null or @intUserID >= 10000)
	begin
		set @chvMsg	= '操作错误 : 用户不存在'
		return -1
	end

	if not exists (select top 1 1 from [dbo].[AgentAdminUser] with (nolock) where id = @intUserID)
	begin
		set @chvMsg	= '操作错误 : 用户不存在'
		return -1
	end

	if not exists (select top 1 1 from [dbo].[AgentAdminUser] with (nolock) where [Password] = @chvOldPassword)
	begin
		set @chvMsg	= '操作错误 : 用户密码错误'
		return -1
	end

	if (@chvOldPassword = [REDACTED]
	begin
		set @chvMsg	= '操作错误 : 新密码及旧密码不可相同'
		return -1
	end

	update [dbo].[AgentAdminUser]
	set [Password] = @chvNewPassword,
		[Encrypt] = @chvEncrypt
	where
		id = @intUserID

	if(@@rowcount <> 1)
	begin
		set @chvMsg	= '操作错误 : 用户变更密码失败'
		return -1
	end

	set @chvMsg	= '操作完成 : 用户修改数据完成'
	return 1
END

