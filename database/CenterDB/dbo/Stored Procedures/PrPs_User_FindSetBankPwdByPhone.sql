-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<手机验证码找回银行密码>
--	2018/03/20	Jason		調整寫法呼叫LINKEDSERVER SP執行
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_User_FindSetBankPwdByPhone]
(
	@intUserID		VARCHAR(32),				--用户ID
	@chvCode		varchar(32),				--验证码
	@chvNewPwd		VARCHAR(32),				--新密码
	@chvIPAddress	VARCHAR(15),				--IP
	@chvErrMsg		VARCHAR(128) OUTPUT			--提示消息
)
AS
BEGIN
	SET NOCOUNT ON

		DECLARE @NowTime	DATETIME = GETDATE()
		DECLARE @chvTel varchar(11),@intReturn int

	SELECT @chvTel = LoginName FROM Users.UserInfo WHERE UserID=@intUserID
	IF @intUserID IS NULL
	BEGIN
		SET @chvErrMsg = '账号不存在'
		RETURN -1
	END

	--检查验证码
	EXEC @intReturn = dbo.PrPs_CheckSMSCode 
			@chvTel = @chvTel,
			@chvCode = @chvCode,
			@tintCodeType = 3,
			@chvErrMsg= @chvErrMsg OUTPUT
		
	if @intReturn<>1
		return -1

	--更新银行密码
	UPDATE Users.UserInfo
	SET BankPassword = [REDACTED]
	WHERE UserID = @intUserID

	/*  --写修改用户信息记录  */
	-------------------------------------------------------------------------------------
	--INSERT INTO Log.UserInfoChangeLog
	--(UserID, ChangeType, OldData, NewData, IPAddress,Memo, WriteTime)
	--SELECT
	--@intUserID, 3, '', '', @chvIPAddress, '找回银行密码修改',GETDATE()

	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrPs_Add_UserInfoChangeLog]
		@intUserID = @intUserID,
		@tnyChangeType = 3,
		@chrOldData = '',
		@chrNewData = '',
		@chrIPAddress = @chvIPAddress,
		@chrMemo = '找回银行密码修改',
		@dtWriteTime = @NowTime
	-------------------------------------------------------------------------------------

	SET @chvErrMsg = '修改银行密码成功'

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_User_FindSetBankPwdByPhone] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_User_FindSetBankPwdByPhone] TO [xbuser]
    AS [dbo];

