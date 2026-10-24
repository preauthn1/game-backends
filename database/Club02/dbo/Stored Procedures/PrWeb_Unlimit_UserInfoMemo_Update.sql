
-- =============================================
-- Author:  jason
-- Create date: 20190604
-- Description: 后台  无极限-用户信息 更新用户备注
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Unlimit_UserInfoMemo_Update]
(
	@intAdminID int,
	@intUserID int,
	@chvMemo	NVARCHAR(100),
	@chvMsg varchar(100) OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON


	declare @admindirectuser int
	declare @userlevelstr varchar(max)
	declare @userplatformid int

	select	@userplatformid = [PlatformID] from [dbo].[Unlimit_UserInfo] with (nolock) where userid = @intUserID


	if(@userplatformid is null)
	begin
		set @chvMsg = '操作错误 : 用户不存在'
		return -1
	end

	if (@intAdminID is not null) and not exists (select top 1 1 from [dbo].[AgentAdminUser] with (nolock) where id = @intAdminID)
	begin
		set @chvMsg = '操作错误 : 管理者账号不存在'
		return -1
	end

	if (@intAdminID is null)
	begin
		set @chvMsg = '操作错误 : 用户不存在'
		return -1
	end

	if (@intAdminID > 1) and not exists (select top 1 1 from [dbo].[AgentPlatformRelations] with (nolock) where [AdminID] = @intAdminID and [PlatformID] = @userplatformid)
	begin
		set @chvMsg = '操作错误 : 管理者权限无法修改此用户'
		return -1
	end

	--if not exists (select top 1 1 from [dbo].[Unlimit_UserInfo] with (nolock) where userid = @intUserID and StageLevel = 1)
	--begin
	--	set @chvMsg = '操作错误 : 对象用户非一级代理'
	--	return -1
	--end


	/*  更新资料 */
	update [dbo].[Unlimit_UserInfo]
	set [Memo] = @chvMemo
	where
		userid = @intUserID

	if @@rowcount <> 1
	begin
		set @chvMsg = '操作错误 : 更新用户备注失败'
		return -1
	end


	set @chvMsg = '操作成功'
	return 1
END






