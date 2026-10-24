USE [CenterDB]
GO

/****** Object:  StoredProcedure [dbo].[PrWeb_GetPaymentType]    Script Date: 2019/7/8 U 11:24:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:  Mars
-- Create date: 2019/07/04
-- Description: oI覡
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetPaymentType]
(
	@intID int = null,
	@chvRtnMsg varchar(50) = '' OUTPUT --~T
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @SelectCount int = 0;

	SELECT ID, GWID, Type, Currencies, BankCode, MinAmount, MaxAmount, AmountFixed, FixedAmount, Status FROM Payment.PayType WHERE ID = @intID
	
	SET @SelectCount = @@ROWCOUNT

	IF (@SelectCount > 0)
	BEGIN
		SET @chvRtnMsg = 'ާ@\'
	END
	SET @chvRtnMsg = 'dL'
	RETURN 1
END
GO


