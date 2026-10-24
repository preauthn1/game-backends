-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 推廣等級明細頁(我的等級)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserAgent_GetUserLevelInfo]
(
	@intUserID	int
)
AS
BEGIN

	set nocount on

	EXEC [Club02].[dbo].[PrPs_UserAgent_GetUserLevelInfo]
		@intUserID = @intUserID

	return 1
END







GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UserAgent_GetUserLevelInfo] TO [GameUser]
    AS [dbo];

