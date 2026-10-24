-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<检查,并更新验证码>
--	2018/03/21	Jason		補上@tintCodeType 註解
--	2018/06/15	Jason		#451 後台增加短信(SMS)發送狀態查詢 呼叫Notify PrNs_UpdSMSCode進行更新
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_CheckSMSCode]
	@chvTel			varchar(11),			--手机号码
	@chvCode		varchar(6),				--验证码
	@tintCodeType	tinyint,				--验证码类型 1注册,2找回密码 3找回银行密码 4第一次綁定支付帳號 5修改支付帳號 6 全民代理
	@chvErrMsg		varchar(128) output		--返回消息
AS
BEGIN

	DECLARE @intCodeID INT
	
	UPDATE Users.UserCode
	SET VCode = ''
	WHERE Tel = @chvTel 
	AND DATEADD(ss,10*60,CreateCodeTime) >= GETDATE() 
	AND CodeType = @tintCodeType 
	AND VCode = @chvCode 
	AND VCode <> ''
	
	IF @@ROWCOUNT <= 0
	BEGIN
		SET @chvErrMsg = '验证码不正确或者验证码已经过期'
		RETURN -1
	END

	--更新簡訊使用時間
	UPDATE [NOTIFYLINKEDSERVER].[Notify].[Notify].[SMSVCode] SET UpdateTime = GETDATE(), [Status] = 2 
	WHERE 
		[Tel] = @chvTel
		AND [VCode] = @chvCode
		AND [Status] = 1
	

	/*
	EXEC [NOTIFYLINKEDSERVER].[Notify].[dbo].[PrNs_UpdSMSCode]
		@chvTel			= @chvTel,	--手机号码
		@chvCode	    = @chvCode,	--SMS Code
		@tnyCodeType	= @tintCodeType,
		@intCodeID		= @intCodeID		OUTPUT		--返回CodeID

	IF @intCodeID<=0
	BEGIN
		SET @chvErrMsg = '验证码不正确或者验证码已经过期'
		RETURN -1
	END
	*/

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_CheckSMSCode] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_CheckSMSCode] TO [xbuser]
    AS [dbo];

