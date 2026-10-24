-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<大厅登录_普通登录>
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserLogon_Normal]
	@chvLoginName		varchar(32) = NULL,				--用户名
	@chvPassword		varchar(32) = NULL,         --密码
	@chvVersion			varchar(16),				--客户端版本号
	@tnyHDType			tinyint,					--硬件类型
	@chvHDCode			varchar(64),				--硬件码
	@sintPlazaServerID	smallint,					--登录广场ID
	@chvIPAddress		varchar(15),				--IP地址
	@intAreaID			int,						--渠道ID
	@intUserID			INT = null OUTPUT ,					--用户ID
	@chvCertification	VARCHAR(32) = null OUTPUT,			--证书
	@chvErrMsg			VARCHAR(128) OUTPUT         --错误消息
AS
SET NOCOUNT ON

--2018/5/6 alan 臨時關閉
--if @intAreaID in (18,3)
--Begin
--	SET @chvErrMsg = '游戏关闭进行维护中'
--	RETURN -1
--End
--


SET @intUserID = CASE WHEN @intUserID = 0 THEN NULL ELSE @intUserID END

DECLARE @tnyLogonType tinyint

--檢查areaID是否存在
IF not exists(select 1 from Area.Area with (nolock) where AreaID = @intAreaID)
BEGIN
	SET @chvErrMsg = '渠道编号不存在'
	RETURN -1
END


--检查用户名
IF RTRIM(LTRIM(@chvLoginName)) = ''
BEGIN
	SET @chvErrMsg = '登录失败:用户名不正确'
	RETURN -1
end

--检查密码
IF ((@chvPassword IS NULL OR @chvPassword  = '') AND @chvLoginName IS NULL)
AND ((@chvCertification IS NULL OR @chvCertification = '') AND @intUserID IS NULL)
BEGIN
	SET @chvErrMsg = '登录失败:用户信息不正确'
	RETURN -1
END

--检查用户名错误次数
IF EXISTS(SELECT 1 FROM Logon.AccountLogonError WHERE LoginName = @chvLoginName AND ErrorTimes >= 5)
BEGIN
	SET @chvErrMsg = '登录失败:此账号15分钟内登录错误次数超过5次，暂时被锁定，请稍后再试'
	RETURN -1
END

--检查IP错误次数
/*
IF EXISTS(SELECT 1 FROM Logon.IPLogonError WHERE IPAddress = @chvIPAddress AND ErrorTimes >= 10)
BEGIN
	SET @chvErrMsg = '登录失败:此IP 15分钟内登录错误次数超过10次，暂时被锁定，请稍后再试'
	RETURN -1
END*/


--检查机器错误次数
IF EXISTS(SELECT 1 FROM Logon.HDLogonError WHERE HDType = @tnyHDType AND HDCode = @chvHDCode AND ErrorTimes >= 8)
BEGIN
	SET @chvErrMsg = '登录失败:此机器15分钟内登录错误次数超过8次，暂时被锁定，请稍后再试'
	RETURN -1
END


--通过账号密码登录
IF @chvLoginName IS NOT NULL AND @chvPassword IS NOT NULL
BEGIN
	--密码加密
	SET @chvPassword = [REDACTED]
	
	--获取用户信息
	SELECT @intUserID = UserID FROM Users.UserInfo
	WHERE LoginName = @chvLoginName AND Password = [REDACTED]
	
	IF @intUserID IS NULL
	BEGIN
		--写用户名错误记录
		UPDATE LogOn.AccountLogonError
		SET ErrorTimes = ErrorTimes + 1
		WHERE LoginName = @chvLoginName
		
		IF @@ROWCOUNT = 0
		INSERT INTO Logon.AccountLogonError
		(LoginName, ErrorTimes)
		SELECT
		@chvLoginName, 1
		
		--写IP错误记录
		UPDATE Logon.IPLogonError
		SET ErrorTimes = ErrorTimes + 1
		WHERE IPAddress = @chvIPAddress
		
		IF @@ROWCOUNT = 0
		INSERT INTO Logon.IPLogonError
		(IPAddress, ErrorTimes)
		SELECT
		@chvIPAddress, 1
		
		--写机器错误记录
		UPDATE Logon.HDLogonError
		SET ErrorTimes = ErrorTimes + 1
		WHERE HDType = @tnyHDType AND HDCode = @chvHDCode
		
		IF @@ROWCOUNT = 0
		INSERT INTO Logon.HDLogonError
		        ( HDType, HDCode, ErrorTimes )
		SELECT
				@tnyHDType, @chvHDCode, 1
		
		SET @chvErrMsg = '登录失败:用户名不存在或者密码错误'
		RETURN -1
	END
		
	--设置登录方式,直接登录
	SET @tnyLogonType = 0
END
--通过证书登录
ELSE 
BEGIN
	IF NOT EXISTS(SELECT 1 FROM Game.Certification WHERE UserID = @intUserID AND CerCode = @chvCertification)
	BEGIN
		SET @chvErrMsg = '登录失败:用户信息不正确'
		RETURN -1
	END
END


--执行公共登录逻辑
DECLARE @intLogonRet INT
EXEC @intLogonRet = dbo.PrPs_UserLogon_Common 
	@intAreaID,
	@intUserID,
    @tnyLogonType,
    @tnyHDType,
    @chvHDCode,
    @chvVersion,
    @sintPlazaServerID,
    @chvIPAddress,
    @chvCertification OUTPUT,
    @chvErrMsg OUTPUT
    
    
    RETURN @intLogonRet

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UserLogon_Normal] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UserLogon_Normal] TO [xbuser]
    AS [dbo];

