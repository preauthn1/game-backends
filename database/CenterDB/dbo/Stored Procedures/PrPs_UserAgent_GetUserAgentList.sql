-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 推廣人數頁面說明
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserAgent_GetUserAgentList]
(
	@intUserID	int
)
AS
BEGIN

	set nocount on

	EXEC [Club02].[dbo].[PrPs_UserAgent_GetUserAgentList]
		@intUserID = @intUserID

	return 1
END







GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UserAgent_GetUserAgentList] TO [GameUser]
    AS [dbo];

