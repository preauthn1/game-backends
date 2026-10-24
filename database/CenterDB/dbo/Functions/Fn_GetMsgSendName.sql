-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<获取邮件发送者身份>
-- =============================================
CREATE FUNCTION [dbo].[Fn_GetMsgSendName]
(
	@intUserID int
)
RETURNS VARCHAR(32)
AS
BEGIN
	DECLARE @chvSenderName VARCHAR(32)
	SET @chvSenderName = '系统中心'
	
	IF @intUserID IS NULL
	SET @chvSenderName = '系统中心'
	ELSE IF @intUserID = 1
	SET @chvSenderName = '客服回复'
	ELSE
	SELECT @chvSenderName = LoginName FROM Users.UserInfo WHERE UserID = @intUserID
	
	RETURN @chvSenderName
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_GetMsgSendName] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_GetMsgSendName] TO [xbuser]
    AS [dbo];

