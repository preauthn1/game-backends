
-- =============================================
-- Author:  jason
-- Create date: 20190430
-- Description: 无极限-我的提现
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Unlimit_GetMyConvert]
(
	@intUserID int,
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intPageCount	INT = 0		OUTPUT,
	@intSumReward bigint OUTPUT, --總獎勵金額 : 從過去到現在已經發放的獎勵金累計
	@intSumConvert bigint OUTPUT, --已提現金額 : 從過去到現在已經提現的金額累計
	@intRemainConvert bigint OUTPUT --可提現金額 : 總獎勵金額 – 可提現金額
)
AS
BEGIN

	SET NOCOUNT ON

	----1.	提現紀錄欄:
	--declare @intOrderId int = 111111 --單號 : 申請的流水號
	--declare @intAmount bigint = 20000 --金額 : 申請提現的金額
	--declare @intType tinyint = 1 --提現類型 : 透過那種支付方式 1=支付宝 2=银联
	--declare @chvAccount varchar(50) = '2000111222' --提現帳號 : 打錢的帳號紀錄
	--declare @tnyStatus  int=1 --狀態 : 0=未發放 1=已發放

	EXEC Club02.[dbo].[PrPs_Unlimit_GetMyConvert]
		@intUserID = @intUserID,
		@intPageIndex = @intPageIndex,
		@intPageSize = @intPageSize,
		@intPageCount = @intPageCount		OUTPUT,
		@intSumReward = @intSumReward OUTPUT, --總獎勵金額 : 從過去到現在已經發放的獎勵金累計
		@intSumConvert = @intSumConvert OUTPUT, --已提現金額 : 從過去到現在已經提現的金額累計
		@intRemainConvert = @intRemainConvert OUTPUT --可提現金額 : 總獎勵金額 – 可提現金額

	set @intPageCount = isnull(@intPageCount,0)
	set @intSumReward = isnull(@intSumReward,0)
	set @intSumConvert = isnull(@intSumConvert,0)
	set @intRemainConvert = isnull(@intRemainConvert,0)

	return 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Unlimit_GetMyConvert] TO [GameUser]
    AS [dbo];

