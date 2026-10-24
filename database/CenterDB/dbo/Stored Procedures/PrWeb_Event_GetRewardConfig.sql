
-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 取得比賽活動獎勵贈送設定
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Event_GetRewardConfig]
(
	@intContestID int=null --場次ID
)
AS
BEGIN

	SET NOCOUNT ON

	SELECT * from Event.Reward where (@intContestID is null or ContestID=@intContestID)  order by type,rank,money asc
	
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_GetRewardConfig] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_GetRewardConfig] TO [xbuser]
    AS [dbo];

