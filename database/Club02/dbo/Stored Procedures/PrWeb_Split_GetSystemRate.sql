
-- =============================================
-- Author:  jason
-- Create date: 20190511
-- Description: 裂变 - 取分成比例
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Split_GetSystemRate]
(
	@intAdminID int,
	@intPlatformID int
)
AS
BEGIN

	SET NOCOUNT ON
	

	/*  取得管理者可读取平台  */
	declare @adminplatformid table (platformid int,platformname varchar(32))

	insert into @adminplatformid
	select	distinct a.platformid,b.name
	from [dbo].[AgentPlatformRelations] as a with (nolock)
		inner join centerdb.area.platform as b with (nolock) on a.PlatformID = b.PlatformID
	where adminid = @intAdminID and a.platformid = @intPlatformID


	--declare @temp_table table ([PlatformID] int,[Level] int,[MinRecruit1] int,[MinRecruit2] int,[MinRecruit3] int,[MaxRecruit1] int,[MaxRecruit2] int,[MaxRecruit3] int
	--							,[Stage1] int,[Stage2] int,[Stage3] int)


	--insert into @temp_table
	select	a.[PlatformID]
		  ,[Level]
		  ,[MinRecruit1]
		  ,[MinRecruit2]
		  ,[MinRecruit3]
		  ,[MaxRecruit1]
		  ,[MaxRecruit2]
		  ,[MaxRecruit3]
		  ,[Stage1]
		  ,[Stage2]
		  ,[Stage3]
	from @adminplatformid as a
		inner join [dbo].[Split_SystemLevelRate] as b with (nolock) on a.platformid = b.PlatformID

	select	distinct a.[PlatformID],[TaxAmount],[WithdrawLimit]
	from @adminplatformid as a
		inner join [dbo].[Split_SystemLevelRate] as b with (nolock) on a.platformid = b.PlatformID


	return 1
END


