-- =============================================
-- Author:  alan
-- Create date: 2018/8/28
-- Description:	隐藏部分內容
-- =============================================
CREATE FUNCTION [dbo].[Fn_HideData]
(
	@chvData varchar(64)
)
RETURNS varchar(32)
AS
BEGIN
	declare @chvReturn varchar(32)
	if @chvData is null
	begin
		set @chvReturn = ''
	end
	else 
	begin
		--如果小于11位显示前3位，其余全部替换*
		if len(@chvData)<11 
		begin		
			set @chvReturn = substring(@chvData,1,3)+replicate('*',LEN(@chvData)-3) 			
		end
		else
		begin
			set @chvReturn = substring(@chvData,1,3)+replicate('*',LEN(@chvData)-7)+substring(@chvData,len(@chvData)-(4-1),4)
		end
	end
		
	return ISNULL(@chvReturn,'')
END
