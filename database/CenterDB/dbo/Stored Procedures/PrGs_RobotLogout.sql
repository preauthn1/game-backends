-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<机器人登出>
--	2018/05/08	Jason	新增登出時產生新的Sort
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_RobotLogout]
@intServerID INT,
@intRobotID INT
AS
BEGIN
	SET NOCOUNT ON
	
	--机器人解锁
	UPDATE Robot.RobotList
	SET ServerID = 0,
		Sort = ABS(CHECKSUM(NEWID()))
	WHERE
		UserID = @intRobotID
		AND ServerID = @intServerID

	RETURN 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_RobotLogout] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_RobotLogout] TO [xbuser]
    AS [dbo];

