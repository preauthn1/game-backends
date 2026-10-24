-- =============================================
-- Author:  jason
-- Create date: 2019/04/22
-- Description: 全民代理后台 平台列表(淨利潤)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_NetProfitAgent_GetAdminList]
(
  @intAdminID    int
)
AS
BEGIN

	set nocount on

	select	id,
			[Account],
			Status = cast(Status as int),		-- 0:停用 1:開啟
			AdminType = cast(AdminType as int),	-- 0:root 1:財務 2:總代理
			CreateTime
	from [dbo].[AgentAdminUser] as a with (nolock)
	
END
