-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<修改密码>
--	2018/04/10		Jason		調整為呼叫LOGDB SP執行紀錄
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_User_ModifyPassword]
(
	@intUserID		INT,					--用户ID
	@chvNewPwd		VARCHAR(32),			--新密码
	@chvIPAddress	VARCHAR(15),			--IP
	@chvErrMsg		VARCHAR(128)	OUTPUT  --提示消息
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @NotifyType	TINYINT = 3
	DECLARE @NowTime	DATETIME = GETDATE()

	UPDATE Users.UserInfo
	SET Password = [REDACTED]
	WHERE UserID = @intUserID

	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrPs_Add_UserInfoChangeLog]
						@intUserID = @intUserID,
						@tnyChangeType = @NotifyType,
						@chrOldData = '',
						@chrNewData = '',
						@chrIPAddress = @chvIPAddress,
						@chrMemo = '网站修改密码',
						@dtWriteTime = @NowTime


	--删除用户证书
	DELETE Game.Certification WHERE UserID = @intUserID

	SET @chvErrMsg = '修改密码成功'
	
	RETURN 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_User_ModifyPassword] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_User_ModifyPassword] TO [xbuser]
    AS [dbo];

