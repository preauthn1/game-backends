-- =============================================
-- Author:  jason
-- Create date: 2019/04/15
-- Description: USER 提現(淨利潤)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_NetProfitAgent_WithDrawOrder]
(
	@intUserID		int,	--用户 ID
	@intAmount		bigint,	--提現金額
	@chvMsg			varchar(100)	OUTPUT
)
AS
BEGIN

	set nocount on

	
	EXEC [Club02].[dbo].[PrPs_NetProfitAgent_WithDrawOrder]
		@intUserID = @intUserID,
		@intAmount = @intAmount,
		@chvMsg = @chvMsg	OUTPUT


	return 1
END






GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_NetProfitAgent_WithDrawOrder] TO [GameUser]
    AS [dbo];

