
-- =============================================
-- Author:  jason
-- Create date: 20190505
-- Description: 裂变 - 说明
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Split_GetRewardTips]
(
	@intUserID int
)
AS
BEGIN

	SET NOCOUNT ON
	
	
	--select UserCounts = 10,RewardB = 20,RewardC = 5,RewardD = 1
	--union all
	--select UserCounts = 20,RewardB = 20,RewardC = 5,RewardD = 1



	
	EXEC Club02.[dbo].[PrPs_Split_GetRewardTips]
		@intUserID = @intUserID


	return 1
END



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Split_GetRewardTips] TO [GameUser]
    AS [dbo];

