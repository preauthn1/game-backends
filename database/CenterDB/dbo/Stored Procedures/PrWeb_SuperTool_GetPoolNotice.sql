
-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	取得有异状的水池
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_GetPoolNotice]

AS
BEGIN

	SET NOCOUNT ON
	
	SELECT G.GameName, P.GameID, P.[Value] from Game.SuperTool_PoolNotice as P with (nolock)
	Left join ServerInfo.GameList as G with (nolock)
	on P.GameID = G.GameID

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_GetPoolNotice] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_GetPoolNotice] TO [xbuser]
    AS [dbo];

