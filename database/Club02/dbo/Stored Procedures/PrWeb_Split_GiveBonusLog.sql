-- =============================================
-- Author:  jason
-- Create date: 2019/05/06
-- Description: USER 任务达成奖励(裂变)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Split_GiveBonusLog]
(
	@intUserID		int,			--用户 ID
	@chvMsg			varchar(250)	OUTPUT
)
AS
BEGIN

	set nocount on


	declare @upperuserid1 int
	declare @upperuserid2 int
	declare @upperuserid3 int
	declare @upperusersuccess1 int
	declare @upperusersuccess2 int
	declare @upperusersuccess3 int
	declare @upperuserbonus1 bigint
	declare @upperuserbonus2 bigint
	declare @upperuserbonus3 bigint


	/*  取上级ID  */
	select	@upperuserid1 = upperuserid
	from [dbo].[Split_UserInfo] with (nolock)
	where
		userid = @intUserID

	select	@upperuserid2 = upperuserid
	from [dbo].[Split_UserInfo] with (nolock)
	where
		userid = @upperuserid1

	select	@upperuserid3 = upperuserid
	from [dbo].[Split_UserInfo] with (nolock)
	where
		userid = @upperuserid2


	/*  取上级成功人数  */
	select	@upperusersuccess1 = isnull([SuccessCountB],0) + isnull([SuccessCountC],0) + isnull([SuccessCountD],0)
	from [dbo].[Split_RecruitSuccess] with (nolock)
	where
		userid = @upperuserid1

	select	@upperusersuccess2 = isnull([SuccessCountB],0) + isnull([SuccessCountC],0) + isnull([SuccessCountD],0)
	from [dbo].[Split_RecruitSuccess] with (nolock)
	where
		userid = @upperuserid2

	select	@upperusersuccess3 = isnull([SuccessCountB],0) + isnull([SuccessCountC],0) + isnull([SuccessCountD],0)
	from [dbo].[Split_RecruitSuccess] with (nolock)
	where
		userid = @upperuserid3

	
	/*  取返利金  */
	select	@upperuserbonus1 = max([Stage1])
	from [dbo].[Split_SystemLevelRate] with (nolock)
	where
		[MinRecruit1] <= @upperusersuccess1 and [MaxRecruit1] > @upperusersuccess1


	select	@upperuserbonus2 = max(Stage2)
	from [dbo].[Split_SystemLevelRate] with (nolock)
	where
		[MinRecruit2] <= @upperusersuccess2 and [MaxRecruit2] > @upperusersuccess2

	select	@upperuserbonus3 = max(Stage3)
	from [dbo].[Split_SystemLevelRate] with (nolock)
	where
		[MinRecruit3] <= @upperusersuccess3 and [MaxRecruit3] > @upperusersuccess3


	/*  写入用户奖励  */
	--insert into [dbo].[Split_UserBonusLog]
	--	([UserID],[FromUserID],[BonusAmount],[CreateTime])
	--select	@upperuserid1,@intUserID,@upperuserbonus1,getdate()
	--union
	--select	@upperuserid2,@intUserID,@upperuserbonus2,getdate()
	--union
	--select	@upperuserid3,@intUserID,@upperuserbonus3,getdate()


	/*  更新用户水池  */
	if(@upperuserid1 is not null)
	begin
		insert into [dbo].[Split_UserBonusLog]
			([UserID],[FromUserID],[BonusAmount],[CreateTime])
		select	@upperuserid1,@intUserID,@upperuserbonus1,getdate()

		update [dbo].[Split_Pool]
		set [PoolAmount] = [PoolAmount] + @upperuserbonus1,
			[SuccessSplitAmount] = [SuccessSplitAmount] + @upperuserbonus1
		where
			userid = @upperuserid1
	end

	if(@upperuserid2 is not null)
	begin
		insert into [dbo].[Split_UserBonusLog]
			([UserID],[FromUserID],[BonusAmount],[CreateTime])
		select	@upperuserid2,@intUserID,@upperuserbonus2,getdate()

		update [dbo].[Split_Pool]
		set [PoolAmount] = [PoolAmount] + @upperuserbonus2,
			[SuccessSplitAmount] = [SuccessSplitAmount] + @upperuserbonus2
		where
			userid = @upperuserid2
	end

	if(@upperuserid3 is not null)
	begin
		insert into [dbo].[Split_UserBonusLog]
			([UserID],[FromUserID],[BonusAmount],[CreateTime])
		select	@upperuserid3,@intUserID,@upperuserbonus3,getdate()

		update [dbo].[Split_Pool]
		set [PoolAmount] = [PoolAmount] + @upperuserbonus3,
			[SuccessSplitAmount] = [SuccessSplitAmount] + @upperuserbonus3
		where
			userid = @upperuserid3
	end

	if(@upperuserid1 is not null)
	begin
		update [dbo].[Split_RecruitSuccess]
		set [SuccessCountB] = [SuccessCountB] + 1
		where
			userid = @upperuserid1
	end

	if(@upperuserid2 is not null)
	begin
		update [dbo].[Split_RecruitSuccess]
		set [SuccessCountC] = [SuccessCountC] + 1
		where
			userid = @upperuserid2
	end

	if(@upperuserid3 is not null)
	begin
		update [dbo].[Split_RecruitSuccess]
		set [SuccessCountD] = [SuccessCountD] + 1
		where
			userid = @upperuserid3
	end


END









