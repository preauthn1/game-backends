

-- =============================================
-- Author:  alan
-- Create date: 20181108
-- Description: 取得系統水池输赢记录
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_Fish_GetSystemPoolWinLost]
(
	@intFlagID int=null --初1 中2 高3 级场
)
AS
BEGIN

	SET NOCOUNT ON


	Select *
	from [Game].[SuperTool_Fish_SystemPoolWinLost] A with (nolock) 
	where 
	(@intFlagID is null or FlagID=@intFlagID)

	return 1

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_Fish_GetSystemPoolWinLost] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_Fish_GetSystemPoolWinLost] TO [xbuser]
    AS [dbo];

