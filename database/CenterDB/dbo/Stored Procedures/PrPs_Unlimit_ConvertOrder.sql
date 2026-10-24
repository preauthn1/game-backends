
-- =============================================
-- Author:  jason
-- Create date: 20190502
-- Description: 无极限-代理提现
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Unlimit_ConvertOrder]
(
	@intUserID int,
	@intType int = 0,		-- 1:支付宝 2:银联
	@chvIPaddress varchar(15),
	@lngAmount bigint = 0,
	@chvMsg	varchar(100) OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON


	exec Club02.[dbo].[PrPs_Unlimit_ConvertOrder]
		@intUserID = @intUserID,
		@intType = @intType,		-- 1:支付宝 2:银联
		@chvIPaddress = @chvIPaddress,
		@lngAmount = @lngAmount,
		@chvMsg	= @chvMsg OUTPUT

	return 1
END







GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Unlimit_ConvertOrder] TO [GameUser]
    AS [dbo];

