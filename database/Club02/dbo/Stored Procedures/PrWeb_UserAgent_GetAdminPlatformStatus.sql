-- =============================================
-- Author:  jason
-- Create date: 2019/04/23
-- Description: 后台 提供admin查询管理者账号对应平台权限
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_GetAdminPlatformStatus]
(
	@intAdminID	int
)
AS
BEGIN

	set nocount on

	select	a.[PlatformID],
			PlatformName = b.Name
	from [dbo].[AgentPlatformRelations] as a with (nolock)
		inner join CenterDB.area.[platform] as b with (nolock) on a.PlatformID = b.PlatformID
	where
		a.AdminID = @intAdminID


	return 1
END







