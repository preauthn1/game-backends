-- =============================================
-- Author:  emily
-- Create date: 2018/8/29
-- Description:	gameserver更新桌子白名單user資料
-- 
-- =============================================
CREATE PROCEDURE [dbo].[PrPsWeb_Game_UpdateTableWhiteUser]
    @intGameID INT,
    @chvShowNameID varchar(20),
	@tnyActive TINYINT, --active 0=历史白名单 1=当前白名单清单
	@chvUserIDList varchar(1000)
AS
BEGIN

	DELETE FROM Game.TableWhiteUser WHERE GameID in (9, 11, 13, 18, 19, 20, 21) and ShowNameID = @chvShowNameID and Active = @tnyActive 
	INSERT INTO  Game.TableWhiteUser(GameID, ShowNameID, Active, UserIDList) Values(9, @chvShowNameID, @tnyActive, @chvUserIDList)  --股市大亨
	INSERT INTO  Game.TableWhiteUser(GameID, ShowNameID, Active, UserIDList) Values(11, @chvShowNameID, @tnyActive, @chvUserIDList) --龙虎斗
	INSERT INTO  Game.TableWhiteUser(GameID, ShowNameID, Active, UserIDList) Values(13, @chvShowNameID, @tnyActive, @chvUserIDList) --百家乐
	INSERT INTO  Game.TableWhiteUser(GameID, ShowNameID, Active, UserIDList) Values(18, @chvShowNameID, @tnyActive, @chvUserIDList) --疯狂二八
	INSERT INTO  Game.TableWhiteUser(GameID, ShowNameID, Active, UserIDList) Values(19, @chvShowNameID, @tnyActive, @chvUserIDList) --百人牛牛
	INSERT INTO  Game.TableWhiteUser(GameID, ShowNameID, Active, UserIDList) Values(20, @chvShowNameID, @tnyActive, @chvUserIDList) --红黑大战
	INSERT INTO  Game.TableWhiteUser(GameID, ShowNameID, Active, UserIDList) Values(21, @chvShowNameID, @tnyActive, @chvUserIDList) --红黑牛牛
END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPsWeb_Game_UpdateTableWhiteUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPsWeb_Game_UpdateTableWhiteUser] TO [xbuser]
    AS [dbo];

