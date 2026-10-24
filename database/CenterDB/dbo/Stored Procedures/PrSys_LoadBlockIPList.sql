-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<加载封锁IP列表>
-- =============================================
CREATE PROCEDURE [dbo].[PrSys_LoadBlockIPList]
AS
SET NOCOUNT ON

SELECT IPAddress FROM Game.ForbidIPAddress

RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrSys_LoadBlockIPList] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrSys_LoadBlockIPList] TO [xbuser]
    AS [dbo];

