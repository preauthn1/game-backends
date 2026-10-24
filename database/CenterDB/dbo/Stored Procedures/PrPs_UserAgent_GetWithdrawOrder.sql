-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 收益提現頁面
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserAgent_GetWithdrawOrder]
(
	@intUserID	int,
	@chvMsg		varchar(100) OUTPUT
)
AS
BEGIN

	set nocount on

	EXEC [Club02].[dbo].[PrPs_UserAgent_GetWithdrawOrder]
		@intUserID = @intUserID,
		@chvMsg = @chvMsg OUTPUT

		return 1
END







GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UserAgent_GetWithdrawOrder] TO [GameUser]
    AS [dbo];

