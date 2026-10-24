
-- =============================================
-- Author:  jason
-- Create date: 20190505
-- Description: 裂变 - 推广信息
--	2019/06/12	Jason		新增回傳上級用戶ID
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Split_GetMyInfo]
(
	@intUserID int,
	@lngRewardAmount bigint OUTPUT,
	@lngSplitAmount bigint OUTPUT,
	@intSuccessCount int OUTPUT,
	@intSuccessToday int OUTPUT,
	@intSuccessLastday int OUTPUT,
	@chvQRCode varchar(250) OUTPUT,
	@chvInviteCode varchar(250) OUTPUT,
	@UpperUserid int OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON
	
	
	--select	@lngRewardAmount = 1000,
	--		@lngSplitAmount = 1500,
	--		@intSuccessCount = 101,
	--		@intSuccessToday = 15,
	--		@intSuccessLastday = 86
	
	
	EXEC Club02.[dbo].[PrPs_Split_GetMyInfo]
		@intUserID = @intUserID,
		@lngRewardAmount = @lngRewardAmount OUTPUT,
		@lngSplitAmount = @lngSplitAmount OUTPUT,
		@intSuccessCount = @intSuccessCount OUTPUT,
		@intSuccessToday = @intSuccessToday OUTPUT,
		@intSuccessLastday = @intSuccessLastday OUTPUT,
		@chvQRCode = @chvQRCode OUTPUT,
		@chvInviteCode = @chvInviteCode OUTPUT,
		@UpperUserid = @UpperUserid OUTPUT


	return 1
END



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Split_GetMyInfo] TO [GameUser]
    AS [dbo];

