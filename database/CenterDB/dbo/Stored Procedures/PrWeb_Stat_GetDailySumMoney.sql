-- =============================================
-- Author:  Jason
-- Create date: 2018/07/05
-- Description:	總金幣統計 #577 不使用ORM stat.SumMoneyLog 改用 stat.SumMoneyDaily(#526)彙總回傳前端
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetDailySumMoney]
(
	@intAdminID		int,
	@intPlatformID	INT = NULL,
	@intAreaID	INT = NULL,
	@dtStatTime	DATETIME = NULL,
	@dtEndTime	DATETIME = NULL
)
AS
BEGIN
	
	SET NOCOUNT ON
	
	--平台篩選
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT)
	
	--撈出該用戶可讀取的全部平台
	INSERT INTO @Temp_PlatformID Select PlatformId From CenterDB.Area.PlatformAdminUser where AdminID=@intAdminID


	select	StatTime = stattime,
			[CashAmount] = sum([totalcashamount]),
			[BankAmount] = sum([totalbankamount]),
			[SumIn] = sum([sumin]) / count(stattime),
			[SumOut] = sum([sumout]) / count(stattime),
			[UserCashAmount] = sum([usercashamount]),
			[UserBankAmount] = sum([userbankamount])
	from [Stat].[SumMoneyDaily] as a with (nolock)
	where
		isnull(@intPlatformID,a.platformid) = a.platformid
		and isnull(@intAreaID,a.areaid) = a.areaid
		and isnull(@dtStatTime,dateadd(day,0,datediff(day,0,getdate()))) <= a.stattime
		and isnull(@dtEndTime,dateadd(day,1,datediff(day,0,getdate()))) > a.stattime
		and a.platformid in (select platformid from @Temp_PlatformID)
	group by stattime
	order by stattime desc

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetDailySumMoney] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetDailySumMoney] TO [xbuser]
    AS [dbo];

