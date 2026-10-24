
-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	游戏ZJH 取得監控名單
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_SuperTool_ZJH_GetVIPUser]
AS
BEGIN

	SET NOCOUNT ON

	declare @dtToday datetime =  convert(varchar,getdate(),110)
	declare @chvYYYYMMDD varchar(8)= Convert(varchar,Dateadd(day,0,getdate()),112)

	--監控用戶
	select UserID into #vipuser from CenterDB.[Game].[SuperTool_ZJH_VIPUser] with (nolock)


	--今天充直與兌換
	select yyyymmdd, userid, 
	SUM(SumCharge) as TodaySumcharge, 
	SUM(-SumConvert) as TodaySumConvert, 
	SUM(SumCharge)- SUM(-SumConvert) as TodayUserWinLose 
	INTO #TodayUserinfo
	from
	(
		select Convert(varchar,@dtToday,112) as yyyymmdd, UserID, SUM(UpdateAmount) as SumCharge, 0 as SumConvert from logdb.[Money].[MoneyLog] with (nolock)
		where [SourceTypeID] >= 11001 and [SourceTypeID] < 11099 and 
		Convert(varchar,CreateTime,112) = @chvYYYYMMDD and
		UserID in (select UserID from #vipuser)
		group by UserID
		union
		select Convert(varchar,@dtToday,112) as yyyymmdd, UserID, 0 as SumCharge, SUM(Amount) as SumConvert from
		logdb.Money.ConvertLog with (nolock)
		Where
		Convert(varchar,CreateTime,112) = @chvYYYYMMDD and
		UserID in (select UserID from #vipuser)
		group by UserID
	) as a
	GROUP BY yyyymmdd, userid


	SELECT 
	Z.UserID,
	ISNULL(Z.NextType,99) as NextType,
	ISNULL(Z.NextWinLose,99) as NextWinLose,
	ISNULL(Z.NextWinLoseNum,99) as NextWinLoseNum,
	ISNULL(Z.Finish,0) as Finish,
	ISNULL(Z.FinishNextWinLoseNum,0) as FinishNextWinLoseNum,
	ISNULL(Z.NextTable,-1) as NextTable,
	Z.Status,
	Z.Operator,
	ISNULL(U.TodaySumcharge,0) as TodaySumcharge,
	ISNULL(U.TodaySumConvert,0) as TodaySumConvert,
	ISNULL(U.TodayUserWinLose,0) as TodayUserWinLose 
	FROM Game.SuperTool_ZJH_VIPUser Z with (nolock)
	LEFT JOIN #TodayUserinfo U with (nolock)
	ON Z.UserID=U.UserID

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_ZJH_GetVIPUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_ZJH_GetVIPUser] TO [xbuser]
    AS [dbo];

