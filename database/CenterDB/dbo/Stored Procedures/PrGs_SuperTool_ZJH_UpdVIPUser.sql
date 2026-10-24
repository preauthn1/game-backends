
-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	游戏ZJH 更新監控
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_SuperTool_ZJH_UpdVIPUser]
(
	@intUserID int,
	@intFinish int=null,
	@intFinishNextWinLoseNum int=null,
	@intNextTable int=null,
	@intNextType int=null
)
AS
BEGIN

	SET NOCOUNT ON

	Declare @intStatus int=null
	
	--如果完成, 就變更為暫停
	IF @intFinish = 1 -- or @intFinishNextWinLoseNum = 1
		SET @intStatus = 0

	UPDATE Game.SuperTool_ZJH_VIPUser 
	SET 
	[Finish]=ISNULL(@intFinish,[Finish]),
	[FinishNextWinLoseNum] = ISNULL(@intFinishNextWinLoseNum,[FinishNextWinLoseNum]),
	[NextTable] = ISNULL(@intNextTable,[NextTable]),
	[Status] = ISNULL(@intStatus,[Status]),
    [NextType] = ISNULL(@intNextType,[Status])
	Where UserID=@intUserID

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_ZJH_UpdVIPUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_ZJH_UpdVIPUser] TO [xbuser]
    AS [dbo];

