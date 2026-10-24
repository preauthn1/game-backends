-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<获取兑换比例>
-- =============================================
CREATE FUNCTION [dbo].[FN_GetScale]
()
RETURNS int
AS
BEGIN
	DECLARE @intScale INT
	
	SELECT @intScale = (SELECT TOP 1 Scale FROM Game.Config)
	RETURN @intScale

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[FN_GetScale] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[FN_GetScale] TO [xbuser]
    AS [dbo];

