-- =============================================
-- Author:  alan
-- Create date: 2018/3/22
-- Description:	账号保密,隐藏部分內容
-- =============================================
CREATE FUNCTION [dbo].[Fn_HideAccount]
(
	@chvAccount varchar(64)
)
RETURNS varchar(32)
AS
BEGIN
	declare @chvReturn varchar(32)
	if @chvAccount is null
	begin
		set @chvReturn = ''
	end
	else 
	begin
		--如果账号小于11位显示前3位，其余全部替换*
		if len(@chvAccount)<11 
		begin		
			set @chvReturn = substring(@chvAccount,1,3)+replicate('*',LEN(@chvAccount)-3) 			
		end
		else
		begin
			set @chvReturn = substring(@chvAccount,1,3)+replicate('*',LEN(@chvAccount)-7)+substring(@chvAccount,len(@chvAccount)-(4-1),4)
		end
	end
		
	return ISNULL(@chvReturn,'')
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_HideAccount] TO [GameUser]
    AS [dbo];

