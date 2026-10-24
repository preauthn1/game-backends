


-- =============================================
-- Author:  alan
-- Create date: 20181108
-- Description: 開關個人水池
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_Fish_UpdPlayerPool]
(
	@intFlagID int, --初1 中2 高3 级场
	@intStatus int --关0  开1
)
AS
BEGIN

	SET NOCOUNT ON

	Update Game.SuperTool_Fish_PlayerPool 
	SET 
	Status = @intStatus
	Where FlagID = @intFlagID
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_Fish_UpdPlayerPool] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_Fish_UpdPlayerPool] TO [xbuser]
    AS [dbo];

