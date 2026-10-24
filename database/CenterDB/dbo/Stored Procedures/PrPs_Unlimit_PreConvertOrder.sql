
-- =============================================
-- Author:  jason
-- Create date: 20190501
-- Description: 无极限-代理預提
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Unlimit_PreConvertOrder]
(
	@intUserID int,
	@lngAmount bigint = 0,
	@chvMsg	varchar(100) OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON


	exec Club02.[dbo].[PrPs_Unlimit_PreConvertOrder]
		@intUserID = @intUserID,
		@lngAmount = @lngAmount,
		@chvMsg = @chvMsg OUTPUT


	return 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Unlimit_PreConvertOrder] TO [GameUser]
    AS [dbo];

