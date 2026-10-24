-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<获取金币金额显示>
-- =============================================
CREATE FUNCTION [dbo].[Fn_ShowMoney]
(
	@lngAmount			bigint	--金币数
)
RETURNS varchar(32)
AS
BEGIN
	--return cast(@lngAmount as varchar(32))
	declare @dmlMoney decimal(18,2)	,@chvMoney varchar(32)
	SET @dmlMoney= @lngAmount*1.0/100
	IF @dmlMoney%1=0
	begin
		SET @chvMoney = cast(CAST(@dmlMoney as decimal(18,0)) as varchar)
	END
	ELSE IF @dmlMoney%0.1=0
	BEGIN
		SET @chvMoney = cast(CAST(@dmlMoney as decimal(18,1)) as varchar)
	END
	ELSE
	BEGIN
		SET @chvMoney = cast(@dmlMoney  as varchar)
	end
	return @chvMoney
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_ShowMoney] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_ShowMoney] TO [xbuser]
    AS [dbo];

