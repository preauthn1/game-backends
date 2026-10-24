

-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 刪除比賽活動獎勵贈送設定
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Event_DelRewardConfig]
(
	@intID	int --主key
)
AS
BEGIN

	SET NOCOUNT ON

	delete from Event.Reward where ID = @intID

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_DelRewardConfig] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_DelRewardConfig] TO [xbuser]
    AS [dbo];

