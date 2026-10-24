-- =============================================
-- Author:  jason
-- Create date: 2019/04/13
-- Description: 全民代理后台 查询下线数据
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_SubUserInfo_Get]
(
	@intAdminID			int = null,			--管理者ID
	@intUserID			int = null,			--用户 ID
	--@intSubUserLevel	int = 1,			--下线阶层 默认显示下线第一层
	@intSubUserID		int = null,			--下线用户ID
	@dtStartTime		datetime = null,	--查询用户加入 开始时间
	@dtEndTime			datetime = null,	--查询用户加入 结束时间
	@chvMsg			varchar(250)	OUTPUT
)
AS
BEGIN

	set nocount on

	if(@intAdminID is null and @intUserID is null)
	begin
		set @chvMsg = '操作失敗 : 查無資料'
		return -1
	end

	declare @StartTime	datetime = ISNULL(@dtStartTime,'1970-01-01 00:00:00')
	declare @EndTime	datetime = ISNULL(@dtEndTime,'2100-01-01 00:00:00')

	declare @temp_buser	table (userid int)
	declare @temp_cuser	table (userid int)
	declare @temp_duser	table (userid int)


	if (@intAdminID is not null)
	begin

		/*  取得管理者可讀取平台  */
		declare @adminplatformid table (platformid int)

		insert into @adminplatformid
		select	distinct platformid from [dbo].[AgentPlatformRelations] with (nolock) where adminid = @intAdminID

		insert into @temp_buser
		select userid from [dbo].[UserAgentInfo] as a with (nolock)
			inner join @adminplatformid as b on a.PlatformID = b.PlatformID
		where TopUserID is null

		insert into @temp_cuser
		select userid from [dbo].[UserAgentInfo] as a with (nolock)
			inner join @adminplatformid as b on a.PlatformID = b.PlatformID
		where TopUserID in (select userid from @temp_buser)

		insert into @temp_duser
		select userid from [dbo].[UserAgentInfo] as a with (nolock)
			inner join @adminplatformid as b on a.PlatformID = b.PlatformID
		where TopUserID in (select userid from @temp_cuser)


		select	b.[UserID],
				[UserLevel],
				[SumTaxTotal],
				[SumTaxBonus],
				[SumTaxConvert],
				[SumTaxPerformed],
				[UnWithDrawTotal],
				[CreateTime],
				[Status],
				[TopUserID],
				[LastUpdatetime],
				b.[PlatformID],
				[AreaID],
				[NickName]
		from @adminplatformid as a
			inner join [dbo].[UserAgentInfo] as b with (nolock) on a.platformid = b.PlatformID
		where
				(@intSubUserID is null or b.[UserID] = @intSubUserID)
				and [CreateTime] >= @StartTime
				and [CreateTime] < @EndTime


		--/*  查询第二级下线  */
		--if (@intSubUserLevel = 2)
		--begin
		--	select	a.[UserID],
		--			[UserLevel],
		--			[SumTaxTotal],
		--			[SumTaxBonus],
		--			[SumTaxConvert],
		--			[SumTaxPerformed],
		--			[UnWithDrawTotal],
		--			[CreateTime],
		--			[Status],
		--			[TopUserID],
		--			[LastUpdatetime],
		--			[PlatformID],
		--			[AreaID],
		--			[NickName]
		--	from [dbo].[UserAgentInfo] as a with (nolock)
		--		inner join @temp_cuser as b on a.userid = b.userid
		--	where
		--		(@intSubUserID is null or a.[UserID] = @intSubUserID)
		--		and [CreateTime] >= @StartTime
		--		and [CreateTime] < @EndTime
		--end


		--/*  查询第三级下线  */
		--if (@intSubUserLevel = 3)
		--begin
		--	select	a.[UserID],
		--			[UserLevel],
		--			[SumTaxTotal],
		--			[SumTaxBonus],
		--			[SumTaxConvert],
		--			[SumTaxPerformed],
		--			[UnWithDrawTotal],
		--			[CreateTime],
		--			[Status],
		--			[TopUserID],
		--			[LastUpdatetime],
		--			[PlatformID],
		--			[AreaID],
		--			[NickName]
		--	from [dbo].[UserAgentInfo] as a with (nolock)
		--		inner join @temp_duser as b on a.userid = b.userid
		--	where
		--		(@intSubUserID is null or a.[UserID] = @intSubUserID)
		--		and [CreateTime] >= @StartTime
		--		and [CreateTime] < @EndTime
		--end
	end


	if (@intUserID is not null)
	begin
		/*  用户不存在或是带错值 直接返回  */
		--if not exists (select top 1 1 from [Club02].[dbo].[UserAgentInfo] with (nolock) where userid = @intUserID) or (@intSubUserLevel not in (1,2,3))
		if not exists (select top 1 1 from [Club02].[dbo].[UserAgentInfo] with (nolock) where userid = @intUserID)
		begin
			SELECT	TOP 0
					[UserID],
					[UserLevel],
					[SumTaxTotal],
					[SumTaxBonus],
					[SumTaxConvert],
					[SumTaxPerformed],
					[UnWithDrawTotal],
					[CreateTime],
					[Status],
					[TopUserID],
					[LastUpdatetime],
					[PlatformID],
					[AreaID],
					[NickName]
			FROM [Club02].[dbo].[UserAgentInfo] with (nolock)
			set @chvMsg = '操作完成'
			return 1
		end

		insert into @temp_buser
		select userid from [dbo].[UserAgentInfo] with (nolock)
		where TopUserID = @intUserID

		insert into @temp_cuser
		select userid from [dbo].[UserAgentInfo] with (nolock)
		where TopUserID in (select userid from @temp_buser)

		insert into @temp_duser
		select userid from [dbo].[UserAgentInfo] with (nolock)
		where TopUserID in (select userid from @temp_cuser)


		--/*  查询第一级下线  */
		--if (@intSubUserLevel = 1)
		--begin
		--	select	a.[UserID],
		--			[UserLevel],
		--			[SumTaxTotal],
		--			[SumTaxBonus],
		--			[SumTaxConvert],
		--			[SumTaxPerformed],
		--			[UnWithDrawTotal],
		--			[CreateTime],
		--			[Status],
		--			[TopUserID],
		--			[LastUpdatetime],
		--			[PlatformID],
		--			[AreaID],
		--			[NickName]
		--	from [dbo].[UserAgentInfo] as a with (nolock)
		--		inner join @temp_buser as b on a.userid = b.userid
		--	where
		--		(@intSubUserID is null or a.[UserID] = @intSubUserID)
		--		and [CreateTime] >= @StartTime
		--		and [CreateTime] < @EndTime
		--end


		--/*  查询第二级下线  */
		--if (@intSubUserLevel = 2)
		--begin
		--	select	a.[UserID],
		--			[UserLevel],
		--			[SumTaxTotal],
		--			[SumTaxBonus],
		--			[SumTaxConvert],
		--			[SumTaxPerformed],
		--			[UnWithDrawTotal],
		--			[CreateTime],
		--			[Status],
		--			[TopUserID],
		--			[LastUpdatetime],
		--			[PlatformID],
		--			[AreaID],
		--			[NickName]
		--	from [dbo].[UserAgentInfo] as a with (nolock)
		--		inner join @temp_cuser as b on a.userid = b.userid
		--	where
		--		(@intSubUserID is null or a.[UserID] = @intSubUserID)
		--		and [CreateTime] >= @StartTime
		--		and [CreateTime] < @EndTime
		--end


		--/*  查询第三级下线  */
		--if (@intSubUserLevel = 3)
		--begin
		--	select	a.[UserID],
		--			[UserLevel],
		--			[SumTaxTotal],
		--			[SumTaxBonus],
		--			[SumTaxConvert],
		--			[SumTaxPerformed],
		--			[UnWithDrawTotal],
		--			[CreateTime],
		--			[Status],
		--			[TopUserID],
		--			[LastUpdatetime],
		--			[PlatformID],
		--			[AreaID],
		--			[NickName]
		--	from [dbo].[UserAgentInfo] as a with (nolock)
		--		inner join @temp_duser as b on a.userid = b.userid
		--	where
		--		(@intSubUserID is null or a.[UserID] = @intSubUserID)
		--		and [CreateTime] >= @StartTime
		--		and [CreateTime] < @EndTime
		--end
	end

	set @chvMsg = '操作完成'
	return 1
END

