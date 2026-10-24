-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<手机验证码找回账号密码>
-- 2018/03/20	Jason		調整寫法呼叫LINKEDSERVER SP執行
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_User_FindSetPwdByPhone]
@chvTel			VARCHAR(32),				--手机号码
@chvCode		varchar(6),				--验证码
@chvNewPwd		VARCHAR(32),				--新密码
@chvIPAddress	VARCHAR(15),				--IP
@chvErrMsg		VARCHAR(128) OUTPUT			--提示消息
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @intUserID	INT
	DECLARE @NowTime	DATETIME = GETDATE()

	SELECT @intUserID = UserID FROM Users.UserInfo WHERE LoginName = @chvTel
	IF @intUserID IS NULL
	BEGIN
		SET @chvErrMsg = '账号不存在'
		RETURN -1
	END

	--检查验证码
	declare @intReturn int
	EXEC @intReturn = dbo.PrPs_CheckSMSCode 
			@chvTel = @chvTel,
			@chvCode = @chvCode,
			@tintCodeType = 2,
			@chvErrMsg= @chvErrMsg OUTPUT		
	if @intReturn<>1
		return -1



	UPDATE Users.UserInfo
	SET Password = [REDACTED]
	WHERE UserID = @intUserID

	--INSERT INTO Log.UserInfoChangeLog
	--(UserID, ChangeType, OldData, NewData, IPAddress,Memo, WriteTime)
	--SELECT
	--@intUserID, 3, '', '', @chvIPAddress, '找回密码修改',GETDATE()

	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrPs_Add_UserInfoChangeLog]
			@intUserID = @intUserID,
			@tnyChangeType = 3,
			@chrOldData = '',
			@chrNewData = '',
			@chrIPAddress = @chvIPAddress,
			@chrMemo = '找回密码修改',
			@dtWriteTime = @NowTime

	--删除用户证书
	DELETE Game.Certification WHERE UserID = @intUserID

	SET @chvErrMsg = '修改密码成功'

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_User_FindSetPwdByPhone] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_User_FindSetPwdByPhone] TO [xbuser]
    AS [dbo];

