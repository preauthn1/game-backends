-- =============================================
-- Author:  jason
-- Create date: 2019/05/02
-- Description: USER 無限級 計算稅收/收益
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_Unlimit_CalcTax]	
(	
	@dtStatTime datetime = null
)
AS
BEGIN

	set nocount on

	declare @startdate datetime = dateadd(day,-1,datediff(day,0,isnull(@dtStatTime,getdate())))
	declare @enddate datetime = dateadd(day,0,datediff(day,0,isnull(@dtStatTime,getdate())))
	declare @logdate datetime = convert(varchar(10),@startdate,23)
	declare @loghour int = datepart(hour,getdate())	


	/*  先取出所有有對應的用戶  */
	create table #temp_agentuser (Num int,userid int, TopUserID int null ,stagelevel int, relations varchar(max))
	CREATE UNIQUE CLUSTERED INDEX IX_1 on #temp_agentuser (Num)
	CREATE INDEX IX_2 on #temp_agentuser (userid)

	insert into #temp_agentuser
	select	row_number() over (order by userid asc) as Num,
			[UserID],
			[TopUserID],
			stagelevel,
			relations
	from [dbo].[Unlimit_UserInfo] with (nolock)
	where status = 2
	  or TopUserID is not null

	/*  取流水紀錄  */
	create table #temp_moneylog	(Num int,userid int primary key,[TaxAmount] bigint)
	insert into #temp_moneylog
	select	row_number() over (order by a.userid asc) as Num, 
			a.[UserID],
			[TaxAmount] = SUM([FlowAmount])
	from logdb.[Money].[MoneyLog] as a with (nolock)
		inner join [dbo].[Unlimit_UserInfo] as b on a.UserID = b.userid
	where
		a.[CreateTime] >= @startdate and a.[CreateTime] < @enddate
		and [SourceTypeID] < 10000
	group by a.[UserID]

	/*  先寫入流水紀錄表  */
	if exists (select top 1 1 from [dbo].[Unlimit_UserTaxLog] with (nolock) where logdate = @LogDate)
	begin
		delete from [dbo].[Unlimit_UserTaxLog] where logdate = @LogDate
	end
	insert into [dbo].[Unlimit_UserTaxLog]
		([LogDate],[LogTime],[UserID],[TaxAmount],[CreateTime])
	select	@LogDate,@loghour,userid,[TaxAmount],getdate()
	from #temp_moneylog
	

	/*  建立用戶匯總資料表*/
	create table #temp_treeperformance (rowno int,userid int,totalperformance bigint,teamperformance bigint,directperformance bigint,TeamBonusAmount bigint,
										directBonusAmount bigint,levellimit int,[TopUserLevelLimit] bigint)
	CREATE UNIQUE CLUSTERED INDEX IX_1 on #temp_treeperformance (rowno)
	CREATE INDEX IX_2 on #temp_treeperformance (userid)

	insert into #temp_treeperformance
	select	row_number() over (order by userid asc) as rowno,
			userid,0,0,0,0,0,0,0
	from #temp_agentuser

	create table #upperuser (userid int PRIMARY KEY,stagelevel int)


	/*  取階層數往上堆業績  */
	declare @minno int
	declare @maxno int

	select	@minno = min(Num),
			@maxno = max(Num)
	from #temp_agentuser
	

	/*  先計算所有用戶階層樹匯總業績  */
	while(@minno <= @maxno)
	begin
		declare @upperuserrelationstr varchar(max) = null
		declare @countuserid int = null
		declare @countuserlv int = null
		declare @topuserid int = null
		declare @performance bigint = null

		
		--取計算的用戶
		select	@countuserid = a.userid,
				@topuserid = a.TopUserID,
				@countuserlv = a.stagelevel
		from #temp_agentuser as a
		where
			Num = @minno

		--取上層用戶關係
		select	@upperuserrelationstr = Relations
		from #temp_agentuser with (nolock)
		where
			userid = @topuserid	

		--取個人業績
		select	@performance = [TaxAmount]
		from #temp_moneylog
		where
			userid = @countuserid


		if(@upperuserrelationstr is not null)
		begin
			truncate table #upperuser

			--取直屬上級資料
			insert into #upperuser
			select	b.userid,
					b.stagelevel
			from [dbo].[fn_Split](@upperuserrelationstr,',') as a
				inner join #temp_agentuser as b on cast(a.value as int) = b.userid

			
			--更新整條上層業績
			update #temp_treeperformance
			set totalperformance = isnull(totalperformance,0) + isnull(@performance,0)
			from (select uid=userid from #upperuser) as a
			where
				userid = a.uid

			--更新直推業績
			if(@topuserid is not null)
			begin
				update #temp_treeperformance
				set directperformance = directperformance + isnull(@performance,0)
				where
					userid = @topuserid
			end

			--更新團隊業績
			update #temp_treeperformance
			set teamperformance = totalperformance - directperformance
			from (select uid=userid from #upperuser) as a
			where
				userid = a.uid
		end

		set @minno = @minno + 1		
	end


	/*  更新自己業績級差值  */
	update #temp_treeperformance
	set levellimit = a.BonusAmount
	from [dbo].[Unlimit_SystemRate] as a
	where
		totalperformance >= a.MinAmount
		and totalperformance < a.MaxAmount


	/*  取自己及上層級差值計算獎勵  */
	select	@minno = min(rowno),
			@maxno = max(rowno)
	from #temp_treeperformance


	/*  取每人團隊貢獻及級差值  */
	create table #lowerusers (userid int primary key,teamperformance bigint,levellimit int,teambonusamount bigint)
	declare @countuseridlevellimit int

	while(@minno <= @maxno)
	begin
		set @countuserid = null
		set @countuseridlevellimit = null
		set @upperuserrelationstr = null
		declare @countuserstatus int = null
		declare @countuserlevel int = null
		declare @totalteambonus bigint = null

		truncate table #lowerusers

		/*  取自己ID/級差值  */
		select	@countuserid = userid,
				@countuseridlevellimit = levellimit
		from #temp_treeperformance
		where
			rowno = @minno


		/*  取下層列表/可返佣金額  */
		select	@upperuserrelationstr = relations,
				@countuserlevel = stagelevel + 1
		from #temp_agentuser with (nolock)
		where
			userid = @countuserid


		insert into #lowerusers
		select	a.userid,
				b.teamperformance,
				b.levellimit,
				teambonusamount = round((@countuseridlevellimit - b.levellimit) * b.totalperformance / 10000,0)
		from #temp_agentuser as a with (nolock)
			inner join #temp_treeperformance as b on a.userid = b.userid
		where
			a.relations like '' + @upperuserrelationstr + '%'
			and @countuserlevel = a.stagelevel

		select	@totalteambonus = sum(isnull(teambonusamount,0))
		from #lowerusers
		
		update #temp_treeperformance
		set	TeamBonusAmount = @totalteambonus
		where
			userid = @countuserid

		set @minno = @minno + 1	
	end

	/*  更新直推業績  */
	update #temp_treeperformance
	set	directBonusAmount = round((levellimit * (directperformance)) / 10000,0)


	/*  寫入紀錄  */
	if exists (select top 1 1 from [dbo].[Unlimit_BonusReport] with (nolock) where logdate = @LogDate)
	begin
		delete from [dbo].[Unlimit_BonusReport] where logdate = @startdate
	end


	/*  每日結算  */
	insert into [dbo].[Unlimit_BonusReport] ([UserID],[LogDate],[TotalPerformance],[TeamPerformance],[DirectPerformance],[TeamBonusAmount],[DirectBonusAmount],[LevelLimit],[TopUserLevelLimit],[Status])
	select	[UserID] = userid,
			[LogDate] = @startdate,
			[TotalPerformance] = isnull([TotalPerformance],0),
			[TeamPerformance] = isnull([TeamPerformance],0),
			[DirectPerformance] = isnull(directperformance,0),
			[TeamBonusAmount] = isnull([TeamBonusAmount],0),
			[DirectBonusAmount] = isnull([DirectBonusAmount],0),
			[LevelLimit] = isnull([LevelLimit],0),
			[TopUserLevelLimit] = isnull([TopUserLevelLimit],0),
			[Status] = 0
	from #temp_treeperformance
	where
		(isnull([TeamBonusAmount],0) + isnull([DirectBonusAmount],0)) > 0


	/*  每週一結算  */
	if(datepart(weekday,@enddate) = 2)
	begin
		
		declare @countsdate datetime = dateadd(day,-7,dateadd(week,datediff(week,0,getdate()),0))
		declare @countedate datetime = dateadd(day,0,dateadd(week,datediff(week,0,getdate()),0))
		declare @userpool table (uid int,pa bigint)


		insert into @userpool
		select	uid = userid,
				poolamount = sum(isnull(DirectBonusAmount,0) + isnull(TeamBonusAmount,0))
		from [dbo].[Unlimit_BonusReport] with (nolock)
		where
			[LogDate] >= @countsdate
			and [LogDate] < @countedate
		group by userid


		update [dbo].[Unlimit_Pool]
		set [PoolAmount] = [PoolAmount] + a.pa
		from @userpool as a
		where
			userid = a.uid
	end

	drop table #temp_agentuser
	drop table #temp_moneylog
	drop table #lowerusers
	drop table #temp_treeperformance
	drop table #upperuser
END
