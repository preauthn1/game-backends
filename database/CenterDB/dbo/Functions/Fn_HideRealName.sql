-- =============================================
-- Author:  alan
-- Create date: 2018/3/22
-- Description:	姓名保密, 隐藏部分內容
-- =============================================
CREATE FUNCTION [dbo].[Fn_HideRealName]
(
	@chvRealName varchar(64)
)
RETURNS varchar(32)
AS
BEGIN
	declare @chvReturn varchar(32)
	if @chvRealName is null
	begin
		set @chvReturn = ''
	end
	else
	begin
		--如果姓名小于3位显示前1位，其余全部替换*
		if len(@chvRealName)<3
		begin		
			set @chvReturn = substring(@chvRealName,1,1)+replicate('*',LEN(@chvRealName)-1)
		end
		else
		begin
			set @chvReturn = substring(@chvRealName,1,1)+replicate('*',LEN(@chvRealName)-2)+substring(@chvRealName,len(@chvRealName)-1,1)
		end	
	end			
	
	return ISNULL(@chvReturn,'')
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_HideRealName] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_HideRealName] TO [xbuser]
    AS [dbo];

