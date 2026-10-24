-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 更新一级下线数据
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_UpdateSubAgentInfo]	
	@intAdminID		int = null,		--管理者ID
	@intUserID		int,		--用户 ID
	@intSubUserID	int,		--下层代理用户ID
	@tnyStatus		tinyint = null,	-- 是否停用用户 1:停用  停用时则将所有比例调为0
	@tnyRateStage1	tinyint = null,
	@tnyRateStage2	tinyint = null,
	@tnyRateStage3	tinyint = null,
	@chvMsg			varchar(500)	OUTPUT
AS
BEGIN

	set nocount on


	declare @UserLevel	int
	declare @maxStage1	tinyint
	declare @maxStage2	tinyint
	declare @maxStage3	tinyint

	/* 先取得一级下线代理等级及对应比例  */

	select	@UserLevel = [UserLevel]
	from [dbo].[UserAgentInfo] with (nolock)
	where userid = @intSubUserID

	select	@maxStage1 = [Stage1],
			@maxStage2 = [Stage2],
			@maxStage3 = [Stage3]
	from [dbo].[UserAgentSystemLevelRate] with (nolock)
	where
		[Level] = @UserLevel

	if((@tnyRateStage1 is not null and @tnyRateStage1 > @maxStage1) or (@tnyRateStage2 is not null and @tnyRateStage2 > @maxStage2) or
				 (@tnyRateStage3 is not null and @tnyRateStage3 > @maxStage3))
	begin
		set @chvMsg	= '操作失败 : 调整用户收益比例错误'
		return -1
	end

	if not exists (select top 1 1 from [dbo].[UserAgentInfo] with (nolock) where [UserID] = @intSubUserID and [TopUserID] = @intUserID) and (@intAdminID <> 999)
	begin
		set @chvMsg	= '操作失败 : 只能调整自己往下一级用户'
		return -1
	end

	declare @temp_userid table (userid int)
	declare @temp_accountststus table (userid int,status tinyint,bf_status tinyint)
	declare @temp_rateupdate table (userid int,stage1 tinyint,stage2 tinyint,stage3 tinyint,bf_stage1 tinyint,bf_stage2 tinyint,bf_stage3 tinyint)


	insert into @temp_userid values (@intSubUserID)

	if(@tnyStatus is not null)
	begin
		update [dbo].[UserAgentInfo]
		set status = @tnyStatus
		output inserted.userid,inserted.status,deleted.status into @temp_accountststus
		where
			userid = @intSubUserID

		if (@@rowcount <> 1)
		begin
			set @chvMsg	= '操作失败 : 调整用户状态错误'
			return -1
		end
	end

	if(@tnyRateStage1 is not null or @tnyRateStage2 is not null or @tnyRateStage3 is not null)
	begin

		if exists(select top 1 1 from [dbo].[Schedule_UpdateUserStageRate] where userid = @intSubUserID)
		begin
			update [dbo].[Schedule_UpdateUserStageRate]
			set [Stage1] = case when @tnyRateStage1 is not null then @tnyRateStage1 else [Stage1] end,
				[Stage2] = case when @tnyRateStage2 is not null then @tnyRateStage2 else [Stage2] end,
				[Stage3] = case when @tnyRateStage3 is not null then @tnyRateStage3 else [Stage3] end
			output inserted.userid,inserted.stage1,inserted.stage2,inserted.stage3,deleted.stage1,deleted.stage2,deleted.stage3 into @temp_rateupdate
			where
				[UserID] = @intSubUserID

			if (@@rowcount <> 1)
			begin
				set @chvMsg	= '操作失败 : 调整用户收益错误'
				return -1
			end
		end
		else
		begin
			insert into [dbo].[Schedule_UpdateUserStageRate]
				([UserID],[Stage1],[Stage2],[Stage3],[UpdateStatus])
			values
				(@intSubUserID,@tnyRateStage1,@tnyRateStage2,@tnyRateStage3,0)

			if (@@rowcount <> 1)
			begin
				set @chvMsg	= '操作失败 : 调整用户收益错误'
				return -1
			end
		end
	end


	declare @oldStage1 tinyint
	declare @oldStage2 tinyint
	declare @oldStage3 tinyint
	declare @oldStatus tinyint

	select	@oldStage1 = bf_stage1,
			@oldStage2 = bf_stage2,
			@oldStage3 = bf_stage3
	from @temp_rateupdate

	select	@oldStatus = bf_status
	from @temp_accountststus


	exec [dbo].[PrWeb_UserAgent_ChangeSubAgentLog]
		@intUserID		= @intUserID,
		@intSubUserID	= @intSubUserID,
		@tnyStage1		= @tnyRateStage1,
		@tnyStage2		= @tnyRateStage2,
		@tnyStage3		= @tnyRateStage3,
		@tnyBFStage1	= @oldStage1,
		@tnyBFStage2	= @oldStage2,
		@tnyBFStage3	= @oldStage3,
		@tnyStatus		= @tnyStatus,
		@tnyBFStatus	= @oldStatus

	set @chvMsg	= '操作完成 : 调整下线数据完成'
	return 1
END






