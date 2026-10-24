-- =============================================
-- Author:  Jason
-- Create date: 2019/05/05
-- Description:	網頁版簡易留存儲值數據
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetRegChargeInfo]
(
	@intAreaID int,
	@chvAreaName VARCHAR(50) OUTPUT,
	@intRegToday INT OUTPUT,
	@intRegLastday INT OUTPUT,
	@intNewChargeUserToday INT OUTPUT,
	@intNewChargeUserLastday INT OUTPUT,
	@lngNewChargeAmountToday BIGINT OUTPUT,
	@lngNewChargeAmountLastday BIGINT OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON


	declare @today datetime = dateadd(day,0,datediff(day,0,getdate()))
	declare @lastday datetime = dateadd(day,-1,datediff(day,0,getdate()))
	declare @PlatformID int


	select	@chvAreaName = AreaName,
			@PlatformID = PlatformID
	from CenterDB.area.area withh (nolock)
	where
		AreaID = @intAreaID


	select	@intRegToday = sum(case when StatTime = @today then [TotalRegCount] else 0 end),
			@intRegLastday = sum(case when StatTime = @lastday then [TotalRegCount] else 0 end)
	from [Stat].[RegUserLog] with (nolock)
	where
		StatTime >= @lastday
		and StatTime <= @today
		and PlatformID = @PlatformID
		and AreaID = @intAreaID


	select	@lngNewChargeAmountToday = sum(case when StatTime = @today then [NewCharge] else 0 end),
			@lngNewChargeAmountLastday = sum(case when StatTime = @lastday then [NewCharge] else 0 end),
			@intNewChargeUserToday = sum(case when StatTime = @today then [NewOrderUser] else 0 end),
			@intNewChargeUserLastday = sum(case when StatTime = @lastday then [NewOrderUser] else 0 end)
	from [Stat].[AreaDataDay] with (nolock)
	where
		StatTime >= @lastday
		and StatTime <= @today
		and PlatformID = @PlatformID
		and AreaID = @intAreaID

	set @chvAreaName = isnull(@chvAreaName,'')
	set @intRegToday = isnull(@intRegToday,0)
	set @intRegLastday = isnull(@intRegLastday,0)
	set @intNewChargeUserToday = isnull(@intNewChargeUserToday,0)
	set @intNewChargeUserLastday = isnull(@intNewChargeUserLastday,0)
	set @lngNewChargeAmountToday = isnull(@lngNewChargeAmountToday,0)
	set @lngNewChargeAmountLastday = isnull(@lngNewChargeAmountLastday,0)



END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetRegChargeInfo] TO [GameUser]
    AS [dbo];

