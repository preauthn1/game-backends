-- =============================================
-- Author:  jason
-- Create date: 2019/04/18
-- Description: 全民代理后台 新增玩家(净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_NetProfitAgent_GetSubUserList]
(
	@intAdminID		int = null,
	@intUserID		int = null,		--用户 ID
	@intStageLevel	int = 0,
	@intTargetID	int = null,
	@intSubUserID	int = null,
	@intStatus		int = null,
	@dtStartTime	datetime = null,
	@dtEndTime		datetime = null,
	@chvMsg		varchar(100)	OUTPUT
)
AS
BEGIN

	set nocount on

	declare @StartDate datetime = convert(varchar(10),isnull(@dtStartTime,'1970-01-01'),23)
	declare @EndDate datetime = dateadd(day,1,convert(varchar(10),isnull(@dtEndTime,'2100-01-01'),23))
	declare @userlevel int

	--平台暂存
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT,PlatformName varchar(50),AreaID int,AreaName varchar(50))

	declare @userlevelstr varchar(max)

	set @intStageLevel = isnull(@intStageLevel,1)


	if(@intAdminID is null and @intUserID is null)
	begin
		set @chvMsg = '操作错误 : 用户不存在'
		return -1
	end

	if(@intStatus is not null) and (@intStatus not in (1,2))
	begin
		set @chvMsg = '操作错误 : 查询类型错误'
		return -1
	end


	declare @adminstatus tinyint
	select	@adminstatus = status
	from [dbo].[AgentAdminUser] with (nolock)
	where
		id = @intAdminID 


	declare @userstatus tinyint
	select	@userstatus = status
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		userid = @intUserID


	declare @targetidlevel int
	declare @useridlevel int
	declare @subuseridlevel int
	declare @targetidrelationstr varchar(max)
	declare @useridrelationstr varchar(max)
	declare @subuseridrelationstr varchar(max)

	if(@intSubUserID is null and (@intTargetID is null or len(@intTargetID) > 10))
	begin
		set @intSubUserID = @intUserID
	end

	select	@targetidlevel = stagelevel,
			@targetidrelationstr = Relations
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		userid = @intTargetID

	select	@useridlevel = stagelevel,
			@useridrelationstr = Relations
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		userid = @intUserID

	select	@subuseridlevel = stagelevel,
			@subuseridrelationstr = Relations
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		userid = @intSubUserID

	declare @temp_users table (userid int)

	insert into @temp_users
	select cast(value as int)
	from [dbo].[fn_Split](isnull(@subuseridrelationstr,@targetidrelationstr),',')

	if(@intUserID is not null) and (@targetidlevel < @useridlevel) or (@subuseridlevel < @useridlevel)
	begin
		set @chvMsg = '操作错误 : 不可越权查询'
		return -1
	end

	if (@intUserID is not null) and not exists (select top 1 1 from @temp_users where userid = @intUserID)
	begin
		set @chvMsg = '操作错误 : 不可跨线查询'
		return -1
	end


	if((@adminstatus is null or @adminstatus <> 1) and (@userstatus is null or @userstatus < 0))
	begin
		set @chvMsg = '操作错误 : 用户已停权'
		return -1
	end


	if(@intAdminID is not null)
	begin
		--捞出该用户可读取的全部平台
		INSERT INTO @Temp_PlatformID
		Select	B.PlatformID,
				C.Name,
				B.AreaID,
				B.AreaName
		From [dbo].[AgentPlatformRelations] as a
			inner join CenterDB.area.area as b with (nolock) on a.PlatformID = B.PlatformID
			inner join CenterDB.area.[platform] as c with (nolock) on b.PlatformID = c.PlatformID
		where
			a.AdminID = @intAdminID

		set @userlevel = @intStageLevel
	end
	else if (@intUserID is not null)
	begin
		--捞出该用户可读取的全部平台
		INSERT INTO @Temp_PlatformID
		Select	B.PlatformID,
				C.Name,
				B.AreaID,
				B.AreaName
		From [dbo].[NetProfitAgent_UserInfo] as a with (nolock)
			inner join CenterDB.area.area as b with (nolock) on a.PlatformID = B.PlatformID
			inner join CenterDB.area.[platform] as c with (nolock) on b.PlatformID = c.PlatformID
		where
			userid = @intUserID

	end


	/*  先取出对应的用户关联  */
	select	@userlevelstr = [Relations]
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		userid = isnull(isnull(@intSubUserID,@intTargetID),@intUserID)

	select	@userlevel = StageLevel + @intStageLevel
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		userid = isnull(isnull(@intSubUserID,@intTargetID),@intUserID)

	--set @userlevelstr = case	when @intSubUserID is null then isnull(@userlevelstr,'') else isnull(@userlevelstr,'') + ',' end

	if(@intSubUserID is null and @intTargetID is null) and (@intUserID is not null or @intAdminID is not null)
	begin
		set @userlevelstr = @userlevelstr + ','
	end
	else if (@intTargetID is not null) and (@intUserID is not null or @intAdminID is not null)
	begin
		set @userlevelstr = @userlevelstr
	end
	else if (@intSubUserID is not null) and (@intUserID is not null or @intAdminID is not null)
	begin
		set @userlevelstr = @userlevelstr + ','
	end

	set @userlevelstr = isnull(@userlevelstr,'')

	declare @temp_userinfo table (
		userid int,CreateTime datetime,StageLevel int,status int,TopUserID int,platformid int,areaid int,platformname varchar(100),areaname varchar(100),
		[PoolAmount] bigint,[WithDrawAmount] bigint,SumSubUser int,SumSubAgent int,Rate int,SumSubChargeAmount bigint,SumSubConvertAmount bigint,
		ConvertRate float,upperuserid int,Memo nvarchar(100), SumFlowAmount bigint
	)

	insert into @temp_userinfo
	select	distinct a.userid,
				a.CreateTime,
				StageLevel = cast(a.StageLevel as int),
				status = cast(status as int),	--0:仅加入但无上线会员  1:一般玩家 2:代理
				a.TopUserID,
				a.platformid,
				a.areaid,
				x.areaname,
				y.name,
				[PoolAmount] = isnull([PoolAmount],0),
				[WithDrawAmount] = isnull([WithDrawAmount],0),
				SumSubUser = isnull(a.SumSubUser,0),
				SumSubAgent = isnull(a.SumSubAgent,0),
				Rate = cast(isnull(a.rate,0) as int),  -- 最大1000
				[SumSubChargeAmount] = isnull(a.[SumSubChargeAmount],0),
				[SumSubConvertAmount] = isnull(a.[SumSubConvertAmount],0),
				[ConvertRate] = case when isnull(a.[SumSubChargeAmount],0) > 0 then round(cast(isnull(a.[SumSubConvertAmount],0) as float) / isnull(a.[SumSubChargeAmount],0),2,1) else 0 end,
				upperuserid = cast(case when TopUserID is not null then substring(relations,1,charindex(',',relations)-1) else null end as int),
				a.Memo,
				u.SumFlowAmount
		from [dbo].[NetProfitAgent_UserInfo] as a with (nolock)
			inner join @Temp_PlatformID as b on a.PlatformID = b.PlatformID
			left join [dbo].[NetProfitAgent_Pool] as c with (nolock) on a.userid = c.UserID
			left join centerdb.area.Area as x with (nolock) on a.PlatformID = x.PlatformID and a.AreaID = x.AreaID
			left join centerdb.area.platform as y with (nolock) on a.PlatformID = y.PlatformID
			left join CenterDB.Users.UserInfo as u with (nolock) on a.UserID = u.UserID
		where
			[Relations] like '' + @userlevelstr + '%'
			and StageLevel >= @userlevel
			and (@intStatus is null or @intStatus = status)
			and a.CreateTime >= @StartDate
			and a.CreateTime < @EndDate


	select	*
	from @temp_userinfo
	where
		(@intTargetID is null or @intTargetID = userid)
	order by StageLevel asc


	set @chvMsg = '操作完成 : 查询完成'
	return 1	
END







