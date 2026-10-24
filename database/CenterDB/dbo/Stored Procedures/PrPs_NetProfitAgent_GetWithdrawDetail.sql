-- =============================================
-- Author:  jason
-- Create date: 2019/04/17
-- Description: USER 今日值兌詳情(淨利潤)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_NetProfitAgent_GetWithdrawDetail]
(
	@intUserID		int,		--用户 ID
	@dtStatTime		datetime	--查詢日期
)
AS
BEGIN

	set nocount on

	
	EXEC [Club02].[dbo].[PrPs_NetProfitAgent_GetWithdrawDetail]
		@intUserID = @intUserID,		--用户 ID
		@dtStatTime	= @dtStatTime	--查詢日期
	
	return 1
END






GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_NetProfitAgent_GetWithdrawDetail] TO [GameUser]
    AS [dbo];

