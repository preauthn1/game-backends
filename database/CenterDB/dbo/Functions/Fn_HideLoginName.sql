-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<隐藏账号>
-- =============================================
CREATE FUNCTION [dbo].[Fn_HideLoginName]
(
	@chvLoginName varchar(64)
)
RETURNS varchar(32)
AS
BEGIN
	declare @chvReturn varchar(32)
	if @chvLoginName is null
	begin
		set @chvReturn = ''
	end
	else 
	begin
		
		if len(@chvLoginName)<11 
		begin		
			
			IF(LEN(@chvLoginName)>=6) 
			BEGIN
				set @chvReturn = substring(@chvLoginName,1,2)+replicate('*',LEN(@chvLoginName)-4)+substring(@chvLoginName,len(@chvLoginName)-(2-1),2)
			END
			ELSE
			BEGIN
				set @chvReturn = substring(@chvLoginName,1,3)+replicate('*',LEN(@chvLoginName)-3)
			END
		end
		else
		begin
			set @chvReturn = substring(@chvLoginName,1,3)+replicate('*',LEN(@chvLoginName)-7)+substring(@chvLoginName,len(@chvLoginName)-(4-1),4)
		end
	end
		
	return @chvReturn
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_HideLoginName] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_HideLoginName] TO [xbuser]
    AS [dbo];

