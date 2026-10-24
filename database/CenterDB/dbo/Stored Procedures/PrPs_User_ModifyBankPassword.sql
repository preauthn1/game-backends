-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<修改银行密码>
-- 2018/03/20	Jason		調整寫法呼叫LINKEDSERVER SP執行
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_User_ModifyBankPassword]
@intUserID INT,				--用户ID
@chvCer VARCHAR(32),		--证书
@chvOldPwd VARCHAR(32),		--旧密码
@chvNewPwd VARCHAR(32),		--新密码
@chvIPAddress VARCHAR(15),  --IP
@chvErrMsg VARCHAR(128) OUTPUT  --提示消息
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @NowTime	DATETIME = GETDATE()

	--检查用户信息
	IF NOT EXISTS(SELECT 1 FROM Game.Certification WHERE UserID = @intUserID AND CerCode = @chvCer)
	BEGIN
		SET @chvErrMsg = '操作失败:用户信息验证错误'
		RETURN 0
	END

	IF NOT EXISTS(SELECT 1 FROM Users.UserInfo WHERE UserID = @intUserID AND BankPassword= [REDACTED]
	BEGIN
		SET @chvErrMsg = '操作失败:旧密码不正确'
		RETURN 0
	END

	UPDATE Users.UserInfo
	SET BankPassword = [REDACTED]
	WHERE UserID = @intUserID

	--INSERT INTO Log.UserInfoChangeLog
	--(UserID, ChangeType, OldData, NewData, IPAddress,Memo, WriteTime)
	--SELECT
	--@intUserID, 3, '', '', @chvIPAddress, '客户端修改银行密码',GETDATE()

	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrPs_Add_UserInfoChangeLog]
				@intUserID = @intUserID,
				@tnyChangeType = 3,
				@chrOldData = '',
				@chrNewData = '',
				@chrIPAddress = @chvIPAddress,
				@chrMemo = '客户端修改银行密码',
				@dtWriteTime = @NowTime

	SET @chvErrMsg = '修改银行密码成功'
	
	RETURN 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_User_ModifyBankPassword] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_User_ModifyBankPassword] TO [xbuser]
    AS [dbo];

