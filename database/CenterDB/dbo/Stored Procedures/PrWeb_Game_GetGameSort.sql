


-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<查询平台游戏排序>
--	2018/10/08	harry
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Game_GetGameSort]
(
	@intAreaID	INT = NULL
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @AreaID	INT = null

	IF EXISTS(SELECT 1 FROM  [CenterDB].[Area].[GameList] WHERE AreaID = @intAreaID)
		BEGIN
			SET @AreaID = @intAreaID
		END

	SELECT 
	Id,
	GameName,
	PlatformID,
	AreaID,
	AG.GameID,
	Sort,
	AG.Status,
	AG.Popular,
	AG.IconID
	FROM [CenterDB].[Area].[GameList] AG
	LEFT JOIN  [CenterDB].[ServerInfo].[GameList] SG ON AG.GameID = SG.GameID
	WHERE AG.AreaID = @AreaID
	ORDER BY AG.Sort DESC
END



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_GetGameSort] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_GetGameSort] TO [xbuser]
    AS [dbo];

