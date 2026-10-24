-- =============================================
-- Author:  jason
-- Create date: 2019/04/15
-- Description: USER 玩家/代理信息(淨利潤)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_NetProfitAgent_GetAgentInfo]
(
	@intUserID		int				--用户 ID
)
AS
BEGIN

	set nocount on

	EXEC [Club02].[dbo].[PrPs_NetProfitAgent_GetAgentInfo]
		@intUserID = @intUserID

	return 1
END






GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_NetProfitAgent_GetAgentInfo] TO [GameUser]
    AS [dbo];

