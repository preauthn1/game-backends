
-- =============================================
-- Author:  jason
-- Create date: 20190505
-- Description: 裂变 - 提现纪录
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Split_GetMyConvert]
(
	@intUserID int,
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intWithdrawTime INT = 0 OUTPUT,
	@intPageCount	INT = 0		OUTPUT,
	@lngRewardAmount bigint OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON
	
	
	--set	@lngRewardAmount = 1000
	--set @intPageCount = 5

	--select LogDate = '20190501',ConvertAmount = 10000
	--union all
	--select LogDate = '20190502',ConvertAmount = 10000
	--union all
	--select LogDate = '20190503',ConvertAmount = 10000
	--union all
	--select LogDate = '20190504',ConvertAmount = 10000
	--union all
	--select LogDate = '20190505',ConvertAmount = 10000



	
	EXEC Club02.[dbo].[PrPs_Split_GetMyConvert]
		@intUserID = @intUserID,
		@intPageIndex = @intPageIndex,
		@intPageSize = @intPageSize,
		@intWithdrawTime = @intWithdrawTime OUTPUT,
		@intPageCount = @intPageCount OUTPUT,
		@lngRewardAmount = @lngRewardAmount OUTPUT

	return 1
END



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Split_GetMyConvert] TO [GameUser]
    AS [dbo];

