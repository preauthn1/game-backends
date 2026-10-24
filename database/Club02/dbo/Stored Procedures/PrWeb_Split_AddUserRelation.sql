-- =============================================
-- Author:  jason
-- Create date: 2019/05/06
-- Description: USER 加入下线(裂变)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Split_AddUserRelation]
(
	@intUserID		int,				--用户 ID
	@chvInviteCode	VARCHAR(20) = null,		--邀请码
	@chvMsg			varchar(250)	OUTPUT
)
AS
BEGIN

	set nocount on


	declare @upperuserid1 int
	declare @upperuserid2 int
	declare @upperuserid3 int
	declare @upperUserStatus tinyint
	declare @platformid int
	declare @areaid int
	declare @permission int
	declare @upperuserlevel int
	declare @relations varchar(max)
	declare @invitecode varchar(8)
	declare @topuserid int


	if not exists (select top 1 1 from [dbo].[UserAgentQRCode] with (nolock) where [QRCode] = @chvInviteCode) and (@chvInviteCode is not null)
	begin
		set @chvMsg	= '操作错误 : 邀请码不存在'
		return -1
	end


	/*  取邀请码及往上两层用户数据  */
	select	@upperUserStatus = b.[Status],
			@upperuserid1 = b.[UserID],
			@upperuserid2 = b.[UpperUserID],
			@upperuserlevel = b.[StageLevel],
			@topuserid = b.TopUserID,
			@platformid = b.PlatformID,
			@areaid = b.AreaID,
			@permission = Permission,
			@invitecode = a.QRCode
	from [dbo].[UserAgentQRCode] as a with (nolock)
		inner join [dbo].[Split_UserInfo] as b with (nolock) on a.userid = b.userid
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

	if(@chvInviteCode is not null) and (@userplatformid <> @platformid)
	begin
		set @chvMsg	= '操作错误 : 不同平台不可綁定'
		return -1
	end

	select	@upperuserid3 = [UpperUserID]
	from [dbo].[Split_UserInfo] with (nolock)
	where
		userid = @upperuserid2


	set @relations = case when @upperuserid3 is not null then cast(@upperuserid3 as varchar) + ',' else '' end +
					case when @upperuserid2 is not null then cast(@upperuserid2 as varchar) + ',' else '' end +
					case when @upperuserid1 is not null then cast(@upperuserid1 as varchar) + ',' else '' end +
					cast(@intUserID as varchar)

	set @topuserid = isnull(@topuserid,isnull(@upperuserid1,@intUserID))


	if not exists (select top 1 1 from [dbo].[Split_Pool] with (nolock) where userid = @intUserID)
	begin
		insert into [dbo].[Split_Pool]
			([UserID],[PoolAmount],[WithDrawAmount],[SuccessSplitAmount],[CreateTime])
		select	@intUserID,0,0,0,getdate()

		if(@@rowcount <> 1)
		begin
			set @chvMsg	= '操作错误 : 更新个人奖池失败'
			return -1
		end
	end
	

	if not exists (select top 1 1 from [dbo].[Split_RecruitSuccess] with (nolock) where userid = @intUserID)
	begin
		insert into [dbo].[Split_RecruitSuccess]
			([UserID],[RecruitCountB],[RecruitCountC],[RecruitCountD],[SuccessCountB],[SuccessCountC],[SuccessCountD])
		select	@intUserID,0,0,0,0,0,0

		if(@@rowcount <> 1)
		begin
			set @chvMsg	= '操作错误 : 更新个人招募数据失败'
			return -1
		end
	end


	if not exists (select top 1 1 from [dbo].[Split_UserMission] with (nolock) where userid = @intUserID)
	begin
		insert into [dbo].[Split_UserMission]
			([UserID],[MobileBind],[AlipayBind],[UnionPayBind],[ChargeBind],[ConvertBind])
		select	@intUserID,0,0,0,0,0

		if(@@rowcount <> 1)
		begin
			set @chvMsg	= '操作错误 : 更新个人任务数据失败'
			return -1
		end
	end


	/*  确认状态  */
	if(@invitecode is not null and @intUserID = @upperuserid1)
	begin
		set @chvMsg	= '操作错误 : 您无法成为自己的下线'
		return -1
	end

	if exists (select top 1 1 from [dbo].[Split_UserInfo] with (nolock) where userid = @intUserID and [UpperUserID] is not null)
	begin
		set @chvMsg	= '操作错误 : 您已有上线无法成为其他人的下线'
		return -1
	end

	if exists (select top 1 1 from [dbo].[Split_UserInfo] with (nolock) where [UpperUserID] = @intUserID)
	begin
		set @chvMsg	= '操作错误 : 您已有下线无法成为其他人的下线'
		return -1
	end

	if(@permission = 0)
	begin
		set @chvMsg	= '操作错误 : 您的上线已被停权,请另外找上线加入'
		return -1
	end

	if(@chvInviteCode is null)
	begin
		set @chvMsg	= '操作完成 : 无邀请码加入代理'
		return -1
	end
	

	/*  写入对应  */
		--更新上级信息
		if(@upperuserid1 is not null)
		begin
			update [dbo].[Split_RecruitSuccess]
			set [RecruitCountB] = [RecruitCountB] + 1
			where
				userid = @upperuserid1

			if(@@rowcount <> 1)
			begin
				set @chvMsg	= '操作错误 : 更新上线招募数据失败'
				return -1
			end
		end

		if(@upperuserid2 is not null)
		begin
			update [dbo].[Split_RecruitSuccess]
			set [RecruitCountC] = [RecruitCountC] + 1
			where
				userid = @upperuserid2

			if(@@rowcount <> 1)
			begin
				set @chvMsg	= '操作错误 : 更新上线招募数据失败'
				return -1
			end
		end

		if(@upperuserid3 is not null)
		begin
			update [dbo].[Split_RecruitSuccess]
			set [RecruitCountD] = [RecruitCountD] + 1
			where
				userid = @upperuserid3

			if(@@rowcount <> 1)
			begin
				set @chvMsg	= '操作错误 : 更新上线招募数据失败'
				return -1
			end
		end

		--更新个人资料
		update [Split_UserInfo]
		set topuserid = @topuserid,
			[UpperUserID] = @upperuserid1,
			[Relations] = @relations,
			[StageLevel] = case when @upperuserid1 is null then 1 else (@upperuserlevel + 1) end,
			[Status] = 2,
			[CreateTime] = case when @upperuserid1 is not null then getdate() else [CreateTime] end
		where
			userid = @intUserID

		if(@@rowcount <> 1)
		begin
			set @chvMsg	= '操作错误 : 加入下线失败'
			return -1
		end


		declare @intRtnCode int

		exec [dbo].[PrWeb_Split_CheckUserMission]
			@intUserID = @intUserID,	--用户 ID
			@intRtnCode = @intRtnCode OUTPUT,
			@chvMsg = @chvMsg OUTPUT


		if(@intRtnCode = 1)
		begin
			exec [dbo].[PrWeb_Split_GiveBonusLog]
				@intUserID = @intUserID,			--用户 ID
				@chvMsg = @chvMsg	OUTPUT
		end


		set @chvMsg	= '操作完成 : 新增上线完成'
		return 1
END









