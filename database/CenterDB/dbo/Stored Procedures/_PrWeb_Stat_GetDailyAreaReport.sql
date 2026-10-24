-- =============================================
-- Author:  Jason
-- Create date: 2018/06/26
-- Description:	每日報表 後台第三方帳號使用 #526
-- =============================================
CREATE PROCEDURE [dbo].[_PrWeb_Stat_GetDailyAreaReport]
(
	@intAreaID	INT = NULL,
	@dtStatTime	DATETIME = NULL
)
AS
BEGIN
	
	SET NOCOUNT ON

	if (@intAreaID = 0)
	begin
		set @intAreaID = NULL
	end

	SET	@dtStatTime = ISNULL(@dtStatTime,GETDATE())

	declare @LogStartTime	DATETIME = DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
	declare @LogEndTime		DATETIME = DATEADD(DAY,1,DATEDIFF(DAY,0,@dtStatTime))
	declare @areaname varchar(32)

	select @areaname = [AreaName] from [Area].[Area] where areaid = @intAreaID
	set @areaname = isnull(@areaname,'')
	
	declare @dau int
	declare @agentsourcetypeid	int = 11008
	declare @intPageCount int

	select	stattime = @LogStartTime,
			areaid,
			dau = sum([TotalCount])
	into #dau
	from [Stat].[DAULog] with (nolock)
	where
		[StatTime] = @LogStartTime
		and platformid > 0
		and areaid > 0
		and gameid = 0
	group by areaid

	set @dau = isnull(@dau,0)

	/*  充值  */
	-------------------------------------------------------------------------------------------
	--代理充值暫存
	declare @temp_agentcharge table (rowNum int,logid bigint,userid int,sourcetypeid int,currentamount bigint,updateamount bigint,taxamount bigint,ipaddress varchar(40),memo varchar(128),createtime datetime,areaid int,platformid int,tradeid bigint)
	
	insert into @temp_agentcharge
	exec logdb.[dbo].[PrWeb_Stat_GetMoneyLog]
		@beginTime = @LogStartTime,
		@endTime = @LogEndTime,
		@intSourceType = @agentsourcetypeid,
		@intAreaID = @intAreaID,
		@intPageIndex = 1,
		@intPageSize = 1000000,
		@intPageCount = @intPageCount output


	--在线充值統計
	declare @ChargeSuccessAmount	float

	select	stattime = @LogStartTime,
			areaid,
			[amount] = (SUM([OldCharge]) + SUM([NewCharge])) * 1.00
	into #inappcharge
	from [CenterDB].[Stat].[AreaDataDay] as a
	where
		platformid > 0
		and isnull(@intAreaID,areaid) = areaid
		and gameid = 0 and serverid = 0
		and stattime = @LogStartTime
	group by areaid

	--代理充值統計
	declare @agentChargeSuccessUserAmount	float

	select	stattime = @LogStartTime,
			a.areaid,
			[amount] = SUM(A.updateamount) * 1.00 / 100
	into #agentcharge
	from @temp_agentcharge as a
		inner join [Users].[UserInfo] as b on a.userid = b.userid
	where
		b.areaid = @intAreaID
	group by a.areaid
	
	select	stattime,
			areaid,
			[amount] = sum([amount])
	into #charge
	from (
			select	stattime,
					areaid,
					[amount]
			from #inappcharge
			union all
			select	stattime,
					areaid,
					[amount]
			from #agentcharge
		) as a
	group by stattime,areaid

	-------------------------------------------------------------------------------------------

	
	/*  兌換  */
	-------------------------------------------------------------------------------------------
	declare @temp_convert table (userid int,converttype tinyint,[Amount] bigint)

	insert into @temp_convert
	select	userid,converttype,abs(amount)
	from logdb.[Money].[ConvertLog] with (nolock)
	where
		[CreateTime] >= @LogStartTime
		AND [CreateTime] < @LogEndTime
		and status = 3

	select	stattime = @LogStartTime,
			areaid,
			convertamount = sum(case when b.createtime < @LogStartTime then amount else 0 end) * 1.00 / 100 + 
								sum(case when b.createtime > @LogStartTime then amount else 0 end) * 1.00 / 100
	into #convert
	from @temp_convert as a
		inner join [Users].[UserInfo] as b on a.userid = b.userid
	where
		b.areaid = @intAreaID
	group by areaid
	-------------------------------------------------------------------------------------------

	
	/*  總金幣  */
	-------------------------------------------------------------------------------------------
	select	stattime = @LogStartTime,
			areaid,
			useramount = sum([usercashamount]) * 1.00 / 100
	into #summoney
	from [Stat].[SumMoneyDaily] with (nolock)
	where
		stattime = @LogStartTime
	group by areaid
	-------------------------------------------------------------------------------------------


	/*  註冊用戶數  */
	-------------------------------------------------------------------------------------------
	declare @reginfo	table (stattime datetime,areaid int,totalcount int,ios int,android int,iosbind int,androidbind int,ipcount int,newipcount int)

	insert into @reginfo
	exec logdb.[dbo].[PrWeb_Stat_GetRegCountByArea]
		@dtStatTime		= @LogStartTime,	
		@dtEndTime		= @LogEndTime

	select	stattime = @LogStartTime,
			areaid,
			totalreguser = isnull(totalcount,0)
	into #reg
	from @reginfo
	-------------------------------------------------------------------------------------------


	/*  登錄用戶數  */
	-------------------------------------------------------------------------------------------
	declare @logonstat table (stattime datetime,areaid int,totalcount int,ios int,android int,isbind int,isnew int,ipcount int)

	insert into @logonstat
	exec logdb.[dbo].[PrWeb_Stat_GetLogonCountByArea]
		@dtStatTime		= @LogStartTime,	
		@dtEndTime		= @LogEndTime

	select	stattime = @LogStartTime,
			areaid,
			totalcount
	into #logon
	from @logonstat
	-------------------------------------------------------------------------------------------


	/*  稅收  */
	-------------------------------------------------------------------------------------------
	select	stattime = @LogStartTime,
			areaid,
			gametaxtotal = sum([UserTaxAmount]) * 1.00 / 100
	into #gameamount
	from [Stat].[GameAmountLog] with (nolock)
	where
		stattime = @LogStartTime
	group by areaid
	-------------------------------------------------------------------------------------------


	/*  遊戲紀錄  */
	-------------------------------------------------------------------------------------------
	declare @gamecountstat table (stattime datetime,areaid int,gameid int,gamecount int)

	insert into @gamecountstat
	exec logdb.[dbo].[PrWeb_Stat_GetGameCountByArea]
		@dtStatTime		= @LogStartTime,	
		@dtEndTime		= @LogEndTime

	select	stattime = @LogStartTime,
			areaid,
			gamecounttotal = sum(gamecount)
	into #gamelog		
	from @gamecountstat
	group by areaid
	-------------------------------------------------------------------------------------------


	/*  回傳資料  */
	-------------------------------------------------------------------------------------------
	select	a.[StatTime],
			a.[AreaID],
			[ChargeConvert] = b.[amount] - c.convertamount,
			[ChargeAmount] = b.[Amount],
			[ConvertAmount] = c.convertamount,
			[GameAmount] = d.useramount,
			[RegUser] = e.totalreguser,
			[LogonUser] = f.totalcount,
			[Tax] = g.gametaxtotal,
			[GameLog] = h.gamecounttotal,
			[LastUpdate] = a.[StatTime]
	from #dau as a
		left join #charge as b on a.stattime = b.stattime and a.AreaID = b.AreaID
		left join #convert as c on a.stattime = c.stattime and a.AreaID = c.AreaID
		left join #summoney as d on a.stattime = d.stattime and a.AreaID = d.areaid
		left join #reg as e on a.stattime = e.stattime and a.AreaID = e.areaid
		left join #logon as f on a.stattime = f.stattime and a.AreaID = f.areaid
		left join #gameamount as g on a.stattime = g.stattime and a.AreaID = g.AreaID
		left join #gamelog as h on a.stattime = h.stattime and a.AreaID = h.areaid
	-------------------------------------------------------------------------------------------
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[_PrWeb_Stat_GetDailyAreaReport] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[_PrWeb_Stat_GetDailyAreaReport] TO [xbuser]
    AS [dbo];

