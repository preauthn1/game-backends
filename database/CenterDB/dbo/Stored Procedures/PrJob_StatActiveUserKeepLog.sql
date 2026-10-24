-- =============================================
-- Author:  alan
-- Create date: 20180929
-- Description:	有效用户留存率
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_StatActiveUserKeepLog]
AS
BEGIN
	SET NOCOUNT ON

	  declare @dtNow datetime=getdate()
	  declare @dtCreateTime datetime
	  declare @intYYYYMMDD int
	  declare @intChargeUser int
	  declare @intTodayActiveCount int
	  declare @intActiveCount int

	  if not exists (select 1 from [Stat].[ActiveUserKeepLog] with (nolock) where YYYYMMDD = CAST ( CONVERT(varchar,@dtNow,112) AS INT))
		  Insert into [Stat].[ActiveUserKeepLog](YYYYMMDD,ActiveUsers,[Day1Keep],[Day3Keep],[Day5Keep],[Day7Keep],[Day14Keep],[Day30Keep],[Day3PayRate],[Day5PayRate],[Day7PayRate])
		  Values(CAST ( CONVERT(varchar,@dtNow,112) AS INT),0,0,0,0,0,0,0,0,0,0)


  	  SET @intYYYYMMDD = CAST ( CONVERT(varchar,@dtNow,112) AS INT)
	  

	  --更新今日有效新增用户数 (当天新增的用户中，任意进入游戏中体验游戏时长>=30秒以上的新用户数量)
	  Select @intTodayActiveCount = Count(Distinct G.UserID)
	  from [CenterDB].[Game].[UserGameInfo] G with (nolock) left join users.UserInfo U on G.userid=U.UserID
	  Where CAST ( CONVERT(varchar,U.CreateTime,112) AS INT) = @intYYYYMMDD AND G.GameTime >= 30 --sec
	  
	  update [Stat].[ActiveUserKeepLog] set ActiveUsers = ISNULL(@intTodayActiveCount,0) where YYYYMMDD=CAST(CONVERT(varchar,@dtNow,112) AS INT)
	  -----------------------------------------------------------


	  --今日登入清單
      select distinct G.userid into #todayLoginUser
	  from logdb.game.GameLog G with (nolock) 
	  left join CenterDB.Users.UserInfo U  with (nolock) on G.UserID=U.UserID
	  where U.UserType <> 2 AND G.LoginTime >= CAST(Convert(varchar,@dtNow,110) as datetime) 
	  -------------------------------------------------------------

	  
	  
	  --有效用户的1日留存
	  SET @dtCreateTime = DateAdd(day,-1,GETDATE())
	  SET @intYYYYMMDD = CAST (CONVERT(varchar,@dtCreateTime,112) AS INT)

	  --1日前的有效用户
	  Select distinct G.UserID into #activeUser1
	  from [CenterDB].[Game].[UserGameInfo] G with (nolock) left join users.UserInfo U on G.userid=U.UserID
	  Where CAST ( CONVERT(varchar,U.CreateTime,112) AS INT) = @intYYYYMMDD AND G.GameTime >= 30 --sec

	  --1日前的有效用户数
	  SELECT @intActiveCount=ISNULL(Count(userid),0) from #activeUser1

	  select COUNT(distinct UserID)*1.0/@intActiveCount * 100 as amount into #result 
	  from #todayLoginUser with (nolock) 
	  where userid in (select userid from #activeUser1 )
	  
	  update [Stat].[ActiveUserKeepLog] set [Day1Keep] = ISNULL((select amount from #result),0) where YYYYMMDD=@intYYYYMMDD

	  -------------------------------------------------------------

	  
	  --有效用户的3日留存
	  SET @dtCreateTime = DateAdd(day,-3,GETDATE())
	  SET @intYYYYMMDD = CAST (CONVERT(varchar,@dtCreateTime,112) AS INT)

	  --3日前的有效用户
	  Select distinct G.UserID into #activeUser3
	  from [CenterDB].[Game].[UserGameInfo] G with (nolock) left join users.UserInfo U on G.userid=U.UserID
	  Where CAST ( CONVERT(varchar,U.CreateTime,112) AS INT) = @intYYYYMMDD AND G.GameTime >= 30 --sec

	  --3日前的有效用户数
	  SELECT @intActiveCount=ISNULL(Count(userid),0) from #activeUser3

	  select COUNT(distinct UserID)*1.0/@intActiveCount * 100 as amount into #result3 
	  from #todayLoginUser with (nolock) 
	  where userid in (select userid from #activeUser3 )

	  update [Stat].[ActiveUserKeepLog] set [Day3Keep] = ISNULL((select amount from #result3),0) where YYYYMMDD=@intYYYYMMDD

	  --新增有效用户的3日付费率 (新增有效用户，在注册日起3日内的累积付费人数/新增有效用户的人数)
	  select @intChargeUser = count(userid) from users.UserInfo U
	  Where CAST ( CONVERT(varchar,U.CreateTime,112) AS INT) = @intYYYYMMDD and (SumCharge+SumAgentCharge)>0
	  update [Stat].[ActiveUserKeepLog] set [Day3Payrate] = ISNULL( (@intChargeUser*1.0/@intActiveCount*100),0 ) where YYYYMMDD=@intYYYYMMDD
	  -------------------------------------------------------------  

  
	  --有效用户的5日留存
	  SET @dtCreateTime = DateAdd(day,-5,GETDATE())
	  SET @intYYYYMMDD = CAST (CONVERT(varchar,@dtCreateTime,112) AS INT)

	  --5日前的有效用户
	  Select distinct G.UserID into #activeUser5
	  from [CenterDB].[Game].[UserGameInfo] G with (nolock) left join users.UserInfo U on G.userid=U.UserID
	  Where CAST ( CONVERT(varchar,U.CreateTime,112) AS INT) = @intYYYYMMDD AND G.GameTime >= 30 --sec

	  --5日前的有效用户数
	  SELECT @intActiveCount=ISNULL(Count(userid),0) from #activeUser5

	  select COUNT(distinct UserID)*1.0/@intActiveCount * 100 as amount into #result5
	  from #todayLoginUser with (nolock) 
	  where userid in (select userid from #activeUser5 )


	  update [Stat].[ActiveUserKeepLog] set [Day5Keep] = ISNULL((select amount from #result5),0) where YYYYMMDD=@intYYYYMMDD

	  --新增有效用户的5日付费率 (新增有效用户，在注册日起5日内的累积付费人数/新增有效用户的人数)
	  select @intChargeUser = count(userid) from users.UserInfo U
	  Where CAST ( CONVERT(varchar,U.CreateTime,112) AS INT) = @intYYYYMMDD and (SumCharge+SumAgentCharge)>0
	  update [Stat].[ActiveUserKeepLog] set [Day5Payrate] = ISNULL( (@intChargeUser*1.0/@intActiveCount*100),0 ) where YYYYMMDD=@intYYYYMMDD
	  -------------------------------------------------------------  


	  --有效用户的7日留存
	  SET @dtCreateTime = DateAdd(day,-7,GETDATE())
	  SET @intYYYYMMDD = CAST (CONVERT(varchar,@dtCreateTime,112) AS INT)

	  --7日前的有效用户
	  Select distinct G.UserID into #activeUser7
	  from [CenterDB].[Game].[UserGameInfo] G with (nolock) left join users.UserInfo U on G.userid=U.UserID
	  Where CAST ( CONVERT(varchar,U.CreateTime,112) AS INT) = @intYYYYMMDD AND G.GameTime >= 30 --sec

	  --7日前的有效用户数
	  SELECT @intActiveCount=ISNULL(Count(userid),0) from #activeUser7

	  select COUNT(distinct UserID)*1.0/@intActiveCount * 100 as amount into #result7
	  from #todayLoginUser with (nolock) 
	  where userid in (select userid from #activeUser7 )


	  update [Stat].[ActiveUserKeepLog] set [Day7Keep] = ISNULL((select amount from #result7),0) where YYYYMMDD=@intYYYYMMDD

	  --新增有效用户的7日付费率 (新增有效用户，在注册日起7日内的累积付费人数/新增有效用户的人数)
	  select @intChargeUser = count(userid) from users.UserInfo U
	  Where CAST ( CONVERT(varchar,U.CreateTime,112) AS INT) = @intYYYYMMDD and (SumCharge+SumAgentCharge)>0
	  update [Stat].[ActiveUserKeepLog] set [Day7Payrate] = ISNULL( (@intChargeUser*1.0/@intActiveCount*100),0 ) where YYYYMMDD=@intYYYYMMDD
	  -------------------------------------------------------------  

	  --有效用户的14日留存
	  SET @dtCreateTime = DateAdd(day,-14,GETDATE())
	  SET @intYYYYMMDD = CAST (CONVERT(varchar,@dtCreateTime,112) AS INT)

	  --14日前的有效用户
	  Select distinct G.UserID into #activeUser14
	  from [CenterDB].[Game].[UserGameInfo] G with (nolock) left join users.UserInfo U on G.userid=U.UserID
	  Where CAST ( CONVERT(varchar,U.CreateTime,112) AS INT) = @intYYYYMMDD AND G.GameTime >= 30 --sec

	  --14日前的有效用户数
	  SELECT @intActiveCount=ISNULL(Count(userid),0) from #activeUser14

	  select COUNT(distinct UserID)*1.0/@intActiveCount * 100 as amount into #result14
	  from #todayLoginUser with (nolock) 
	  where userid in (select userid from #activeUser14)


	  update [Stat].[ActiveUserKeepLog] set [Day14Keep] = ISNULL((select amount from #result14),0) where YYYYMMDD=@intYYYYMMDD

	  -------------------------------------------------------------  

	  --有效用户的30日留存
	  SET @dtCreateTime = DateAdd(day,-30,GETDATE())
	  SET @intYYYYMMDD = CAST (CONVERT(varchar,@dtCreateTime,112) AS INT)

	  --30日前的有效用户
	  Select distinct G.UserID into #activeUser30
	  from [CenterDB].[Game].[UserGameInfo] G with (nolock) left join users.UserInfo U on G.userid=U.UserID
	  Where CAST ( CONVERT(varchar,U.CreateTime,112) AS INT) = @intYYYYMMDD AND G.GameTime >= 30 --sec

	  --30日前的有效用户数
	  SELECT @intActiveCount=ISNULL(Count(userid),0) from #activeUser30

	  select COUNT(distinct UserID)*1.0/@intActiveCount * 100 as amount into #result30
	  from #todayLoginUser with (nolock) 
	  where userid in (select userid from #activeUser30 )


	  update [Stat].[ActiveUserKeepLog] set [Day30Keep] = ISNULL((select amount from #result30),0) where YYYYMMDD=@intYYYYMMDD

	  -------------------------------------------------------------  

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_StatActiveUserKeepLog] TO [xbuser]
    AS [dbo];

