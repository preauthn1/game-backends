
-- =============================================
-- Author:  jason
-- Create date: 20190502
-- Description: 后台  无极限- 我的预提纪录
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Unlimit_GetUserPreConvertInfo]
(
	@intAdminID int = null,
	@intUserID int = null,
	@intTargetID int = null,
	@lngOrderID bigint = null,
	@dtStartTime	datetime = null,
	@dtEndTime		datetime = null,
	@chvMsg varchar(100) OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON


	declare @StartDate datetime = convert(varchar(10),isnull(@dtStartTime,'1970-01-01'),23)
	declare @EndDate datetime = dateadd(day,1,convert(varchar(10),isnull(@dtEndTime,'2100-01-01'),23))
	declare @admindirectuser int
	declare @userlevelstr varchar(max)


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


	/*  取资料 */
	select	[OrderID] = c.ID,
			b.userid,
			b.nickname,
			a.platformid,
			a.platformname,
			PreConvertAmount = c.[PoolAmount],
			[LastDayBonusAmount] = isnull(d.[TeamBonusAmount],0) + isnull(d.[DirectBonusAmount],0),
			c.status,
			c.[CreateTime],
			c.[Updatetime],
			Updater = [UpdateUser]
	from @temp_platformid as a
		inner join [dbo].[Unlimit_UserInfo] as b with (nolock) on a.platformid = b.platformid and (@intTargetID is null or @intTargetID = b.userid)
		inner join [dbo].[Unlimit_WithDrawLog] as c with (nolock) on b.userid = c.userid
		left join [dbo].[Unlimit_BonusReport] as d with (nolock) on b.userid = d.userid and convert(varchar(10),c.[CreateTime],23) = convert(varchar(10),dateadd(day,1,[LogDate]),23)
	where
		(@intUserID is null or b.userid = @intUserID)
		and c.WithdrawType = 1
		and (@lngOrderID is null or @lngOrderID = c.ID)
		and c.CreateTime >= @StartDate and c.CreateTime < @EndDate


	set @chvMsg = '操作成功'
	return 1
END




