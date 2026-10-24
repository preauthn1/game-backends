-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 查詢領取收益提現頁面
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserAgent_GetWithdrawList]
(
	@intUserID	int
)
AS
BEGIN

	set nocount on

	EXEC [Club02].[dbo].[PrPs_UserAgent_GetWithdrawList]
		@intUserID = @intUserID

	return 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UserAgent_GetWithdrawList] TO [GameUser]
    AS [dbo];

