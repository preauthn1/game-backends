
-- =============================================
-- Author:  jason
-- Create date: 20190501
-- Description: 后台  无极限-用户信息
--	2019/06/04	Jason		新增Memo欄位/當周總業績(TotalPerformance)
--	2019/06/26	Jason		查詢效能調整
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Unlimit_GetUserInfo]
(
	@intAdminID int = null,
	@intUserID int = null,
	@intTargetID int = null,
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intPageCount	INT = 0		OUTPUT,
	@chvMsg varchar(100) OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON


	declare @admindirectuser int
	declare @userlevelstr varchar(max)
	DECLARE	@StartRow			INT = @intPageSize * (@intPageIndex -1) + 1
	DECLARE	@EndRow				INT = @intPageSize * @intPageIndex


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
	declare @temp_platformid table (platformid int,platformname varchar(32))

	if(@intAdminID is not null)
	begin
		insert into @temp_platformid
		select	a.platformid,
				platformname = b.Name
		from [dbo].[AgentPlatformRelations] as a with (nolock)
			inner join centerdb.area.platform as b with (nolock) on a.platformid = b.platformid
		where
			[AdminID] = @intAdminID

		select	@admindirectuser = count(1) from [dbo].[Unlimit_UserInfo] with (nolock) where topuserid is null
		set @admindirectuser = isnull(@admindirectuser,0)

		set @userlevelstr = ''
	end
	else
	begin
		insert into @temp_platformid
		select	a.platformid,
				platformname = b.Name
		from [dbo].[Unlimit_UserInfo] as a with (nolock)
			inner join centerdb.area.platform as b with (nolock) on a.platformid = b.platformid
		where
			userid = @intUserID

		select	@userlevelstr = relations from [dbo].[Unlimit_UserInfo] with (nolock) where userid = @intUserID
	end

	select	@intPageCount = COUNT(1) 
	from (
			select	userid
			from (
					select	b.userid,
							b.Relations
					from @temp_platformid as a
						inner join [dbo].[Unlimit_UserInfo] as b with (nolock) on a.platformid = b.platformid and (@intTargetID is null or @intTargetID = b.userid)
				) as a
			where
				[Relations] like '' + @userlevelstr + '%'
				and (@intUserID is null or userid <> @intUserID)
		) as a

	declare @temp_users table (rowno int,userid int,platformid int,relations varchar(max),stagelevel int)

	insert into @temp_users
	select	rowno,
			userid,
			PlatformID,
			Relations,
			stagelevel
	from (
			select	row_number() over (order by PoolAmount desc, userid asc) as rowno,
					userid,
					PlatformID,
					Relations,
					stagelevel
			from (
					select	b.userid,
							b.PlatformID,
							b.Relations,
							b.stagelevel,
							[PoolAmount] = isnull([PoolAmount],0) - isnull([WithDrawAmount],0)
					from @temp_platformid as a
						inner join [dbo].[Unlimit_UserInfo] as b with (nolock) on a.platformid = b.platformid and (@intTargetID is null or @intTargetID = b.userid)
						left join [dbo].[Unlimit_Pool] as e with (nolock) on b.userid = e.userid

				) as a
			where
				[Relations] like '' + @userlevelstr + '%'
				and (@intUserID is null or userid <> @intUserID)
		) as a
	WHERE
		rowno BETWEEN @StartRow AND @EndRow

	declare @temp_userperformance table (userid int,TotalPerformance bigint)
	declare @minno int
	declare @maxno int
	declare @startdate datetime = dateadd(week,datediff(week,'',getdate()),'')
	

	select	@minno = min(rowno),
			@maxno = max(rowno)
	from @temp_users

	while(@minno <= @maxno)
	begin
		declare @userstr varchar(max) = null
		declare @userid int = null
		declare @userlevel int = null

		select	@userstr = Relations,
				@userid = userid,
				@userlevel = stagelevel
		from @temp_users
		where
			rowno = @minno

		insert into @temp_userperformance
		select	@userid as userid,
				TotalPerformance = sum(isnull(b.[FlowAmount],0))
		from [dbo].[Unlimit_UserInfo] as a with (nolock)
			left join LogDB.[Stat].[StatisticsUserAmount_Day] as b with (nolock) on a.userid = b.userid and [LogDate] >= @startdate
		where
			a.relations like '' + @userstr + '%'
			and a.StageLevel > @userlevel
		group by a.userid

		set @minno = @minno + 1			
	end

	/*  取资料 */
	select * 
	  from (
	select	b.userid,
			b.NickName,
			b.platformid,
			a.platformname,
			b.StageLevel,
			b.status,
			topuserid = cast(case when b.relations is not null and b.stagelevel > 1 then substring(b.relations,1,charindex(',',b.relations)-1) else 0 end as int),
			upperuserid = topuserid,
			[SubDirectUser],
			[SubUser],
			[TotalPerformance] = isnull(d.[TotalPerformance],0),
			[PoolAmount] = isnull([PoolAmount],0) - (isnull([WithDrawAmount],0) - isnull(PreConvertAmount, 0)),
			[WithDrawAmount] = isnull([WithDrawAmount],0) - isnull(PreConvertAmount, 0),
			b.CreateTime,
			b.Memo
			
	from @temp_platformid as a
		inner join @temp_users as z on a.platformid = z.platformid
		inner join [dbo].[Unlimit_UserInfo] as b with (nolock) on z.userid = b.userid and (@intTargetID is null or @intTargetID = b.userid)
		inner join [dbo].[Unlimit_Promotion] as c with (nolock) on b.userid = c.userid
		left join (
					select	userid,
							[TotalPerformance] = sum(isnull([TotalPerformance],0))
					from @temp_userperformance
					group by userid
				) as d on b.userid = d.userid
		left join [dbo].[Unlimit_Pool] as e with (nolock) on b.userid = e.userid
		left join (
		            select A.[UserID],
		            	   PreConvertAmount = SUM([WithDrawAmount])  
		              from [Club02].[dbo].[Unlimit_WithDrawLog] A with (nolock)
		              join @temp_users B on A.UserID = B.userid	 
		             where WithdrawType  = 1
		               and status = 1
		              group by A.[UserID]
				  ) as f on z.userid = f.UserID
	) as a
	order by PoolAmount desc

	set @chvMsg = '操作成功'
	return 1
END




