
-- =============================================
-- Author:  jason
-- Create date: 20190501
-- Description: 无极限-我的业绩
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Unlimit_GetMyPerformance]
(
	@intUserID int=null,
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intPageCount	INT = 0		OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON
	----1.	總業績欄(每周重置)
	--declare @intWeekMyFlows bigint=123456 --本周總業績 : 計算自己一周的總流水
	--declare @intWeekTeamFlows bigint=45678 --本周團隊總業績 : 計算自己+樹狀下線流水
	--declare @intWeekMemberFlows bigint=654321	--本周直屬總業績 : 直屬下線的流水總和
	--declare @intReward bigint=100	--現金獎勵 : 自己直推業績*自己級別的提成比例+下線直屬代理所有業績*(自己級別的提成比例-下線直屬代理級別的提成比例)+….直到所有代理的返利都加完 + 直屬下線提現的5%  – 每日估算,以周一收到的獎勵為準

	----2.	每日業績欄位
	--declare @intDayMyFlows bigint=2222 --日總業績 : 每日自己的流水業績
	--declare @intDayTeamFlows bigint=3333 --日團隊業績 : 每日自己+樹狀下線流水的業績
	--declare @intDayMemberFlows bigint=4444	--每日直屬+樹狀下線流水的業績

	--set @intPageCount=10


	EXEC Club02.[dbo].[PrPs_Unlimit_GetMyPerformance]
		@intUserID = @intUserID,
		@intPageIndex = @intPageIndex,
		@intPageSize = @intPageSize,
		@intPageCount = @intPageCount OUTPUT

	return 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Unlimit_GetMyPerformance] TO [GameUser]
    AS [dbo];

