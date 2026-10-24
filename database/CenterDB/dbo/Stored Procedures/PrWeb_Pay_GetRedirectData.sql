-- =============================================
-- Author:  CCC
-- Create date: 2019/07/08
-- Description: 取得支付轉址參數
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Pay_GetRedirectData]
	@intID		int,
	@chvGUID	varchar(32)
AS
BEGIN
	
	DECLARE @dtNow	datetime = GETDATE()	

	SELECT Id, GWID, FormData, CreateTime, ExpireTime, [GUID]
	  FROM [CenterDB].[Pay].[RedirectData]
	 WHERE Id = @intID
	   AND [GUID] = @chvGUID
	   AND ExpireTime > @dtNow
END