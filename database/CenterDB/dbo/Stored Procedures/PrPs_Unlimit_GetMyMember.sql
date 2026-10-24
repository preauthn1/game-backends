
-- =============================================
-- Author:  jason
-- Create date: 20190501
-- Description: 无极限-我的玩家
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Unlimit_GetMyMember]
(
	@intUserID int=null,
	@intTargetID int = null,
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intPageCount	INT = 0		OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON
	--declare @intWeekTeamFlows bigint=123456 --本周團隊貢獻 : 這個玩家+下線所貢獻之流水
	--declare @intWeekMemberFlows bigint=45678	--本周玩家貢獻 : 這個玩家個人貢獻的流水
	--declare @intTeamMember bigint=50	--團隊人數 : 這個玩家樹狀下線所有的人數
	--declare @intAgent int=20		--直屬下線人數
	--declare @intMember int=5		--直屬玩家 : 這個玩家的直屬玩家人數
	--declare @intMemberType int=1	--類型 : 該玩家的身分級別(最下線則是玩家)


	EXEC Club02.[dbo].[PrPs_Unlimit_GetMyMember]
		@intUserID = @intUserID,
		@intTargetID = @intTargetID,
		@intPageIndex = @intPageIndex,
		@intPageSize = @intPageSize,
		@intPageCount = @intPageCount	OUTPUT


	return 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Unlimit_GetMyMember] TO [GameUser]
    AS [dbo];

