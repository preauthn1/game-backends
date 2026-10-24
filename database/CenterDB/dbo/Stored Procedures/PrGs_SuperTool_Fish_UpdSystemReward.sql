
-- =============================================
-- Author:  alan
-- Create date: 20180908
-- Description:	修改 捕魚系統放分
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_SuperTool_Fish_UpdSystemReward]
(
	@intFlagID int,
	@intFishID int,
	@intFinishAmount bigint=null	-- 完成放分金額, 單位"分"
)
AS
BEGIN

	SET NOCOUNT ON
	DECLARE @intAmount bigint
	DECLARE @intFinish tinyint=0 --是否已經完成放分 0 未完成 1 完成

	Select @intAmount=Amount from  Game.SuperTool_Fish_SystemReward with (nolock) where FishID = @intFishID and FlagID = @intFlagID
	--放分時
	if @intAmount > 0 AND @intFinishAmount >= @intAmount
		SET  @intFinish = 1
 
	--殺分時
	if @intAmount < 0 AND  @intFinishAmount <= @intAmount
		SET  @intFinish = 1
	

	Update Game.SuperTool_Fish_SystemReward 
	SET 
	FinishAmount = ISNULL(@intFinishAmount,FinishAmount),
	Finish = @intFinish,
	FinishTime = CASE WHEN @intFinish=1 THEN Getdate() ELSE FinishTime END,
	Status = CASE WHEN @intFinish=1 THEN 0 ELSE Status END
	Where FishID = @intFishID and FlagID = @intFlagID

	--立即提供server端結果
	select * from Game.SuperTool_Fish_SystemReward with (nolock) where Status = 1 and FlagID = @intFlagID

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_Fish_UpdSystemReward] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_Fish_UpdSystemReward] TO [xbuser]
    AS [dbo];

