-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<修改密码>

-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_User_ModifyBankPassword]
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
	SET BankPassword = [REDACTED]
	WHERE UserID = @intUserID

	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrPs_Add_UserInfoChangeLog]
						@intUserID = @intUserID,
						@tnyChangeType = @NotifyType,
						@chrOldData = '',
						@chrNewData = '',
						@chrIPAddress = @chvIPAddress,
						@chrMemo = '网站修改保险箱密码',
						@dtWriteTime = @NowTime

	SET @chvErrMsg = '修改保险箱密码成功'
	
	RETURN 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_User_ModifyBankPassword] TO [GameUser]
    AS [dbo];

