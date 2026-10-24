
-- =============================================
-- Author:  
-- Create date: 20190430
-- Description: 无极限-我的预提
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Unlimit_GetMyPreConvert]
(
	@intUserID int,
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intPageCount	INT = 0		OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON

	----1.	預提紀錄欄:
	--declare @intOrderId int = 111111 --單號 : 申請的流水號
	--declare @intAmount bigint = 20000 --預提的金額(最低為100,最高不能超過當周預估獎勵的90%)
	--declare @intType tinyint = 1 --目前只能提到遊戲中,一律先顯示遊戲
	--declare @chvAccount varchar(50) = '2000111222' --提現帳號 : 遊戲帳號
	--declare @tnyStatus  int=1 --狀態 : 0=未發放 1=已發放
	

	--set @intReward = 20000000
	--set @intConvert = 1000000
	--set @intSumConvert = 500000
	--set @intRemainConvert = 400000
	--set @intLastWeekSumConvert = 600000
	--set @chvHelp = '
	--<BR>1. 预提时间为周二至周五,每天.....
	--<BR>2. 可预提时间.....
	--<BR>3. 下周一结束时.....
	--<BR>4. 每日可预提.....
	--'

	--select 
	--'2019-4-30' as StatTime,
	--@intOrderId as OrderId,
	--@intAmount as Amount,
	--@intType as Type,
	--@chvAccount as Account,
	--@tnyStatus as Status


	exec Club02.[dbo].[PrPs_Unlimit_GetMyPreConvert]
		@intUserID = @intUserID,
		@intPageIndex = @intPageIndex,
		@intPageSize = @intPageSize,
		@intPageCount = @intPageCount		OUTPUT


	return 1
END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Unlimit_GetMyPreConvert] TO [GameUser]
    AS [dbo];

