
-- =============================================
-- Author:  jason
-- Create date: 20190506
-- Description: 裂变 - 推广信息
--	2019/06/12	Jason		新增回傳上級用戶ID
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Split_GetMyInfo]
(
	@intUserID int,
	@lngRewardAmount bigint OUTPUT,
	@lngSplitAmount bigint OUTPUT,
	@intSuccessCount int OUTPUT,
	@intSuccessToday int OUTPUT,
	@intSuccessLastday int OUTPUT,
	@chvQRCode varchar(250) OUTPUT,
	@chvInviteCode varchar(250) OUTPUT,
	@UpperUserid int	OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON
	
	
	--select	@lngRewardAmount = 1000,
	--		@lngSplitAmount = 1500,
	--		@intSuccessCount = 101,
	--		@intSuccessToday = 15,
	--		@intSuccessLastday = 86

	declare @today datetime = dateadd(day,0,datediff(day,0,getdate()))
	declare @tomorrow datetime = dateadd(day,1,datediff(day,0,getdate()))
	declare @lastday datetime = dateadd(day,-1,datediff(day,0,getdate()))

	declare @temp_userqecode table (userid int,qrcode varchar(250),[InviteCode] varchar(8),UpperUserID int)
	
	
	select	@Upperuserid = [UpperUserID]
	from [dbo].[Split_UserInfo] with (nolock)
	where
		userid = @intUserID

	select	@intSuccessCount = count(1)
	from [dbo].[Split_UserBonusLog] with (nolock)
	where
		userid = @intUserID

	select	@intSuccessToday = count(1)
	from [dbo].[Split_UserBonusLog] with (nolock)
	where
		userid = @intUserID
		and [CreateTime] >= @today
		and [CreateTime] < @tomorrow

	select	@intSuccessLastday = count(1)
	from [dbo].[Split_UserBonusLog] with (nolock)
	where
		userid = @intUserID
		and [CreateTime] >= @lastday
		and [CreateTime] < @today

	select	@lngRewardAmount = isnull([PoolAmount],0) - isnull([WithDrawAmount],0),
			@lngSplitAmount = [SuccessSplitAmount]
	from [dbo].[Split_Pool] with (nolock)
	where
		userid = @intUserID


	insert into @temp_userqecode
	exec [dbo].[PrPs_UserAgent_GetUserQRCode]
		@intUserID = @intUserID

	select	@chvQRCode = qrcode
	from @temp_userqecode

	select	@chvInviteCode = qrcode
	from [dbo].[UserAgentQRCode] with (nolock)
	where
		userid = @intUserID
	
	set @lngRewardAmount = isnull(@lngRewardAmount,0)
	set @lngSplitAmount = isnull(@lngSplitAmount,0)
	set @intSuccessCount = isnull(@intSuccessCount,0)
	set @intSuccessToday = isnull(@intSuccessToday,0)
	set @intSuccessLastday = isnull(@intSuccessLastday,0)
	set @chvQRCode = isnull(@chvQRCode,'')
	set @chvInviteCode = isnull(@chvInviteCode,'')
	set @UpperUserid = isnull(@UpperUserid,0)


	return 1
END


