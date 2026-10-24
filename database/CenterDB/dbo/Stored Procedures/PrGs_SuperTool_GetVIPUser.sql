-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	超端 Server取得監控人數
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_SuperTool_GetVIPUser]
(
	@intGameID int
)
AS
BEGIN

	SET NOCOUNT ON

	/*
	declare @dtToday datetime =  convert(varchar,getdate(),110)
	--declare @intGameID int = 19
	declare @chvYYYYMMDD varchar(8)= Convert(varchar,Dateadd(day,0,getdate()),112)

	--監控用戶
	select UserID into #vipuser from CenterDB.[Game].[SuperTool_VIPUser]


	--遊戲總勝負
	select UserID, 
	(WinCount+LoseCount+DrawCount) as TotalAll --遊戲總局數
	into #totalgameinfo
	from [CenterDB].Game.UserGameInfo with (nolock) where UserID in (select UserID from #vipuser) and GameID = @intGameID


	--今天充直與兌換
	select yyyymmdd, userid, 
	SUM(SumCharge)- SUM(-SumConvert) as TodayUserWinLose --今日輸贏
	into #TodayUserinfo
	from
	(
		select Convert(varchar,@dtToday,112) as yyyymmdd, UserID, SUM(UpdateAmount) as SumCharge, 0 as SumConvert from logdb.[Money].[MoneyLog] with (nolock)
		where [SourceTypeID] >= 11001 and [SourceTypeID] < 11099 and 
		Convert(varchar,CreateTime,112) = @chvYYYYMMDD and
		UserID in (select UserID from #vipuser)
		group by UserID
		union
		/*
		select Convert(varchar,getdate(),112) as yyyymmdd, UserID, 0 as SumCharge, SUM(UpdateAmount) as SumConvert from logdb.[Money].[MoneyLog] with (nolock)
		where [SourceTypeID] =14000 and 
		Convert(varchar,CreateTime,112) = @chvYYYYMMDD and
		UserID in (select UserID from #vipuser)
		group by UserID
		*/
		select Convert(varchar,@dtToday,112) as yyyymmdd, UserID, 0 as SumCharge, SUM(Amount) as SumConvert from
		logdb.Money.ConvertLog with (nolock)
		Where
		Convert(varchar,CreateTime,112) = @chvYYYYMMDD and
		UserID in (select UserID from #vipuser)
		group by UserID

	) as info
	group by yyyymmdd, userid


	--用戶總充值與兌換
	select U.userid, 
	((SumCharge+SumAgentCharge) - SumConvert - C.Amount) as TotalUserWinLose, --總輸贏
	C.Amount as UserCash --用戶金幣
	into #TotalUserinfo
	from [CenterDB].users.UserInfo U with (nolock)
	left join
	[CenterDB].Money.Cash C with (nolock)
	ON U.UserID = C.UserID
	where U.UserID in (select UserID from #vipuser)
	*/

	select 
	vip.UserID,
	--vip.AssignRate,
	ISNULL(R.Value,0) as AssignRate,
	vip.AssignAmount,
	vip.Complete,
	vip.CompleteAmount,
	vip.Status,
	vip.Operator,
	vip.SumAssignAmount,
	vip.UpdateTime,
	vip.finishTime,
	vip.CreateTime
	
	--ISNULL(tt_game.TotalAll,0) as TotalAll, --遊戲總局數
	--ISNULL(tt_user.TotalUserWinLose,0) as TotalUserWinLose, --總輸贏
	--ISNULL(td_user.TodayUserWinLose,0) as TodayUserWinLose, --今日輸贏
	--ISNULL(tt_user.UserCash,0) as UserCash
	
	from CenterDB.[Game].[SuperTool_VIPUser] as vip  
	left join Game.SuperTool_RateInfo R with (nolock) on R.GameID = @intGameID and vip.AssignRate = R.RateID
	--left join #totalgameinfo as tt_game on tt_game.UserID = vip.UserID
	--left join #TotalUserinfo as tt_user on tt_user.UserID = vip.UserID
	--left join #TodayUserinfo as td_user on td_user.UserID = vip.UserID

	order by VIP.UpdateTime desc

	
	RETURN 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_GetVIPUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_GetVIPUser] TO [xbuser]
    AS [dbo];

