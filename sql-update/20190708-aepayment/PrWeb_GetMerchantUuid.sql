USE [CenterDB]
GO

/****** Object:  StoredProcedure [dbo].[PrWeb_GetMerchantUuid]    Script Date: 2019/7/9 下午 05:34:00 ******/
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id('dbo.PrWeb_GetMerchantUuid') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[PrWeb_GetMerchantUuid]
GO

/****** Object:  StoredProcedure [dbo].[PrWeb_GetMerchantUuid]    Script Date: 2019/7/9 下午 05:34:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Mars
-- Create date: 2019/07/08
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetMerchantUuid]
	@intPayTypeID int,
	@chvRtnMsg varchar(50) = '' OUTPUT	--錯誤訊息
AS
BEGIN

	SET NOCOUNT ON;

	SELECT T.BankCode, T.Currencies, M.uuid
	FROM payment.PayType AS T 
	JOIN payment.PayGateway AS G ON G.ID = T.GWID AND G.Status = 1 
	JOIN payment.merchantInfo AS M ON M.GWID = G.ID AND M.Status = 1 
	JOIN Payment.BankCodeType AS B ON  B.BankCode = T.BankCode 
	WHERE T.ID = @intPayTypeID;

	SET @chvRtnMsg = '';
	RETURN 1;
END
GO


