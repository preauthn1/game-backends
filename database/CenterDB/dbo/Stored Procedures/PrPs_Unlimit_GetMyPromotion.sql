
-- =============================================
-- Author:  
-- Create date: 20190430
-- Description: 无极限-我的推广
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Unlimit_GetMyPromotion]
(
	@intUserID int
)
AS
BEGIN

	SET NOCOUNT ON
	
	EXEC Club02.[dbo].[PrPs_Unlimit_GetMyPromotion]
		@intUserID = @intUserID

	return 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Unlimit_GetMyPromotion] TO [GameUser]
    AS [dbo];

