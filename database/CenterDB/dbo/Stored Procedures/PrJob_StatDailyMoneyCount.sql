
-- =============================================
-- Author:  jason
-- Create date: #526 每日報表 後台第三方帳號使用
-- Description:	排程統計每日金幣異動(用來取代PrJob_StatSumMoney)
--	2019/03/27	Jason		新增PlatformID AreaID 對應 寫到新的[Stat].[StatisticsMoneySourceLog_New]
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_StatDailyMoneyCount]
(
	@dtStatTime		DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON

	set @dtStatTime = isnull(@dtStatTime,getdate())
	
	declare @TotalCashAmount		bigint
	declare @TotalBankAmount		bigint
	declare @Usercashamount	bigint
	declare @Userbankamount	bigint
	declare @lasttotalcashdiff bigint
	declare @lasttotalbankdiff bigint
	declare @lastusercashdiff bigint
	declare @lastuserbankdiff bigint
	declare @SumIn			bigint
	declare @SumOut			bigint
	declare @LogStartTime	datetime
	declare @LogEndTime		datetime
	declare @lastdatetime	datetime

	if (datepart(hour,@dtStatTime) = 0) and (datepart(minute,@dtStatTime) < 10)
	begin
		set @LogStartTime = dateadd(day,-1,datediff(day,0,@dtStatTime))
		set @LogEndTime	= dateadd(day,0,datediff(day,0,@dtStatTime))
	end
	else
	begin
		set @LogStartTime		= dateadd(day,0,datediff(day,0,@dtStatTime))
		set @LogEndTime			= dateadd(day,1,datediff(day,0,@dtStatTime))
	end

	set @lastdatetime = dateadd(day,-1,@LogStartTime)

	select	@TotalCashAmount = sum(amount) from [Money].[Cash] with (nolock)

	select	@TotalBankAmount = sum(amount) from [Money].[Bank] with (nolock)

	select	@Usercashamount = sum(a.amount) from [Money].[Cash] as a with (nolock)
		inner join [Users].[UserInfo] as b with (nolock) on a.userid = b.userid and b.usertype <> 2

	select	@Userbankamount = sum(a.amount) from [Money].[Bank] as a with (nolock)
		inner join [Users].[UserInfo] as b with (nolock) on a.userid = b.userid and b.usertype <> 2

	/*  20190327 因應Stat.MoneySourceLog 需要新增PlatformID AreaID 故讀取新表 */
	-------------------------------------------------------------------------------------------------------------
	--SELECT @SumIn = SUM(CASE WHEN t.ChangeType = 0 THEN l.Amount ELSE 0 END),
	--		@SumOut = SUM(CASE WHEN t.ChangeType = 1 THEN l.Amount ELSE 0 END)
	--FROM Stat.MoneySourceLog l , Stat.MoneySourceType t
	--WHERE
	--	l.SourceID = t.SourceID
	--	AND t.SourceID NOT IN(15001,15002)
	--	AND StatTime >= @LogStartTime AND StatTime < @LogEndTime

	SELECT @SumIn = SUM(CASE WHEN t.ChangeType = 0 THEN l.Amount ELSE 0 END),
			@SumOut = SUM(CASE WHEN t.ChangeType = 1 THEN l.Amount ELSE 0 END)
	FROM Stat.MoneySourceLog_New l , Stat.MoneySourceType t
	WHERE
		l.SourceID = t.SourceID
		AND t.SourceID NOT IN(15001,15002)
		AND StatTime >= @LogStartTime AND StatTime < @LogEndTime
	-------------------------------------------------------------------------------------------------------------

	select	b.PlatformID,
			a.areaid,
			totalcashamount = sum(c.amount),
			totalbankamount = sum(d.amount),
			usercashamount = sum(case when a.usertype <> 2 then c.amount else 0 end),
			userbankamount = sum(case when a.usertype <> 2 then d.amount else 0 end),
			sumin = @SumIn,
			sumout = @SumOut
	into #T1
	from [Users].[UserInfo] as a with (nolock)
		inner join [Area].[Area] as b with (nolock) on a.areaid = b.areaid
		left join [Money].[Cash] as c with (nolock) on a.userid = c.userid
		left join [Money].[Bank] as d with (nolock) on a.userid = d.userid
	group by b.PlatformID,a.areaid

	
	select	PlatformID,
			areaid,
			totalcashamount,
			totalbankamount,
			usercashamount,
			userbankamount,
			sumin,
			sumout
	into #T2
	from [Stat].[SumMoneyDaily] with (nolock)
	where
		stattime = @lastdatetime


	select	stattime = @LogStartTime,
			PlatformID = a.PlatformID,
			areaid = a.AreaID,
			totalcashamount = isnull(a.totalcashamount,0),
			totalbankamount = isnull(a.totalbankamount,0),
			usercashamount = isnull(a.usercashamount,0),
			userbankamount = isnull(a.userbankamount,0),
			totalcashdiff = case when isnull(a.totalcashamount,0) < 0 and isnull(b.totalcashamount,0) < 0 and isnull(a.totalcashamount,0) > isnull(b.totalcashamount,0)
									then isnull(a.totalcashamount,0) - isnull(b.totalcashamount,0)
								when isnull(a.totalcashamount,0) < 0 and isnull(b.totalcashamount,0) < 0 and isnull(a.totalcashamount,0) < isnull(b.totalcashamount,0)
									then abs(isnull(a.totalcashamount,0) - isnull(b.totalcashamount,0))
								when isnull(a.totalcashamount,0) < 0 and isnull(b.totalcashamount,0) > 0
									then abs(isnull(a.totalcashamount,0)) - isnull(b.totalcashamount,0)
								when isnull(a.totalcashamount,0) > 0 and isnull(b.totalcashamount,0) < 0
									then isnull(a.totalcashamount,0) - isnull(b.totalcashamount,0)
								when isnull(a.totalcashamount,0) > 0 and isnull(b.totalcashamount,0) > 0
									then isnull(a.totalcashamount,0) - isnull(b.totalcashamount,0)
								end,
			totalbankdiff = case when isnull(a.totalbankamount,0) < 0 and isnull(b.totalbankamount,0) < 0 and isnull(a.totalbankamount,0) > isnull(b.totalbankamount,0)
									then isnull(a.totalbankamount,0) - isnull(b.totalbankamount,0)
								when isnull(a.totalbankamount,0) < 0 and isnull(b.totalbankamount,0) < 0 and isnull(a.totalbankamount,0) < isnull(b.totalbankamount,0)
									then abs(isnull(a.totalbankamount,0) - isnull(b.totalbankamount,0))
								when isnull(a.totalbankamount,0) < 0 and isnull(b.totalbankamount,0) > 0
									then abs(isnull(a.totalbankamount,0)) - isnull(b.totalbankamount,0)
								when isnull(a.totalbankamount,0) > 0 and isnull(b.totalbankamount,0) < 0
									then isnull(a.totalbankamount,0) - isnull(b.totalbankamount,0)
								when isnull(a.totalbankamount,0) > 0 and isnull(b.totalbankamount,0) > 0
									then isnull(a.totalbankamount,0) - isnull(b.totalbankamount,0)
								end,
			usercashdiff = case when isnull(a.usercashamount,0) < 0 and isnull(b.usercashamount,0) < 0 and isnull(a.usercashamount,0) > isnull(b.usercashamount,0)
									then isnull(a.usercashamount,0) - isnull(b.usercashamount,0)
								when isnull(a.usercashamount,0) < 0 and isnull(b.usercashamount,0) < 0 and isnull(a.usercashamount,0) < isnull(b.usercashamount,0)
									then abs(isnull(a.usercashamount,0) - isnull(b.usercashamount,0))
								when isnull(a.usercashamount,0) < 0 and isnull(b.usercashamount,0) > 0
									then abs(isnull(a.usercashamount,0)) - isnull(b.usercashamount,0)
								when isnull(a.usercashamount,0) > 0 and isnull(b.usercashamount,0) < 0
									then isnull(a.usercashamount,0) - isnull(b.usercashamount,0)
								when isnull(a.usercashamount,0) > 0 and isnull(b.usercashamount,0) > 0
									then isnull(a.usercashamount,0) - isnull(b.usercashamount,0)
								end,
			userbankdiff = case when isnull(a.userbankamount,0) < 0 and isnull(b.userbankamount,0) < 0 and isnull(a.userbankamount,0) > isnull(b.userbankamount,0)
									then isnull(a.userbankamount,0) - isnull(b.userbankamount,0)
								when isnull(a.userbankamount,0) < 0 and isnull(b.userbankamount,0) < 0 and isnull(a.userbankamount,0) < isnull(b.userbankamount,0)
									then abs(isnull(a.userbankamount,0) - isnull(b.userbankamount,0))
								when isnull(a.userbankamount,0) < 0 and isnull(b.userbankamount,0) > 0
									then abs(isnull(a.userbankamount,0)) - isnull(b.userbankamount,0)
								when isnull(a.userbankamount,0) > 0 and isnull(b.userbankamount,0) < 0
									then isnull(a.userbankamount,0) - isnull(b.userbankamount,0)
								when isnull(a.userbankamount,0) > 0 and isnull(b.userbankamount,0) > 0
									then isnull(a.userbankamount,0) - isnull(b.userbankamount,0)
								end,
			a.sumin,
			a.sumout,
			sumindiff = case when isnull(a.sumin,0) < 0 and isnull(b.sumin,0) < 0 and isnull(a.sumin,0) > isnull(b.sumin,0)
									then isnull(a.sumin,0) - isnull(b.sumin,0)
								when isnull(a.sumin,0) < 0 and isnull(b.sumin,0) < 0 and isnull(a.sumin,0) < isnull(b.sumin,0)
									then abs(isnull(a.sumin,0) - isnull(b.sumin,0))
								when isnull(a.sumin,0) < 0 and isnull(b.sumin,0) > 0
									then abs(isnull(a.sumin,0)) - isnull(b.sumin,0)
								when isnull(a.sumin,0) > 0 and isnull(b.sumin,0) < 0
									then isnull(a.sumin,0) - isnull(b.sumin,0)
								when isnull(a.sumin,0) > 0 and isnull(b.sumin,0) > 0
									then isnull(a.sumin,0) - isnull(b.sumin,0)
								end,
			sumoutdiff = case when isnull(a.sumout,0) < 0 and isnull(b.sumout,0) < 0 and isnull(a.sumout,0) > isnull(b.sumout,0)
									then isnull(a.sumout,0) - isnull(b.sumout,0)
								when isnull(a.sumout,0) < 0 and isnull(b.sumout,0) < 0 and isnull(a.sumout,0) < isnull(b.sumout,0)
									then abs(isnull(a.sumout,0) - isnull(b.sumout,0))
								when isnull(a.sumout,0) < 0 and isnull(b.sumout,0) > 0
									then abs(isnull(a.sumout,0)) - isnull(b.sumout,0)
								when isnull(a.sumout,0) > 0 and isnull(b.sumout,0) < 0
									then isnull(a.sumout,0) - isnull(b.sumout,0)
								when isnull(a.sumout,0) > 0 and isnull(b.sumout,0) > 0
									then isnull(a.sumout,0) - isnull(b.sumout,0)
								end
	into #T3
	from #T1 as a
		left join #T2 as b on a.platformid = b.platformid and a.areaid = b.areaid

	MERGE [Stat].[SumMoneyDaily] AS A
	USING (
				SELECT	* FROM #T3
		) AS B ON (A.[StatTime] = B.[StatTime] AND A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID])
	WHEN MATCHED THEN
		UPDATE SET	A.[totalcashamount] = isnull(B.[totalcashamount],0),
					A.[totalbankamount] = isnull(B.[totalbankamount],0),
					A.[usercashamount] = isnull(B.[usercashamount],0),
					A.[userbankamount] = isnull(B.[userbankamount],0),
					A.[totalcashdiff] = isnull(B.[totalcashdiff],0),
					A.[totalbankdiff] = isnull(B.[totalbankdiff],0),
					A.[usercashdiff] = isnull(B.[usercashdiff],0),
					A.[userbankdiff] = isnull(B.[userbankdiff],0),
					A.[sumin] = isnull(B.[sumin],0),
					A.[sumout] = isnull(B.[sumout],0),
					A.[sumindiff] = isnull(B.[sumindiff],0),
					A.[sumoutdiff] = isnull(B.[sumoutdiff],0)
	WHEN NOT MATCHED THEN
		INSERT ([stattime],[PlatformID],[AreaID],[totalcashamount],[totalbankamount],[usercashamount],[userbankamount],[totalcashdiff],[totalbankdiff],[usercashdiff],[userbankdiff]
				,[sumin],[sumout],[sumindiff],[sumoutdiff])
		VALUES
			(
				[stattime],[PlatformID],[AreaID],
				isnull([totalcashamount],0),
				isnull([totalbankamount],0),
				isnull([usercashamount],0),
				isnull([userbankamount],0),
				isnull([totalcashdiff],0),
				isnull([totalbankdiff],0),
				isnull([usercashdiff],0),
				isnull([userbankdiff],0),
				isnull([sumin],0),
				isnull([sumout],0),
				isnull([sumindiff],0),
				isnull([sumoutdiff],0)
			);


	drop table #T1
	drop table #T2
	drop table #T3
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_StatDailyMoneyCount] TO [xbuser]
    AS [dbo];

