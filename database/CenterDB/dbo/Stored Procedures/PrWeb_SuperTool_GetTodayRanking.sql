
-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	超端 今日輸贏排行榜
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_GetTodayRanking]
(
	@intGameID int,
	@intOnline tinyint = null -- 篩選: 1 在線 0 離線
)
AS
BEGIN

	SET NOCOUNT ON

	declare @dtToday datetime =  convert(varchar,getdate(),110)
	--declare @intGameID int = 19
	declare @chvYYYYMMDD varchar(8)= Convert(varchar,Dateadd(day,0,getdate()),112)

	--今天有玩遊戲的用戶
	select distinct UserID into #gameuser from logdb.[Game].[GameLog] where LoginTime >= @dtToday and gameid = @intGameID


	--今日遊戲勝負
	select userid,
	sum(wincount+losecount+drawcount) as TodayAll, --今日局數
	sum(wincount) as TodayWin, --今日勝局

	CASE WHEN sum(wincount+losecount+drawcount)=0 THEN 0
	ELSE sum(wincount) / (sum(wincount+losecount+drawcount) *1.0) * 100 END as TodayWinRate, --今日勝率
	
	min(logintime) as LoginTime,  --上線
	max(logouttime) as LogoutTime   --下線
	into #todaygameinfo
	from logdb.[Game].[GameLog] where LoginTime >= @dtToday and gameid = @intGameID and UserID in (select UserID from #gameuser)
	group by userid



	--今天充直與兌換
	select yyyymmdd, userid, 
	SUM(SumCharge)- SUM(-SumConvert) as TodayUserWinLose --今日輸贏
	into #TodayUserinfo
	from
	(
		select Convert(varchar,@dtToday,112) as yyyymmdd, UserID, SUM(UpdateAmount) as SumCharge, 0 as SumConvert from logdb.[Money].[MoneyLog] with (nolock)
		where [SourceTypeID] >= 11001 and [SourceTypeID] < 11099 and 
		Convert(varchar,CreateTime,112) = @chvYYYYMMDD and
		UserID in (select UserID from #gameuser)
		group by UserID
		union
		/*
		select Convert(varchar,getdate(),112) as yyyymmdd, UserID, 0 as SumCharge, SUM(UpdateAmount) as SumConvert from logdb.[Money].[MoneyLog] with (nolock)
		where [SourceTypeID] =14000 and 
		Convert(varchar,CreateTime,112) = @chvYYYYMMDD and
		UserID in (select UserID from #gameuser)
		group by UserID
		*/
		select Convert(varchar,@dtToday,112) as yyyymmdd, UserID, 0 as SumCharge, SUM(Amount) as SumConvert from
		logdb.Money.ConvertLog with (nolock)
		Where
		Convert(varchar,CreateTime,112) = @chvYYYYMMDD
		AND UserID in (select UserID from #gameuser)
		AND Status = 3
		group by UserID
	) as a
	group by yyyymmdd, userid






	--遊戲總勝負
	select UserID,
	CASE WHEN (WinCount+LoseCount+DrawCount)=0 THEN 0 
	ELSE WinCount/((WinCount+LoseCount+DrawCount) * 1.0) * 100 END as TotalWinRate --總勝率
	
	into #totalgameinfo
	from [CenterDB].Game.UserGameInfo with (nolock) where UserID in (select UserID from #gameuser) and GameID = @intGameID



	--上線清單
	Select UserID,1 as Online 
	into #online
	from CenterDB.Game.GameUserlist  with (nolock)
	where ServerID in (select ServerID from CenterDB.ServerInfo.GameServerList with (nolock) where GameID = @intGameID)


	select 
	td_game.*,
	tt_game.TotalWinRate,
	td_user.TodayUserWinLose,
	ISNULL(online_user.online,0) as Online,

	ISNULL(VIP.UserID, 0) as VIPUserID --增加監控/刪除監控

	from #todaygameinfo as td_game  
	left join #totalgameinfo as tt_game on tt_game.UserID = td_game.UserID
	left join #online as online_user on online_user.UserID = td_game.UserID
	left join Game.SuperTool_VIPUser as VIP with (nolock) on VIP.UserID = td_game.UserID 
	left join #TodayUserinfo as td_user on td_user.UserID = td_game.UserID
	WHERE (@intOnline IS NULL OR Online = @intOnline)
	order by td_user.TodayUserWinLose desc

	--drop table #gameuser
	--drop table #totalgameinfo
	--drop table #todaygameinfo
	--drop table #TodayUserinfo
	--drop table #online
	
END





/****** Object:  StoredProcedure [dbo].[PrWeb_SuperTool_GetTodayUser]    Script Date: 2018/10/5 下午 02:07:58 ******/
SET ANSI_NULLS ON

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_GetTodayRanking] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_GetTodayRanking] TO [xbuser]
    AS [dbo];

