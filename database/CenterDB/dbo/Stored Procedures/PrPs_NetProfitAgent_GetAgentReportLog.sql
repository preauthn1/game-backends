-- =============================================
-- Author:  jason
-- Create date: 2019/04/15
-- Description: USER 新增代理(淨利潤)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_NetProfitAgent_GetAgentReportLog]
(
	@intUserID		int,		--用户 ID
	@dtStatTime		datetime
)
AS
BEGIN

	set nocount on

	EXEC [Club02].[dbo].[PrPs_NetProfitAgent_GetAgentReportLog]
		@intUserID = @intUserID,
		@dtStatTime	= @dtStatTime

	return 1
END






GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_NetProfitAgent_GetAgentReportLog] TO [GameUser]
    AS [dbo];

