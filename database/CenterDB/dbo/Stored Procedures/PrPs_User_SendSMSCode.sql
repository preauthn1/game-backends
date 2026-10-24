-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<发送验证码>
--	2018/03/21	Jason		調整@tnyCodeType not in(1,2,3,4,5) & @tnyCodeType in (3,4,5) 
--  2018/03/22  alan		調整倒數秒數, 120s -> 180s
--	2018/06/13	Jason		#451 後台增加短信(SMS)發送狀態查詢 新增@chvIPAddress,@intUserID及新增PrNs_AddSMSCode @tnyCodeType,@chvIPAddress
--	2018/09/10	alan		增加全民代理type 6
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_User_SendSMSCode]
@intUserID		INT=NULL,					--用户ID
@chvTel			VARCHAR(16) = NULL,			--手机号码
@tnyCodeType	TINYINT,					--验证码类型 1注册,2找回密码 3找回银行密码 4第一次綁定支付帳號 5修改支付帳號 6全民代理 7净利润
@chvIPAddress	VARCHAR(40) = NULL,
@tnyCountdown	TINYINT OUTPUT,				--验证码倒计时长
@intCodeID      int OUTPUT,					--對應驗證碼編號
@chvErrMsg		VARCHAR(128) OUTPUT			--返回消息
AS
SET NOCOUNT ON

DECLARE @intDEFAULT_Coundown smallint = 180 -- 預設倒數秒數3mins

IF(@tnyCodeType not in(1,2,3,4,5,6,7))
BEGIN
	SET @chvErrMsg = '非法操作，类型錯誤'
	return -1
END

--如果是升级账号or 找回密码, 需要檢查手機號
IF @tnyCodeType in (1,2,7) AND @chvTel not like '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'
BEGIN
	SET @chvErrMsg = '请输入正确的11位数字手机号格式'
	return -1
END

--如果是其他type, 表示用戶已經註冊手機號了
IF @tnyCodeType in (3,4,5) 
BEGIN	
	SET @chvTel = NULL
	SELECT @chvTel = LoginName FROM Users.UserInfo with(nolock) where UserID = @intUserID and UserType = 1
	IF @chvTel IS NULL 
	BEGIN
		SET @chvErrMsg = '非法操作，手机号码錯誤'
		return -1
	END		
END


SET @tnyCountdown = 0
SET @intCodeID = 0

--(因應帳號綁定轉移關閉檢查)升级账号时需要判断手机号是否被使用
--IF @tnyCodeType=1 AND EXISTS(SELECT 1 FROM Users.UserInfo with(nolock) WHERE LoginName = @chvTel)
--BEGIN
--	SET @chvErrMsg = '手机号码已经被使用'
--	RETURN -1	
--END



SELECT @tnyCountdown = @intDEFAULT_Coundown - datediff(ss,CreateCodeTime,getdate()) FROM Users.UserCode 
	WHERE Tel = @chvTel and @tnyCodeType = CodeType AND datediff(ss,CreateCodeTime,getdate()) < @intDEFAULT_Coundown
IF @tnyCountdown is not null and @tnyCountdown > 0
BEGIN
	SET @chvErrMsg = '验证码已经发送，请勿重复发送'
	RETURN -1
END

IF (SELECT COUNT(1) FROM Users.UserCode WHERE Tel = @chvTel AND DATEDIFF(mi, CreateCodeTime, GETDATE()) <= 2) >= 2
BEGIN
	SET @chvErrMsg = '发送验证码太频繁'
	RETURN -1
END

--取得短信商
DECLARE @chvVender varchar(20) = NULL
SELECT top 1 @chvVender = vender from [ServerInfo].[SMSConfig] with (nolock) where [status] = 1
IF (@chvVender IS NULL) 
BEGIN
	SET @chvErrMsg = '短信平台选择失败'
	RETURN -1
END


--取得使用者IP
IF (@chvIPAddress IS NULL)
BEGIN
	SELECT @chvIPAddress = LastLogonIPAddress
	FROM Users.UserInfo WITH (NOLOCK)
	WHERE
		UserID = @intUserID

	SET @chvIPAddress = ISNULL(@chvIPAddress,'')
END


DECLARE @chvCode VARCHAR(32)
SET @chvCode = cast(ceiling(rand() * 999999) as int)

UPDATE Users.UserCode
SET VCode = @chvCode
,CreateCodeTime = GETDATE()
WHERE Tel = @chvTel AND CodeType = @tnyCodeType

IF @@ROWCOUNT<=0
BEGIN
	INSERT INTO Users.UserCode(Tel,VCode,CodeType,CreateCodeTime)
	SELECT @chvTel,@chvCode,@tnyCodeType,GETDATE()
END

DECLARE @dtExpireTime DATETIME = DATEADD(SECOND,@intDEFAULT_Coundown,GETDATE())


-- 寫入到 Notify Server 並取得對應的CodeID, 提供給Game server
EXEC NOTIFYLINKEDSERVER.Notify.dbo.PrNs_AddSMSCode
	@intUserID = @intUserID,
	@chvTel = @chvTel,
	@chvCode = @chvCode,
	@tnyCodeType = @tnyCodeType,
	@chvIPAddress = @chvIPAddress,
	@dtExpireTime = @dtExpireTime,
	@chvVender = @chvVender,
	@intCodeID = @intCodeID OUTPUT;

SET @chvErrMsg = '发送验证码成功'
SET @tnyCountdown = @intDEFAULT_Coundown

RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_User_SendSMSCode] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_User_SendSMSCode] TO [xbuser]
    AS [dbo];

