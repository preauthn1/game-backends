-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<设置Flag值>
-- =============================================
CREATE FUNCTION [dbo].[Fn_SetFlag]
(
	@chvFlag		varchar(32),
	@intPos			int,
	@chvVal			varchar(1)
)
RETURNS varchar(32)
AS
BEGIN
	SET @chvFlag = ISNULL(@chvFlag,'')
	IF(@intPos>LEN(@chvFlag))
	BEGIN
		SET @chvFlag = @chvFlag + REPLICATE('0',@intPos)
	END
	SET @chvFlag = LEFT(@chvFlag,@intPos-1) + @chvVal+SUBSTRING(@chvFlag,@intPos+1,LEN(@chvFlag))
	RETURN @chvFlag 
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_SetFlag] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_SetFlag] TO [xbuser]
    AS [dbo];

