-- =============================================
-- Author:  alan
-- Create date: 2018/08/3
-- Description: 新增电话或支付宝封号
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Game_ForbidAccount]
(
	@chvAccountID varchar(50),
	@tnyIdType	tinyint, --1 正式帐号(电话) 2支付宝帐号 3銀聯帳號
	@chvMemo varchar(100)
)
AS
BEGIN
	SET NOCOUNT ON
	
	IF NOT EXISTS(Select 1 FROM Game.ForbidAccount WITH(NOLOCK) WHERE AccountID=@chvAccountID AND IdType=@tnyIdType)
	BEGIN
		INSERT INTO  Game.ForbidAccount(AccountID,IdType,Memo) Values(@chvAccountID,@tnyIdType,@chvMemo)
	END
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_ForbidAccount] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_ForbidAccount] TO [xbuser]
    AS [dbo];

