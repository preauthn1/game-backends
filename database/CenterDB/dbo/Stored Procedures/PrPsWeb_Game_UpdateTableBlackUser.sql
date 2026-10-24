-- =============================================
-- Author:  alan
-- Create date: 2018/7/9
-- Description:	gameserver更新桌子黑名單user資料
-- 2018/08/28  #690 後台桌子管理增加百人類黑名單時, 全部百人類的都加入黑名單
-- =============================================
CREATE PROCEDURE [dbo].[PrPsWeb_Game_UpdateTableBlackUser]
    @intGameID INT,
    @chvShowNameID varchar(20),
	@tnyActive TINYINT, --active 0=历史黑名单 1=当前黑名单清单
	@chvUserIDList varchar(1000)
	--@intType INT = 0
AS
BEGIN
   -- IF (@tnyActive = 1 AND @intType = 0)
		--BEGIN
		    DELETE FROM Game.TableBlackUser WHERE GameID in (9, 11, 13, 18, 19, 20, 21) and ShowNameID = @chvShowNameID and Active = @tnyActive 
			INSERT INTO  Game.TableBlackUser(GameID, ShowNameID, Active, UserIDList) Values(9, @chvShowNameID, @tnyActive, @chvUserIDList)  --股市大亨
			INSERT INTO  Game.TableBlackUser(GameID, ShowNameID, Active, UserIDList) Values(11, @chvShowNameID, @tnyActive, @chvUserIDList) --龙虎斗
			INSERT INTO  Game.TableBlackUser(GameID, ShowNameID, Active, UserIDList) Values(13, @chvShowNameID, @tnyActive, @chvUserIDList) --百家乐
			INSERT INTO  Game.TableBlackUser(GameID, ShowNameID, Active, UserIDList) Values(18, @chvShowNameID, @tnyActive, @chvUserIDList) --疯狂二八
			INSERT INTO  Game.TableBlackUser(GameID, ShowNameID, Active, UserIDList) Values(19, @chvShowNameID, @tnyActive, @chvUserIDList) --百人牛牛
			INSERT INTO  Game.TableBlackUser(GameID, ShowNameID, Active, UserIDList) Values(20, @chvShowNameID, @tnyActive, @chvUserIDList) --红黑大战
			INSERT INTO  Game.TableBlackUser(GameID, ShowNameID, Active, UserIDList) Values(21, @chvShowNameID, @tnyActive, @chvUserIDList) --红黑牛牛
       -- END
    --ELSE
		--IF NOT EXISTS (SELECT 1 From Game.TableBlackUser with (nolock) Where GameID = @intGameID and ShowNameID = @chvShowNameID and Active = @tnyActive)
		--  Begin
		--	INSERT INTO  Game.TableBlackUser(GameID, ShowNameID, Active, UserIDList) Values(@intGameID, @chvShowNameID, @tnyActive, @chvUserIDList)
		--	Return
		--  END
		--Update Game.TableBlackUser SET UserIDList = @chvUserIDList Where GameID = @intGameID and ShowNameID = @chvShowNameID and Active = @tnyActive
END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPsWeb_Game_UpdateTableBlackUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPsWeb_Game_UpdateTableBlackUser] TO [xbuser]
    AS [dbo];

