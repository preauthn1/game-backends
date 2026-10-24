-- =============================================
-- Author:  jason
-- Create date: 2019/04/19
-- Description: 全民代理后台 取得平台比例设定 (净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_NetProfitAgent_GetSystemRateDaylimit]
(
	@intAdminID		int,
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

	
	declare @temp_platformid table (platformid int,platformName varchar(100))

	insert into @temp_platformid
	select	distinct a.PlatformID,
			platformname = b.Name
	from [dbo].[AgentPlatformRelations] as a
		inner join centerdb.area.platform as b with (nolock) on a.PlatformID = b.PlatformID
	where
		adminid = @intAdminID


	select	a.platformid,
			a.platformName,
			Rate = ISNULL(cast(b.Rate as int), 0),
			WithDrawDayLimit = ISNULL(cast(b.WithDrawDayLimit as int), 7),
			StageLevel = ISNULL(cast(b.StageLevel as int), 10)
	from @temp_platformid as a
		left join [dbo].[NetProfitAgent_SystemRate] as b on a.platformid = b.PlatformID


	set @chvMsg = '操作完成 : 完成'
	return 1
END





