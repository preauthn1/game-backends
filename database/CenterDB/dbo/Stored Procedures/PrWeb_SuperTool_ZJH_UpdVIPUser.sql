
-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	游戏ZJH 取得監控名單
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_ZJH_UpdVIPUser]
(
	@intUserID int,
	@intNextType int=null,
	@intNextWinLose  int=null,
	@intNextWinLoseNum int=null,
	@intNextTable int=null,
	@intStatus	tinyint=null,
	@chvOperator varchar(50)=null
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @intFinish tinyint
	DECLARE @intFinishNextWinLoseNum tinyint=null


	--是否完成: 1完成 0否
	SELECT @intFinish = Finish from Game.SuperTool_ZJH_VIPUser with (nolock) where UserID = @intUserID
	IF @@ROWCOUNT = 0
		return 0
		
	IF @intFinish >= 1
	BEGIN
		--狀態歸0
		SET @intFinishNextWinLoseNum = 0
		SET @intFinish = 0
	END


	Update Game.SuperTool_ZJH_VIPUser
	SET
	[Finish]=ISNULL(@intFinish,[Finish]),
	NextType = ISNULL(@intNextType,NextType),
	NextWinLose = ISNULL(@intNextWinLose,NextWinLose),
	NextWinLoseNum = ISNULL(@intNextWinLoseNum,NextWinLoseNum),
	NextTable = @intNextTable,
	FinishNextWinLoseNum = ISNULL(@intFinishNextWinLoseNum, FinishNextWinLoseNum),
	Status = ISNULL(@intStatus,Status),
	Operator = ISNULL(@chvOperator,Operator),
	CreateTime = GETDATE()
	Where UserID=@intUserID

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_ZJH_UpdVIPUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_ZJH_UpdVIPUser] TO [xbuser]
    AS [dbo];

