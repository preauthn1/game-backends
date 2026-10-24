-- =============================================
-- Author:  alan
-- Create date: 20180929
-- Description:	充直统计留存率
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_StatChargeKeepLog]
AS
BEGIN
	SET NOCOUNT ON

	  declare @dtNow datetime=getdate()
	  declare @dtCreateTime datetime
	  declare @intYYYYMMDD int
	  declare @intTodayRegCount int
	  declare @intRegCount int

	  if not exists (select 1 from [Stat].[ChargeKeepLog] with (nolock) where YYYYMMDD = CAST ( CONVERT(varchar,@dtNow,112) AS INT) and HDtype=2)
		  Insert into [Stat].[ChargeKeepLog](YYYYMMDD,HDType,[Day1Keep],[Day2Keep],[Day7Keep],[Day30Keep])
		  Values(CAST ( CONVERT(varchar,@dtNow,112) AS INT),2,0,0,0,0)

	  if not exists (select 1 from [Stat].[ChargeKeepLog] with (nolock) where YYYYMMDD = CAST ( CONVERT(varchar,@dtNow,112) AS INT) and HDtype=3)
		  Insert into [Stat].[ChargeKeepLog](YYYYMMDD,HDType,[Day1Keep],[Day2Keep],[Day7Keep],[Day30Keep])
		  Values(CAST ( CONVERT(varchar,@dtNow,112) AS INT),3,0,0,0,0)



	  --更新今日註冊且付費人數
  	  SET @intYYYYMMDD = CAST ( CONVERT(varchar,@dtNow,112) AS INT)
	  select @intTodayRegCount=Count(userid) from CenterDB.users.UserInfo with (nolock) where CAST ( CONVERT(varchar,CreateTime,112) AS INT) = @intYYYYMMDD and (SumCharge+SumAgentCharge) > 0 and HDType=2
	  update [Stat].[ChargeKeepLog] set RegChargeUsers = ISNULL(@intTodayRegCount,0) where YYYYMMDD=CAST(CONVERT(varchar,@dtNow,112) AS INT) and HDType=2

	  select @intTodayRegCount=Count(userid) from CenterDB.users.UserInfo with (nolock) where CAST ( CONVERT(varchar,CreateTime,112) AS INT) = @intYYYYMMDD and (SumCharge+SumAgentCharge) > 0 and HDType=3
	  update [Stat].[ChargeKeepLog] set RegChargeUsers = ISNULL(@intTodayRegCount,0) where YYYYMMDD=CAST(CONVERT(varchar,@dtNow,112) AS INT) and HDType=3
	  -----------------------------------------------------------


	  --今日登入清單
      select userid,hdtype,logintime into #loginuser
	  from logdb.game.GameLog with (nolock) where LoginTime >= CAST(Convert(varchar,@dtNow,110) as datetime)
	  and hdtype <> 1
	  -------------------------------------------------------------

	  --1day前註冊且有充值
	  SET @dtCreateTime = DateAdd(day,-1,GETDATE())
	  SET @intYYYYMMDD = CAST (CONVERT(varchar,@dtCreateTime,112) AS INT)

	  select userid into #chargeuser from CenterDB.users.UserInfo  with (nolock)
	  where CAST ( CONVERT(varchar,CreateTime,112) AS INT) = @intYYYYMMDD
	  and (SumCharge+SumAgentCharge) > 0
	  
	  SELECT @intRegCount=ISNULL(Count(userid),0) from #chargeuser

	  select HDType,COUNT(distinct UserID)*1.0/@intRegCount * 100 as amount
	  into #result from #loginuser with (nolock) where LoginTime >=  CAST(Convert(varchar,@dtNow,110) as datetime)
	  and userid in (
		select userid from #chargeuser
	  )
	  group by hdtype

	  update [Stat].[ChargeKeepLog] set [Day1Keep] = ISNULL((select amount from #result where hdtype=2),0) where YYYYMMDD=@intYYYYMMDD and HDType=2
	  update [Stat].[ChargeKeepLog] set [Day1Keep] = ISNULL((select amount from #result where hdtype=3),0) where YYYYMMDD=@intYYYYMMDD and HDType=3
	  -------------------------------------------------------------

	  --2day前註冊且有充值
	  SET @dtCreateTime = DateAdd(day,-2,GETDATE())
	  SET @intYYYYMMDD = CAST ( CONVERT(varchar,@dtCreateTime,112) AS INT)

	  select userid into #chargeuser2 from CenterDB.users.UserInfo  with (nolock)
	  where CAST ( CONVERT(varchar,CreateTime,112) AS INT) = @intYYYYMMDD
	  and (SumCharge+SumAgentCharge) > 0

	  SELECT @intRegCount=ISNULL(Count(userid),0) from #chargeuser2

	  select HDType,COUNT(distinct UserID)*1.0/@intRegCount * 100 as amount
	  into #result2 from #loginuser with (nolock) where LoginTime >=  CAST(Convert(varchar,@dtNow,110) as datetime)
	  and userid in (
		select userid from #chargeuser2
	  )
	  group by hdtype
	  update [Stat].[ChargeKeepLog] set [Day2Keep] = ISNULL((select amount from #result2 where hdtype=2),0) where YYYYMMDD=@intYYYYMMDD and HDType=2
	  update [Stat].[ChargeKeepLog] set [Day2Keep] = ISNULL((select amount from #result2 where hdtype=3),0) where YYYYMMDD=@intYYYYMMDD and HDType=3
	  -------------------------------------------------------------


	  --7day前註冊且有充值
	  SET @dtCreateTime = DateAdd(day,-7,GETDATE())
	  SET @intYYYYMMDD = CAST ( CONVERT(varchar,@dtCreateTime,112) AS INT)

	  select userid into #chargeuser7 from CenterDB.users.UserInfo  with (nolock)
	  where CAST ( CONVERT(varchar,CreateTime,112) AS INT) = @intYYYYMMDD
	  and (SumCharge+SumAgentCharge) > 0

	  SELECT @intRegCount=ISNULL(Count(userid),0) from #chargeuser7

	  select HDType,COUNT(distinct UserID)*1.0/@intRegCount * 100 as amount
	  into #result7 from #loginuser with (nolock) where LoginTime >=  CAST(Convert(varchar,@dtNow,110) as datetime)
	  and userid in (select userid from #chargeuser7 )
	  group by hdtype

	  update [Stat].[ChargeKeepLog] set [Day7Keep] = ISNULL((select amount from #result7 where hdtype=2),0) where YYYYMMDD=@intYYYYMMDD and HDType=2
	  update [Stat].[ChargeKeepLog] set [Day7Keep] = ISNULL((select amount from #result7 where hdtype=3),0) where YYYYMMDD=@intYYYYMMDD and HDType=3
	  -------------------------------------------------------------

	  --30day前註冊且有充值
	  SET @dtCreateTime = DateAdd(day,-30,GETDATE())
	  SET @intYYYYMMDD = CAST ( CONVERT(varchar,@dtCreateTime,112) AS INT)

	  select userid into #chargeuser30 from CenterDB.users.UserInfo  with (nolock)
	  where CAST ( CONVERT(varchar,CreateTime,112) AS INT) = @intYYYYMMDD
	  and (SumCharge+SumAgentCharge) > 0
	  
	  SELECT @intRegCount=ISNULL(Count(userid),0) from #chargeuser30

	  select HDType,COUNT(distinct UserID)*1.0/@intRegCount * 100 as amount
	  into #result30 from #loginuser with (nolock) where LoginTime >=  CAST(Convert(varchar,@dtNow,110) as datetime)
	  and userid in (
		select userid from #chargeuser30
	  )
	  group by hdtype

	  update [Stat].[ChargeKeepLog] set [Day30Keep] = ISNULL((select amount from #result30 where hdtype=2),0) where YYYYMMDD=@intYYYYMMDD and HDType=2
	  update [Stat].[ChargeKeepLog] set [Day30Keep] = ISNULL((select amount from #result30 where hdtype=3),0) where YYYYMMDD=@intYYYYMMDD and HDType=3
	  -------------------------------------------------------------
  

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_StatChargeKeepLog] TO [xbuser]
    AS [dbo];

