
-- =============================================
-- Author:  alan
-- Create date: 20180908
-- Description:	按魚放分
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_Fish_StartSystemReward]
(
	@intFlagID int,
	@intFishID int,
	@intStatus int,
	@chvOperator varchar(20)
)
AS
BEGIN

	SET NOCOUNT ON

	Update Game.SuperTool_Fish_SystemReward 
	SET
	Status = @intStatus, Operator = @chvOperator
	Where FishID = @intFishID AND flagID=@intFlagID

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_Fish_StartSystemReward] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_Fish_StartSystemReward] TO [xbuser]
    AS [dbo];

