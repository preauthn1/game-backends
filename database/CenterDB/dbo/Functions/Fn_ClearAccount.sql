CREATE FUNCTION [dbo].[Fn_ClearAccount]
(
	@String VARCHAR(100)
)
RETURNS VARCHAR(100)
AS
BEGIN
	set @String = replace(@String,'script','')
	set @String = replace(@String,'style','')
	set @String = replace(@String,'<','')
	set @String = replace(@String,'>','')
	set @String = replace(@String,'=','')
	set @String = replace(@String,' ','')
	set @String = replace(@String,'.cn','')
	--set @String = replace(@String,'.com','')
	set @String = replace(@String,'www.','')
	--set @String = replace(@String,'.net','')
	set @String = replace(@String,'http:','')
	set @String = replace(@String,CHAR(13),'') --CR
	set @String = replace(@String,CHAR(10),'') --LF
	set @String = replace(@String,CHAR(9),'') --tab
	
	RETURN LTRIM(RTRIM(@String))
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_ClearAccount] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_ClearAccount] TO [xbuser]
    AS [dbo];

