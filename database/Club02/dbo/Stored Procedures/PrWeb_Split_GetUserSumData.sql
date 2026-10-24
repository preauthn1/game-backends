
-- =============================================
-- Author:  jason
-- Create date: 20190501
-- Description: 后台  列变-标头
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Split_GetUserSumData]
(
	@intAdminID int = null,
	@chvMsg	varchar(100) OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON


	declare @intSuccessCount int	--成功人数
	declare @intSuccessToday int	--今日成功人数
	declare @intSuccessLastday int	--昨日成功人数
	declare @lngRewardAmount bigint	--可提金额
	declare @lngSplitAmount bigint	--裂变用户金额

	declare @today datetime = dateadd(day,0,datediff(day,0,getdate()))
	declare @tomorrow datetime = dateadd(day,1,datediff(day,0,getdate()))
	declare @lastday datetime = dateadd(day,-1,datediff(day,0,getdate()))
	
	
	select	@intSuccessCount = sum(isnull([SuccessCountB],0) + isnull([SuccessCountC],0) + isnull([SuccessCountD],0)) 
	from [dbo].[Split_RecruitSuccess] with (nolock)


	select	@intSuccessToday = count(1)
	from [dbo].[Split_UserBonusLog] with (nolock)
	where
		[CreateTime] >= @today
		and [CreateTime] < @tomorrow

	select	@intSuccessLastday = count(1)
	from [dbo].[Split_UserBonusLog] with (nolock)
	where
		[CreateTime] >= @lastday
		and [CreateTime] < @today

	select	@lngRewardAmount = sum(isnull([PoolAmount],0) - isnull([WithDrawAmount],0)),
			@lngSplitAmount = sum([SuccessSplitAmount])
	from [dbo].[Split_Pool] with (nolock)


	set @lngRewardAmount = isnull(@lngRewardAmount,0)
	set @lngSplitAmount = isnull(@lngSplitAmount,0)
	set @intSuccessCount = isnull(@intSuccessCount,0)
	set @intSuccessToday = isnull(@intSuccessToday,0)
	set @intSuccessLastday = isnull(@intSuccessLastday,0)

	select	@lngRewardAmount as lngRewardAmount,
			@lngSplitAmount as lngSplitAmount,
			@intSuccessCount as intSuccessCount,
			@intSuccessToday as intSuccessToday,
			@intSuccessLastday as intSuccessLastday
	

	set @chvMsg = '操作成功'
	return 1
END






