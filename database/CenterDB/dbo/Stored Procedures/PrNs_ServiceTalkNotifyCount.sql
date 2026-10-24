-- =============================================
-- Author:		alan
-- Description:	取得線上客服未讀通知/清除count
-- =============================================
CREATE PROCEDURE [dbo].[PrNs_ServiceTalkNotifyCount]
@intUserID int,
@intCount int=null --更新count, 0: 清除為0 
AS
SET NOCOUNT ON

IF @intCount IS NULL
BEGIN
	DECLARE @intUnreadCount int = 0
	SELECT @intUnreadCount = ISNULL(UnreadCount,0) From Notify.Notify.ServiceTalk with (nolock) WHERE UserID = @intUserID
	Return @intUnreadCount
END
ELSE
BEGIN
	Update Notify.Notify.ServiceTalk set UnreadCount=0 WHERE UserID = @intUserID
	Return 0
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrNs_ServiceTalkNotifyCount] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrNs_ServiceTalkNotifyCount] TO [xbuser]
    AS [dbo];

