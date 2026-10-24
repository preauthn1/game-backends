-- =============================================
-- Author:  alan
-- Create date: 2018-3-14
-- Description:	获取銀聯卡銀行代號
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetUnionpayBankCode]
AS
BEGIN	
	EXEC CenterDB.dbo.PrPs_GetUnionpayBankCode
END