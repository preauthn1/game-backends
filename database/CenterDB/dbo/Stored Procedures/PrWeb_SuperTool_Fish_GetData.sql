
-- =============================================
-- Author:  alan
-- Create date: 20180930
-- Description: 取得捕魚統計資料
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_Fish_GetData]
(
	@intFlagID int = null
)
AS
BEGIN

	SET NOCOUNT ON

	select F.*,M.Name from Game.SuperTool_Fish_Data F with (nolock)
	left join Game.SuperTool_FishItem M with (nolock) 
	on F.FishID=M.FishId
	where (@intFlagID is null or F.FlagID=@intFlagID)
	and YYYYMMDD =  CAST ( CONVERT(varchar,getdate(),112) AS INT)
	order by F.FlagID, F.FishID

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_Fish_GetData] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_Fish_GetData] TO [xbuser]
    AS [dbo];

