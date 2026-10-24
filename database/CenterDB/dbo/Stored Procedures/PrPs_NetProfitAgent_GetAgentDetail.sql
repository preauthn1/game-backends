-- =============================================
-- Author:  jason
-- Create date: 2019/04/15
-- Description: USER 推廣信息(淨利潤)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_NetProfitAgent_GetAgentDetail]
(
	@intUserID		int,	--用户 ID
	@chvMsg			varchar(100)	OUTPUT
)
AS
BEGIN

	set nocount on


	EXEC [Club02].[dbo].[PrPs_NetProfitAgent_GetAgentDetail]
		@intUserID = @intUserID,
		@chvMsg = @chvMsg OUTPUT


	return 1
END






GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_NetProfitAgent_GetAgentDetail] TO [GameUser]
    AS [dbo];

