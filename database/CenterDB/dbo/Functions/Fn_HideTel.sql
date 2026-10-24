-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<隐藏手机号>
-- =============================================
CREATE FUNCTION [dbo].[Fn_HideTel]
(
	@chvTel		varchar(32)
)
RETURNS varchar(32)
AS
BEGIN
	declare @chvReturn varchar(32)
	if @chvTel is null or @chvTel like 'Fast_%'
	begin
		set @chvReturn = @chvTel
	end
	else 
	begin
		set @chvReturn = substring(@chvTel,1,3)+replicate('*',LEN(@chvTel)-7)+substring(@chvTel,len(@chvTel)-(4-1),4)		
	end
		
	return @chvReturn
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_HideTel] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_HideTel] TO [xbuser]
    AS [dbo];

