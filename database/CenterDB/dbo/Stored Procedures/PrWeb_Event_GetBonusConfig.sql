
-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 取得比賽活動充直贈送設定
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Event_GetBonusConfig]
(
	@intContestID int=null --場次ID
)
AS
BEGIN

	SET NOCOUNT ON

	SELECT * from Event.Bonus where (@intContestID is null or ContestID=@intContestID) order by OrderReach asc
	
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_GetBonusConfig] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_GetBonusConfig] TO [xbuser]
    AS [dbo];

