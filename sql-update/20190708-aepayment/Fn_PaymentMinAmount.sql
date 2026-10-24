USE [CenterDB]
GO

/****** Object:  UserDefinedFunction [dbo].[Fn_PaymentMinAmount]    Script Date: 2019/7/10 上午 12:02:24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<计算用户等级>
--	2018/05/10	Jason	#87 依運營需求調整等級判斷 @lngTaxAmount改@lngChargeAmount計算	
-- =============================================
CREATE FUNCTION [dbo].[Fn_PaymentMinAmount]
(
	@minAmount			BIGINT,
	@amountFixed		BIT,
	@fixedAmount		VARCHAR(MAX)
)
RETURNS INT
AS
BEGIN

	DECLARE @returnValue INT = 0
	DECLARE @tmpAmount TABLE (amount INT)
	IF @amountFixed = 1
	BEGIN
		INSERT INTO @tmpAmount SELECT [Value] FROM [dbo].[fn_Split2](@fixedAmount, ',')
		SELECT @returnValue = MIN(amount) FROM @tmpAmount
	END
	ELSE
	BEGIN
	IF @minAmount <= 0
		SELECT @returnValue = 1000
	ELSE
		SELECT @returnValue = @minAmount
	END
	RETURN @returnValue
END
GO


