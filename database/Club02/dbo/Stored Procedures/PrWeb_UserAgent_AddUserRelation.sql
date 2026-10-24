-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 加入下线(全民代理)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_AddUserRelation]
(
@intUserID		int,				--用户 ID
@chvInviteCode	VARCHAR(20),		--邀请码
@chvMsg			varchar(250)	OUTPUT
)
AS
BEGIN

	set nocount on

	declare @userlevel int = 1
	declare @defaultvalues int = 0
	declare @status tinyint = 1
	declare @upperUserStatus tinyint
	declare @invitecode varchar(20)
	declare @intUpperUserID int
	declare @permission int
	declare @platformid int

	/*  先取邀请码对应用户状态  */
	select	@upperUserStatus = a.[Status],
			@invitecode = a.[QRCode],
			@intUpperUserID = b.[UserID],
			@permission = permission,
			@platformid = PlatformID
	from [dbo].[UserAgentQRCode] as a
		inner join [dbo].[UserAgentInfo] as b on a.userid = b.userid
	where
		[QRCode] = @chvInviteCode
		and a.Status = 1

	declare @userareaid int
	declare @userplatformid int
	select	@userareaid = areaid
	from Centerdb.users.userinfo with (nolock)
	where
		userid = @intUserID

	select	@userplatformid = platformid
	from centerdb.area.area with (nolock)
	where
		areaid = @userareaid

	/*  确认状态  */
	if(@chvInviteCode is not null) and (@userplatformid <> @platformid)
	begin
		set @chvMsg	= '操作错误 : 不同平台不可綁定'
		return -1
	end

	if (@invitecode is null)
	begin
		set @chvMsg	= '操作错误 : 邀请码不存在'
		return -1
	end

	if (@upperUserStatus <> 1)
	begin
		set @chvMsg	= '操作错误 : 上线用户未启用'
		return -1
	end

	if(@intUserID = @intUpperUserID)
	begin
		set @chvMsg	= '操作错误 : 您无法成为自己的下线'
		return -1
	end

	if exists (select top 1 1 from [dbo].[UserAgentInfo] with (nolock) where userid = @intUserID and [TopUserID] is not null)
	begin
		set @chvMsg	= '操作错误 : 您已有上线无法成为其他人的下线'
		return -1
	end

	if exists (select top 1 1 from [dbo].[UserAgentInfo] with (nolock) where [TopUserID] = @intUserID)
	begin
		set @chvMsg	= '操作错误 : 您已有下线无法成为其他人的下线'
		return -1
	end

	if(@permission = 0)
	begin
		set @chvMsg	= '操作错误 : 上线用户已被停权,您无法成为他的下线'
		return -1
	end

	/*  写入对应  */
	begin tran

	--insert into [dbo].[UserAgentInfo]
	--	([UserID],[UserLevel],[SumTaxTotal],[SumTaxConvert],[SumTaxPerformed],[TopUserID],[status],[LastUpdatetime])
	--values
	--	(@intUserID,@userlevel,@defaultvalues,@defaultvalues,@defaultvalues,@intUpperUserID,@status,getdate())

	--if(@@rowcount <> 1)
	--begin
	--	set @chvMsg	= '操作错误 : 加入下线失败'
	--	rollback tran
	--	return -1
	--end
	update [dbo].[UserAgentInfo]
	set topuserid = @intUpperUserID
	where
		userid = @intUserID

	if(@@rowcount <> 1)
	begin
		set @chvMsg	= '操作错误 : 加入下线失败'
		rollback tran
		return -1
	end

	if not exists (select top 1 1 from [dbo].[UserAgentUserRate] with (nolock) where userid = @intUserID)
	begin
		insert into [dbo].[UserAgentUserRate] ([UserID],[UserLevel],[Stage1],[Stage2],[Stage3])
		select	@intUserID,[Level],[Stage1],[Stage2],[Stage3]
		from [dbo].[UserAgentSystemLevelRate] with (nolock)

		if(@@rowcount = 0)
		begin
			set @chvMsg	= '操作错误 : 下线预设比例失败'
			rollback tran
			return -1
		end
	end

	set @chvMsg	= '操作完成 : 新增上线完成'
	commit tran
	return 1
END







