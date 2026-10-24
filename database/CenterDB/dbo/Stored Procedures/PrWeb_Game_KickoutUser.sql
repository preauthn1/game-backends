-- =============================================
-- Author:  alan
-- Create date: 2018-3-13
-- Description:	<把用户踢出房間>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Game_KickoutUser]
	@intUserID			INT = null,	--用户ID
	@intServerID		INT	= null	--ServerID

AS
BEGIN
	
	
	IF @intUserID IS NULL And @intServerID is not null
	Begin
		-- 踢出所有人
		Delete from Game.GameUserList where ServerID = @intServerID
		return 1
	End
	
	IF @intServerID IS NULL And @intUserID is not null
	Begin
		-- 踢出特定房間
		Delete from Game.GameUserList where UserID = @intUserID
		return 1
	End

	return 0

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_KickoutUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_KickoutUser] TO [xbuser]
    AS [dbo];

