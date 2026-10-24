

-- =============================================
-- Author:  alan
-- Create date: 20181110
-- Description: 取得玩家个人水池输赢记录
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_Fish_GetPlayerPoolWinLost]
(
	@intFlagID int=null, --初1 中2 高3 级场
	@intUserID int=null
)
AS
BEGIN

	SET NOCOUNT ON

	IF @intUserID is null
	BEGIN
		--找捕魚上線的玩家
		select userID into #onlineuser from game.GameUserList with (nolock) where [ServerID] in (select serverid from ServerInfo.GameServerList with (nolock) where gameid=14)

		Select A.* 
		from [Game].[SuperTool_Fish_PlayerPoolWinLost] A with (nolock) 
		Where 
		(@intFlagID is NULL or A.FlagID = @intFlagID)
		and A.UserID in (select UserID from #onlineuser)

		return 1
	END

	Select A.* 
	from [Game].[SuperTool_Fish_PlayerPoolWinLost] A with (nolock) 
	Where 
	(@intFlagID is NULL or A.FlagID = @intFlagID)
	and A.UserID = @intUserID

	return 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_Fish_GetPlayerPoolWinLost] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_Fish_GetPlayerPoolWinLost] TO [xbuser]
    AS [dbo];

