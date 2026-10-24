-- =============================================
-- Author:  jason
-- Create date: 2019/04/21
-- Description: 全民代理取得用戶對應等級
-- =============================================
CREATE FUNCTION [dbo].[Fn_UserAgent_GetUserLevel]
(
	@intAmount bigint
)
RETURNS int
AS
BEGIN

	--取得一個域名對應ID
	DECLARE @intLevel int=1
	SELECT @intLevel = max([Level]) FROM [dbo].[UserAgentSystemLevelRate] with (nolock) where TaxAmount <= @intAmount

	set @intLevel = isnull(@intLevel,1)

	RETURN @intLevel
END
