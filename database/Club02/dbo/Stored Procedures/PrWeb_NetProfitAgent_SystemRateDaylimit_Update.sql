-- =============================================
-- Author:  jason
-- Create date: 2019/04/19
-- Description: 全民代理后台 取得平台比例设定 (净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_NetProfitAgent_SystemRateDaylimit_Update]
(
	@intAdminID		int,
	@intPlatformID	int = null,
	@intRate		int = null,
	@intWithDrawDay int = null,
	@chvMsg			varchar(100)		OUTPUT
)
AS
BEGIN

	set nocount on

	
	declare @adminplatformid int
	declare @userplatformid int
	declare @userareaid int
	declare @userstatus tinyint
	declare @usernickname varchar(32)
	declare @userpwd varchar(32)

	
	if not exists (select top 1 1 from [dbo].[AgentPlatformRelations] with (nolock) where adminid = @intAdminID and platformid = @intPlatformID)
	begin
		set @chvMsg = '操作错误 : 您没有权限修改平台比例'
		return -1
	end

	if(@intPlatformID is null)
	begin
		set @chvMsg = '操作错误 : 您没有权限修改此平台'
		return -1
	end

	if(@intRate > 1000) or (@intRate < 0)
	begin
		set @chvMsg = '操作错误 : 超过可设定比例'
		return -1
	end

	declare @checkuserrate int = 0 -- 0:不须调整用户 1:要调整用户

	if ((select max(rate) from [dbo].[NetProfitAgent_UserInfo] with (nolock) where platformid = @intPlatformID) > @intRate)
	begin
		set @checkuserrate = 1
	end


	declare @temp_rateday table (platformid int,rate int,daylimit int,bf_rate int,bf_daylimit int)
	update [dbo].[NetProfitAgent_SystemRate]
	set [Rate] = case when @intRate is null then [Rate] else @intRate end,
		[WithDrawDayLimit] = case when @intWithDrawDay is null then [WithDrawDayLimit] else @intWithDrawDay end
	output inserted.platformid,inserted.rate,inserted.WithDrawDayLimit,deleted.Rate,deleted.WithDrawDayLimit into @temp_rateday
	where
		platformid = @intPlatformID

	update [dbo].[NetProfitAgent_UserInfo]
	set rate = case when rate >= @intRate then @intRate - 10 else rate end
	where
		platformid = @intPlatformID



	insert into [dbo].[Log_SystemAdminChangeUserStatusLog]
		([AdminID],[UserID],[systemrate],[BF_systemrate],[CreateTime])
	select	@intAdminID,@intAdminID,
			'PlatformID : ' + cast(platformid as varchar(10)) + ' , Rate : ' + cast(rate as varchar(4)) + ' , WithDrawDayLimit : ' + cast(daylimit as varchar(10)),
			'PlatformID : ' + cast(platformid as varchar(10)) + ' , Rate : ' + cast(bf_rate as varchar(4)) + ' , WithDrawDayLimit : ' + cast(bf_daylimit as varchar(10)),
			getdate()
	from @temp_rateday


	set @chvMsg = '操作完成 : 完成'
	return 1
END





