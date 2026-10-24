-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<绑定游客账号、銀行卡>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_User_BindGuestUnionpay]
@intUserID			INT,					--用户ID
@chvCode			VARCHAR(32),			--验证码
@chvTel				VARCHAR(32),			--绑定手机号码
@chvPassword		VARCHAR(32),			--账号密码
@chvRealName		VARCHAR(64),			--銀行账号对应的姓名
@chvUnionpayBank	VARCHAR(5),				--所屬銀行代號
@chvUnionpayAccount	VARCHAR(64),			--銀行账号
@tintCodeType		tinyint = 7,			--验证码类型 1注册,2找回密码 3找回银行密码 4第一次綁定支付帳號 5修改支付帳號
@chvErrMsg			VARCHAR(128) OUTPUT		--返回消息
AS
BEGIN

DECLARE @intResult		int = 0
DECLARE @chvExLoginName	VARCHAR(32)
DECLARE @chvCer			VARCHAR(32)

--检查账号类型
SELECT	@chvExLoginName = LoginName
	FROM [CenterDB].Users.UserInfo 
	WHERE
		UserID = @intUserID AND
		UserType = 3

--凭证
SELECT @chvCer = [CerCode]
  FROM [CenterDB].[Game].[Certification] with(nolock) 
 WHERE [UserID] = @intUserID

--账号是游客才绑定
IF @chvExLoginName IS NOT NULL
BEGIN
	EXEC @intResult = [CenterDB].[dbo].[PrPs_User_BindGuest]
		@intUserID = @intUserID,
		@chvCode = @chvCode,
		@chvTel = @chvTel,
		@chvPassword = [REDACTED],
		@tintCodeType = @tintCodeType,
		@chvErrMsg = @chvErrMsg OUTPUT

	UPDATE centerdb.Users.UserCode
		SET VCode = @chvCode
		WHERE Tel = @chvTel 
		AND DATEADD(ss,10*60,CreateCodeTime) >= GETDATE() 
		AND CodeType = @tintCodeType
		AND VCode = ''

	if @intResult < 1
	begin
		return @intResult
	end	
END


EXEC @intResult = [CenterDB].[dbo].[PrPs_BindUnionPayAccount]
	@intUserID = @intUserID,
	@chvCer	= @chvCer,			
	@chvCode = @chvCode,		
	@chvRealName = @chvRealName,
	@chvUnionpayBank = @chvUnionpayBank,
	@chvUnionpayAccount	= @chvUnionpayAccount,
	@tintCodeType = @tintCodeType,
	@chvErrMsg = @chvErrMsg	OUTPUT


return @intResult

END