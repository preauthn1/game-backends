

-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<更新游戏排序>
--	2018/10/08	harry
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Game_UpdateGameSort]
(
	@intPlatformID	INT,
	@intAreaID	INT,
	@intGameID	INT,
	@intSort	INT,
	@intPopular	INT,
	@intIconID	INT,
	@intStatus	INT
)
AS
BEGIN

	SET NOCOUNT ON
	IF NOT EXISTS(SELECT 1 FROM  [CenterDB].[Area].[GameList] WHERE PlatformID = @intPlatformID AND AreaID = @intAreaID AND GameID = @intGameID)
		BEGIN
				INSERT INTO [CenterDB].[Area].[GameList] (PlatformID, AreaID, GameID, Sort,Popular,IconID, Status)
				VALUES (@intPlatformID, @intAreaID, @intGameID, @intSort, @intPopular, @intIconID, @intStatus);
		END
	ELSE
		BEGIN
				UPDATE [CenterDB].[Area].[GameList]
				SET Sort = @intSort, IconID= @intIconID, Popular=@intPopular, Status = @intStatus
				WHERE PlatformID = @intPlatformID AND AreaID = @intAreaID AND GameID = @intGameID;
		END
	RETURN 1
END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_UpdateGameSort] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_UpdateGameSort] TO [xbuser]
    AS [dbo];

