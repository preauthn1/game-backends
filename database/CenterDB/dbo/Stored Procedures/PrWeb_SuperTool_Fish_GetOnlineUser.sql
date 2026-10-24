

-- =============================================
-- Author:  alan
-- Create date: 20180908
-- Description:	捕魚超端 取得在線用戶資訊
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_Fish_GetOnlineUser] 
(
@chvShowName varchar(64)=null
)

AS
BEGIN

	SET NOCOUNT ON

	declare @dtToday datetime =  convert(varchar,getdate(),110)
	declare @intGameID int = 14
	declare @chvYYYYMMDD varchar(8)= Convert(varchar,Dateadd(day,0,getdate()),112)


	--上線清單
	Select G.UserID,S.ShowName
	into #online
	from CenterDB.Game.GameUserlist G  with (nolock)
	LEFT Join Users.UserInfo U  with (nolock)
	ON G.UserID=U.UserID
	LEFT Join CenterDB.ServerInfo.GameServerList S with (nolock)
	ON G.ServerID=S.ServerID
	where 
	U.UserType <> 2
	AND
	G.ServerID	
	in (
	select ServerID from CenterDB.ServerInfo.GameServerList with (nolock) 
	where (@chvShowName is NULL or ShowName=@chvShowName) and GameID=@intGameID)


	--今天充直與兌換
	select yyyymmdd, userid, 
	SUM(SumCharge) as TodaySumcharge, 
	SUM(-SumConvert) as TodaySumConvert, 
	SUM(SumCharge)- SUM(-SumConvert) as TodayUserWinLose 
	into #TodayUserinfo
	from
	(
		select Convert(varchar,@dtToday,112) as yyyymmdd, UserID, SUM(UpdateAmount) as SumCharge, 0 as SumConvert from logdb.[Money].[MoneyLog] with (nolock)
		where [SourceTypeID] >= 11001 and [SourceTypeID] < 11099 and 
		Convert(varchar,CreateTime,112) = @chvYYYYMMDD and
		UserID in (select UserID from #online)
		group by UserID
		union
		select Convert(varchar,@dtToday,112) as yyyymmdd, UserID, 0 as SumCharge, SUM(Amount) as SumConvert from
		logdb.Money.ConvertLog with (nolock)
		Where
		Convert(varchar,CreateTime,112) = @chvYYYYMMDD and
		UserID in (select UserID from #online)
		group by UserID

	) as a
	group by yyyymmdd, userid


	--用戶總充值與兌換
	select U.Userid, 
	(SumCharge+SumAgentCharge) as TotalSumCharge, --總充直
	SumConvert as TotalSumConvert, --總兌換
	((SumCharge+SumAgentCharge) - SumConvert - C.Amount) as TotalUserWinLose, --總輸贏
	C.Amount as UserCash --用戶金幣
	into #TotalUserinfo
	from [CenterDB].users.UserInfo U with(nolock) 
	LEFT JOIN [CenterDB].Money.Cash C with (nolock)
	ON U.UserID=C.UserID
	where U.UserID in (select UserID from #online)


	select 
	online.UserID,
	online.ShowName as FlagID,
	ISNULL(tt_user.TotalSumCharge,0) as TotalSumCharge,
	ISNULL(tt_user.TotalSumConvert,0) as TotalSumConvert,
	ISNULL(tt_user.TotalUserWinLose,0) as TotalUserWinLose,
	ISNULL(tt_user.UserCash,0) as UserCash,
	ISNULL(td_user.TodaySumCharge,0) as TodaySumCharge,
	ISNULL(td_user.TodaySumConvert,0) as TodaySumConvert,
	ISNULL(td_user.TodayUserWinLose,0) as TodayUserWinLose,
	ISNULL(VIP.UserID, 0) as VIPUserID --增加監控/刪除監控

	from #online as online  
	left join #TotalUserinfo as tt_user on tt_user.UserID = online.UserID
	left join #TodayUserinfo as td_user on td_user.UserID = online.UserID
	left join Game.SuperTool_VIPUser as VIP with (nolock) on VIP.UserID = online.UserID
	order by VIPUserID desc

	--drop table #TodayUserinfo
	--drop table #TotalUserinfo
	--drop table #online
	
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_Fish_GetOnlineUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_Fish_GetOnlineUser] TO [xbuser]
    AS [dbo];

