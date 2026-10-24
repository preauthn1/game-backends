-- =============================================
-- Author:  jason
-- Create date: 2019/04/15
-- Description: USER 新增玩家(淨利潤)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_NetProfitAgent_GetSubUserList]
(
	@intUserID		int,		--用户 ID
	@dtStatDate	datetime
)
AS
BEGIN

	set nocount on

	EXEC [Club02].[dbo].[PrPs_NetProfitAgent_GetSubUserList]
		@intUserID = @intUserID,
		@dtStatDate = @dtStatDate

	return 1
END






GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_NetProfitAgent_GetSubUserList] TO [GameUser]
    AS [dbo];

