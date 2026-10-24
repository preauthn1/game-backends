create function [Fn_CheckValidIP] 
(
@chvIPAddress varchar(15)
) 
returns bit
as
begin


declare @result bit
set @result = 0

if @chvIPAddress like '%_.%_.%_.%_'
	and @chvIPAddress not like '%.%.%.%.%'
	and @chvIPAddress not like '%[^0-9.]%'
	and @chvIPAddress not like '%[0-9][0-9][0-9][0-9]%'
	and @chvIPAddress not like '%[3-9][0-9][0-9]%'
	and @chvIPAddress not like '%2[6-9][0-9]%'
	and @chvIPAddress not like '%25[6-9]%'
	set @result = 1

return @result

end
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_CheckValidIP] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_CheckValidIP] TO [xbuser]
    AS [dbo];

