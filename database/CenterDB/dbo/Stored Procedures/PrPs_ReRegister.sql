-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<广场注册>
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_ReRegister]
@intServerID INT
AS
SET NOCOUNT ON

UPDATE ServerInfo.PlazaServerList
SET LastUpdateTime = GETDATE()
WHERE ServerID = @intServerID

RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_ReRegister] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_ReRegister] TO [xbuser]
    AS [dbo];

