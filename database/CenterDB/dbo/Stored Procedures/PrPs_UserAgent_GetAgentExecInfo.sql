
-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 查詢推廣等級/今日稅收/今日收益/推廣人數
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserAgent_GetAgentExecInfo]
(
	@intUserID	int
)
AS
BEGIN

	EXEC [Club02].[dbo].[PrPs_UserAgent_GetAgentExecInfo]
		@intUserID = @intUserID

	return 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UserAgent_GetAgentExecInfo] TO [GameUser]
    AS [dbo];

