-- =============================================
-- Author:  jason
-- Create date: 2019/04/16
-- Description: USER 審核代理申請列表(淨利潤)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_NetProfitAgent_AgentApplyList]
(
	@intUserID		int = null			--用户 ID
)
AS
BEGIN

	set nocount on


	EXEC [Club02].[dbo].[PrPs_NetProfitAgent_AgentApplyList]
		@intUserID = @intUserID

	return 1
END






GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_NetProfitAgent_AgentApplyList] TO [GameUser]
    AS [dbo];

