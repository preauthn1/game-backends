-- =============================================
-- Author:  CCC
-- Create date: 2019/07/24
-- Description: 兌換管理-第三方銀行代碼對照
-- Changelog:
-- =============================================
create PROCEDURE [dbo].[PrWeb_Pay_GetConvertBankCode]
(
	@intGWID		INT
)
AS
BEGIN

	SET NOCOUNT ON

	SELECT [GWID],
           [Code],
           [TradeCode],
           [Status]
      FROM [CenterDB].[Pay].[ConvertBankCode]
     WHERE GWID = @intGWID
	   AND [Status] = 1
END