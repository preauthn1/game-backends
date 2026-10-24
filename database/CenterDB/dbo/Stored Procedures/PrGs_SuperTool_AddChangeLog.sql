
-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	記錄或變更超端操作
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_SuperTool_AddChangeLog]
(
	@intGameID int,
	@intServerID int,
	@intTableNo int,
	@intType tinyint, -- 1: 編輯獎池金額 2:增加放分金額
	@chvResult varchar(500),
	@chvOperator varchar(50)	
)
AS
BEGIN

	SET NOCOUNT ON

	INSERT INTO Game.SuperTool_ChangeLog(GameID, ServerId, TableNo, Type, Result, Operator)
	VALUES(@intGameID, @intServerID, @intTableNo, @intType, @chvResult, @chvOperator)

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_AddChangeLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_AddChangeLog] TO [xbuser]
    AS [dbo];

