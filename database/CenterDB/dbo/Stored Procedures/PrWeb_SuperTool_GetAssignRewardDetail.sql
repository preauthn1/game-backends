
-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	取得 "指定特定局開獎" 細節
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_GetAssignRewardDetail]
(
	@intGameID int
)
AS
BEGIN

	SET NOCOUNT ON
	DECLARE @intLogID int
	DECLARE @intMainStatus int

	--取得最後的log
	--status 0 取消  1 啟用 2 暫停
	SELECT top 1 @intLogID = LogID, @intMainStatus = Status from Game.SuperTool_AssignReward with (nolock) WHERE GameID = @intGameID
	Order by LogID desc
	
	--status 0 未完成, 1 己取用, 2 己完成, 3指定開獎
	select Idx, Description, Status, ISNULL(Reward,'') as Reward, @intMainStatus as MainStatus ,CreateTime from Game.SuperTool_AssignRewardDetail with (nolock) WHERE LogID = @intLogID 

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_GetAssignRewardDetail] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_GetAssignRewardDetail] TO [xbuser]
    AS [dbo];

