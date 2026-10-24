

-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description:	捕鱼新手优惠
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_Fish_GetBeginnerOffer]
(
@intUserID int
)
AS
BEGIN

	SET NOCOUNT ON

	IF @intUserID = 0
		select F.* from Game.Fish_BeginnerOffer F with (nolock) left join Money.Cash C with (nolock)
		on F.UserID=C.UserID
		where C.Amount >= 2 --排除低於0.02元的用戶
	else
		select F.* from Game.Fish_BeginnerOffer F with (nolock) left join Money.Cash C with (nolock)
		on F.UserID=C.UserID
		where C.Amount >= 2 --排除低於0.02元的用戶
		and F.UserID=@intUserID

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_Fish_GetBeginnerOffer] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_Fish_GetBeginnerOffer] TO [xbuser]
    AS [dbo];

