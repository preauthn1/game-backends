-- =============================================
-- Author:  jason
-- Create date: 2019/04/23
-- Description: 后台 管理者账户列表
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_AgentUserList]
(
	@intAdminID		int,			--管理者账号
	@intUserID		int = null,		--总代账号
	@chvMsg			varchar(250)	OUTPUT
)
AS
BEGIN

	set nocount on

	if not exists (select top 1 1 from [dbo].[AgentAdminUser] with (nolock) where id = @intAdminID and [AdminType] = 0)
	begin
		set @chvMsg	= '操作失败 : 您的权限不足'
		return -1
	end

	select	[ID],
			[Account],
			[Status],
			[AdminType],	--0:系统管理者 1:财务 2:总代
			[CreateTime],
			[Encrypt],
			[Memo],
			[Mode1],
			[Mode2],
			[Mode3],
			[Mode4],
			[Mode5],
			[Mode6]
	from [dbo].[AgentAdminUser] with (nolock)
	where
		(@intUserID is null or [ID] = @intUserID)


	set @chvMsg	= '操作完成 : 查询管理者完成'
	return 1
END

