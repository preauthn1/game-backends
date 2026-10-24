
-- =============================================
-- Author:  jason
-- Create date: 20190505
-- Description: 裂变 - 马上提现
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Split_GetMyReward]
(
	@intUserID int
)
AS
BEGIN

	SET NOCOUNT ON
	
	
	--select MyReward = 1000

	
	EXEC Club02.[dbo].[PrPs_Split_GetMyReward]
		@intUserID = @intUserID


	return 1
END



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Split_GetMyReward] TO [GameUser]
    AS [dbo];

