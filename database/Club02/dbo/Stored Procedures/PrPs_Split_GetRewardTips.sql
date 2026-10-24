
-- =============================================
-- Author:  jason
-- Create date: 20190505
-- Description: 裂变 - 说明
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Split_GetRewardTips]
(
	@intUserID int
)
AS
BEGIN

	SET NOCOUNT ON


	declare @platformid int

	select	@platformid = platformid
	from [dbo].[Split_UserInfo] with (nolock)
	where
		userid = @intUserID

	
	--select	UserCounts = case when [MaxRecruits] = 999999999 then 20 else [MaxRecruits] end,
	--		RewardB = [Stage1],
	--		RewardC = [Stage2],
	--		RewardD = [Stage3]
	--from [dbo].[Split_SystemLevelRate] with (nolock)
	--where
	--	platformid = @platformid


	select	Level,
			[RecruitB] = [MaxRecruit1],
			[RecruitC] = [MaxRecruit2],
			[RecruitD] = [MaxRecruit3],
			RewardB = [Stage1],
			RewardC = [Stage2],
			RewardD = [Stage3]
	from [dbo].[Split_SystemLevelRate] with (nolock)
	where
		platformid = @platformid


	select	top 1 [TaxAmount]
	from [dbo].[Split_SystemLevelRate] with (nolock)
	where
		platformid = @platformid


	return 1
END


