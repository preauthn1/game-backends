
-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	更新 "指定特定局開獎" 結果
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_SuperTool_UpdAssignReward]
(
	@intLogID int,
	@intComplete int -- 完成局數
)
AS
BEGIN

	SET NOCOUNT ON

	Update Game.SuperTool_AssignReward SET Complete = @intComplete WHERE LogID = @intLogID

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_UpdAssignReward] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_UpdAssignReward] TO [xbuser]
    AS [dbo];

