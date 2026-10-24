-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<base64>
-- =============================================
CREATE FUNCTION [dbo].[Fn_Encode]
(
	@chvCode VARCHAR(max)
)
RETURNS VARCHAR(max)
AS
BEGIN
	DECLARE @chvRetCode VARCHAR(MAX)
	SELECT @chvRetCode= (SELECT CONVERT(varbinary(max), @chvCode) FOR XML PATH(''), TYPE).value('/', 'nvarchar(max)') 

	RETURN @chvRetCode
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_Encode] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_Encode] TO [xbuser]
    AS [dbo];

