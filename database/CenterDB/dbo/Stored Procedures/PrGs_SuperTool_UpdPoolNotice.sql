
-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	更新异常的水池
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_SuperTool_UpdPoolNotice]
(
	@intGameID int,
	@intValue int,
	@intAction int -- 0删除 1新增 
)

AS
BEGIN

	SET NOCOUNT ON
	
	IF @intAction = 0
		DELETE Game.SuperTool_PoolNotice  where GameID = @intGameID

	IF @intAction = 1
		IF NOT EXISTS (select 1 from Game.SuperTool_PoolNotice with (nolock)  where GameID = @intGameID)
			INSERT INTO Game.SuperTool_PoolNotice(GameID,[Value]) VALUES(@intGameID, @intValue)

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_UpdPoolNotice] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_UpdPoolNotice] TO [xbuser]
    AS [dbo];

