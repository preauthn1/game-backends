-- =============================================
-- Author:		alan
-- Create date: 2018/7/26
-- Description:	记录指定开奖记录
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_AddWinPrizeLog]
	@chvAdminUser		varchar(20),
	@intGameID			INT,
	@intServerID		INT,
	@intTableNo			INT,
	@chvPrizeType		varchar(50)
AS
BEGIN

	INSERT INTO [PerLog].[WinPrizeLog](AdminUser,GameID,ServerID,TableNo,PrizeType)
	VALUES(@chvAdminUser,@intGameID,@intServerID,@intTableNo, @chvPrizeType)

	RETURN @@IDENTITY

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_AddWinPrizeLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_AddWinPrizeLog] TO [xbuser]
    AS [dbo];

