
-- =============================================
-- Author:  jason
-- Create date: 20190506
-- Description: 裂变 - 提现纪录
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Split_GetMyConvert]
(
	@intUserID int,
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intWithdrawTime INT = 0	OUTPUT,
	@intPageCount	INT = 0		OUTPUT,
	@lngRewardAmount bigint OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON
	
	
	--set	@lngRewardAmount = 1000
	--set @intPageCount = 5

	--select LogDate = '20190501',ConvertAmount = 10000
	--union all
	--select LogDate = '20190502',ConvertAmount = 10000
	--union all
	--select LogDate = '20190503',ConvertAmount = 10000
	--union all
	--select LogDate = '20190504',ConvertAmount = 10000
	--union all
	--select LogDate = '20190505',ConvertAmount = 10000
	

	select	@lngRewardAmount = [PoolAmount] - [WithDrawAmount]
	from [dbo].[Split_Pool] with (nolock)
	where
		userid = @intUserID

	set @lngRewardAmount = isnull(@lngRewardAmount,0)


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

	declare @temp_table table (LogDate varchar(10),ConvertAmount bigint)

	insert into @temp_table
	select	LogDate = convert(varchar(10),[CreateTime],23),
			ConvertAmount = [WithDrawAmount]
	from [dbo].[Split_WithDrawLog] with (nolock)
	where
		userid = @intUserID

	select @intPageCount = count(1) from @temp_table
	set @intPageCount = isnull(@intPageCount,0)

	select	LogDate,
			ConvertAmount
	from @temp_table
	order by LogDate desc

	return 1
END


