
-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	游戏ZJH 取得監控名單
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_ZJH_DelVIPUser]
(
	@intUserID int
)
AS
BEGIN

	SET NOCOUNT ON
	Delete from [Game].[SuperTool_ZJH_VIPUser] where userid = @intUserID

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_ZJH_DelVIPUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_ZJH_DelVIPUser] TO [xbuser]
    AS [dbo];

