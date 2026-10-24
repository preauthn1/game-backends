
-- =============================================
-- Author:		alan
-- Create date: 20181219
-- Description:	更新用戶訊息通知索引
-- changelog:
-- =============================================
CREATE PROCEDURE [dbo].[PrNs_UpdateNotifyIdx]
@intUserID int,
@intIdx int
AS
SET NOCOUNT ON

--hall/game server更新用戶最後讀取的索引
update Notify.UserNotifyIdx Set Idx=@intIdx where userid = @intUserID
if @@ROWCOUNT=0
	insert into Notify.UserNotifyIdx (UserID,Idx) values(@intUserID,@intIdx)

RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrNs_UpdateNotifyIdx] TO [xbuser]
    AS [dbo];

