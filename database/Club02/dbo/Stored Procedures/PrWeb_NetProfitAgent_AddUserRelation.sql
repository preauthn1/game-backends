-- =============================================
-- Author:  jason
-- Create date: 2019/04/15
-- Description: USER 加入下线(净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_NetProfitAgent_AddUserRelation]
(
	@intUserID		int,				--用户 ID
	@chvInviteCode	VARCHAR(20) = null,	--邀请码
	@intTopUserID	int	= null,			--上级ID
	@chvMsg			varchar(250)	OUTPUT
)
AS
BEGIN

	set nocount on

	declare @defaultvalues int = 0
	declare @status tinyint = 1
	declare @upperUserStatus tinyint
	declare @upperUserStageLevel tinyint
	declare @upperUserRelations varchar(max)
	declare @invitecode varchar(20)
	declare @intUpperUserID int
	declare @systemrate tinyint
	declare @platformid int
	declare @areaid int
	declare @permission int

	/*  先取邀请码对应用户状态  */
	IF @chvInviteCode IS NOT NULL
	BEGIN
		select	@upperUserStatus = b.[Status],
				@invitecode = a.[QRCode],
				@intUpperUserID = b.[UserID],
				@platformid = b.PlatformID,
				@areaid = b.AreaID,
				@upperUserRelations = b.[Relations],
				@upperUserStageLevel = [StageLevel],
				@permission = permission
		from [dbo].[UserAgentQRCode] as a with (nolock)
			inner join [dbo].[NetProfitAgent_UserInfo] as b with (nolock) on a.userid = b.userid
		where
			[QRCode] = @chvInviteCode
			and a.Status = 1
	END

	IF @intTopUserID IS NOT NULL
	BEGIN
		select	@upperUserStatus = b.[Status],
				@invitecode = a.[QRCode],
				@intUpperUserID = b.[UserID],
				@platformid = b.PlatformID,
				@areaid = b.AreaID,
				@upperUserRelations = b.[Relations],
				@upperUserStageLevel = [StageLevel],
				@permission = permission
		from [dbo].[UserAgentQRCode] as a with (nolock)
			inner join [dbo].[NetProfitAgent_UserInfo] as b with (nolock) on a.userid = b.userid
		where
			a.UserID = @intTopUserID
			and a.Status = 1

		if(@invitecode is null)
		begin
			set @chvMsg	= '操作错误 : 上级ID无效'
			return -1
		end

		SET @chvInviteCode = @invitecode
	END

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

	/*  取净利润系统默认比例  */
	set	@systemrate = 0

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

	if (@upperUserStatus <> 2)
	begin
		set @chvMsg	= '操作错误 : 上线用户未启用'
		return -1
	end

	if(@intUserID = @intUpperUserID)
	begin
		set @chvMsg	= '操作错误 : 您无法成为自己的下线'
		return -1
	end

	if exists (select top 1 1 from [dbo].[NetProfitAgent_UserInfo] with (nolock) where userid = @intUserID and [TopUserID] is not null)
	begin
		set @chvMsg	= '操作错误 : 您已有上线无法成为其他人的下线'
		return -1
	end

	if exists (select top 1 1 from [dbo].[NetProfitAgent_UserInfo] with (nolock) where [TopUserID] = @intUserID)
	begin
		set @chvMsg	= '操作错误 : 您已有下线无法成为其他人的下线'
		return -1
	end

	if(@permission = 0)
	begin
		set @chvMsg	= '操作错误 : 上线用户已被停权,您无法成为他的下线'
		return -1
	end


	/*  取出用户关连  */
	declare @topuserrelation varchar(max)

	if exists (select top 1 1 from [dbo].[NetProfitAgent_UserInfo] with (nolock) where topuserid is null and status = 2 and userid = @intUpperUserID)
	begin
		set @topuserrelation = cast(@intUpperUserID as varchar(20))
	end
	else
	begin
	select	@topuserrelation = [Relations]
	from (
			select	userid
			from [dbo].[NetProfitAgent_UserInfo] with (nolock)
			where
				userid = @intUpperUserID
		) as a
		inner join [dbo].[NetProfitAgent_UserInfo] as b with (nolock) on a.userid = b.UserID
	end

	declare @temp_upuser table (userid int)
	insert into @temp_upuser
	select	cast(value as int)
	from [dbo].[fn_Split](@topuserrelation,',')


	/*  写入对应  */


		update [dbo].[NetProfitAgent_UserInfo]
		set topuserid = @intUpperUserID,
			[StageLevel] = @upperUserStageLevel + 1,
			[Relations] = case when @upperUserRelations is null then cast(@intUpperUserID as varchar(20)) + ',' + cast(@intUserID as varchar(20))
								else @upperUserRelations + ',' + cast(@intUserID as varchar(20)) end,
			[Rate] = 0
		where
			userid = @intUserID

		if(@@rowcount <> 1)
		begin
			set @chvMsg	= '操作错误 : 加入下线失败'
			return -1
		end

		update [dbo].[NetProfitAgent_UserInfo]
		set [SumSubUser] = [SumSubUser] + 1
		where
			userid in (select userid from @temp_upuser)

		if(@@rowcount = 0)
		begin
			set @chvMsg	= '操作错误 : 加入下线失败'
			return -1
		end

		set @chvMsg	= '操作完成 : 新增上线完成'


		if not exists (select top 1 1 from [dbo].[NetProfitAgent_Pool] with (nolock) where userid = @intUserID)
		begin
			insert into [dbo].[NetProfitAgent_Pool]
				([UserID],[PoolAmount],[WithDrawAmount],[Rate],[CreateTime])
			select	@intUserID,@defaultvalues,@defaultvalues,@systemrate,getdate()
		end

		return 1
END







