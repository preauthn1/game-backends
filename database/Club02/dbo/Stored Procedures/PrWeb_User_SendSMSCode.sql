-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<发送验证码>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_User_SendSMSCode]
@intUserID		INT=NULL,					--用户ID
@chvTel			VARCHAR(16) = NULL,			--手机号码
@tnyCodeType	TINYINT,					--验证码类型 1注册,2找回密码 3找回银行密码 4第一次綁定支付帳號 5修改支付帳號 6全民代理
@chvIPAddress	VARCHAR(40) = NULL,
@tnyCountdown	TINYINT OUTPUT,				--验证码倒计时长
@intCodeID      int OUTPUT,					--對應驗證碼編號
@chvErrMsg		VARCHAR(128) OUTPUT			--返回消息
AS
BEGIN
   declare @intResult int

	EXEC @intResult = [CenterDB].[dbo].[PrPs_User_SendSMSCode]
		@intUserID = @intUserID,
		@chvTel = @chvTel,
		@tnyCodeType = @tnyCodeType,	
		@chvIPAddress = @tnyCountdown,	
		@tnyCountdown = @tnyCountdown OUTPUT,	
		@intCodeID = @intCodeID OUTPUT,      
		@chvErrMsg = @chvErrMsg OUTPUT

	return @intResult
END