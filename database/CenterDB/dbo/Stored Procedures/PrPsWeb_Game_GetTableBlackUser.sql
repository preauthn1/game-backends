-- =============================================
-- Author:  alan
-- Create date: 2018/7/9
-- Description:	gameserver取得桌子黑名單user資料
-- =============================================
CREATE PROCEDURE [dbo].[PrPsWeb_Game_GetTableBlackUser]
    @intGameID INT,
    @chvShowNameID varchar(20),
    @tnyActive TINYINT  =NULL --active 0=历史黑名单 1=当前黑名单清单
AS
BEGIN 

Select UserIDList, Active From Game.TableBlackUser with(nolock) Where GameID = @intGameID and ShowNameID = @chvShowNameID and ISNULL(@tnyActive, Active) = Active
	
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPsWeb_Game_GetTableBlackUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPsWeb_Game_GetTableBlackUser] TO [xbuser]
    AS [dbo];

