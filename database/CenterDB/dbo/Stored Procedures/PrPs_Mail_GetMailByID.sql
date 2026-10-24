-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<根据ID获取邮件内容>
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Mail_GetMailByID]
@intUserID INT,
@chvCer VARCHAR(32),
@intMsgID INT
AS
SET NOCOUNT ON


SELECT 
1 as ReadType,
L.MsgID as ID,
MsgContent as Content,
Title,
CONVERT(varchar(16),CreateTime,120)  as SendTime
FROM Message.MessageList as L with(nolock) 
LEFT JOIN Message.ReceiveUser as R ON L.MsgID=R.MsgID
WHERE L.MsgID = @intMsgID
AND UserID = @intUserID
AND CreateTime > dbo.Fn_EmailStartTime()
AND NOT EXISTS(SELECT 1 FROM Message.ReceiveUser WHERE UserId = @intUserID AND MsgID = @intMsgID AND Status = 2)

IF @@ROWCOUNT>0
BEGIN	
	--更新消息状态
	UPDATE Message.ReceiveUser 
	SET Status = 1,
		ReadTime = GETDATE()
	WHERE Msgid = @intMsgID 
	AND UserID = @intUserID 
	AND Status = 0
END


RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Mail_GetMailByID] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Mail_GetMailByID] TO [xbuser]
    AS [dbo];

