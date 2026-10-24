-- =============================================
-- Author:  CCC
-- Create date: 2019/07/08
-- Description: 支付轉址參數
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Pay_CreateRedirectData]
	@intGWID		int,
	@chvFormData	varchar(max),
	@intID			int OUTPUT,
	@chvGUID		varchar(32) OUTPUT,
	@chvErrMsg		varchar(128) OUTPUT  --提示消息
AS
BEGIN
	
	IF not exists (SELECT TOP 1 1 FROM [CenterDB].[Pay].[PayType] WHERE GWID = @intGWID)
	BEGIN
		SET @chvErrMsg = '操作错误:支付方式不存在'
		RETURN -1
	END

	SET @chvGUID = REPLACE(NEWID(),'-','')

	INSERT INTO [CenterDB].[Pay].[RedirectData]
		(
			GWID, 
			FormData, 
			CreateTime, 
			ExpireTime,
			[GUID]
		)
	SELECT @intGWID, 
		   @chvFormData,
		   getdate(),
		   DATEADD(MINUTE, 5, getdate()),
		   @chvGUID

	SET @intID = @@IDENTITY
	SET @chvErrMsg = '操作成功'	
	RETURN 1
END