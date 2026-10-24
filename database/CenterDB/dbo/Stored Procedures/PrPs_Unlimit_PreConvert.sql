
-- =============================================
-- Author:  jason
-- Create date: 20190501
-- Description: 无极限-代理預提
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Unlimit_PreConvert]
(
	@intUserID int,
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intPageCount	INT = 0		OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON


	exec Club02.[dbo].[PrPs_Unlimit_PreConvert]
		@intUserID = @intUserID,
		@intPageIndex = @intPageIndex,
		@intPageSize = @intPageSize,
		@intPageCount = @intPageCount		OUTPUT


	return 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Unlimit_PreConvert] TO [GameUser]
    AS [dbo];

