-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<账号密码加密>
-- =============================================
CREATE FUNCTION [dbo].[Fn_EncryptPwd] 
(
	@chvPassword VARCHAR(32)
)
RETURNS VARCHAR(32)
AS
BEGIN
	SET @chvPassword = [REDACTED]

	DECLARE @chvCode VARCHAR(32)
	
	SET @chvCode = dbo.Fn_Encode(@chvCode)
	SET @chvCode = dbo.Fn_MD5(@chvPassword)
	--SET @chvCode = dbo.Fn_Encode(@chvCode)
	SET @chvCode = dbo.Fn_MD5(@chvCode)
	
	RETURN @chvCode
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_EncryptPwd] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_EncryptPwd] TO [xbuser]
    AS [dbo];

