


-- =============================================
-- Author:  alan
-- Create date: 20181108
-- Description: 取得個人水池開關
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_Fish_GetPlayerPool]
AS
BEGIN

	SET NOCOUNT ON
	Select * from  Game.SuperTool_Fish_PlayerPool 
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_Fish_GetPlayerPool] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_Fish_GetPlayerPool] TO [xbuser]
    AS [dbo];

