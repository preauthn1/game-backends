CREATE FUNCTION [dbo].[FN_ClearHTMLTag]
(
	@String NVARCHAR(MAX)
)
RETURNS NVARCHAR(MAX)
AS
BEGIN
	DECLARE @Start INT
	DECLARE @End INT
	DECLARE @Length INT
 
	SET @Start = CHARINDEX('<',@String)
	SET @End = CHARINDEX('>',@String,CHARINDEX('<',@String))
	SET @Length = (@End - @Start) + 1
	
	WHILE @Start > 0 AND @End > 0 AND @Length > 0
	BEGIN
		SET @String = STUFF(@String,@Start,@Length,'')
		SET @Start = CHARINDEX('<',@String) SET @End = CHARINDEX('>',@String,CHARINDEX('<',@String))
		SET @Length = (@End - @Start) + 1
	END
 
	set @String = replace(@String,'script','')
	set @String = replace(@String,'style','')
	set @String = replace(@String,'#','')

	RETURN LTRIM(RTRIM(@String))
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[FN_ClearHTMLTag] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[FN_ClearHTMLTag] TO [xbuser]
    AS [dbo];

