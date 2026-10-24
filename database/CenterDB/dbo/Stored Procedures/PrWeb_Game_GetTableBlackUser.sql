-- =============================================
-- Author:  alan
-- Create date: 2018/7/9
-- Description:	取得桌子黑名單user資料
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Game_GetTableBlackUser]
    @intGameID INT,
    @intServerID INT
AS
BEGIN 

--Active 0=历史黑名单 1=当前黑名单清单
Select UserIDList From Game.TableBlackUser with(nolock) Where GameID = @intGameID and ServerID = @intServerID and Active = 0
	
END



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_GetTableBlackUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_GetTableBlackUser] TO [xbuser]
    AS [dbo];

