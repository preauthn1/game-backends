-- =============================================
-- Author:  jason
-- Create date: 2019/04/15
-- Description: 全民代理后台 设定下线比例(净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_NetProfitAgent_UserRate_Update]
(
	@intAdminID		int = null,
	@intUserID		int = null,			--用户 ID
	@intTargetID	int,
	@rate			int,
	@chvMsg			varchar(100)		OUTPUT
)
AS
BEGIN

	set nocount on

	if(@intAdminID is null and @intUserID is null)
	begin
		set @chvMsg = '操作错误 : 用户不存在'
		return -1
	end

	if (@intAdminID is null) and not exists (select top 1 1 from [dbo].[NetProfitAgent_UserInfo] with (nolock) where userid = @intTargetID and [TopUserID] = @intUserID)
	begin
		set @chvMsg = '操作错误 : 用户无权限'
		return -1
	end

	if (@rate > 1000 or @rate < 0)
	begin
		set @chvMsg = '操作错误 : 比例设定错误'
		return -1
	end
	
	declare @topuserrate int = 0
	declare @targetrage int
	declare @userplatformid int
	declare @topuserlv int
	declare @targetuserlv int
	declare @targerplatformid int
	declare @topuserid int

	/*  取系统比例或上级比例  */
	select	@topuserrate = Rate,
			@userplatformid = platformid,
			@topuserlv = StageLevel
	from [dbo].[NetProfitAgent_UserInfo] with (nolock) where userid = @intUserID

	select	@targetuserlv = StageLevel,
			@targerplatformid = platformid,
			@topuserid = TopUserID
	from [dbo].[NetProfitAgent_UserInfo] with (nolock) where userid = @intTargetID

	declare @systemrate int
	select	@systemrate = rate from [dbo].[NetProfitAgent_SystemRate] with (nolock) where @targerplatformid = platformid


	if (@systemrate < @rate) and (@topuserrate < @rate)
	begin
		set @chvMsg = '操作错误 : 比例设定错误'
		return -1
	end
	
	--if(@intAdminID is not null)
	--begin
	--	set @targetrage = @rate
	--end
	--else
	--begin
	--	if(@rate > @topuserrate)
	--	begin
	--		set @chvMsg = '操作错误 : 比例设定错误'
	--		return -1
	--	end
	--	else
	--	begin
	--		set @targetrage = @rate
	--	end
	--end

	if((@targetuserlv > (@topuserlv + 1))) or (@intAdminID is not null)	and (@topuserid is not null)
	begin
		set @chvMsg = '操作错误 : 不可跨级调整'
		return -1
	end

	set @targetrage = @rate



	--平台暂存
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT,PlatformName varchar(50),AreaID int,AreaName varchar(50))


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

		if exists (select top 1 1 from @Temp_PlatformID where platformid = @targerplatformid)
		begin
			update [dbo].[NetProfitAgent_UserInfo]
			set [Rate] = @targetrage
			where
				userid = @intTargetID

			insert into [dbo].[NetProfitAgent_UserRateChangeLog]
				([UserID],[Rate],[Status],[CreateTime],[UpdateUser],[UpdateAdmin])
			values
				(@intTargetID,@targetrage,1,getdate(),'',@intAdminID)
		end


		set @chvMsg = '操作完成 : 更新用户比例完成'
		return 1
	end

	if(@intUserID is not null)
	begin
		update [dbo].[NetProfitAgent_UserInfo]
		set [Rate] = @targetrage
		where
			userid = @intTargetID

		insert into [dbo].[NetProfitAgent_UserRateChangeLog]
			([UserID],[Rate],[Status],[CreateTime],[UpdateUser],[UpdateAdmin])
		values
			(@intTargetID,@targetrage,1,getdate(),'',@intAdminID)


		set @chvMsg = '操作完成 : 更新用户比例完成'
		return 1
	end

	set @chvMsg = '操作完成 : 更新完成'
	return 1
END







