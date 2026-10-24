
-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	取消指定局開獎資料
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_CancelAssignReward]
(
	@intGameID int
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @intLogID int

	--取得最後的log
	SELECT top 1 @intLogID = LogID from Game.SuperTool_AssignReward with (nolock) WHERE GameID = @intGameID
	Order by LogID desc

	UPDATE  [Game].[SuperTool_AssignReward] SET STATUS = 0 WHERE LogID=@intLogID
	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_CancelAssignReward] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_CancelAssignReward] TO [xbuser]
    AS [dbo];

