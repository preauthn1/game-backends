
-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	取得 超端變更 紀錄
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_GetChangeLog]
(
	@GameID  int,
	@intType int
)
AS
BEGIN

	SET NOCOUNT ON

	Select * from Game.SuperTool_ChangeLog with (nolock) where GameID = @GameID AND Type = @intType
	and CreateTime >= DateAdd(day,-30, getdate())
	order by CreateTime desc

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_GetChangeLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_GetChangeLog] TO [xbuser]
    AS [dbo];

