








-- =============================================
-- Author:  harry
-- Create date: 2018/09/7
-- Description: 新增验证码
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SMS_Create]
	@vCode		varchar(20),
	@chvErrMsg		Varchar(50) OUT
AS
BEGIN

	--新增验证码
	INSERT INTO  dbo.SMS(VerifyCode) Values (@vCode)

	SET @chvErrMsg = '送出成功!'
	RETURN 1
END




