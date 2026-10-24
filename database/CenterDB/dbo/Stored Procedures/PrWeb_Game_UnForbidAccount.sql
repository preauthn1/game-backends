-- =============================================
-- Author:  alan
-- Create date: 2018/08/3
-- Description: 解除电话或支付宝封号
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Game_UnForbidAccount]
(
	@chvAccountID varchar(50),
	@tnyIdType	tinyint --1 正式帐号(电话) 2支付宝帐号  3銀聯帳號
)
AS
BEGIN
	SET NOCOUNT ON

	Declare @intUserID int = 0

	DELETE FROM Game.ForbidAccount where AccountID = @chvAccountID AND IdType = @tnyIdType


	
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_UnForbidAccount] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_UnForbidAccount] TO [xbuser]
    AS [dbo];

