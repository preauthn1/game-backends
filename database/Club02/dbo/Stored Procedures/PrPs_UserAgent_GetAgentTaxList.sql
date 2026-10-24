-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 查询系统默认比例
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserAgent_GetAgentTaxList]
AS
BEGIN

	set nocount on

	/*  回传明细  */
	SELECT [Level]
		  ,[Stage1]
		  ,[Stage2]
		  ,[Stage3]
		--  ,[TaxAmount]
	FROM [Club02].[dbo].[UserAgentSystemLevelRate]

	return 1
END





