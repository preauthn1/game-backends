-- =============================================
-- Author:  alan
-- Create date: 20181225
-- Description:	提供時間戳
-- =============================================
CREATE FUNCTION [dbo].[Fn_GetTimeSeq]
()
RETURNS bigint
AS
BEGIN


	declare @lngTimeSeq bigint
	select @lngTimeSeq=
	cast(DatePart(year, getdate()) as varchar) +
	RIGHT(REPLICATE('0', 2) + cast(DatePart(month, getdate()) as varchar),2) +
	RIGHT(REPLICATE('0', 2) + cast(DatePart(day, getdate()) as varchar),2) +
	RIGHT(REPLICATE('0', 2) + cast(DatePart(hour, getdate()) as varchar),2) +
	RIGHT(REPLICATE('0', 2) + cast(DatePart(minute, getdate()) as varchar),2) +
	RIGHT(REPLICATE('0', 2) + cast(DatePart(second, getdate()) as varchar),2) +
	RIGHT(REPLICATE('0', 3) + cast(DatePart(millisecond, getdate()) as varchar),3)
	

		
	return @lngTimeSeq
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_GetTimeSeq] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_GetTimeSeq] TO [xbuser]
    AS [dbo];

