

-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description:	捕鱼新手优惠
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_Fish_AddBeginnerOffer]
(
@intUserID int,
@intBullet int,
@intScore int,
@tnyStatus tinyint
)
AS
BEGIN

	SET NOCOUNT ON

	IF EXISTS (select 1 from Game.Fish_BeginnerOffer with (nolock) where UserID=@intUserID)
		Update Game.Fish_BeginnerOffer SET Bullet=@intBullet, Score=@intScore, Status=@tnyStatus where UserID=@intUserID
	ELSE
		Insert Game.Fish_BeginnerOffer(UserID,Bullet,Score,Status) Values(@intUserID,@intBullet,@intScore,@tnyStatus)

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_Fish_AddBeginnerOffer] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_Fish_AddBeginnerOffer] TO [xbuser]
    AS [dbo];

