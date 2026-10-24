-- =============================================
-- Author:  Eddie
-- Create date: 2019/07/24
-- Description:	otμХ(eg. B986Bp)
-- =============================================
CREATE FUNCTION [dbo].[Fn_GetSystemTag]
()
RETURNS varchar(20)
AS
BEGIN
	
	declare @chvSystemTag varchar(20)
	select  @chvSystemTag = SystemTag
	from [CenterDB].[ServerInfo].[SystemConfig]
	return ISNULL(@chvSystemTag,'--')
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[FN_GetScale] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[FN_GetScale] TO [xbuser]
    AS [dbo];


