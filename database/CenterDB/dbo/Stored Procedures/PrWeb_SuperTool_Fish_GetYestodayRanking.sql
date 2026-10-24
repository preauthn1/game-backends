-- =============================================
-- Author:  alan
-- Create date: 20180908
-- Description:	取得 捕魚 昨日輸贏排行
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_Fish_GetYestodayRanking]

AS
BEGIN

	SET NOCOUNT ON


	declare @dtToday datetime =  convert(varchar,getdate(),110)
	declare @intGameID int = 14
	declare @chvYYYYMMDD varchar(8)= Convert(varchar,Dateadd(day,0,getdate()),112)

	--全部有玩捕魚遊戲的用戶
	select distinct UserID into #gameuser from logdb.[Game].[GameLog] where gameid = @intGameID

	

	--用戶昨日輸贏
	select userid,SUM(amount) as  TodayUserWinLose
	into #YestodayUserinfo 
	from logdb.game.GameLog with (nolock) where 
	GameID = @intGameID and UserID in (select UserID from #gameuser) 
	and logintime >= CAST( Convert(varchar, DateAdd(day,-1,getdate()),110) as Datetime ) 
	group by userid

	--用戶截至昨天總輸贏
	select userid,SUM(amount) as  TotalUserWinLose
	into #TotalUserinfo 
	from logdb.game.GameLog with (nolock) where 
	GameID = @intGameID and UserID in (select UserID from #gameuser) 
	and logintime < CAST( Convert(varchar,getdate(),110) as Datetime ) 
	group by userid



	--放分清單
	select UserID, SumAssignAmount 
	into #VIP
	from Game.SuperTool_VIPUser with (nolock) 	
	where userid in (
		select userid from #gameuser
	)	
	group by UserID, SumAssignAmount



	--昨日排行榜
	select top 100
	YestodayUserinfo.userid, 
	YestodayUserinfo.TodayUserWinLose, --昨日輸贏
	TotalUserinfo.TotalUserWinLose, --總輸贏
	VIP.SumAssignAmount --放分金額
	from 
	#YestodayUserinfo YestodayUserinfo
	left join #TotalUserinfo TotalUserinfo on YestodayUserinfo.UserID = TotalUserinfo.UserID
	left join #VIP VIP on YestodayUserinfo.UserID = VIP.UserID
	order by YestodayUserinfo.TodayUserWinLose desc
	


	RETURN 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_Fish_GetYestodayRanking] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_Fish_GetYestodayRanking] TO [xbuser]
    AS [dbo];

