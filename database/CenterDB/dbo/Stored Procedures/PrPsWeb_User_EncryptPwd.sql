-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<密码加密>
-- =============================================
CREATE PROCEDURE [dbo].[PrPsWeb_User_EncryptPwd]
	@chvPwd				VARCHAR(32),		--明文密码
	@chvEncryptPwd		VARCHAR(32)OUTPUT	--加密的密码
AS
BEGIN
	
	SET @chvEncryptPwd = [REDACTED]
	
	RETURN 1	
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPsWeb_User_EncryptPwd] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPsWeb_User_EncryptPwd] TO [xbuser]
    AS [dbo];

