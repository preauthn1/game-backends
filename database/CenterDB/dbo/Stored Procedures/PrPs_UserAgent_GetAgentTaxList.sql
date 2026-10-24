-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 查詢系統預設比例
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserAgent_GetAgentTaxList]
AS
BEGIN

	set nocount on

	EXEC [Club02].[dbo].[PrPs_UserAgent_GetAgentTaxList]

	return 1
END






GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UserAgent_GetAgentTaxList] TO [GameUser]
    AS [dbo];

