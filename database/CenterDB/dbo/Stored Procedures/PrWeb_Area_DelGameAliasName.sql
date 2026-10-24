


-- =============================================
-- Author:  <xxxx>
-- Create date: <xxxx>
-- Description:	<移除游戏别名>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Area_DelGameAliasName]
@intGameID INT,
@intIconID INT
AS
BEGIN

	Delete Area.GameAliasName Where GameID=@intGameID AND IconID=@intIconID

	return 1

END



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Area_DelGameAliasName] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Area_DelGameAliasName] TO [xbuser]
    AS [dbo];

