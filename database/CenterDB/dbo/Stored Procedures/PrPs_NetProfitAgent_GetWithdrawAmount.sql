-- =============================================
-- Author:  jason
-- Create date: 2019/04/15
-- Description: USER 提現紀錄(淨利潤)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_NetProfitAgent_GetWithdrawAmount]
(
	@intUserID		int		--用户 ID
)
AS
BEGIN

	set nocount on

		
	EXEC [Club02].[dbo].[PrPs_NetProfitAgent_GetWithdrawAmount]
		@intUserID = @intUserID
	
	return 1
END






GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_NetProfitAgent_GetWithdrawAmount] TO [GameUser]
    AS [dbo];

