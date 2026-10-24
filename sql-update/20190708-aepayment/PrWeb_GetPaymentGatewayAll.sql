USE [CenterDB]
GO

/****** Object:  StoredProcedure [dbo].[PrWeb_GetPaymentGatewayAll]    Script Date: 2019/7/8 U 11:24:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:  Mars
-- Create date: 2019/07/04
-- Description: oID
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetPaymentGatewayAll]
(
	@chvRtnMsg			varchar(50) = '' 	OUTPUT --~T
)
AS
BEGIN
	SET NOCOUNT ON

	SELECT ID, GatewayCode, GatewayName, MerchantInfo, SupportPayin, SupportPayout, ReviseAmount, BankCodes, Status FROM Payment.PayGateway
	
	SET @chvRtnMsg = 'ާ@\'
	RETURN 1
END
GO


