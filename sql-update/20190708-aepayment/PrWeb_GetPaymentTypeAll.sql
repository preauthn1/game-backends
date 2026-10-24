USE [CenterDB]
GO

/****** Object:  StoredProcedure [dbo].[PrWeb_GetPaymentTypeAll]    Script Date: 2019/7/8 下午 11:24:51 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:  Mars
-- Create date: 2019/07/05
-- Description:	获取PayType信息
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetPaymentTypeAll]	
	@chvRtnMsg varchar(50) = '' OUTPUT --錯誤訊息
AS
BEGIN
	SELECT G.GatewayName as GWName, T.ID, T.GWID, T.Type, t.Currencies, T.BankCode, T.MinAmount, T.MaxAmount, T.AmountFixed, T.FixedAmount, T.Status 
	FROM [Payment].[PayGateway] AS G 
	JOIN [Payment].[PayType] AS T ON G.ID = T.GWID
	ORDER BY GWID ASC, Type ASC, BankCode ASC
	
	SET @chvRtnMsg = '查詢成功'
	return 1
END


GO


