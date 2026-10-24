-- =============================================
-- Author:		alan
-- Create date: 20180319
-- Description:	新增通知簡訊資料
--	2018/06/13	Jason		#451 後台增加短信(SMS)發送狀態查詢 新增@tnyCodeType,@chvIPAddress,@intUserID 參數
-- =============================================
CREATE PROCEDURE [dbo].[PrNs_AddSMSCode]
@intUserID		INT=NULL,	
@chvTel			VARCHAR(16),	--手机号码
@chvCode	    VARCHAR(32),	--SMS Code
@tnyCodeType	TINYINT,		--验证码类型 1注册,2找回密码 3找回银行密码 4第一次綁定支付帳號 5修改支付帳號 6 全民代理
@chvIPAddress	VARCHAR(40),
@dtExpireTime	DATETIME,
@chvVender		VARCHAR(20),	--短信商
@intCodeID		Int OUTPUT		--返回CodeID

AS
BEGIN
SET NOCOUNT ON;
SET @intCodeID = 0;


INSERT INTO Notify.SMSVCode
	(	Tel,		VCode,		Status,	CreateTime,	LastSendTime,	IPAddress,		CodeType,		UserID,		ExpireTime, Vender) 
SELECT	@chvTel,	@chvCode,	0,		GETDATE(),	GETDATE(),		@chvIPAddress,	@tnyCodeType,	@intUserID,	@dtExpireTime, @chvVender

set @intCodeID = @@IDENTITY;

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrNs_AddSMSCode] TO [xbuser]
    AS [dbo];

