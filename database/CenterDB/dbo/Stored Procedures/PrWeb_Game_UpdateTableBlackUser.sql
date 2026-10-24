-- =============================================
-- Author:  alan
-- Create date: 2018/7/9
-- Description:	更新桌子黑名單user資料
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Game_UpdateTableBlackUser]
    @intGameID INT,
    @intServerID INT,
	@chvUserIDList varchar(1000)
AS
BEGIN 
IF NOT EXISTS (SELECT 1 From Game.TableBlackUser with (nolock) Where GameID = @intGameID and ServerID = @intServerID)
Begin
	INSERT INTO  Game.TableBlackUser(GameID, ServerID, UserIDList) Values(@intGameID, @intServerID, @chvUserIDList)
	Return
End

--Active 0=历史黑名单 1=当前黑名单清单
Update Game.TableBlackUser SET UserIDList = @chvUserIDList Where GameID = @intGameID and ServerID = @intServerID and Active = 0
	
END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_UpdateTableBlackUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_UpdateTableBlackUser] TO [xbuser]
    AS [dbo];

