-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<定时清理封锁IP>
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_ClearForbidIP]
	
AS
BEGIN 
	DELETE FROM Game.ForbidIPAddress where ExpireTime < getdate()
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_ClearForbidIP] TO [xbuser]
    AS [dbo];

