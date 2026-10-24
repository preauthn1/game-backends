-- =============================================
-- Author:  emily
-- Create date: 2018/8/29
-- Description:	gameserver取得桌子白名單user資料
-- =============================================
Create PROCEDURE [dbo].[PrPsWeb_Game_GetTableWhiteUser]
    @intGameID INT,
    @chvShowNameID varchar(20),
    @tnyActive TINYINT =null--active 0=历史白名单 1=当前白名单清单
AS
BEGIN 

Select UserIDList, Active From Game.TableWhiteUser with(nolock) Where GameID = @intGameID and ShowNameID = @chvShowNameID and ISNULL(@tnyActive, Active) = Active
	
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPsWeb_Game_GetTableWhiteUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPsWeb_Game_GetTableWhiteUser] TO [xbuser]
    AS [dbo];

