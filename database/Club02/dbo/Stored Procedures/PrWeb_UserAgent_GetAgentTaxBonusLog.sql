-- =============================================
-- Author:  jason
-- Create date: 2019/04/14
-- Description: 全民代理后台 查询今日税收
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_GetAgentTaxBonusLog]
(
	@intAdminID int = null,
	@intUserID	int = null,
	@dtStartTime	datetime = null,
	@dtEndTime		datetime = null,
	@chvMsg		varchar(100) OUTPUT
)
AS
BEGIN

	set nocount on

	declare @StartTime datetime = isnull(@dtStartTime,'1970-01-01')
	declare @EndTime datetime = isnull(@dtEndTime,'2100-01-01')
	set @StartTime = dateadd(day,0,datediff(day,0,@StartTime))
	set @EndTime = dateadd(day,0,datediff(day,0,@EndTime))


	declare @Temp_PlatformID table (PlatformID int)


	if(@intAdminID is null)
	begin
		set @chvMsg	= '操作失败 : 管理者无权限'
		return -1
	end


	--捞出该用户可读取的全部平台
	if(@intAdminID is not null)
	begin
		INSERT INTO @Temp_PlatformID Select PlatformId From [dbo].[AgentPlatformRelations] where AdminID=@intAdminID
	end

	if(@intUserID is not null)
	begin
		INSERT INTO @Temp_PlatformID
		select [PlatformID] from [dbo].[UserAgentInfo] with (nolock) where userid = @intUserID
	end


	--/*  取出三层用户  */
	--declare @temp_buser table (userid int)
	--declare @temp_cuser table (buser int ,userid int)
	--declare @temp_duser table (buser int, userid int)

	--insert into @temp_buser
	--select userid from [dbo].[UserAgentInfo] as a with (nolock)
	--	inner join @Temp_PlatformID as b on a.platformid = b.platformid --where TopUserID = @intUserID

	--	insert into @temp_cuser
	--	select	buser = b.userid,
	--			a.userid
	--	from [dbo].[UserAgentInfo] as a with (nolock)
	--		inner join @temp_buser as b on a.TopUserID = b.userid

	--	insert into @temp_duser
	--	select	buser = c.buser,
	--			a.userid
	--	from [dbo].[UserAgentInfo] as a with (nolock)
	--		inner join @temp_cuser as c on a.TopUserID = c.userid

	--	/*  计算B级税收收益  */
	--	declare @bonus table (logdate datetime,userid int,bonus bigint)
	--	insert into @bonus
	--	select	b.[LogDate],
	--			a.userid,
	--			amount = sum(amount)
	--	from @temp_buser as a
	--		inner join [dbo].[UserAgentBonus] as b with (nolock) on a.userid = b.fromuserid
	--	where
	--		b.userid = @intUserID
	--	group by b.[LogDate],a.userid

	
	--	/*  CD税收收益  */
	--	declare @temp_bcduser table (userid int,cduserid int)
	--	declare @temp_cdbonus table (logdate datetime,userid int,bonus bigint)

	--	insert into @temp_cdbonus
	--	select	b.[LogDate],
	--			a.buser,
	--			amount = sum(amount)
	--	from (
	--			select buser,userid from @temp_cuser
	--			union
	--			select buser,userid from @temp_duser
	--		) as a
	--		inner join [dbo].[UserAgentBonus] as b with (nolock) on a.userid = b.fromuserid
	--	where
	--		b.userid in (	select userid from @temp_buser
	--						union
	--						select userid from @temp_cuser
	--						union
	--						select @intUserID)
	--	group by b.[LogDate],a.buser


	--	/*  暂存统计  */
	--	declare @temp_totaldata table (logdate datetime,B_TaxBonusAmount bigint,CD_TaxBonusAmount bigint,TotalTaxBonusAmount bigint)
	--	insert into @temp_totaldata
	--	select	distinct
	--			a.logdate,
	--			B_TaxBonusAmount = sum(case when a.logdate = e.logdate then isnull(e.bonus,0) else 0 end),
	--			CD_TaxBonusAmount = sum(case when a.logdate = f.logdate  then isnull(f.bonus,0) else 0 end),
	--			TotalTaxBonusAmount = sum(isnull(e.bonus,0)) + sum(isnull(f.bonus,0))
	--	from (
	--			select distinct logdate,userid from @bonus
	--			union
	--			select distinct logdate,userid from @temp_cdbonus
	--		) as a
	--		left join @bonus as e on a.logdate = e.logdate and a.userid = e.userid
	--		left join @temp_cdbonus as f on a.logdate = f.logdate and a.userid = f.userid
	--	where
	--		a.LogDate >= @dtStartTime
	--		and a.LogDate <= @dtEndTime
	--	group by a.logdate

	--/*  回传明细  */
	--	select	distinct
	--			[LogDate],
	--			B_TaxBonusAmount = isnull(B_TaxBonusAmount,0),
	--			CD_TaxBonusAmount = isnull(CD_TaxBonusAmount,0),
	--			[TotalTaxBonusAmount] = isnull(B_TaxBonusAmount,0) + isnull(CD_TaxBonusAmount,0)
	--	from @temp_totaldata
	--	order by [LogDate] desc

	
	/*  回传明细  */
	select	logdate,
			b.userid,
			Amount = sum(Amount)
	from @Temp_PlatformID as a
		inner join [dbo].[UserAgentInfo] as b with (nolock) on a.platformid = b.platformid
		inner join [dbo].[UserAgentBonus] as c with (nolock) on b.userid = c.userid
	where
		LogDate >= @StartTime
		and LogDate <= @EndTime
	group by logdate,b.userid
	order by logdate desc


	set @chvMsg	= '操作成功'

	return 1
END






