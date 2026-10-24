-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<查询游戏总局数前N名玩家>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetGameTotalRank]
	@intTopN		int,
	@intGameID		int	
AS
BEGIN		
	SELECT TOP (@intTopN) UserID, (WinCount+LoseCount+DrawCount) as Total
	FROM Game.UserGameInfo			
	WHERE GameID = @intGameID
	ORDER BY WinCount+LoseCount+DrawCount desc	
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetGameTotalRank] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetGameTotalRank] TO [xbuser]
    AS [dbo];

