-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 查詢今日收益
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserAgent_GetAgentTaxBonusLog]
(
	@intUserID	int
)
AS
BEGIN

	set nocount on

	EXEC [Club02].[dbo].[PrPs_UserAgent_GetAgentTaxBonusLog]
		@intUserID = @intUserID

	return 1
END







GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UserAgent_GetAgentTaxBonusLog] TO [GameUser]
    AS [dbo];

