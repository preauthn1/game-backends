-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<检查金币锁>
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_CheckServerLock]
@intUserID INT,
@intServerID INT,
@intRenterID INT = NULL,
@tnyHDType TINYINT
AS
SET NOCOUNT ON
DECLARE @intExServerID INT

IF NOT EXISTS(SELECT 1 FROM Game.GameUserList WHERE UserID = @intUserID)
INSERT INTO Game.GameUserList
        ( UserID, ServerID, HDType )
SELECT @intUserID, @intServerID, @tnyHDType


RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_CheckServerLock] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_CheckServerLock] TO [xbuser]
    AS [dbo];

