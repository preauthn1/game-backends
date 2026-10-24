-- =============================================
-- Author:  jason
-- Create date: 2019/04/30
-- Description: USER 加入下线(无限级)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Unlimit_AddUserRelation]
(
	@intUserID		int,					--用户 ID
	@chvInviteCode	VARCHAR(20) = null,		--邀请码
	@intTopUserID	int	= null,				--上级ID
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
	declare @logdate datetime = convert(varchar(10),getdate(),23)

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
				@permission = Permission
		from [dbo].[UserAgentQRCode] as a with (nolock)
			inner join [dbo].[Unlimit_UserInfo] as b with (nolock) on a.userid = b.userid
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
				@permission = Permission
		from [dbo].[UserAgentQRCode] as a with (nolock)
			inner join [dbo].[Unlimit_UserInfo] as b with (nolock) on a.userid = b.userid
		where
			b.UserID = @intTopUserID
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

	if(@invitecode is not null and @intUserID = @intUpperUserID)
	begin
		set @chvMsg	= '操作错误 : 您无法成为自己的下线'
		return -1
	end

	if exists (select top 1 1 from [dbo].[Unlimit_UserInfo] with (nolock) where userid = @intUserID and [TopUserID] is not null)
	begin
		set @chvMsg	= '操作错误 : 您已有上线无法成为其他人的下线'
		return -1
	end

	if exists (select top 1 1 from [dbo].[Unlimit_UserInfo] with (nolock) where [TopUserID] = @intUserID)
	begin
		set @chvMsg	= '操作错误 : 您已有下线无法成为其他人的下线'
		return -1
	end

	if(@permission = 0)
	begin
		set @chvMsg	= '操作错误 : 您的上线已被停权,请另外找上线加入'
		return -1
	end

	
	--if(@upperUserStageLevel is null)
	--begin
	--	set @chvMsg	= '操作完成 : 查无上线用户数据,请联系管理员'
	--	return -1
	--end


	if(@chvInviteCode is null)
	begin
		set @chvMsg	= '操作完成 : 无邀请码加入代理'
		return 1
	end


	if not exists (select top 1 1 from [dbo].[UserAgentQRCode] with (nolock) where [QRCode] = @chvInviteCode)
	begin
		set @chvMsg	= '操作完成 : 邀请码无效'
		return -1
	end
	

	/*  取出用户关连  */
	declare @topuserrelation varchar(max)

	if exists (select top 1 1 from [dbo].[Unlimit_UserInfo] with (nolock) where topuserid is null and status = 2 and userid = @intUpperUserID)
	begin
		set @topuserrelation = cast(@intUpperUserID as varchar(20))
	end
	else
	begin
	select	@topuserrelation = [Relations]
	from (
			select	userid
			from [dbo].[Unlimit_UserInfo] with (nolock)
			where
				userid = @intUpperUserID
		) as a
		inner join [dbo].[Unlimit_UserInfo] as b with (nolock) on a.userid = b.UserID
	end

	declare @temp_upuser table (userid int)
	insert into @temp_upuser
	select	cast(value as int)
	from [dbo].[fn_Split](@topuserrelation,',')


	/*  写入对应  */

		update [dbo].[Unlimit_UserInfo]
		set topuserid = @intUpperUserID,
			[StageLevel] = @upperUserStageLevel + 1,
			[Relations] = case when @upperUserRelations is null then cast(@intUpperUserID as varchar(20)) + ',' + cast(@intUserID as varchar(20))
								else @upperUserRelations + ',' + cast(@intUserID as varchar(20)) end,
			[Rate] = 0
		where
			userid = @intUserID

		if(@@rowcount <> 1)
		begin
			set @chvMsg	= '操作错误 : 加入上线失败'
			return -1
		end


		/*  更新上级为代理  */
		if(@upperUserStatus = 1)
		begin
			update [dbo].[Unlimit_UserInfo]
			set [Status] = 2,
				[SumSubUser] = [SumSubUser] + 1,
				AgentCreateTime = getdate()
			where
				userid = @intUpperUserID

			/*  上层为代理时更新全部上层信息  */
			update [dbo].[Unlimit_UserInfo]
			set [SumSubAgent] = [SumSubAgent] + 1
			where
				userid in (select userid from @temp_upuser where userid <> @intUpperUserID)

		end
		else
		begin

			/*  所有上层+1  */
			update [dbo].[Unlimit_UserInfo]
			set [SumSubUser] = [SumSubUser] + 1
			where
				userid in (select * from @temp_upuser)
		end


		set @chvMsg	= '操作完成 : 新增上线完成'


		if not exists (select top 1 1 from [dbo].[Unlimit_Pool] with (nolock) where userid = @intUserID)
		begin
			insert into [dbo].[Unlimit_Pool]
				([UserID],[PoolAmount],[WithDrawAmount],[Rate],[CreateTime])
			select	@intUserID,@defaultvalues,@defaultvalues,@systemrate,getdate()
		end

		if not exists (select top 1 1 from [dbo].[Unlimit_UserLogin] where userid = @intUserID)
		begin
			insert into [dbo].[Unlimit_UserLogin]
				([LogDate],[UserID],[LoginCount])
			values
				(@logdate,@intUserID,1)
		end
		else
		begin
			update [dbo].[Unlimit_UserLogin]
			set [LoginCount] = [LoginCount] + 1
			where
				LogDate = @logdate
				and userid = @intUserID
		end

		if(@@rowcount <> 1)
		begin
			set @chvMsg	= '操作错误 : 登陆记录失败'
			return -1
		end

		if exists (select top 1 1 from @temp_upuser)
		begin
			update [dbo].[Unlimit_Promotion]
			set [SubDirectUser] = case when userid = @intUpperUserID then [SubDirectUser] + 1 else [SubDirectUser] end,
				[SubUser] = case when userid <> @intUpperUserID then [SubUser] + 1 else [SubUser] end,
				[DAU] = [DAU] + 1,
				[NewUser] = [NewUser] + 1
			where
				userid in (select userid from @temp_upuser)

			if(@@rowcount = 0)
			begin
				set @chvMsg	= '操作错误 : 更新用户纪录失败'
				return -1
			end
		end

		return 1
END









