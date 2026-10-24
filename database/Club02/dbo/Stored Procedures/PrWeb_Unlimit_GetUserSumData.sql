
-- =============================================
-- Author:  jason
-- Create date: 20190501
-- Description: 后台  无极限-标头
--	2019/06/04	Jason	WeekTotalPerformance調整為不論是否達標均匯總	
--  2019/0714	CCC     每周啟始日期修正
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Unlimit_GetUserSumData]
(
	@intAdminID int = null,
	@intUserID int = null,
	@chvMsg	varchar(100) OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON
	
	--declare @logsdate datetime = dateadd(day,0,dateadd(week,datediff(week,0,getdate()),0))
	declare @logsdate datetime = dbo.Fn_StartOfWeek1(getdate())
	declare @logedate datetime = dateadd(day,6,@logsdate)
	declare @admindirectuser int
	declare @userlevelstr	varchar(max)
	declare @WeekTotalPerformance bigint
	declare @TotalPerformance bigint


	if(@intAdminID is null and @intUserID is null)
	begin
		set @chvMsg = '操作错误 : 用户不存在'
		return -1
	end

	if (@intAdminID is not null and @intUserID is null) and not exists (select top 1 1 from [dbo].[AgentAdminUser] with (nolock) where id = @intAdminID)
	begin
		set @chvMsg = '操作错误 : 用户不存在'
		return -1
	end

	if (@intAdminID is null and @intUserID is not null) and not exists (select top 1 1 from [dbo].[Unlimit_UserInfo] with (nolock) where userid = @intUserID)
	begin
		set @chvMsg = '操作错误 : 用户不存在'
		return -1
	end


	/*  用户平台  */
	declare @temp_platformid table (platformid int)

	if(@intAdminID is not null)
	begin
		insert into @temp_platformid
		select platformid
		from [dbo].[AgentPlatformRelations] with (nolock)
		where
			[AdminID] = @intAdminID

		select	@admindirectuser = count(1) from @temp_platformid as a
					inner join [dbo].[Unlimit_UserInfo] as b with (nolock) on a.platformid = b.PlatformID
			where topuserid is null
		set @admindirectuser = isnull(@admindirectuser,0)
	end
	else
	begin
		insert into @temp_platformid
		select	platformid
		from [dbo].[Unlimit_UserInfo] with (nolock)
		where
			userid = @intUserID

		select	@userlevelstr = relations from [dbo].[Unlimit_UserInfo] with (nolock) where userid = @intUserID
	end


	/*  取资料 */
	if(@intUserID is not null)
	begin
		select	@WeekTotalPerformance = sum(case when b.logdate >= @logsdate and b.logdate <= @logedate then isnull([FlowAmount],0) else 0 end),
				@TotalPerformance = sum(isnull([FlowAmount],0))
		from (
				select userid, CreateTime
				from [dbo].[Unlimit_UserInfo] with (nolock)
				where
					relations like '' + @userlevelstr + '%'
					and userid <> @intUserID
			) as a
			inner join LogDB.[Stat].[StatisticsUserAmount_Day] as b with (nolock) on a.userid = b.userid
			join @temp_platformid as c on b.PlatformID = c.PlatformID
		where
			convert(varchar(10),a.CreateTime,23) <= b.logdate
	end
	else
	begin		
		select	@WeekTotalPerformance = sum(case when c.logdate >= @logsdate and c.logdate <= @logedate then isnull([FlowAmount],0) else 0 end),
				@TotalPerformance = sum(isnull([FlowAmount],0))
		from @temp_platformid as a
			join [dbo].[Unlimit_UserInfo] as b with (nolock) on a.PlatformID = b.PlatformID
			inner join LogDB.[Stat].[StatisticsUserAmount_Day] as c  with (nolock)  on b.userid = c.userid
		where
			convert(varchar(10),b.CreateTime,23) <= c.logdate			
	end

	set @WeekTotalPerformance = isnull(@WeekTotalPerformance,0)


	declare @temp_sumdata table ([PoolAmount] bigint,Totaluser int,[SubDirectUser] int,[TotalPerformance] bigint,WeekTotalPerformance bigint)
	insert into @temp_sumdata
	select	[PoolAmount] = sum(isnull([PoolAmount],0)),
			Totaluser = case when @intUserID is null then count(c.userid)
								else sum(isnull(d.[SubUser],0)) + sum(isnull(d.[SubDirectUser],0)) end,
			[SubDirectUser] = case when @intUserID is null then @admindirectuser
								else sum(isnull(d.[SubDirectUser],0)) end,
			--[TotalPerformance] = sum(isnull(e.[TotalPerformance],0)),
			[TotalPerformance] = @TotalPerformance,
			--WeekTotalPerformance = sum(isnull(e.WeekTotalPerformance,0))
			WeekTotalPerformance = @WeekTotalPerformance
	from @temp_platformid as a
		inner join [dbo].[Unlimit_UserInfo] as b with (nolock) on a.platformid = b.platformid
		inner join [dbo].[Unlimit_Pool] as c with (nolock) on b.userid = c.userid
		inner join [dbo].[Unlimit_Promotion] as d with (nolock) on c.userid = d.userid
		--left join (
		--			select	userid,
		--					[TotalPerformance] = sum([TotalPerformance]),
		--					WeekTotalPerformance = sum(case when logdate >= @logsdate and logdate < @logedate then isnull([TotalPerformance],0) else 0 end)
		--			from [dbo].[Unlimit_BonusReport] with (nolock)
		--			group by userid 
		--		) as e on d.userid = e.userid
	where
		(@intUserID is null or b.userid = @intUserID)
	
	select	[PoolAmount] = isnull([PoolAmount],0),
			Totaluser = isnull(Totaluser,0),
			[SubDirectUser] = isnull([SubDirectUser],0),
			[TotalPerformance] = isnull([TotalPerformance],0),
			WeekTotalPerformance = isnull(@WeekTotalPerformance,0)
	from @temp_sumdata


	set @chvMsg = '操作成功'
	return 1
END




