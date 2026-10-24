-- =============================================
-- Author:  jason
-- Create date: 2019/04/22
-- Description: 全民代理后台 平台列表(淨利潤)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_NetProfitAgent_GetAdminRelationPlatform]
(
	@intAdminID		int
)
AS
BEGIN

	set nocount on

	select	a.PlatformID,
			platformName = b.Name
	from [dbo].[AgentPlatformRelations] as a with (nolock)
		inner join centerdb.area.[platform] as b with (nolock) on a.platformid = b.platformid
	where
		a.adminid = @intAdminID

	return 1
END





