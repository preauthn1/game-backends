
-- =============================================
-- Author:		alan
-- Create date: 20181219
-- Description:	取得用戶訊息通知清單
-- changelog:
-- =============================================
CREATE PROCEDURE [dbo].[PrNs_GetUserNotify]
@intUserID int
AS
SET NOCOUNT ON

--取得用户最后一次读取通知的index
declare @intIdx int=0
select @intIdx=ISNULL(Idx,0) from Notify.UserNotifyIdx with (nolock) where userid = @intUserID

--回传最后一次读取通知的index之后的讯息
select * from Notify.Notify with (nolock) where userid=@intUserID and Idx > @intIdx

RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrNs_GetUserNotify] TO [xbuser]
    AS [dbo];

