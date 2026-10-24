-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<获取兑换手续费>
-- =============================================
CREATE FUNCTION [dbo].[Fn_GetConvertTax]
(
	@lngAmount		bigint			--兑换数量
)
RETURNS BIGINT
AS
BEGIN
	
	DECLARE @dmlRate decimal(18,2),@lngTaxAmount BIGINT
	SELECT @dmlRate = ConvertRate FROM Money.ConvertRate WHERE @lngAmount BETWEEN MinRang AND MaxRang
	IF @dmlRate IS NULL
	BEGIN
		SET @dmlRate  = 0.01			--如果没有配置默认为0.01的手续费
	END
	
	IF @dmlRate>=1
	BEGIN
		SET @lngTaxAmount = CAST(@dmlRate AS BIGINT) * 100
	END
	ELSE
	BEGIN
		SET @lngTaxAmount = CAST(@lngAmount*@dmlRate AS BIGINT)
	END
	RETURN @lngTaxAmount 
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_GetConvertTax] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_GetConvertTax] TO [xbuser]
    AS [dbo];

