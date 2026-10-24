
-- =============================================
-- Author:  alan
-- Create date: 2018/10/9
-- Description: 取得一個域名位置
-- =============================================
CREATE FUNCTION [dbo].[Fn_GetDomain]
(
)
RETURNS int
AS
BEGIN

	--取得一個域名對應ID
	DECLARE @intDomainID int=0
	Select top 1 @intDomainID=ID from dbo.DomainConfig with (nolock) where available = 1  --count < 30 
	order by ID
	IF @intDomainID is null or @intDomainID = 0
		return 0

	RETURN @intDomainID
END
