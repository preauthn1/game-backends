
-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	游戏server更新監控
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_SuperTool_UpdVIPUser]
(
	@intUserID int,
	@intCompleteAmount bigint, --完成金额
	@intComplete int -- 是否完成
)
AS
BEGIN

	SET NOCOUNT ON

	UPDATE Game.SuperTool_VIPUser 
	SET 
	CompleteAmount=@intCompleteAmount, 
	Complete=@intComplete, 
	SumAssignAmount=CASE WHEN @intComplete=1 THEN SumAssignAmount+@intCompleteAmount ELSE SumAssignAmount END,
	Status = CASE WHEN @intComplete=1 THEN 0 ELSE 1 END,
	FinishTime = CASE WHEN @intComplete=1 THEN getdate() ELSE FinishTime END
	Where UserID=@intUserID


	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_UpdVIPUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_UpdVIPUser] TO [xbuser]
    AS [dbo];

