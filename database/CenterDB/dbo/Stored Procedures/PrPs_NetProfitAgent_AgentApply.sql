-- =============================================
-- Author:  jason
-- Create date: 2019/04/16
-- Description: USER 申請加入代理 (淨利潤)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_NetProfitAgent_AgentApply]
(
	@intUserID		int,		--用户 ID
	@chvMsg		varchar(100) OUTPUT
)
AS
BEGIN

	set nocount on

	declare @topuserid int


	EXEC [Club02].[dbo].[PrPs_NetProfitAgent_AgentApply]
		@intUserID = @intUserID,
		@chvMsg = @chvMsg	OUTPUT

	return 1
END






GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_NetProfitAgent_AgentApply] TO [GameUser]
    AS [dbo];

