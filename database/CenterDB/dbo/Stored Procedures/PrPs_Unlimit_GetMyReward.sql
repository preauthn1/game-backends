
-- =============================================
-- Author:  jason
-- Create date: 20190501
-- Description: 无极限-我的奖励
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Unlimit_GetMyReward]
(
	@intUserID int=null
)
AS
BEGIN

	SET NOCOUNT ON



	--declare @intWeekMyFlows bigint=123456		--本周總業績 : 計算自己一周的總流水
	--declare @intWeekTeamFlows bigint=45678		--本周團隊總業績 : 計算自己+樹狀下線流水
	--declare @intWeekMemberFlows bigint=654321	--本周直屬總業績 : 直屬下線的流水總和
	--declare @intReward bigint=100				--獎勵 : 當周發放的金額
	--declare @intStatus	tinyint=1					--狀態 : 1=可提的會顯示可提現, 0=其他是顯示為預估值

	
	EXEC Club02.[dbo].[PrPs_Unlimit_GetMyReward]
		@intUserID = @intUserID

	return 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Unlimit_GetMyReward] TO [GameUser]
    AS [dbo];

