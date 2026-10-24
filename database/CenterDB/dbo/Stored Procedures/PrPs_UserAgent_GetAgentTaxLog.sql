-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 查詢今日稅收
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserAgent_GetAgentTaxLog]
(
	@intUserID	int,
	@dtStattime	datetime
)
AS
BEGIN

	set nocount on

	EXEC [Club02].[dbo].[PrPs_UserAgent_GetAgentTaxLog]
		@intUserID = @intUserID,
		@dtStattime = @dtStattime

	return 1
END







GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UserAgent_GetAgentTaxLog] TO [GameUser]
    AS [dbo];

