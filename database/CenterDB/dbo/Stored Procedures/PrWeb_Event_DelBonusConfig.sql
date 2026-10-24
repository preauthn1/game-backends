

-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 删除比賽活動充值贈送設定
-- =============================================
create PROCEDURE [dbo].[PrWeb_Event_DelBonusConfig]
(
	@intID	int --主key
)
AS
BEGIN

	SET NOCOUNT ON

	Delete from Event.Bonus where ID=@intID

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_DelBonusConfig] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_DelBonusConfig] TO [xbuser]
    AS [dbo];

