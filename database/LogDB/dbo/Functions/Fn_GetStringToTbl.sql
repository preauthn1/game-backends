-- =============================================
-- Author:		<xxx>
-- Create date: <2016-9-6>
-- Description:	<获取用户退出桌子时的扣款信息>
-- =============================================
CREATE FUNCTION [dbo].[Fn_GetStringToTbl]
(	
	@strings		VARCHAR(MAX)
)
RETURNS @returnTable TABLE (ID int)
BEGIN 		

	DECLARE @i INT
	
	SET @strings = RTRIM(LTRIM(@strings))
	SET @i = CHARINDEX(',', @strings)
	
	WHILE (@i >= 1)
	BEGIN
		INSERT @returnTable
		VALUES (LEFT(@strings,@i -1))
		
		SET @strings = SUBSTRING(@strings, @i + 1, LEN(@strings) -@i)
		SET @i = CHARINDEX(',', @strings)
	END

	IF @strings <> '\'
	BEGIN
		INSERT @returnTable
		VALUES (@strings)
	END

	RETURN
END
GO
GRANT SELECT
    ON OBJECT::[dbo].[Fn_GetStringToTbl] TO [GameUser]
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[Fn_GetStringToTbl] TO [xbuser]
    AS [dbo];

