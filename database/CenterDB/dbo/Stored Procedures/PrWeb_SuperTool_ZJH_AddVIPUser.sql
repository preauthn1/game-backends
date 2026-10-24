
-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	游戏ZJH 取得監控名單
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_ZJH_AddVIPUser]
(
	@intUserID int,
	@intNextType int=99,
	@intNextWinLose int=99,
	@intNextWinLoseNum int=99,
	@intNextTable int=null,
	@intStatus	tinyint=0,
	@chvOperator varchar(50)
)
AS
BEGIN

	SET NOCOUNT ON

	IF NOT EXISTS(Select 1 from Game.SuperTool_ZJH_VIPUser with(nolock) where UserID=@intUserID)
	Begin

		Insert into Game.SuperTool_ZJH_VIPUser (
		[UserID],
		[NextType],
		[NextWinLose],
		[NextWinLoseNum],
		[NextTable],
		[Status],
		[Operator],
        [CreateTime]
		) values(
		@intUserID,
		@intNextType,
		@intNextWinLose,
		@intNextWinLoseNum,
		@intNextTable,
		@intStatus,
		@chvOperator, GETDATE())

	End

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_ZJH_AddVIPUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_ZJH_AddVIPUser] TO [xbuser]
    AS [dbo];

