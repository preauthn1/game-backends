
-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	更新 "指定特定局開獎" 明細
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_SuperTool_UpdAssignRewardDetail]
(
	@intLogID int,
	@intIdx int,
	@chvReward varchar(500),
	@intStatus tinyint
)
AS
BEGIN

	SET NOCOUNT ON

	Update Game.SuperTool_AssignRewardDetail SET Reward = @chvReward, Status = @intStatus
	WHERE LogID = @intLogID and Idx = @intIdx

	--更新完成度 0 未完成, 1 己取用, 2 己完成, 3指定開獎
	IF @intStatus = 2
	Begin
		Update [Game].[SuperTool_AssignReward] SET Complete=Complete+1 where LogID = @intLogID
	End

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_UpdAssignRewardDetail] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_UpdAssignRewardDetail] TO [xbuser]
    AS [dbo];

