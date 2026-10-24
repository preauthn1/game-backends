-- =============================================
-- Author:  jason
-- Create date: 2019/04/16
-- Description: USER 申请加入代理 (净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_NetProfitAgent_AgentApply]
(
	@intUserID		int,		--用户 ID
	@chvMsg		varchar(100) OUTPUT
)
AS
BEGIN

	set nocount on

	declare @topuserid int
	declare @agentstatus int
	declare @status	tinyint = 0
	declare @applystatus tinyint

	select	@topuserid = TopUserID,
			@agentstatus = Status
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		userid = @intUserID

	select @applystatus = status from [dbo].[NetProfitAgent_Apply] with (nolock) where userid = @intUserID

	if not exists (select top 1 1 from [dbo].[NetProfitAgent_UserInfo] with (nolock) where userid = @intUserID)
	begin
		set @chvMsg = '操作错误 : 用户不存在'
		return -1
	end

	if (@applystatus = 0)
	begin
		set @chvMsg = '操作错误 : 您已申请加入代理,请等候上线审核'
		return -1
	end

	if (@applystatus = 1)
	begin
		set @chvMsg = '操作错误 : 您已加入代理'
		return -1
	end

	if (@agentstatus = 2)
	begin
		set @chvMsg = '操作错误 : 您已有下线用户,无法再申请'
		return -1
	end

	insert into [dbo].[NetProfitAgent_Apply]
		([UserID],[TopUserID],[CreateTime],[Status])
	values
		(@intUserID,@topuserid,getdate(),@status)

	if(@@rowcount <> 1)
	begin
		set @chvMsg = '操作错误 : 申请加入代理失败'
		return -1
	end

	set @chvMsg = '操作完成 : 已申请加入代理'
	return 1
END







