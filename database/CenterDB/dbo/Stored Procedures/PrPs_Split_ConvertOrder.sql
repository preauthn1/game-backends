
-- =============================================
-- Author:  jason
-- Create date: 20190505
-- Description: 裂变 - 提现纪录 送出提现金额
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Split_ConvertOrder]
(
	@intUserID int,
	@chvIPaddress	varchar(15),
	@lngAmount bigint,
	@chvMsg	varchar(100) OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON
	
	

	EXEC Club02.[dbo].[PrPs_Split_ConvertOrder]
		@intUserID = @intUserID,
		@chvIPaddress = @chvIPaddress,
		@lngAmount = @lngAmount,
		@chvMsg = @chvMsg OUTPUT
	
	set @chvMsg = '操作成功'
	return 1
END



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Split_ConvertOrder] TO [GameUser]
    AS [dbo];

