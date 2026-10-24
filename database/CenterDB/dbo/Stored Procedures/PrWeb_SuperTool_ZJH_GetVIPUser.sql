
-- =============================================
-- Author:  alan
-- Create date: 20180918
-- Description:	超端 ZJH 取得監控人數
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_ZJH_GetVIPUser]
(
@intUserID int=null
)
AS
BEGIN

	SET NOCOUNT ON

	SELECT 
	UserID,
	ISNULL(Z.NextType,99) as NextType,
	ISNULL(Z.NextWinLose,99) as NextWinLose,
	ISNULL(Z.NextWinLoseNum,99) as NextWinLoseNum,
	ISNULL(Z.Finish,0) as Finish,
	ISNULL(Z.FinishNextWinLoseNum,0) as FinishNextWinLoseNum,
	CASE WHEN Z.NextTable = -1 then null else Z.NextTable END as NextTable, --如果是-1, 前端用null判斷
	Status,
	Operator,
	CreateTime

	FROM Game.SuperTool_ZJH_VIPUser Z with (nolock)
	WHERE (@intUserID is null or UserID=@intUserID)
	
	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_ZJH_GetVIPUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_ZJH_GetVIPUser] TO [xbuser]
    AS [dbo];

