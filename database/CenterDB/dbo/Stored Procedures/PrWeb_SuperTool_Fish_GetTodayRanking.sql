-- =============================================
-- Author:  alan
-- Create date: 20180908
-- Description:	取得 捕魚 今日輸贏排行
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_Fish_GetTodayRanking]

AS
BEGIN

	SET NOCOUNT ON


	declare @dtToday datetime =  convert(varchar,getdate(),110)
	declare @intGameID int = 14
	declare @chvYYYYMMDD varchar(8)= Convert(varchar,Dateadd(day,0,getdate()),112)

	--全部有玩捕魚遊戲的用戶
	select distinct UserID into #gameuser from logdb.[Game].[GameLog] where gameid = @intGameID

	/*
	--今天充直兌換與總輸贏
	select  yyyymmdd, userid, 
	SUM(SumCharge)- SUM(-SumConvert) as TodayUserWinLose --今日輸贏
	into #TodayUserinfo
	from
	(
		select Convert(varchar,@dtToday,112) as yyyymmdd, UserID, SUM(UpdateAmount) as SumCharge, 0 as SumConvert from logdb.[Money].[MoneyLog] with (nolock)
		where [SourceTypeID] >= 11001 and [SourceTypeID] < 11099 --支付充直
		and Convert(varchar,CreateTime,112) = @chvYYYYMMDD
		and UserID in (select UserID from #gameuser)
		group by UserID
		union
		select Convert(varchar,@dtToday,112) as yyyymmdd, UserID, 0 as SumCharge, SUM(Amount) as SumConvert from
		logdb.Money.ConvertLog with (nolock)
		Where
		Convert(varchar,CreateTime,112) = @chvYYYYMMDD and
		UserID in (select UserID from #gameuser)
		group by UserID
	) as a
	group by yyyymmdd, userid
	

	--用戶總充值兌換總輸贏
	select U.Userid, 
	((SumCharge+SumAgentCharge) - SumConvert - C.Amount) as TotalUserWinLose, --總輸贏
	C.Amount as UserCash --用戶金幣
	into #TotalUserinfo
	from [CenterDB].users.UserInfo U with (nolock) 
	LEFT JOIN [CenterDB].Money.Cash C with (nolock)
	ON U.UserID=C.UserID	
	where U.UserID in (select UserID from #gameuser)
	*/
	
	

	--用戶今日輸贏
	select userid,SUM(amount) as  TodayUserWinLose
	into #TodayUserinfo 
	from logdb.game.GameLog with (nolock) where 
	GameID = @intGameID and UserID in (select UserID from #gameuser) 
	and logintime >= CAST( Convert(varchar,getdate(),110) as Datetime ) 
	group by userid

	--用戶總輸贏
	select userid,SUM(amount) as  TotalUserWinLose
	into #TotalUserinfo 
	from logdb.game.GameLog with (nolock) where 
	GameID = @intGameID and UserID in (select UserID from #gameuser) 
	group by userid



	--放分清單
	select UserID, SumAssignAmount 
	into #VIP
	from Game.SuperTool_VIPUser with (nolock) 	
	where userid in (
		select userid from #gameuser
	)	
	group by UserID, SumAssignAmount



	--今日排行榜
	select top 100
	TodayUserinfo.userid, 
	TodayUserinfo.TodayUserWinLose, --今日輸贏
	TotalUserinfo.TotalUserWinLose, --總輸贏
	VIP.SumAssignAmount --放分金額
	from 
	#TodayUserinfo TodayUserinfo
	left join #TotalUserinfo TotalUserinfo on TodayUserinfo.UserID = TotalUserinfo.UserID
	left join #VIP VIP on TodayUserinfo.UserID = VIP.UserID
	order by TodayUserinfo.TodayUserWinLose desc
	



	--drop table #gameuser
	--drop table #TotalUserinfo
	--drop table #TodayUserinfo
	--drop table #onlineinfo
	--drop table #VIP
	RETURN 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_Fish_GetTodayRanking] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_Fish_GetTodayRanking] TO [xbuser]
    AS [dbo];

