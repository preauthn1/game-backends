
-- =============================================
-- Author:  jason
-- Create date: 20190505
-- Description: 裂变 - 马上提现
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Split_GetMyReward]
(
	@intUserID int
)
AS
BEGIN

	SET NOCOUNT ON
	
	declare @MyReward bigint
	declare @intWithdrawTime int

	select @MyReward = isnull([PoolAmount],0) - isnull([WithDrawAmount],0)
	from [dbo].[Split_Pool] with (nolock)
	where
		userid = @intUserID

	set @MyReward = isnull(@MyReward,0)

	
	declare @withdrawtime int
	declare @withdrawlimithour int
	declare @lastwithdrawdate datetime

	select	top 1 @withdrawlimithour = [WithdrawLimit]
	from [dbo].[Split_UserInfo] as a with (nolock)
		inner join [dbo].[Split_SystemLevelRate] as b with (nolock) on a.PlatformID = b.PlatformID
	where
		userid = @intUserID

	select	top 1 @lastwithdrawdate = dateadd(day,0,[CreateTime])
	from [dbo].[Split_WithDrawLog] with (nolock)
	where
		userid = @intUserID
	order by id desc

	set @intWithdrawTime = isnull(datediff(second,'1970-01-01',@lastwithdrawdate),datediff(second,'1970-01-01',getdate()))

	if(@lastwithdrawdate is null)
	begin
		set @intWithdrawTime = 0
	end
	else
	begin
		set @intWithdrawTime = datediff(second,'1970-01-01',dateadd(hour,@withdrawlimithour,@lastwithdrawdate)) - datediff(second,'1970-01-01',getdate())
	end

	if(@intWithdrawTime < 0)
	begin
		set @intWithdrawTime = 0
	end

	
	select	@MyReward as MyReward,
			@intWithdrawTime as WithdrawTime

	return 1
END


